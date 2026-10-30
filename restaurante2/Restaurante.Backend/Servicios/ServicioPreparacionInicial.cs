using System.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;

namespace Restaurante.Backend.Servicios;

public sealed class ServicioPreparacionInicial(IConfiguracionConexion configuracion)
{
    public EstadoInstalacionInicial ConsultarEstado()
    {
        new RepositorioRestaurante(configuracion).ValidarEsquema();
        using var c = configuracion.CrearConexion(); c.Open();
        return LeerEstado(c);
    }

    public PreparacionPrimerAdministrador Preparar(bool confirmarReemplazo)
    {
        new RepositorioRestaurante(configuracion).ValidarEsquema();
        using var c = configuracion.CrearConexion(); c.Open();
        if (!LeerEstado(c).AutorizadoWindows)
            throw new UnauthorizedAccessException("La preparación requiere autenticación Windows y permiso sysadmin de SQL Server. Solicite al responsable que prepare la instalación; no necesita compartir su contraseña.");
        // Same resource as first-user creation. Session ownership allows BACKUP outside a transaction.
        using var bloqueo = new SqlCommand("DECLARE @r int; EXEC @r=sys.sp_getapplock @Resource=N'SaboresInstalacion',@LockMode='Exclusive',@LockOwner='Session',@LockTimeout=10000; SELECT @r;", c);
        if ((int)bloqueo.ExecuteScalar()! < 0) throw new InvalidOperationException("Otra instalación está trabajando. Inténtelo nuevamente.");
        try
        {
            var estado = LeerEstado(c);
            if (!estado.Pendiente) throw new InvalidOperationException("La instalación ya está completada o la base tiene usuarios. Inicie sesión con una cuenta existente; no se reinicializará.");
            if (estado.TieneToken && !confirmarReemplazo)
                throw new InvalidOperationException("Existe un token anterior. Debe confirmar su reemplazo; el token anterior quedará invalidado.");
            var ruta = Respaldar(c);
            var secreto = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(secreto)));
            using var transaccion = c.BeginTransaction(IsolationLevel.Serializable);
            using var q = new SqlCommand("UPDATE dbo.InstalacionSabores WITH(UPDLOCK,HOLDLOCK) SET TokenInstalacionHash=@hash WHERE Id=1 AND Completada=0 AND NOT EXISTS(SELECT 1 FROM dbo.Usuarios WITH(UPDLOCK,HOLDLOCK));", c, transaccion);
            q.Parameters.Add("@hash", SqlDbType.VarChar, 64).Value = hash;
            if (q.ExecuteNonQuery() != 1) throw new InvalidOperationException("El estado cambió. La base no se reinicializará; vuelva a comprobarla.");
            transaccion.Commit();
            return new PreparacionPrimerAdministrador(configuracion, secreto, ruta);
        }
        finally
        {
            using var liberar = new SqlCommand("EXEC sys.sp_releaseapplock @Resource=N'SaboresInstalacion',@LockOwner='Session';", c);
            liberar.ExecuteNonQuery();
        }
    }

    private static EstadoInstalacionInicial LeerEstado(SqlConnection c)
    {
        using var q = new SqlCommand("SELECT CASE WHEN EXISTS(SELECT 1 FROM dbo.Usuarios) THEN 1 ELSE 0 END,Completada,CASE WHEN TokenInstalacionHash IS NULL THEN 0 ELSE 1 END,COALESCE(IS_SRVROLEMEMBER('sysadmin'),0) FROM dbo.InstalacionSabores WHERE Id=1;", c);
        using var lector = q.ExecuteReader();
        if (!lector.Read()) throw new InvalidOperationException("La base no está preparada para la instalación. Solicite revisión técnica sin reinicializarla.");
        var windows = new SqlConnectionStringBuilder(c.ConnectionString).IntegratedSecurity;
        return new(lector.GetInt32(0) == 1, lector.GetBoolean(1), lector.GetInt32(2) == 1, windows && lector.GetInt32(3) == 1);
    }

    private static string Respaldar(SqlConnection c)
    {
        using var q = new SqlCommand("SELECT CONVERT(nvarchar(4000),SERVERPROPERTY('InstanceDefaultBackupPath'));", c);
        var carpeta = q.ExecuteScalar() as string;
        if (string.IsNullOrWhiteSpace(carpeta)) throw new InvalidOperationException("SQL Server no tiene carpeta de respaldo disponible. Solicite configurarla antes de preparar la instalación.");
        var ruta = carpeta.TrimEnd('\\', '/') + "\\Sabores_Inicial_" + Guid.NewGuid().ToString("N") + ".bak";
        var baseSql = "[" + c.Database.Replace("]", "]]") + "]";
        q.CommandTimeout = 120;
        q.CommandText = $"BACKUP DATABASE {baseSql} TO DISK=@ruta WITH COPY_ONLY,CHECKSUM; RESTORE VERIFYONLY FROM DISK=@ruta WITH CHECKSUM;";
        q.Parameters.Add("@ruta", SqlDbType.NVarChar, 4000).Value = ruta;
        q.ExecuteNonQuery();
        return ruta;
    }
}
