using Microsoft.Data.SqlClient;
namespace Restaurante.Backend.Servicios;
public sealed class ConfiguracionConexionTemporal(string cadena):IConfiguracionConexion
{
    public bool EstaConfigurada=>true;
    public SqlConnection CrearConexion()=>new(cadena);
    public void ProbarYGuardar(string servidor,int puerto,string baseDatos,string usuario,string contrasena,bool confiarCertificado)
        =>throw new InvalidOperationException("La conexión temporal no guarda configuración.");
}
