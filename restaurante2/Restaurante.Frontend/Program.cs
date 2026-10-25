using Restaurante.Backend.Servicios;
using Restaurante.Frontend.Servicios;
namespace Restaurante.Frontend;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        if (args.Length > 0 && !args.SequenceEqual(new[] { "--pruebas" }))
        {
            MessageBox.Show("Opción desconocida. Use --pruebas únicamente con una conexión aislada.", "Inicio");
            return;
        }
        IConfiguracionConexion configuracion;
        if (args.SequenceEqual(new[] { "--pruebas" }))
        {
            var cadena = Environment.GetEnvironmentVariable("RESTAURANTE_PRUEBAS_CONEXION");
            if (string.IsNullOrWhiteSpace(cadena))
            {
                MessageBox.Show("Indique RESTAURANTE_PRUEBAS_CONEXION para una base aislada.", "Modo de prueba");
                return;
            }
            try { configuracion = new ConfiguracionConexionPruebas(cadena); }
            catch (ArgumentException ex) { MessageBox.Show(ex.Message, "Modo de prueba"); return; }
        }
        else
        {
            var guardada = new ConfiguracionConexion();
            if (guardada.ErrorCarga is not null)
                MessageBox.Show(guardada.ErrorCarga, "Configuración SQL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            configuracion = guardada;
        }
        CicloSesion.Ejecutar(new RepositorioRestaurante(configuracion), configuracion);
    }
}
