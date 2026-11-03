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
using System.Windows.Forms;
using Restaurante.Frontend.Servicios;

internal sealed class DialogosPrueba : IDialogosUsuario
{
    public DialogResult Mostrar(IWin32Window propietario,string mensaje,string titulo,
        MessageBoxButtons botones=MessageBoxButtons.OK,MessageBoxIcon icono=MessageBoxIcon.None)
    {
        if(icono==MessageBoxIcon.Error) throw new InvalidOperationException(titulo+": "+mensaje);
        return botones==MessageBoxButtons.YesNo?DialogResult.Yes:DialogResult.OK;
    }
}
