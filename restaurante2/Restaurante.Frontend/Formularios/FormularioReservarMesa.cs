using Restaurante.Backend.Dominio;

namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioReservarMesa : Form
{
    private readonly IRepositorioRestaurante repo = null!;
    private readonly Mesa mesa = null!;
    private SolicitudReserva? solicitud;
    private bool ocupado;
    public ReservaResumen? Resultado { get; private set; }

    public FormularioReservarMesa() => InitializeComponent();
    public FormularioReservarMesa(IRepositorioRestaurante repo, Mesa mesa) : this()
    {
        this.repo = repo;
        this.mesa = mesa;
        lblMesa.Text = $"Mesa {mesa.Numero} · capacidad {mesa.Capacidad}";
        FormClosing += (_, e) => e.Cancel = ocupado;
    }

    private async void Reservar_Click(object? sender, EventArgs e)
    {
        if (ocupado)
            return;
        if (solicitud is null)
        {
            if (DiarioOperaciones.Listar(repo).Count > 0)
            {
                lblMensaje.Text = "Recupera las operaciones pendientes desde Inicio antes de registrar otra reserva.";
                return;
            }

            if (!ValidacionCredenciales.Campo(ctNombreReserva, lblMensaje, "el nombre de la persona que reserva"))
                return;
            if (!chkConfirmar.Checked)
            {
                lblMensaje.Text = "Confirma que deseas apartar esta mesa para la persona indicada.";
                return;
            }

            solicitud = new(Guid.NewGuid(), mesa.Id, ctNombreReserva.Text.Trim());
        }

        ocupado = true;
        btnReservarMesa.Enabled = btnCancelar.Enabled = ctNombreReserva.Enabled = chkConfirmar.Enabled = false;
        try
        {
            Resultado = await DiarioOperaciones.Ejecutar(repo, "Reservar", solicitud.OperacionId, solicitud, () => repo.ReservarMesa(solicitud));
            ocupado = false;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            lblMensaje.Text = MensajesReserva.Error(ex) + " Si hay una solicitud pendiente, revísala desde Inicio con su mismo identificador.";
        }
        finally
        {
            ocupado = false;
            if (!IsDisposed)
            {
                btnCancelar.Enabled = true;
                btnReservarMesa.Enabled = false;
            }
        }
    }
}
