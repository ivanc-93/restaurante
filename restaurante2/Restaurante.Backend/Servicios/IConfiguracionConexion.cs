using Microsoft.Data.SqlClient;

namespace Restaurante.Backend.Servicios;

public interface IConfiguracionConexion
{
    bool EstaConfigurada { get; }
    SqlConnection CrearConexion();
    void ProbarYGuardar(string servidor, int puerto, string baseDatos, string usuario,
        string contrasena, bool confiarCertificado);
}
