namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioPrimerAdministrador
{
    private TextBox ctToken = null!, ctNombre = null!, ctUsuario = null!, ctContrasena = null!, ctRepetir = null!;
    private Label lblMensaje = null!, lblEstadoInicial = null!, lblEtiquetaToken = null!;
    private Button btnGuardar = null!, btnPrepararWindows = null!;
    private void InitializeComponent()
    {
        var flpP = DisenoFormulario.Preparar(this, "Crear primer administrador");
        lblEstadoInicial = new Label
        {
            Text = "Si es tu primera vez, prepara la instalación con tu cuenta Windows autorizada. Después elige tu usuario y contraseña de Sabores. No necesitas buscar un token.",
            AutoSize = true,
            MaximumSize = new Size(650, 0),
            Name = "lblEstadoInicial"
        };
        flpP.Controls.Add(lblEstadoInicial);
        btnPrepararWindows = DisenoFormulario.Boton(flpP, "Preparar instalación con Windows", PrepararWindows_Click);
        btnPrepararWindows.Enabled = false;
        lblEtiquetaToken = new Label
        {
            Text = "Token entregado por un responsable (opcional si preparas con Windows)",
            AutoSize = true,
            MaximumSize = new Size(650, 0),
            Name = "lblEtiquetaToken"
        };
        flpP.Controls.Add(lblEtiquetaToken);
        ctToken = new TextBox
        {
            UseSystemPasswordChar = true,
            MaxLength = 128,
            Width = 490,
            Name = "ctToken"
        };
        flpP.Controls.Add(ctToken);
        ctNombre = DisenoFormulario.Campo(flpP, "Nombre");
        ctUsuario = DisenoFormulario.Campo(flpP, "Usuario");
        ctContrasena = DisenoFormulario.Campo(flpP, "Contraseña de al menos 12 caracteres", true);
        ctRepetir = DisenoFormulario.Campo(flpP, "Confirmar contraseña", true);
        flpP.Controls.Add(new Label { Text = Restaurante.Backend.Servicios.Contrasenas.Ayuda, AutoSize = true, MaximumSize = new Size(650, 0) });
        btnGuardar = DisenoFormulario.Boton(flpP, "Completar instalación de la base", Guardar_Click);
        lblMensaje = DisenoFormulario.Mensaje(flpP);
    }
}
