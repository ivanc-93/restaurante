using Restaurante.Backend.Dominio;

namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioIncidencias : Form
{
    private readonly IRepositorioRestaurante repo = null!;
    private bool ocupado;
    public FormularioIncidencias() => InitializeComponent();
    public FormularioIncidencias(IRepositorioRestaurante repo) : this()
    {
        this.repo = repo;
        Shown += async (_, _) => await Cargar();
        FormClosing += (_, e) => e.Cancel = ocupado;
        btnResolver.Visible = repo.UsuarioActual?.Rol is "Administrador" or "Gerente";
    }

    private async Task Cargar()
    {
        if (ocupado || repo is null)
            return;
        ocupado = true;
        btnResolver.Enabled = false;
        cbFiltro.Enabled = false;
        try
        {
            var todas = cbFiltro.SelectedIndex == 1;
            var datos = await Task.Run(repo.ListarIncidencias);
            dgvRegistros.DataSource = datos.Where(x => todas || x.Estado == "Pendiente").ToList();
            FormatoPresentacion.Aplicar(this);
            dgvRegistros.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            var orden = new[]
            {
                "PedidoId",
                "Estado",
                "Desenlace",
                "Importe",
                "Mesa",
                "Cliente",
                "Motivo"
            };
            for (var i = 0; i < orden.Length; i++)
                if (dgvRegistros.Columns.Contains(orden[i]))
                    dgvRegistros.Columns[orden[i]].DisplayIndex = i;
            lblMensaje.Text = "El importe conserva el saldo de origen. Revisa Estado y Desenlace; una pérdida o condonación no equivale a dinero cobrado.";
        }
        catch (Exception ex)
        {
            lblMensaje.Text = ex.Message;
        }
        finally
        {
            ocupado = false;
            btnResolver.Enabled = true;
            cbFiltro.Enabled = true;
        }
    }

    private async void Resolver_Click(object? sender, EventArgs e)
    {
        if (ocupado)
            return;
        if (dgvRegistros.CurrentRow?.DataBoundItem is not IncidenciaResumen { Estado: "Pendiente" } incidencia)
        {
            lblMensaje.Text = "Selecciona una incidencia pendiente. Las resueltas conservan su historial y no admiten otro desenlace.";
            return;
        }

        ocupado = true;
        try
        {
            var cuenta = await Task.Run(() => repo.RevisarCuenta(incidencia.PedidoId));
            using var f = new FormularioResolverIncidencia(repo, cuenta);
            f.ShowDialog(this);
        }
        catch (Exception ex)
        {
            lblMensaje.Text = ex.Message;
        }
        finally
        {
            ocupado = false;
            await Cargar();
        }
    }
}
