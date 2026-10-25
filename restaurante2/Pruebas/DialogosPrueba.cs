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
