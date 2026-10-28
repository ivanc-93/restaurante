using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Restaurante.Backend.Dominio;
namespace Restaurante.Backend.Servicios;

public sealed partial class RepositorioRestaurante
{
    private void ExigirLecturaPedido(int pedidoId)
    {
        using var c=AbrirConexion();using var t=c.BeginTransaction(IsolationLevel.Serializable);
        ExigirLecturaPedido(c,t,pedidoId);t.Commit();
    }
    private void ExigirLecturaPedido(SqlConnection c,SqlTransaction t,int pedidoId)
    {
        var actor=SesionEn(c,t);if(actor.Rol is "Administrador" or "Gerente")return;
        using var q=Consulta(c,t,"SELECT COUNT(*) FROM dbo.Pedidos p LEFT JOIN dbo.Facturas f ON f.PedidoId=p.Id WHERE p.Id=@p AND (p.Estado IN(N'Abierto',N'En preparación') OR f.UsuarioId=@actor);",("@p",pedidoId),("@actor",actor.Id));
        if((int)q.ExecuteScalar()!!=1)throw new UnauthorizedAccessException("Solo puede consultar pedidos abiertos y sus propios comprobantes.");
    }
    private UsuarioAutenticado? VerificarAcceso(string usuario,string clave,bool actualizarHash)
    {
        if(string.IsNullOrWhiteSpace(usuario)||usuario.Length>50||clave.Length>128)return null;
        using var c=AbrirConexion();using var t=c.BeginTransaction(IsolationLevel.Serializable);
        using var q=Consulta(c,t,"SELECT Id,Nombre,Rol,ContrasenaHash,Sal,Algoritmo,Iteraciones,VersionSeguridad,CambioObligatorio,BloqueadoHasta FROM dbo.Usuarios WITH(UPDLOCK,HOLDLOCK) WHERE Usuario=@u AND Activo=1;",("@u",usuario.Trim()));
        int id,version,iter;string nombre,rol,hash,sal,alg;bool cambio;
        using(var r=q.ExecuteReader())
        {
            if(!r.Read()){return null;}
            if(!r.IsDBNull(9)&&r.GetDateTime(9)>DateTime.UtcNow){return null;}
            id=r.GetInt32(0);nombre=r.GetString(1);rol=r.GetString(2);hash=r.GetString(3);sal=r.GetString(4);alg=r.GetString(5);iter=r.GetInt32(6);version=r.GetInt32(7);cambio=r.GetBoolean(8);
        }
        if(!Contrasenas.Verificar(clave,hash,sal,alg,iter))
        {
            Ejecutar(c,t,"UPDATE dbo.Usuarios SET IntentosFallidos=IntentosFallidos+1,BloqueadoHasta=CASE WHEN IntentosFallidos+1>=5 THEN DATEADD(minute,5,SYSUTCDATETIME()) ELSE NULL END WHERE Id=@id;",("@id",id));
            Seguridad(c,t,null,id,"Acceso rechazado","Credenciales incorrectas");t.Commit();return null;
        }
        Ejecutar(c,t,"UPDATE dbo.Usuarios SET IntentosFallidos=0,BloqueadoHasta=NULL WHERE Id=@id;",("@id",id));
        if(actualizarHash && alg=="SHA256")
        {
            // Legacy demo secrets must be changed rather than silently promoted as safe.
            if(clave.Length>=12){var h=Contrasenas.Crear(clave);Ejecutar(c,t,"UPDATE dbo.Usuarios SET ContrasenaHash=@h,Sal=@s,Algoritmo=N'PBKDF2-SHA256',Iteraciones=@i WHERE Id=@id;",("@h",h.Hash),("@s",h.Sal),("@i",Contrasenas.Iteraciones),("@id",id));}
            else {cambio=true;Ejecutar(c,t,"UPDATE dbo.Usuarios SET CambioObligatorio=1 WHERE Id=@id;",("@id",id));}
        }
        Seguridad(c,t,id,id,"Acceso comprobado","Autenticación individual");t.Commit();
        return new(nombre,rol,id,version,cambio);
    }
    private static void Seguridad(SqlConnection c,SqlTransaction t,int? actor,int? destino,string accion,string detalle)
        =>Ejecutar(c,t,"INSERT dbo.AuditoriaSeguridad(UsuarioId,DestinoId,Accion,Detalle)VALUES(@a,@d,@op,@txt);",("@a",actor),("@d",destino),("@op",accion),("@txt",detalle));

