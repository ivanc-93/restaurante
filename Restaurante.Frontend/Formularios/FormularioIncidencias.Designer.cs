namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioIncidencias
{
    private DataGridView dgvRegistros = null!;
    private Button btnResolver = null!;
    private Label lblMensaje = null!;
    private ComboBox cbFiltro = null!;
    private void InitializeComponent()
    {
        var flpP = DisenoFormulario.Preparar(this, "Incidencias de consumo");
        cbFiltro = DisenoFormulario.Opciones(flpP, "Mostrar", "Pendientes", "Todas");
        cbFiltro.SelectedIndexChanged += async (_, _) => await Cargar();
        dgvRegistros = new DataGridView
        {
            Width = 650,
            Height = 280,
            ReadOnly = true,
            AutoGenerateColumns = true,
            AllowUserToAddRows = false,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            Name = "dgvRegistros"
        };
        flpP.Controls.Add(dgvRegistros);
        Componentes.EstilizarTabla(dgvRegistros);
        btnResolver = DisenoFormulario.Boton(flpP, "Resolver incidencia seleccionada", Resolver_Click);
        DisenoFormulario.Boton(flpP, "Actualizar", async (_, _) => await Cargar());
        var btnCerrar = DisenoFormulario.Boton(flpP, "Cerrar", (_, _) => Close());
        CancelButton = btnCerrar;
        lblMensaje = DisenoFormulario.Mensaje(flpP);
    }
}
