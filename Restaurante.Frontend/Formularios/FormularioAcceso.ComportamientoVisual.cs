namespace Restaurante.Frontend.Formularios;

public sealed partial class FormularioAcceso
{
    private void MostrarContrasena_Cambio(object? sender, EventArgs e)
    {
        ctContrasena.UseSystemPasswordChar = !chkMostrarContrasena.Checked;
    }

    private void AjustarMensaje_Resize(object? sender, EventArgs e)
    {
        lblMensaje.MaximumSize = new Size(Math.Max(180, tlpPie.ClientSize.Width - tlpPie.Padding.Horizontal), 0);
    }

    private void AjustarEtiquetas_Resize(object? sender, EventArgs e)
    {
        var ancho = Math.Max(180, tlpEstructura.ClientSize.Width - tlpEstructura.Padding.Horizontal - SystemInformation.VerticalScrollBarWidth);
        foreach (var etiqueta in tlpEstructura.Controls.OfType<Label>())
            etiqueta.MaximumSize = new Size(ancho, 0);
    }
}
