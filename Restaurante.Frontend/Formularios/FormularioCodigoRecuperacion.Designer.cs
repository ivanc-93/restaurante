namespace Restaurante.Frontend.Formularios;
partial class FormularioCodigoRecuperacion
{
    private TextBox ctContrasena = null!;
    private TextBox ctCodigo = null!;
    private Label lblMensaje = null!;
    private Button btnGenerar = null!;
    private Button btnContinuar = null!;
    private CheckBox chkGuardado = null!;
    private void InitializeComponent()
    {
        var flpP = DisenoFormulario.Preparar(this, "Código de emergencia del Administrador");
        flpP.Controls.Add(new Label { Text = "Guarda este código fuera del equipo, en un gestor seguro o papel protegido. Permite recuperar tu cuenta si olvidas la contraseña. Quien lo posea podría tomar el control de tu cuenta. Generar otro revoca el anterior.", AutoSize = true, MaximumSize = new Size(650, 0) });
        ctContrasena = DisenoFormulario.Campo(flpP, "Contraseña vigente de tu usuario Administrador", true);
        flpP.Controls.Add(new Label { Text = "Aquí se escribe la contraseña con la que inicias sesión. El código de emergencia se genera en el campo de abajo; no sustituye tu contraseña en este paso.", AutoSize = true, MaximumSize = new Size(650, 0) });
        ctCodigo = DisenoFormulario.Campo(flpP, "Código nuevo: se muestra únicamente en esta ventana", true);
        ctCodigo.ReadOnly = true;
        var chkMostrar = new CheckBox
        {
            Text = "Mostrar código de emergencia",
            AutoSize = true,
            Name = "chkMostrar"
        };
        chkMostrar.CheckedChanged += (_, _) => ctCodigo.UseSystemPasswordChar = !chkMostrar.Checked;
        flpP.Controls.Add(chkMostrar);
        chkGuardado = new CheckBox
        {
            Text = "He guardado el código en un lugar seguro fuera del equipo",
            AutoSize = true,
            Name = "chkGuardado"
        };
        flpP.Controls.Add(chkGuardado);
        chkGuardado.CheckedChanged += (_, _) => btnContinuar.Enabled = chkGuardado.Checked && ctCodigo.Text.Length > 0;
        btnGenerar = DisenoFormulario.Boton(flpP, "Generar código", Generar_Click);
        btnContinuar = DisenoFormulario.Boton(flpP, "He guardado el código", Confirmar_Click);
        btnContinuar.Enabled = false;
        DisenoFormulario.Boton(flpP, "Cancelar", (_, _) => Close());
        lblMensaje = DisenoFormulario.Mensaje(flpP);
        AcceptButton = btnGenerar;
    }
}
