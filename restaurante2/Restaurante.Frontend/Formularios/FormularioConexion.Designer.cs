namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioConexion
{
    private TextBox ctServidor = null!, ctBaseDatos = null!, ctSqlUsuario = null!, ctContrasenaSql = null!, ctAdmin = null!, ctAdminClave = null!;
    private ComboBox cbAutenticacion = null!;
    private NumericUpDown nudPuerto = null!;
    private CheckBox chkCertificado = null!;
    private Label lblMensaje = null!;
    private Button btnProbar = null!, btnGuardar = null!;
    private void InitializeComponent()
    {
        var flpP = DisenoFormulario.Preparar(this, "Preparar conexión del equipo");
        flpP.Controls.Add(new Label { Text = "Conecte una base preparada. Este asistente no crea ni migra bases y no permite vender.", AutoSize = true, MaximumSize = new Size(500, 0) });
        ctServidor = DisenoFormulario.Campo(flpP, "Servidor SQL");
        ctServidor.Text = "localhost";
        ctBaseDatos = DisenoFormulario.Campo(flpP, "Base de datos");
        ctBaseDatos.Text = "RestauranteDB";
        cbAutenticacion = DisenoFormulario.Opciones(flpP, "Autenticación de SQL", "Windows", "SQL Server");
        nudPuerto = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 65535,
            Value = 0,
            Width = 180,
            Name = "nudPuerto"
        };
        flpP.Controls.Add(new Label { Text = "Puerto TCP opcional · 0 usa la conexión automática del servidor", AutoSize = true });
        flpP.Controls.Add(nudPuerto);
        ctSqlUsuario = DisenoFormulario.Campo(flpP, "Usuario SQL solo para autenticación SQL");
        ctContrasenaSql = DisenoFormulario.Campo(flpP, "Contraseña SQL", true);
        chkCertificado = new CheckBox
        {
            Text = "Confiar en certificado SQL del entorno local",
            Checked = true,
            AutoSize = true,
            Name = "chkCertificado"
        };
        flpP.Controls.Add(chkCertificado);
        ctAdmin = DisenoFormulario.Campo(flpP, "Administrador de Sabores de esta base");
        ctAdminClave = DisenoFormulario.Campo(flpP, "Contraseña de Sabores para guardar el destino", true);
        btnProbar = DisenoFormulario.Boton(flpP, "Probar conexión y esquema", Probar_Click);
        btnGuardar = DisenoFormulario.Boton(flpP, "Guardar conexión validada", Guardar_Click);
        lblMensaje = DisenoFormulario.Mensaje(flpP);
        cbAutenticacion.SelectedIndexChanged += (_, _) => ActualizarCampos();
        ctServidor.TextChanged += (_, _) => ActualizarCampos();
        ActualizarCampos();
    }

    private void ActualizarCampos()
    {
        ctSqlUsuario.Enabled = ctContrasenaSql.Enabled = cbAutenticacion.SelectedIndex == 1;
        nudPuerto.Enabled = !ctServidor.Text.Trim().StartsWith("(localdb)\\", StringComparison.OrdinalIgnoreCase);
    }
}
