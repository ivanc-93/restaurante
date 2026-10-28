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
    private static void ExigirRecuperacion(SqlConnection c,SqlTransaction? t)
    {
        using var q=Consulta(c,t,"SELECT CASE WHEN OBJECT_ID(N'dbo.OperacionesRecuperacion',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.CodigosRecuperacion',N'U') IS NOT NULL THEN 1 ELSE 0 END;");
        if((int)q.ExecuteScalar()!!=1)throw new InvalidOperationException("La recuperación necesita la migración 006. Solicita al responsable técnico un respaldo y restauración comprobada antes de actualizar. No se modificará automáticamente la base.");
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
    public string GenerarCodigoRecuperacion(string claveActual)
    {
        var sesion=_sesion??throw new UnauthorizedAccessException("Inicia sesión como Administrador.");
        var cuenta=VerificarAcceso(UsuarioDeId(sesion.Id),claveActual,false);
        if(cuenta is null||cuenta.Id!=sesion.Id)throw new UnauthorizedAccessException("No se pudo verificar la contraseña vigente.");
        using var c=AbrirConexion();ExigirRecuperacion(c,null);using var t=c.BeginTransaction(IsolationLevel.Serializable);
        BloquearSeguridad(c,t);var actor=SesionEn(c,t,administrador:true);
        if(actor.VersionSeguridad!=cuenta.VersionSeguridad)throw new UnauthorizedAccessException("La cuenta cambió. Vuelve a iniciar sesión.");
        using(var q=Consulta(c,t,"SELECT COUNT(*) FROM dbo.Usuarios WHERE Id=@u AND (BloqueadoHasta IS NULL OR BloqueadoHasta<=SYSUTCDATETIME());",("@u",actor.Id)))
            if((int)q.ExecuteScalar()!!=1)throw new UnauthorizedAccessException("La cuenta está temporalmente bloqueada. Espera antes de generar el código.");
        var codigo=Convert.ToHexString(RandomNumberGenerator.GetBytes(32));var hash=HuellaCodigo(InstalacionRecuperacion(c,t),actor.Id,codigo);
        Ejecutar(c,t,"DELETE dbo.CodigosRecuperacion WHERE UsuarioId=@u; INSERT dbo.CodigosRecuperacion(UsuarioId,Verificador,VersionSeguridad)VALUES(@u,@h,@v);",("@u",actor.Id),("@h",hash),("@v",actor.VersionSeguridad));
        Seguridad(c,t,actor.Id,actor.Id,"Código de recuperación","Generación o rotación; código anterior revocado");t.Commit();return codigo;
    }
    public void ConfirmarCodigoRecuperacion(string codigo)
    {
        using var c=AbrirConexion();ExigirRecuperacion(c,null);using var t=c.BeginTransaction(IsolationLevel.Serializable);BloquearSeguridad(c,t);var actor=SesionEn(c,t,administrador:true);
        using var q=Consulta(c,t,"UPDATE dbo.CodigosRecuperacion SET Confirmado=1 WHERE UsuarioId=@u AND Consumido=0 AND VersionSeguridad=@v AND Verificador=@h AND Fecha>DATEADD(minute,-5,SYSUTCDATETIME());",("@u",actor.Id),("@v",actor.VersionSeguridad),("@h",HuellaCodigo(InstalacionRecuperacion(c,t),actor.Id,codigo)));
        if(q.ExecuteNonQuery()!=1)throw new InvalidOperationException("El código cambió. Genera y guarda un reemplazo.");t.Commit();
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
        using(var q=Consulta(c,t,"SELECT Prueba FROM dbo.OperacionesRecuperacion WITH(UPDLOCK,HOLDLOCK) WHERE Id=@id;",("@id",operacion)))
        {if(q.ExecuteScalar() is byte[] anterior)return CryptographicOperations.FixedTimeEquals(anterior,prueba)?new(true,"El restablecimiento ya se completó. Vuelve a iniciar sesión con la contraseña elegida; no lo repitas."):RechazoRecuperacion;}
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
