using Microsoft.Data.SqlClient;
using Restaurante.Backend.Servicios;
namespace Sabores.Mantenimiento;
public static class RestablecimientoAdministrativo
{
    public static string Ejecutar(string cadena,string usuario,string temporal,string responsable,string motivo,Guid operacion)
    {
        if(string.IsNullOrWhiteSpace(responsable)||string.IsNullOrWhiteSpace(motivo)||responsable.Length>100||motivo.Length>200||operacion==Guid.Empty)throw new ArgumentException("Indica responsable, motivo e identificador válidos.");
        var h=Contrasenas.Crear(temporal);
        using(var previa=new SqlConnection(cadena))
        {
            previa.Open();using var q=new SqlCommand("SELECT CASE WHEN OBJECT_ID(N'dbo.OperacionesRecuperacion',N'U') IS NOT NULL THEN 1 ELSE 0 END;",previa);
            if(Convert.ToInt32(q.ExecuteScalar())!=1)throw new InvalidOperationException("Requiere migración 006 previamente respaldada y aprobada.");
        }
        var respaldo=RespaldoRecuperacion.Verificar(cadena);
        using var c=new SqlConnection(cadena);c.Open();using var t=c.BeginTransaction(System.Data.IsolationLevel.Serializable);
        using var cmd=new SqlCommand("DECLARE @r int;EXEC @r=sys.sp_getapplock @Resource=N'SaboresInstalacion',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=15000;SELECT @r;",c,t);
        if(Convert.ToInt32(cmd.ExecuteScalar())<0)throw new InvalidOperationException("Seguridad ocupada.");
        cmd.CommandText="SELECT COUNT(*) FROM dbo.OperacionesRecuperacion WHERE Id=@op;";cmd.Parameters.AddWithValue("@op",operacion);
        if(Convert.ToInt32(cmd.ExecuteScalar())!=0)throw new InvalidOperationException("La intervención ya fue registrada. No la repitas; comprueba el inicio de sesión.");
        cmd.CommandText="SELECT Id FROM dbo.Usuarios WITH(UPDLOCK,HOLDLOCK) WHERE Usuario=@u AND Rol=N'Administrador' AND Activo=1;";cmd.Parameters.AddWithValue("@u",usuario);
        if(cmd.ExecuteScalar() is not int id)throw new InvalidOperationException("No se encontró el Administrador activo seleccionado. No se reactiva ni crea ninguna cuenta.");
        cmd.CommandText="UPDATE dbo.Usuarios SET ContrasenaHash=@h,Sal=@s,Algoritmo=N'PBKDF2-SHA256',Iteraciones=@i,CambioObligatorio=1,VersionSeguridad=VersionSeguridad+1,IntentosFallidos=0,BloqueadoHasta=NULL WHERE Id=@id; UPDATE dbo.CodigosRecuperacion SET Consumido=1 WHERE UsuarioId=@id; DELETE dbo.AutorizacionesOperacion WHERE Consumida=0 AND (SolicitanteId=@id OR AutorizadorId=@id); INSERT dbo.OperacionesRecuperacion(Id,UsuarioId,Prueba,Mecanismo)VALUES(@op,@id,CRYPT_GEN_RANDOM(32),N'Mantenimiento'); INSERT dbo.AuditoriaSeguridad(DestinoId,Accion,Detalle)VALUES(@id,N'Recuperación excepcional',@detalle);";
        cmd.Parameters.AddWithValue("@h",h.Hash);cmd.Parameters.AddWithValue("@s",h.Sal);cmd.Parameters.AddWithValue("@i",Contrasenas.Iteraciones);cmd.Parameters.AddWithValue("@id",id);cmd.Parameters.AddWithValue("@detalle",$"Responsable: {responsable}; motivo: {motivo}; operación: {operacion}");cmd.ExecuteNonQuery();t.Commit();return respaldo;
    }
}
