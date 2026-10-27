using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Restaurante.Backend.Dominio;
namespace Restaurante.Backend.Servicios;
public sealed partial class RepositorioRestaurante
{
    private UsuarioAutenticado? SesionAutorizacion(SqlConnection c,SqlTransaction? t)
    {
        var s=_sesion;if(s is null)return null;
        using var q=Consulta(c,t,"SELECT Nombre,Rol,VersionSeguridad FROM dbo.Usuarios WITH(HOLDLOCK) WHERE Id=@id AND Activo=1 AND CambioObligatorio=0;",("@id",s.Id));
        using var r=q.ExecuteReader();
        if(!r.Read()||r.GetInt32(2)!=s.VersionSeguridad||r.GetString(1) is not("Mesero" or "Empleado" or "Gerente" or "Administrador"))return null;
        return new(r.GetString(0),r.GetString(1),s.Id,r.GetInt32(2));
    }
    public bool RevocarAutorizacion(Guid id)
    {
        using var c=AbrirConexion();using var t=c.BeginTransaction(IsolationLevel.Serializable);
        var actor=SesionAutorizacion(c,t);if(actor is null)return false;
        Ejecutar(c,t,"DELETE dbo.AutorizacionesOperacion WHERE OperacionId=@id AND SolicitanteId=@actor AND Consumida=0;",("@id",id),("@actor",actor.Id));t.Commit();return true;
    }
    public ResultadoAutorizacion IntentarAutorizarOperacion(Guid id,string tipo,string json,string usuario,string clave)
    {
        static ResultadoAutorizacion R(EstadoAutorizacion e,string m)=>new(e,m);
        // Revoke only this actor's unused approval in a committed transaction, including failed retries.
        if(!RevocarAutorizacion(id))return R(EstadoAutorizacion.SesionModificada,"Tu sesión cambió. Vuelve a iniciar sesión antes de solicitar autorización.");
        if(string.IsNullOrWhiteSpace(usuario)||string.IsNullOrEmpty(clave))return R(EstadoAutorizacion.DatosIncompletos,"Escribe usuario y contraseña vigentes del gerente o administrador.");
        var autorizador=VerificarAcceso(usuario,clave,true);
        if(autorizador is null)return R(EstadoAutorizacion.CredencialesNoVerificadas,"No se pudo verificar la cuenta. Revisa usuario y contraseña. Si acumulaste cinco intentos fallidos, espera cinco minutos o consulta al administrador.");
        if(autorizador.CambioObligatorio)return R(EstadoAutorizacion.CambioObligatorio,"Debes cambiar tu contraseña desde Iniciar sesión antes de autorizar.");
        if(autorizador.Rol is not("Administrador" or "Gerente"))return R(EstadoAutorizacion.SinPermiso,"Esta cuenta no tiene permiso para autorizar. Utiliza una cuenta de Gerente o Administrador.");
        object? solicitud;
        try
        {
            solicitud=tipo switch {
                "GestionMesa"=>JsonSerializer.Deserialize<SolicitudGestionMesa>(json),
                "ConciliarTarjeta"=>JsonSerializer.Deserialize<SolicitudConciliacion>(json),
                "Ajuste"=>JsonSerializer.Deserialize<SolicitudAjuste>(json),_=>null};
        }
        catch(JsonException){return R(EstadoAutorizacion.SolicitudInvalida,"La solicitud es inválida. Cierra esta ventana y vuelve a revisar la cuenta.");}
        var solicitudId=solicitud switch{SolicitudGestionMesa s=>s.OperacionId,SolicitudAjuste s=>s.OperacionId,SolicitudConciliacion s=>s.OperacionId,_=>Guid.Empty};
        if(id==Guid.Empty||id!=solicitudId||solicitud is null)return R(EstadoAutorizacion.SolicitudInvalida,"La solicitud es inválida. Cierra esta ventana y vuelve a revisar la cuenta.");
        var hash=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(solicitud,solicitud.GetType()))));
        using var c=AbrirConexion();using var t=c.BeginTransaction(IsolationLevel.Serializable);
        var actor=SesionAutorizacion(c,t);if(actor is null)return R(EstadoAutorizacion.SesionModificada,"Tu sesión cambió. Vuelve a iniciar sesión.");
        using(var q=Consulta(c,t,"SELECT UsuarioId FROM dbo.OperacionesRestaurante WITH(UPDLOCK,HOLDLOCK) WHERE Id=@id;",("@id",id)))
        {
            if(q.ExecuteScalar() is int propietario)
                return propietario==actor.Id?R(EstadoAutorizacion.OperacionAplicada,"SQL ya registró esta operación. Recupérala desde Inicio con su identificador original; no la repitas.")
                    :R(EstadoAutorizacion.SolicitudInvalida,"La solicitud no corresponde a esta sesión.");
        }
        using(var q=Consulta(c,t,"SELECT VersionSeguridad FROM dbo.Usuarios WITH(UPDLOCK,HOLDLOCK) WHERE Id=@id AND Activo=1 AND CambioObligatorio=0 AND Rol IN(N'Administrador',N'Gerente');",("@id",autorizador.Id)))
            if(q.ExecuteScalar() is not int v||v!=autorizador.VersionSeguridad)return R(EstadoAutorizacion.SesionModificada,"Los permisos de esta cuenta cambiaron. Vuelve a verificar la autorización.");
        int? mesa=null,pedido=null;string? version=null;decimal? total=null;string? motivo=null;
        if(solicitud is SolicitudGestionMesa g)
        {
            if(g.Accion is not("Cancelar" or "Incidencia" or "Corregir estado"))return R(EstadoAutorizacion.SolicitudInvalida,"Acción no autorizable.");
            mesa=g.MesaId;pedido=g.PedidoId;version=g.Version;motivo=g.Motivo;
        }
        else if(solicitud is SolicitudAjuste a){pedido=a.PedidoId;version=a.Version;total=a.TotalEsperado;motivo=a.Motivo;}
        else if(solicitud is SolicitudConciliacion conciliacion)
        {
            motivo=conciliacion.Motivo;
            using var lectura=AbrirConexion();
            using var q=Consulta(lectura,null,"SELECT PedidoId,SolicitudOriginal FROM dbo.PagosManuales WHERE OperacionId=@op AND Estado=N'Incierto';",("@op",conciliacion.OperacionIncierta));
            using var r=q.ExecuteReader();
            if(!r.Read())return R(EstadoAutorizacion.SolicitudModificada,"La tarjeta ya no está pendiente. Actualiza la cuenta antes de solicitar autorización.");
            pedido=r.GetInt32(0);var original=JsonSerializer.Deserialize<SolicitudCobro>(r.GetString(1))!;version=original.Version;total=original.TotalEsperado;
        }
        if(string.IsNullOrWhiteSpace(motivo)||motivo.Length>500)return R(EstadoAutorizacion.SolicitudInvalida,"Escribe un motivo válido antes de solicitar autorización.");
        if(pedido.HasValue&&!mesa.HasValue)
        {
            using var lectura=AbrirConexion();using var q=Consulta(lectura,null,"SELECT MesaId FROM dbo.Pedidos WHERE Id=@p;",("@p",pedido));
            if(q.ExecuteScalar() is int m)mesa=m;
        }
        if(mesa.HasValue)
        {
            using var q=Consulta(c,t,"SELECT Id FROM dbo.Mesas WITH(UPDLOCK,HOLDLOCK) WHERE Id=@m;",("@m",mesa));
            if(q.ExecuteScalar() is null)return R(EstadoAutorizacion.SolicitudModificada,"La mesa cambió. Actualiza la cuenta antes de autorizar.");
        }
        if(pedido.HasValue)
        {
            using var q=Consulta(c,t,"SELECT MesaId,Estado,VersionCuenta,Total FROM dbo.Pedidos WITH(UPDLOCK,HOLDLOCK) WHERE Id=@p;",("@p",pedido));using var r=q.ExecuteReader();
            if(!r.Read()||r.GetString(1) is not("Abierto" or "En preparación")||Convert.ToBase64String((byte[])r[2])!=version||total.HasValue&&r.GetDecimal(3)!=total||mesa.HasValue&&r.GetInt32(0)!=mesa)
                return R(EstadoAutorizacion.SolicitudModificada,"La solicitud cambió o venció. Actualiza la cuenta y solicita una nueva autorización.");
        }
        else if(solicitud is SolicitudGestionMesa {Accion:"Corregir estado"} correccion)
        {
            using var q=Consulta(c,t,"SELECT COUNT(*) FROM dbo.Mesas m WITH(UPDLOCK,HOLDLOCK) WHERE Id=@m AND Estado=N'Ocupada' AND NOT EXISTS(SELECT 1 FROM dbo.Pedidos WHERE MesaId=m.Id AND Estado IN(N'Abierto',N'En preparación'));",("@m",correccion.MesaId));
            if((int)q.ExecuteScalar()!!=1)return R(EstadoAutorizacion.SolicitudModificada,"El estado de la mesa cambió. Actualiza antes de solicitar autorización.");
        }
        else return R(EstadoAutorizacion.SolicitudInvalida,"Selecciona un pedido válido.");
        Ejecutar(c,t,"INSERT dbo.AutorizacionesOperacion(OperacionId,Tipo,SolicitudHash,SolicitanteId,AutorizadorId,VersionAutorizador,Caduca)VALUES(@id,@tipo,@hash,@actor,@jefe,@v,DATEADD(minute,2,SYSUTCDATETIME()));",("@id",id),("@tipo",tipo),("@hash",hash),("@actor",actor.Id),("@jefe",autorizador.Id),("@v",autorizador.VersionSeguridad));
        Seguridad(c,t,actor.Id,autorizador.Id,"Autorizar operación",$"{tipo}; operación {id}");t.Commit();
        return R(EstadoAutorizacion.Autorizada,"Operación autorizada únicamente para los datos revisados.");
    }
}