    private UsuarioAutenticado SesionEn(SqlConnection c,SqlTransaction? t,bool gerente=false,bool administrador=false,bool permitirCambio=false)
    {
        var s=_sesion??throw new UnauthorizedAccessException("Inicie sesión.");
        using var q=Consulta(c,t,"SELECT Nombre,Rol,VersionSeguridad,CambioObligatorio FROM dbo.Usuarios WHERE Id=@id AND Activo=1;",("@id",s.Id));
        using var r=q.ExecuteReader();
        if(!r.Read()||r.GetInt32(2)!=s.VersionSeguridad)throw new UnauthorizedAccessException("Su acceso cambió. Cierre sesión y vuelva a entrar.");
        var rol=r.GetString(1);var cambio=r.GetBoolean(3);
        if(cambio&&!permitirCambio)throw new UnauthorizedAccessException("Cambie primero su contraseña inicial.");
        if(administrador&&rol!="Administrador" || gerente&&rol is not("Administrador" or "Gerente"))throw new UnauthorizedAccessException("Esta acción requiere autorización del gerente o administrador.");
        if(rol is not("Administrador" or "Gerente" or "Mesero" or "Empleado"))throw new UnauthorizedAccessException("Rol no permitido.");
        return new(r.GetString(0),rol,s.Id,r.GetInt32(2),cambio);
    }
    public void CambiarMiContrasena(string anterior,string nueva)
    {
        var s=_sesion??throw new UnauthorizedAccessException("Inicie sesión.");
        var verificada=VerificarAcceso(UsuarioDeId(s.Id),anterior,false);
        if(verificada is null||verificada.Id!=s.Id)throw new UnauthorizedAccessException("Contraseña actual incorrecta o acceso bloqueado.");
        var hash=Contrasenas.Crear(nueva);
        if(anterior==nueva||nueva=="Admin123!")throw new ArgumentException("La nueva contraseña debe ser diferente.");
        using var c=AbrirConexion();using var t=c.BeginTransaction(IsolationLevel.Serializable);
        SesionEn(c,t,permitirCambio:true);
        Ejecutar(c,t,"UPDATE dbo.Usuarios SET ContrasenaHash=@h,Sal=@s,Algoritmo=N'PBKDF2-SHA256',Iteraciones=@i,CambioObligatorio=0,VersionSeguridad=VersionSeguridad+1 WHERE Id=@id;",("@h",hash.Hash),("@s",hash.Sal),("@i",Contrasenas.Iteraciones),("@id",s.Id));
        Seguridad(c,t,s.Id,s.Id,"Cambio de contraseña","Cambio individual");t.Commit();_sesion=null;
    }
    private string UsuarioDeId(int id)
    {using var c=AbrirConexion();using var q=Consulta(c,null,"SELECT Usuario FROM dbo.Usuarios WHERE Id=@id;",("@id",id));return q.ExecuteScalar() as string??"";}
    public IReadOnlyList<UsuarioGestion> ListarUsuarios()
    {
        using var c=AbrirConexion();SesionEn(c,null,administrador:true);using var q=Consulta(c,null,"SELECT Id,Nombre,Usuario,Rol,Activo,CambioObligatorio FROM dbo.Usuarios ORDER BY Nombre;");using var r=q.ExecuteReader();var lista=new List<UsuarioGestion>();
        while(r.Read())lista.Add(new(r.GetInt32(0),r.GetString(1),r.GetString(2),r.GetString(3),r.GetBoolean(4),r.GetBoolean(5)));return lista;
    }
    public IReadOnlyList<RegistroSeguridad> ConsultarAuditoriaSeguridad()
    {
        using var c=AbrirConexion();SesionEn(c,null,administrador:true);
        using var q=Consulta(c,null,"SELECT TOP(200) DATEADD(hour,-6,a.Fecha),COALESCE(u.Nombre,N'Acceso sin sesión'),COALESCE(d.Nombre,N'No aplica'),a.Accion,a.Detalle FROM dbo.AuditoriaSeguridad a LEFT JOIN dbo.Usuarios u ON u.Id=a.UsuarioId LEFT JOIN dbo.Usuarios d ON d.Id=a.DestinoId ORDER BY a.Id DESC;");
        using var r=q.ExecuteReader();var filas=new List<RegistroSeguridad>();while(r.Read())filas.Add(new(r.GetDateTime(0),r.GetString(1),r.GetString(2),r.GetString(3),r.GetString(4)));return filas;
    }
    public void GuardarUsuario(int? id,string nombre,string usuario,string rol,bool activo,string? nuevaClave)
    {
        nombre=ReglasRestaurante.Texto(nombre,"Nombre",100);usuario=ReglasRestaurante.Texto(usuario,"Usuario",50);
        if(rol is not("Administrador" or "Gerente" or "Mesero" or "Empleado"))throw new ArgumentException("Seleccione un rol válido.");
        (string Hash,string Sal)? hash=string.IsNullOrEmpty(nuevaClave)?null:Contrasenas.Crear(nuevaClave);
        if(id is null&&hash is null)throw new ArgumentException("Indique contraseña temporal de al menos 12 caracteres.");
        using var c=AbrirConexion();using var t=c.BeginTransaction(IsolationLevel.Serializable);
        BloquearSeguridad(c,t);var actor=SesionEn(c,t,administrador:true);
        if(id.HasValue)
        {
            using var q=Consulta(c,t,"SELECT Rol,Activo FROM dbo.Usuarios WITH(UPDLOCK,HOLDLOCK) WHERE Id=@id;",("@id",id));bool ultimo;
            using(var r=q.ExecuteReader()){if(!r.Read())throw new ArgumentException("Usuario inexistente.");ultimo=r.GetString(0)=="Administrador"&&r.GetBoolean(1)&&(!activo||rol!="Administrador");}
            if(ultimo){using var n=Consulta(c,t,"SELECT COUNT(*) FROM dbo.Usuarios WHERE Activo=1 AND Rol=N'Administrador';");if((int)n.ExecuteScalar()!<=1)throw new InvalidOperationException("No puede desactivar ni degradar al último administrador.");}
            Ejecutar(c,t,"UPDATE dbo.Usuarios SET Nombre=@n,Usuario=@u,Rol=@r,Activo=@a,VersionSeguridad=VersionSeguridad+1 WHERE Id=@id;",("@n",nombre),("@u",usuario),("@r",rol),("@a",activo),("@id",id));
        }
        else
        {
            using var q=Consulta(c,t,"INSERT dbo.Usuarios(Nombre,Usuario,Rol,Activo,ContrasenaHash,Sal)OUTPUT INSERTED.Id VALUES(@n,@u,@r,@a,N'',N'');",("@n",nombre),("@u",usuario),("@r",rol),("@a",activo));id=(int)q.ExecuteScalar()!;
        }
        if(hash.HasValue)Ejecutar(c,t,"UPDATE dbo.Usuarios SET ContrasenaHash=@h,Sal=@s,Algoritmo=N'PBKDF2-SHA256',Iteraciones=@i,CambioObligatorio=1,IntentosFallidos=0,BloqueadoHasta=NULL WHERE Id=@id;",("@h",hash.Value.Hash),("@s",hash.Value.Sal),("@i",Contrasenas.Iteraciones),("@id",id));
        Seguridad(c,t,actor.Id,id,"Guardar usuario",$"Rol {rol}; activo {activo}; cambio de clave {hash.HasValue}");t.Commit();
    }
    private static void BloquearSeguridad(SqlConnection c,SqlTransaction t)
    {using var q=Consulta(c,t,"DECLARE @r int;EXEC @r=sys.sp_getapplock @Resource=N'SaboresInstalacion',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=10000;SELECT @r;");if((int)q.ExecuteScalar()!<0)throw new InvalidOperationException("Seguridad ocupada. Reintente.");}

