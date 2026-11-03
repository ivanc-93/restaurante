using Restaurante.Backend.Aplicacion;
using Restaurante.Backend.Aplicacion.Contratos;
using Restaurante.Backend.Configuration;
using Restaurante.Backend.Data;
using Restaurante.Backend.Data.Repositorios;
using Restaurante.Backend.Dominio.Reglas;
using Restaurante.Backend.Seguridad;
using Restaurante.Frontend.Presentacion;
using Restaurante.Frontend.Controles;
using Restaurante.Frontend.Infraestructura.Recuperacion;
using Microsoft.Data.SqlClient;


internal sealed class ConexionPrueba(string cadena) : IConfiguracionConexion
{
    public bool EstaConfigurada => true;
    public SqlConnection CrearConexion() => new(cadena);
    public void ProbarYGuardar(string servidor,int puerto,string baseDatos,string usuario,string contrasena,bool confiarCertificado)
        => throw new NotSupportedException("Las pruebas no cambian la configuración del usuario.");
}
