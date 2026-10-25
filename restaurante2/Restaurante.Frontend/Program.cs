using Restaurante.Backend.Servicios;
using Restaurante.Frontend.Formularios;

namespace Restaurante.Frontend;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        var configuracion = new ConfiguracionConexion();
        if (configuracion.ErrorCarga is not null)
            MessageBox.Show(configuracion.ErrorCarga, "Configuración SQL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        IRepositorioRestaurante repositorio = new RepositorioRestaurante(configuracion);
        while (true)
        {
            using var acceso = new FormularioAcceso(repositorio, configuracion);
            if (acceso.ShowDialog() != DialogResult.OK || acceso.UsuarioActual is null) return;
            using var principal = new FormularioPrincipal(repositorio, configuracion, acceso.UsuarioActual);
            Application.Run(principal);
            if (!principal.SesionCerrada) return;
        }
    }
}
