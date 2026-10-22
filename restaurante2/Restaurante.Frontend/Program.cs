using Restaurante.Backend.Dominio;
using Restaurante.Backend.Servicios;
using Restaurante.Frontend.Formularios;

namespace Restaurante.Frontend;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        IConfiguracionConexion configuracion = new ConfiguracionConexion();
        IRepositorioRestaurante repositorio = new RepositorioRestaurante(configuracion);
        using var acceso = new FormularioAcceso(repositorio, configuracion);
        if (acceso.ShowDialog() != DialogResult.OK || acceso.UsuarioActual is null) return;
        Application.Run(new FormularioPrincipal(repositorio, configuracion, acceso.UsuarioActual));
    }
}
