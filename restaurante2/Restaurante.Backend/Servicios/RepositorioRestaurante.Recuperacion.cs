using System.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;
using Restaurante.Backend.Dominio;
namespace Restaurante.Backend.Servicios;
public sealed partial class RepositorioRestaurante
{
    private static readonly ResultadoRecuperacion RechazoRecuperacion=new(false,"No se pudo verificar la recuperación. Revisa los datos o solicita asistencia técnica.");
    private static byte[] Huella(string valor)=>SHA256.HashData(Encoding.UTF8.GetBytes(valor));
    private static bool TieneRecuperacion(Microsoft.Data.SqlClient.SqlConnection c,Microsoft.Data.SqlClient.SqlTransaction? t)
    {
        using var q=Consulta(c,t,"""
          SELECT CASE WHEN NOT EXISTS(
            SELECT 1 FROM (VALUES
              (N'RecuperacionInstalacion',N'Id'),(N'RecuperacionInstalacion',N'InstalacionId'),
              (N'CodigosRecuperacion',N'UsuarioId'),(N'CodigosRecuperacion',N'Verificador'),(N'CodigosRecuperacion',N'VersionSeguridad'),
              (N'CodigosRecuperacion',N'Consumido'),(N'CodigosRecuperacion',N'Confirmado'),(N'CodigosRecuperacion',N'Intentos'),(N'CodigosRecuperacion',N'PausaHasta'),(N'CodigosRecuperacion',N'Fecha'),
              (N'OperacionesRecuperacion',N'Id'),(N'OperacionesRecuperacion',N'UsuarioId'),(N'OperacionesRecuperacion',N'Prueba'),(N'OperacionesRecuperacion',N'Mecanismo'),(N'OperacionesRecuperacion',N'Fecha')
            ) x(Tabla,Columna) WHERE COL_LENGTH(N'dbo.'+Tabla,Columna) IS NULL
          ) THEN 1 ELSE 0 END;
          """);
        if((int)q.ExecuteScalar()!!=1)return false;
        q.CommandText="SELECT COUNT(*) FROM dbo.VersionEsquema WHERE Version=6;";
        if((int)q.ExecuteScalar()!!=1)return false;
        q.CommandText="SELECT COUNT(*) FROM dbo.RecuperacionInstalacion WHERE Id=1;";
        return (int)q.ExecuteScalar()! ==1;
    }
    public bool RecuperacionDisponible(){using var c=AbrirConexion();return TieneRecuperacion(c,null);}
    private static void ExigirRecuperacion(SqlConnection c,SqlTransaction? t)
    {
        if(!TieneRecuperacion(c,t))throw new InvalidOperationException("La recuperación no está disponible: requiere esquema 006 completo. Solicita respaldo y restauración verificados antes de actualizar; no se reinicializará la base.");
    }
    private static Guid InstalacionRecuperacion(SqlConnection c,SqlTransaction t)
    {using var q=Consulta(c,t,"SELECT InstalacionId FROM dbo.RecuperacionInstalacion WHERE Id=1;");return (Guid)q.ExecuteScalar()!;}
    private static byte[] HuellaCodigo(Guid instalacion,int usuario,string codigo)=>Huella($"{instalacion:N}|{usuario}|{codigo.Trim()}");
    public bool NecesitaCodigoRecuperacion()
    {
        using var c=AbrirConexion();var actor=SesionEn(c,null,administrador:true);
        using var q=Consulta(c,null,"SELECT CASE WHEN OBJECT_ID(N'dbo.CodigosRecuperacion',N'U') IS NULL THEN 0 ELSE 1 END;");
        if((int)q.ExecuteScalar()! ==0)return false;
        q.CommandText="SELECT COUNT(*) FROM dbo.CodigosRecuperacion WHERE UsuarioId=@u AND Consumido=0 AND Confirmado=1 AND VersionSeguridad=@v;";
        q.Parameters.AddWithValue("@u",actor.Id);q.Parameters.AddWithValue("@v",actor.VersionSeguridad);return (int)q.ExecuteScalar()! ==0;
    }
    // Expected authorization failures are results for the UI; the compatibility API still rejects them.
    private ResultadoGeneracionCodigo? RevisarSesionCodigo(SqlConnection c,SqlTransaction? t,UsuarioAutenticado sesion)
    {
        var actual=_sesion;
        if(actual is null||actual.Id!=sesion.Id||actual.VersionSeguridad!=sesion.VersionSeguridad)return new(EstadoGeneracionCodigo.SesionModificada,"Inicia sesión como Administrador antes de generar tu código de emergencia.");
        using var q=Consulta(c,t,"SELECT Rol,VersionSeguridad,CambioObligatorio,BloqueadoHasta FROM dbo.Usuarios WHERE Id=@id AND Activo=1;",("@id",sesion.Id));
        using var r=q.ExecuteReader();
        if(!r.Read()||r.GetInt32(1)!=sesion.VersionSeguridad)return new(EstadoGeneracionCodigo.SesionModificada,"Tu acceso cambió o se desactivó. Cierra esta ventana y vuelve a iniciar sesión.");
        if(r.GetString(0)!="Administrador")return new(EstadoGeneracionCodigo.SinPermiso,"Solo un Administrador puede generar su código de emergencia. Gerente y Mesero no tienen este permiso.");
        if(r.GetBoolean(2))return new(EstadoGeneracionCodigo.CambioObligatorio,"Primero cambia tu contraseña temporal desde Iniciar sesión; después entra con la nueva contraseña y genera el código.");
        if(!r.IsDBNull(3)&&r.GetDateTime(3)>DateTime.UtcNow)return new(EstadoGeneracionCodigo.CuentaBloqueada,"Tu cuenta está temporalmente bloqueada por intentos fallidos. Espera cinco minutos antes de volver a comprobar tu contraseña.");
        return null;
    }
    public ResultadoGeneracionCodigo IntentarGenerarCodigoRecuperacion(string claveActual)
    {
        if(string.IsNullOrEmpty(claveActual))return new(EstadoGeneracionCodigo.DatosIncompletos,"Escribe tu contraseña vigente de Administrador. No escribas aquí el código de emergencia.");
        var sesion=_sesion;
        if(sesion is null)return new(EstadoGeneracionCodigo.SesionModificada,"Inicia sesión como Administrador antes de generar el código.");
        using var c=AbrirConexion();
        if(RevisarSesionCodigo(c,null,sesion) is {} rechazo)return rechazo;
        if(!TieneRecuperacion(c,null))return new(EstadoGeneracionCodigo.NoDisponible,"La recuperación no está habilitada en esta base. Solicita su actualización respaldada al responsable técnico.");
        var cuenta=VerificarAcceso(UsuarioDeId(sesion.Id),claveActual,false);
        if(cuenta is null||cuenta.Id!=sesion.Id)
            return RevisarSesionCodigo(c,null,sesion)??new(EstadoGeneracionCodigo.CredencialesNoVerificadas,"No se pudo verificar tu contraseña vigente. Escríbela nuevamente respetando mayúsculas y minúsculas. Cinco intentos fallidos bloquean temporalmente la cuenta; no se cambió tu código.");
        using var t=c.BeginTransaction(IsolationLevel.Serializable);BloquearSeguridad(c,t);
        if(RevisarSesionCodigo(c,t,sesion) is {} modificado)return modificado;
        if(cuenta.VersionSeguridad!=sesion.VersionSeguridad)return new(EstadoGeneracionCodigo.SesionModificada,"La cuenta cambió durante la comprobación. Vuelve a iniciar sesión antes de generar el código.");
        var codigo=Convert.ToHexString(RandomNumberGenerator.GetBytes(32));var hash=HuellaCodigo(InstalacionRecuperacion(c,t),sesion.Id,codigo);
        Ejecutar(c,t,"DELETE dbo.CodigosRecuperacion WHERE UsuarioId=@u; INSERT dbo.CodigosRecuperacion(UsuarioId,Verificador,VersionSeguridad)VALUES(@u,@h,@v);",("@u",sesion.Id),("@h",hash),("@v",sesion.VersionSeguridad));
        Seguridad(c,t,sesion.Id,sesion.Id,"Código de recuperación","Generación o rotación; código anterior revocado");t.Commit();
        return new(EstadoGeneracionCodigo.Generado,"Código generado. El anterior quedó revocado. Guarda este reemplazo de forma segura; después marca la casilla y pulsa He guardado el código para activar su recuperación.",codigo);
    }
    public string GenerarCodigoRecuperacion(string claveActual)
    {
        var resultado=IntentarGenerarCodigoRecuperacion(claveActual);
        return resultado.Generado?resultado.Codigo!:throw new UnauthorizedAccessException(resultado.Mensaje);
    }
    public ResultadoRecuperacion IntentarConfirmarCodigoRecuperacion(string codigo)
    {
        if(string.IsNullOrEmpty(codigo)||codigo.Length!=64||codigo.Any(c=>!Uri.IsHexDigit(c)))return new(false,"No hay un código válido para confirmar. Genera un reemplazo y guárdalo antes de continuar.");
        var sesion=_sesion;if(sesion is null)return new(false,"Tu sesión terminó. Vuelve a iniciar sesión como Administrador.");
        using var c=AbrirConexion();
        if(!TieneRecuperacion(c,null))return new(false,"La recuperación no está habilitada en esta base. Solicita revisión técnica.");
        using var t=c.BeginTransaction(IsolationLevel.Serializable);BloquearSeguridad(c,t);
        if(RevisarSesionCodigo(c,t,sesion) is {} rechazo)return new(false,rechazo.Mensaje);
        using var q=Consulta(c,t,"UPDATE dbo.CodigosRecuperacion SET Confirmado=1 WHERE UsuarioId=@u AND Consumido=0 AND VersionSeguridad=@v AND Verificador=@h AND Fecha>DATEADD(minute,-5,SYSUTCDATETIME());",("@u",sesion.Id),("@v",sesion.VersionSeguridad),("@h",HuellaCodigo(InstalacionRecuperacion(c,t),sesion.Id,codigo)));
        if(q.ExecuteNonQuery()!=1)return new(false,"El código cambió o pasó el tiempo de confirmación. Genera un reemplazo, guárdalo y vuelve a confirmar.");
        t.Commit();return new(true,"Código de emergencia confirmado. Consérvalo de forma segura fuera del equipo.");
    }
    public void ConfirmarCodigoRecuperacion(string codigo)
    {
        var resultado=IntentarConfirmarCodigoRecuperacion(codigo);
        if(!resultado.Correcto)throw new InvalidOperationException(resultado.Mensaje);
    }
    public ResultadoRecuperacion RecuperarConAdministrador(Guid operacion,string recibo,string objetivo,string administrador,string claveAdministrador,string nueva)
    {
        var cuenta=VerificarAcceso(administrador,claveAdministrador,true);
        if(cuenta is null)return RechazoRecuperacion;
        if(cuenta.CambioObligatorio)return new(false,"Cambia tu contraseña desde Iniciar sesión antes de autorizar.");
        if(cuenta.Rol!="Administrador")return new(false,"Esta recuperación requiere otro Administrador activo.");
        return RestablecerRecuperacion(operacion,recibo,objetivo,nueva,cuenta,null);
    }
    public ResultadoRecuperacion RecuperarConCodigo(Guid operacion,string recibo,string usuario,string codigo,string nueva)
        =>RestablecerRecuperacion(operacion,recibo,usuario,nueva,null,codigo);
    private ResultadoRecuperacion RestablecerRecuperacion(Guid operacion,string recibo,string usuario,string nueva,UsuarioAutenticado? administrador,string? codigo)
    {
        if(operacion==Guid.Empty||recibo.Length!=64)return new(false,"Solicitud inválida. Vuelve a abrir la recuperación.");
        if(Contrasenas.ErrorValidacion(nueva) is string error)return new(false,error);
        if(string.IsNullOrWhiteSpace(usuario)||usuario.Length>50||codigo?.Length>100)return RechazoRecuperacion;
        using var c=AbrirConexion();ExigirRecuperacion(c,null);using var t=c.BeginTransaction(IsolationLevel.Serializable);BloquearSeguridad(c,t);
        var prueba=Huella(recibo);
        using(var q=Consulta(c,t,"SELECT o.Prueba,o.Mecanismo,CASE WHEN u.Usuario=@usuario THEN 1 ELSE 0 END,u.ContrasenaHash,u.Sal,u.Algoritmo,u.Iteraciones,CASE WHEN o.Mecanismo=N'Código' AND @autorizador IS NULL THEN 1 WHEN EXISTS(SELECT 1 FROM dbo.AuditoriaSeguridad a WHERE a.DestinoId=o.UsuarioId AND a.UsuarioId=@autorizador AND a.Accion=N'Contraseña recuperada' AND a.Detalle LIKE N'%'+CONVERT(nvarchar(36),o.Id)) THEN 1 ELSE 0 END FROM dbo.OperacionesRecuperacion o WITH(UPDLOCK,HOLDLOCK) JOIN dbo.Usuarios u ON u.Id=o.UsuarioId WHERE o.Id=@id;",("@id",operacion),("@usuario",usuario.Trim()),("@autorizador",administrador?.Id)))
        using(var r=q.ExecuteReader())
        {
            if(r.Read())
            {
                if(!CryptographicOperations.FixedTimeEquals((byte[])r[0],prueba))return RechazoRecuperacion;
                var mismo=r.GetInt32(7)==1&&r.GetString(1)==(administrador is null?"Código":"Otro administrador")&&r.GetInt32(2)==1
                    &&Contrasenas.Verificar(nueva,r.GetString(3),r.GetString(4),r.GetString(5),r.GetInt32(6));
                return mismo?new(true,"El restablecimiento original ya se completó. Inicia sesión; no se modificó otra vez la cuenta.")
                    :new(false,"El identificador pertenece a una recuperación distinta o la contraseña cambió después. Consulta el resultado original; no se modificó ninguna cuenta.");
            }
        }
        int destino,version;string rol,hash,sal,algoritmo;int iteraciones;
        using(var q=Consulta(c,t,"SELECT Id,VersionSeguridad,Rol,ContrasenaHash,Sal,Algoritmo,Iteraciones FROM dbo.Usuarios WITH(UPDLOCK,HOLDLOCK) WHERE Usuario=@u AND Activo=1;",("@u",usuario.Trim())))
        using(var r=q.ExecuteReader())
        {if(!r.Read())return RechazoRecuperacion;destino=r.GetInt32(0);version=r.GetInt32(1);rol=r.GetString(2);hash=r.GetString(3);sal=r.GetString(4);algoritmo=r.GetString(5);iteraciones=r.GetInt32(6);}
        if(administrador is not null)
        {
            using var q=Consulta(c,t,"SELECT COUNT(*) FROM dbo.Usuarios WHERE Id=@id AND Activo=1 AND CambioObligatorio=0 AND Rol=N'Administrador' AND VersionSeguridad=@v AND (BloqueadoHasta IS NULL OR BloqueadoHasta<=SYSUTCDATETIME());",("@id",administrador.Id),("@v",administrador.VersionSeguridad));
            if((int)q.ExecuteScalar()!!=1||administrador.Id==destino)return new(false,"Se necesita otro Administrador vigente. Para tu propia cuenta usa el cambio de contraseña o un código de emergencia.");
        }
        else
        {
            if(rol!="Administrador")return RechazoRecuperacion;
            byte[] esperado;bool consumido;int versionCodigo;DateTime? pausa;
            using(var q=Consulta(c,t,"SELECT Verificador,CASE WHEN Consumido=1 OR Confirmado=0 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END,VersionSeguridad,PausaHasta FROM dbo.CodigosRecuperacion WITH(UPDLOCK,HOLDLOCK) WHERE UsuarioId=@u;",("@u",destino)))
            using(var r=q.ExecuteReader()){if(!r.Read())return RechazoRecuperacion;esperado=(byte[])r[0];consumido=r.GetBoolean(1);versionCodigo=r.GetInt32(2);pausa=r.IsDBNull(3)?null:r.GetDateTime(3);}
            if(pausa>DateTime.UtcNow)return RechazoRecuperacion;
            if(consumido||versionCodigo!=version||!CryptographicOperations.FixedTimeEquals(esperado,HuellaCodigo(InstalacionRecuperacion(c,t),destino,codigo??"")))
            {
                Ejecutar(c,t,"UPDATE dbo.CodigosRecuperacion SET Intentos=CASE WHEN PausaHasta<=SYSUTCDATETIME() THEN 1 ELSE Intentos+1 END,PausaHasta=CASE WHEN PausaHasta<=SYSUTCDATETIME() THEN NULL WHEN Intentos+1>=5 THEN DATEADD(minute,5,SYSUTCDATETIME()) ELSE NULL END WHERE UsuarioId=@u;",("@u",destino));
                Seguridad(c,t,null,destino,"Recuperación rechazada","Verificación no válida");t.Commit();return RechazoRecuperacion;
            }
        }
        if(Contrasenas.Verificar(nueva,hash,sal,algoritmo,iteraciones))return new(false,"Elige una contraseña diferente de la anterior.");
        var h=Contrasenas.Crear(nueva);
        Ejecutar(c,t,"UPDATE dbo.Usuarios SET ContrasenaHash=@h,Sal=@s,Algoritmo=N'PBKDF2-SHA256',Iteraciones=@i,CambioObligatorio=0,VersionSeguridad=VersionSeguridad+1,IntentosFallidos=0,BloqueadoHasta=NULL WHERE Id=@u; UPDATE dbo.CodigosRecuperacion SET Consumido=1 WHERE UsuarioId=@u; DELETE dbo.AutorizacionesOperacion WHERE Consumida=0 AND (SolicitanteId=@u OR AutorizadorId=@u);",("@h",h.Hash),("@s",h.Sal),("@i",Contrasenas.Iteraciones),("@u",destino));
        var mecanismo=administrador is null?"Código":"Otro administrador";
        Ejecutar(c,t,"INSERT dbo.OperacionesRecuperacion(Id,UsuarioId,Prueba,Mecanismo)VALUES(@id,@u,@p,@m);",("@id",operacion),("@u",destino),("@p",prueba),("@m",mecanismo));
        Seguridad(c,t,administrador?.Id,destino,"Contraseña recuperada",$"{mecanismo}; operación {operacion}");t.Commit();
        return new(true,rol=="Administrador"?"Contraseña actualizada. Inicia sesión y genera un nuevo código de emergencia desde Usuarios y roles.":"Contraseña actualizada. Vuelve a iniciar sesión.");
    }
    public ResultadoRecuperacion ConsultarRecuperacion(Guid operacion,string recibo)
    {
        if(recibo.Length!=64)return RechazoRecuperacion;
        using var c=AbrirConexion();ExigirRecuperacion(c,null);
        using var q=Consulta(c,null,"SELECT Prueba FROM dbo.OperacionesRecuperacion WHERE Id=@id;",("@id",operacion));
        return q.ExecuteScalar() is byte[] hash&&CryptographicOperations.FixedTimeEquals(hash,Huella(recibo))?new(true,"El restablecimiento se completó. Inicia sesión con la contraseña elegida."):new(false,"No se confirmó el restablecimiento. Conserva esta ventana y comprueba la conexión antes de reintentar.");
    }
}
