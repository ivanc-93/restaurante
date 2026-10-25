namespace Restaurante.Frontend.Servicios;

public interface IDialogosUsuario
{
    DialogResult Mostrar(IWin32Window propietario, string mensaje, string titulo,
        MessageBoxButtons botones = MessageBoxButtons.OK, MessageBoxIcon icono = MessageBoxIcon.None);
}
