using Microsoft.Data.SqlClient;
using Restaurante.Backend.Servicios;

internal sealed class ConexionPrueba(string cadena) : IConfiguracionConexion
{
    public bool EstaConfigurada => true;
    public SqlConnection CrearConexion() => new(cadena);
    public void ProbarYGuardar(string servidor,int puerto,string baseDatos,string usuario,string contrasena,bool confiarCertificado)
        => throw new NotSupportedException("Las pruebas no cambian la configuración del usuario.");
}
