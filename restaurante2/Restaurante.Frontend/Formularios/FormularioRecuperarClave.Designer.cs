namespace Restaurante.Frontend.Formularios;
partial class FormularioRecuperarClave
{
    private ComboBox cbMetodo = null!;
    private TextBox ctUsuario = null!, ctCodigo = null!, ctAdministrador = null!, ctClaveAdministrador = null!, ctNueva = null!, ctConfirmacion = null!;
    private Label lblMensaje = null!;
    private Button btnRecuperar = null!, btnConsultar = null!, btnCancelar = null!;
    private CheckBox chkMostrar = null!;
    private Control[] camposAdministrador = [], camposCodigo = [], camposNueva = [];
    private void InitializeComponent()
    {
        var flpP = DisenoFormulario.Preparar(this, "Recuperar acceso a Sabores");
        flpP.Controls.Add(new Label { Text = "Establece una contraseña nueva. La contraseña anterior no puede mostrarse. No se abrirá la operación diaria.", AutoSize = true, MaximumSize = new Size(650, 0) });
        cbMetodo = DisenoFormulario.Opciones(flpP, "¿Cómo verificarás tu identidad?", "Con ayuda de otro Administrador", "Con mi código de emergencia", "No tengo administrador ni código");
        ctUsuario = DisenoFormulario.Campo(flpP, "Usuario que necesita recuperar acceso");
        ctUsuario.MaxLength = 50;
        var comienzo = flpP.Controls.Count;
        ctAdministrador = DisenoFormulario.Campo(flpP, "Usuario del otro Administrador");
        ctAdministrador.MaxLength = 50;
        ctClaveAdministrador = DisenoFormulario.Campo(flpP, "Contraseña vigente del otro Administrador", true);
        camposAdministrador = flpP.Controls.Cast<Control>().Skip(comienzo).ToArray();
        comienzo = flpP.Controls.Count;
        ctCodigo = DisenoFormulario.Campo(flpP, "Código de emergencia guardado previamente", true);
        ctCodigo.MaxLength = 100;
        camposCodigo = flpP.Controls.Cast<Control>().Skip(comienzo).ToArray();
        comienzo = flpP.Controls.Count;
        ctNueva = DisenoFormulario.Campo(flpP, "Contraseña nueva (12 a 128 caracteres)", true);
        ctConfirmacion = DisenoFormulario.Campo(flpP, "Confirmar contraseña nueva", true);
        flpP.Controls.Add(new Label { Text = Restaurante.Backend.Servicios.Contrasenas.Ayuda, AutoSize = true, MaximumSize = new Size(650, 0) });
        chkMostrar = new CheckBox
        {
            Text = "Mostrar contraseñas y código",
            AutoSize = true,
            Name = "chkMostrar"
        };
        chkMostrar.CheckedChanged += (_, _) =>
        {
            foreach (var campo in new[]
            {
                ctClaveAdministrador,
                ctCodigo,
                ctNueva,
                ctConfirmacion
            }

            )
                campo.UseSystemPasswordChar = !chkMostrar.Checked;
        };
        flpP.Controls.Add(chkMostrar);
        camposNueva = flpP.Controls.Cast<Control>().Skip(comienzo).ToArray();
        btnRecuperar = DisenoFormulario.Boton(flpP, "Restablecer contraseña", Recuperar_Click);
        btnConsultar = DisenoFormulario.Boton(flpP, "Comprobar resultado", Consultar_Click);
        btnConsultar.Enabled = false;
        btnCancelar = DisenoFormulario.Boton(flpP, "Volver al inicio", (_, _) => Close());
        lblMensaje = DisenoFormulario.Mensaje(flpP);
        cbMetodo.SelectedIndexChanged += (_, _) => ActualizarMetodo();
        AcceptButton = btnRecuperar;
        var tab = 0;
        foreach (Control control in flpP.Controls)
            if (control.TabStop)
                control.TabIndex = tab++;
        ActualizarMetodo();
    }

    private void ActualizarMetodo()
    {
        var ayuda = cbMetodo.SelectedIndex == 0;
        var emergencia = cbMetodo.SelectedIndex == 1;
        foreach (var c in camposAdministrador)
            c.Visible = ayuda;
        foreach (var c in camposCodigo)
            c.Visible = emergencia;
        foreach (var c in camposNueva)
            c.Visible = ayuda || emergencia;
        ctAdministrador.Enabled = ctClaveAdministrador.Enabled = ayuda;
        ctCodigo.Enabled = emergencia;
        btnRecuperar.Enabled = ayuda || emergencia;
        ctNueva.Enabled = ctConfirmacion.Enabled = ctUsuario.Enabled = ayuda || emergencia;
        ctClaveAdministrador.Clear();
        ctCodigo.Clear();
        lblMensaje.Text = ayuda ? "Otro Administrador debe comprobar tu identidad antes de autorizar." : emergencia ? "Introduce el código que guardaste fuera del equipo. Cinco fallos pausan su verificación durante cinco minutos." : "Solicita mantenimiento al responsable autorizado de SQL Server. Necesita identificar la base, respaldarla y comprobar su restauración antes de restablecer únicamente tu cuenta. Conectarse a SQL no recupera por sí solo tu usuario. No se ejecutará mantenimiento desde esta pantalla.";
    }
}
