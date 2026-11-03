namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioCambiarClave
{
    private TextBox ctActual = null!;
    private TextBox ctNueva = null!;
    private TextBox ctRepetir = null!;
    private Label lblMensaje = null!;
    private Button btnGuardar = null!;
    private void InitializeComponent()
    {
        var flpP = DisenoFormulario.Preparar(this, "Cambiar mi contraseña");
        ctActual = DisenoFormulario.Campo(flpP, "Contraseña actual", true);
        ctNueva = DisenoFormulario.Campo(flpP, "Nueva contraseña de 12 a 128 caracteres", true);
        ctRepetir = DisenoFormulario.Campo(flpP, "Repetir nueva contraseña", true);
        flpP.Controls.Add(new Label { Text = Restaurante.Backend.Seguridad.Contrasenas.Ayuda, AutoSize = true, MaximumSize = new Size(650, 0) });
        btnGuardar = DisenoFormulario.Boton(flpP, "Cambiar y volver al acceso", Guardar_Click);
        lblMensaje = DisenoFormulario.Mensaje(flpP);
    }
}