    public void AutorizarOperacion(Guid id,string tipo,string json,string usuario,string clave)
    {
        var resultado=IntentarAutorizarOperacion(id,tipo,json,usuario,clave);
        if(!resultado.Autorizada)throw new UnauthorizedAccessException(resultado.Mensaje);
    }

    private static int ExigirAutorizacion(SqlConnection c,SqlTransaction t,Guid id,string tipo,string hash,UsuarioAutenticado actor)
    {
        if(actor.Rol is "Administrador" or "Gerente")return actor.Id;
        using var q=Consulta(c,t,"SELECT a.AutorizadorId FROM dbo.AutorizacionesOperacion a WITH(UPDLOCK,HOLDLOCK) JOIN dbo.Usuarios u WITH(UPDLOCK,HOLDLOCK) ON u.Id=a.AutorizadorId WHERE a.OperacionId=@id AND a.Tipo=@tipo AND a.SolicitudHash=@hash AND a.SolicitanteId=@actor AND a.Consumida=0 AND a.Caduca>SYSUTCDATETIME() AND u.Activo=1 AND u.CambioObligatorio=0 AND u.Rol IN(N'Administrador',N'Gerente') AND u.VersionSeguridad=a.VersionAutorizador;",("@id",id),("@tipo",tipo),("@hash",hash),("@actor",actor.Id));
        return q.ExecuteScalar() is int jefe?jefe:throw new UnauthorizedAccessException("Solicite autorización vigente del gerente para esta operación exacta.");
    }
}
