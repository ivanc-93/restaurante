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
            if(!configuracion.EstaConfigurada)
            {
                using var preparar=new FormularioConexion(configuracion);
                if(preparar.ShowDialog()!=DialogResult.OK)return;
            }
            using var acceso = new FormularioAcceso(repositorio, configuracion);
            if (acceso.ShowDialog() != DialogResult.OK || acceso.UsuarioActual is null) return;
            if(acceso.UsuarioActual.Rol=="Administrador")
            {
                try
                {
                    if(repositorio.NecesitaCodigoRecuperacion())
                    {
                        using var codigo=new FormularioCodigoRecuperacion(repositorio);
                        if(codigo.ShowDialog()!=DialogResult.OK)continue;
                    }
                }
                catch(Exception ex){MessageBox.Show(ex is Microsoft.Data.SqlClient.SqlException?"No se pudo verificar la seguridad con SQL. Vuelve a iniciar sesión.":ex.Message,"Seguridad",MessageBoxButtons.OK,MessageBoxIcon.Warning);continue;}
            }
            using var principal = new FormularioPrincipal(repositorio, configuracion, acceso.UsuarioActual, dialogos);
            Application.Run(principal);
            if (!principal.SesionCerrada) return;
        }
    }
}
