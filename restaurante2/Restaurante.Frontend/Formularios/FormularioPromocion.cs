using Restaurante.Backend.Dominio;

namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioPromocion : Form
{
    private readonly IRepositorioRestaurante repo = null!;
    private bool ocupado;
    public FormularioPromocion() => InitializeComponent();
    public FormularioPromocion(IRepositorioRestaurante repo) : this()
    {
        this.repo = repo;
        Shown += async (_, _) =>
        {
            try
            {
                var r = await Task.Run(repo.ConsultarReglaFrecuente);
                chkActiva.Checked = r.Activa;
                nudVisitas.Value = r.VisitasMinimas;
                nudPorcentaje.Value = r.Porcentaje;
                dtDesde.Value = r.Desde;
                dtHasta.Value = r.Hasta;
            }
            catch (Exception ex)
            {
                lblMensaje.Text = MensajesReserva.Error(ex);
                btnGuardar.Enabled = false;
            }
        };
        FormClosing += (_, e) => e.Cancel = ocupado;
    }

    private async void Guardar_Click(object? sender, EventArgs e)
    {
        if (ocupado)
            return;
        ocupado = true;
        btnGuardar.Enabled = false;
        try
        {
            var s = new SolicitudReglaFrecuente(Guid.NewGuid(), new(chkActiva.Checked, (int)nudVisitas.Value, nudPorcentaje.Value, dtDesde.Value.Date, dtHasta.Value.Date));
            await DiarioOperaciones.Ejecutar(repo, "ReglaFrecuente", s.OperacionId, s, () => repo.GuardarReglaFrecuente(s));
            ocupado = false;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            lblMensaje.Text = MensajesReserva.Error(ex);
            btnGuardar.Enabled = DiarioOperaciones.Listar(repo).Count == 0;
        }
        finally
        {
            ocupado = false;
        }
    }
}
