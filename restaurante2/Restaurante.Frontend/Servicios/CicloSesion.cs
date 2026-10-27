using Restaurante.Backend.Servicios;
using Restaurante.Frontend.Formularios;
namespace Restaurante.Frontend.Servicios;

public static class CicloSesion
{
    public static void Ejecutar(IRepositorioRestaurante repositorio, IConfiguracionConexion configuracion,
        IDialogosUsuario? dialogos = null)
    {
        while (true)
        {
            repositorio.CerrarSesion();
            using var acceso = new FormularioAcceso(repositorio, configuracion);
            if (acceso.ShowDialog() != DialogResult.OK || acceso.UsuarioActual is null) return;
            using var principal = new FormularioPrincipal(repositorio, configuracion, acceso.UsuarioActual, dialogos);
            Application.Run(principal);
            if (!principal.SesionCerrada) return;
        }
    }
}
