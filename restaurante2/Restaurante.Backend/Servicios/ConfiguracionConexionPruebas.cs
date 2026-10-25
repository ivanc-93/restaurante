using Microsoft.Data.SqlClient;
namespace Restaurante.Backend.Servicios;

// Modo explícito y aislado: nunca carga ni guarda el archivo de conexión del usuario.
public sealed class ConfiguracionConexionPruebas : IConfiguracionConexion
{
    private readonly string _cadena;
    public ConfiguracionConexionPruebas(string cadena)
    {
        var opciones = new SqlConnectionStringBuilder(cadena);
        if (!opciones.InitialCatalog.StartsWith("Restaurante_Pruebas_", StringComparison.Ordinal)
            || !opciones.IntegratedSecurity || opciones.AttachDBFilename.Length != 0)
            throw new ArgumentException("El modo de prueba exige una base Restaurante_Pruebas_ y autenticación integrada.");
        _cadena = opciones.ConnectionString;
    }
    public bool EstaConfigurada => true;
    public SqlConnection CrearConexion() => new(_cadena);
    public void ProbarYGuardar(string servidor, int puerto, string baseDatos, string usuario, string contrasena, bool confiarCertificado)
        => throw new InvalidOperationException("La conexión de prueba no puede guardar configuración del usuario.");
}
