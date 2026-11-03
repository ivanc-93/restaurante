namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioAutorizar
{
    private TextBox ctUsuario = null!, ctContrasena = null!;
    private Label lblResumen = null!, lblMensaje = null!;
    private Button btnAutorizar = null!, btnCancelar = null!;
    private CheckBox chkMostrarContrasena = null!;
    private void InitializeComponent()
    {
        var flpP = DisenoFormulario.Preparar(this, "Autorización puntual del gerente");
        lblResumen = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(500, 0),
            Name = "lblResumen"
        };
        flpP.Controls.Add(lblResumen);
        ctUsuario = DisenoFormulario.Campo(flpP, "Usuario del gerente o administrador");
        ctContrasena = DisenoFormulario.Campo(flpP, "Contraseña individual", true);
        ctUsuario.MaxLength = 50;
        ctUsuario.TabIndex = 0;
        ctContrasena.TabIndex = 1;
        chkMostrarContrasena = new CheckBox
        {
            Text = "Mostrar contraseña",
            AutoSize = true,
            TabIndex = 2,
            Name = "chkMostrarContrasena"
        };
        chkMostrarContrasena.CheckedChanged += (_, _) => ctContrasena.UseSystemPasswordChar = !chkMostrarContrasena.Checked;
        flpP.Controls.Add(chkMostrarContrasena);
        flpP.Controls.Add(new Label { Text = "Introduce la contraseña vigente de esa cuenta. No es una contraseña nueva.", AutoSize = true, MaximumSize = new Size(650, 0) });
        btnAutorizar = DisenoFormulario.Boton(flpP, "Autorizar esta operación", Autorizar_Click);
        btnAutorizar.TabIndex = 3;
        btnCancelar = DisenoFormulario.Boton(flpP, "Cancelar", Cancelar_Click);
        btnCancelar.TabIndex = 4;
        lblMensaje = DisenoFormulario.Mensaje(flpP);
        AcceptButton = btnAutorizar;
        CancelButton = btnCancelar;
    }
}
