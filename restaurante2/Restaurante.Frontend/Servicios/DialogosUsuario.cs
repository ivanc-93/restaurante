namespace Restaurante.Frontend.Servicios;

public sealed class DialogosUsuario : IDialogosUsuario
{
    public DialogResult Mostrar(IWin32Window propietario, string mensaje, string titulo,
        MessageBoxButtons botones = MessageBoxButtons.OK, MessageBoxIcon icono = MessageBoxIcon.None)
        => MessageBox.Show(propietario, mensaje, titulo, botones, icono);
}
