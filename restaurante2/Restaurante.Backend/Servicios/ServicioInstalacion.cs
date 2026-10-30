using System.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;
namespace Restaurante.Backend.Servicios;
public sealed class ServicioInstalacion(IConfiguracionConexion configuracion)
{
    public bool NecesitaAdministrador()
    {
        new RepositorioRestaurante(configuracion).ValidarEsquema();
        using var c=configuracion.CrearConexion();c.Open();using var q=new SqlCommand("SELECT COUNT(*) FROM dbo.Usuarios;",c);
        return (int)q.ExecuteScalar()! ==0;
    }
    public void CrearPrimerAdministrador(string token,string nombre,string usuario,string clave)
    {
        var h=Contrasenas.Crear(clave);nombre=ReglasRestaurante.Texto(nombre,"Nombre",100);usuario=ReglasRestaurante.Texto(usuario,"Usuario",50);
        using var c=configuracion.CrearConexion();c.Open();using var t=c.BeginTransaction(IsolationLevel.Serializable);
        using var q=new SqlCommand("DECLARE @r int;EXEC @r=sys.sp_getapplock @Resource=N'SaboresInstalacion',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=10000;SELECT @r;",c,t);
        if((int)q.ExecuteScalar()!<0)throw new InvalidOperationException("Instalación ocupada.");
        q.CommandText="SELECT COUNT(*) FROM dbo.Usuarios WITH(UPDLOCK,HOLDLOCK);";
        if((int)q.ExecuteScalar()!!=0)throw new InvalidOperationException("La base ya tiene usuarios. Inicie sesión con una cuenta existente.");
        q.CommandText="SELECT TokenInstalacionHash FROM dbo.InstalacionSabores WITH(UPDLOCK,HOLDLOCK) WHERE Id=1 AND Completada=0;";
        var esperado=q.ExecuteScalar() as string;
        var recibido=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        if(esperado is null)throw new InvalidOperationException("La instalación no está aprovisionada o ya fue completada. Use Preparar instalación con Windows o solicite ayuda al responsable.");
        if(!CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(esperado),Encoding.ASCII.GetBytes(recibido)))throw new UnauthorizedAccessException("El token no corresponde a esta instalación o fue reemplazado. Use Preparar instalación con Windows o solicite un token vigente al responsable.");
        q.CommandText="INSERT dbo.Usuarios(Nombre,Usuario,Rol,ContrasenaHash,Sal,Algoritmo,Iteraciones)VALUES(@n,@u,N'Administrador',@h,@s,N'PBKDF2-SHA256',@i);DECLARE @id int=CONVERT(int,SCOPE_IDENTITY());INSERT dbo.AuditoriaSeguridad(UsuarioId,DestinoId,Accion,Detalle)VALUES(@id,@id,N'Primer administrador',N'Aprovisionamiento autorizado de un solo uso');UPDATE dbo.InstalacionSabores SET Completada=1,TokenInstalacionHash=NULL WHERE Id=1;";
        q.Parameters.AddWithValue("@n",nombre);q.Parameters.AddWithValue("@u",usuario);q.Parameters.AddWithValue("@h",h.Hash);q.Parameters.AddWithValue("@s",h.Sal);q.Parameters.AddWithValue("@i",Contrasenas.Iteraciones);q.ExecuteNonQuery();t.Commit();
    }
}
