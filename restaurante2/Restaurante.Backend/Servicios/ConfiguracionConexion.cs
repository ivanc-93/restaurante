using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;

namespace Restaurante.Backend.Servicios;

public sealed class ConfiguracionConexion : IConfiguracionConexion
{
    private readonly string _ruta = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SaboresRestaurante", "conexion.bin");
    private readonly object _candado = new();
    private string? _cadena;

    public ConfiguracionConexion() : this(null) { }

    public ConfiguracionConexion(string? rutaArchivo)
    {
        if (rutaArchivo is not null) _ruta = Path.GetFullPath(rutaArchivo);
        if (!File.Exists(_ruta)) return;
        try
        {
            var protegido = File.ReadAllBytes(_ruta);
            _cadena = Encoding.UTF8.GetString(
                ProtectedData.Unprotect(protegido, null, DataProtectionScope.CurrentUser));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or CryptographicException)
        {
            ErrorCarga = "No se pudo leer la configuración guardada. Configure nuevamente la conexión SQL.";
        }
    }

    public string? ErrorCarga { get; private set; }

    public bool EstaConfigurada => !string.IsNullOrWhiteSpace(_cadena);

    public SqlConnection CrearConexion()
    {
        lock (_candado)
        {
            if (string.IsNullOrWhiteSpace(_cadena))
                throw new InvalidOperationException("Configure primero la conexión SQL Server desde Administración.");
            return new SqlConnection(_cadena);
        }
    }

    public void ProbarYGuardar(string servidor, int puerto, string baseDatos, string usuario,
        string contrasena, bool confiarCertificado)
    {
        if (string.IsNullOrWhiteSpace(servidor) || string.IsNullOrWhiteSpace(baseDatos))
            throw new ArgumentException("La IP del servidor y la base de datos son obligatorias.");
        if (puerto is < 1 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(puerto), "El puerto debe estar entre 1 y 65535.");

        var opciones = new SqlConnectionStringBuilder
        {
            DataSource = $"{servidor.Trim()},{puerto}",
            InitialCatalog = baseDatos.Trim(),
            UserID = usuario.Trim(),
            Password = contrasena,
            IntegratedSecurity = string.IsNullOrWhiteSpace(usuario),
            Encrypt = true,
            TrustServerCertificate = confiarCertificado,
            ConnectTimeout = 8,
            PersistSecurityInfo = false
        };
        using (var prueba = new SqlConnection(opciones.ConnectionString))
            prueba.Open();

        var directorio = Path.GetDirectoryName(_ruta)!;
        Directory.CreateDirectory(directorio);
        var protegido = ProtectedData.Protect(Encoding.UTF8.GetBytes(opciones.ConnectionString),
            null, DataProtectionScope.CurrentUser);
        var temporal = _ruta + ".tmp";
        File.WriteAllBytes(temporal, protegido);
        File.Move(temporal, _ruta, true);
        lock (_candado) _cadena = opciones.ConnectionString;
        ErrorCarga = null;
    }
}
