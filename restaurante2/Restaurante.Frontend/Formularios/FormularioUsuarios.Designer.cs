namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioUsuarios
{
    private DataGridView dgvRegistros = null!;
    private TextBox ctNombre = null!, ctUsuario = null!, ctContrasena = null!, ctConfirmarClave = null!;
    private ComboBox cbRol = null!;
    private CheckBox chkActivo = null!;
    private Label lblMensaje = null!;
    private Button btnGuardar = null!;
    private void InitializeComponent()
    {
        var flpP = DisenoFormulario.Preparar(this, "Usuarios y roles");
        dgvRegistros = new DataGridView
        {
            Width = 570,
            Height = 180,
            ReadOnly = true,
            AutoGenerateColumns = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AllowUserToAddRows = false,
            Name = "dgvRegistros"
        };
        flpP.Controls.Add(dgvRegistros);
        DisenoFormulario.Boton(flpP, "Editar seleccionado", Editar_Click);
        DisenoFormulario.Boton(flpP, "Nuevo usuario", async (_, _) =>
        {
            Limpiar();
            await Recargar();
        });
        DisenoFormulario.Boton(flpP, "Ver auditoría", Auditoria_Click);
        DisenoFormulario.Boton(flpP, "Mi código de emergencia", (_, _) =>
        {
            using var f = new FormularioCodigoRecuperacion(repo);
            f.ShowDialog(this);
        });
        ctNombre = DisenoFormulario.Campo(flpP, "Nombre");
        ctUsuario = DisenoFormulario.Campo(flpP, "Usuario individual");
        cbRol = DisenoFormulario.Opciones(flpP, "Rol", "Mesero", "Gerente", "Administrador");
        chkActivo = new CheckBox
        {
            Text = "Usuario activo",
            Checked = true,
            AutoSize = true,
            Name = "chkActivo"
        };
        flpP.Controls.Add(chkActivo);
        ctContrasena = DisenoFormulario.Campo(flpP, "Contraseña temporal de 12 caracteres o más; vacío conserva la existente", true);
        ctConfirmarClave = DisenoFormulario.Campo(flpP, "Confirmar contraseña temporal si se cambia", true);
        flpP.Controls.Add(new Label { Text = Restaurante.Backend.Servicios.Contrasenas.Ayuda, AutoSize = true, MaximumSize = new Size(650, 0) });
        btnGuardar = DisenoFormulario.Boton(flpP, "Guardar usuario", Guardar_Click);
        lblMensaje = DisenoFormulario.Mensaje(flpP);
    }
}
