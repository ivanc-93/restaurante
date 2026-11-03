using Restaurante.Backend.Dominio;
using Restaurante.Frontend.Servicios;

namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioReservaDetalle : Form
{
    private readonly IRepositorioRestaurante repo = null!;
    private readonly Mesa mesa = null!;
    private readonly ReservaResumen? reserva;
    private readonly IDialogosUsuario dialogos = new DialogosUsuario();
    private bool ocupado;
    public FormularioReservaDetalle() => InitializeComponent();
    public FormularioReservaDetalle(IRepositorioRestaurante repo, Mesa mesa, ReservaResumen? reserva, IDialogosUsuario? dialogos = null) : this()
    {
        this.repo = repo;
        this.mesa = mesa;
        this.reserva = reserva;
        this.dialogos = dialogos ?? new DialogosUsuario();
        lblMesa.Text = $"Mesa {mesa.Numero} · Reservada";
        ctTitular.Text = reserva?.Nombre ?? "Reserva anterior sin titular registrado.";
        lblMensaje.Text = reserva is null ? "Esta reserva anterior requiere revisión del responsable. No se inventará un titular ni se liberará automáticamente." : $"Reserva #{reserva.Id} · {reserva.Fecha:dd/MM/yyyy HH:mm}. El nombre no se copia automáticamente al comprobante.";
        var habilitada = reserva is not null && DiarioOperaciones.Listar(repo).Count == 0;
        btnAtenderReserva.Enabled = btnCancelarReserva.Enabled = ctMotivo.Enabled = habilitada;
        FormClosing += (_, e) => e.Cancel = ocupado;
    }

    private async void Atender_Click(object? sender, EventArgs e)
    {
        if (ocupado || reserva is null)
            return;
        ocupado = true;
        try
        {
            using var detalle = new FormularioDetalleMesa(repo, mesa, dialogos, reserva);
            detalle.ShowDialog(this);
            if (await Task.Run(() => repo.ConsultarReservaMesa(mesa.Id))is null)
            {
                ocupado = false;
                DialogResult = DialogResult.OK;
                Close();
            }
        }
        catch (Exception ex)
        {
            lblMensaje.Text = MensajesReserva.Error(ex);
        }
        finally
        {
            ocupado = false;
        }
    }

    private async void Cancelar_Click(object? sender, EventArgs e)
    {
        if (ocupado || reserva is null)
            return;
        if (string.IsNullOrWhiteSpace(ctMotivo.Text))
        {
            lblMensaje.Text = "Escribe el motivo de la cancelación.";
            ctMotivo.Focus();
            return;
        }

        var solicitud = new SolicitudCancelacionReserva(Guid.NewGuid(), reserva.Id, mesa.Id, reserva.Version, ctMotivo.Text.Trim());
        var resumen = $"Cancelar reserva #{reserva.Id} · Mesa {mesa.Numero}\nTitular: {reserva.Nombre}\nMotivo: {solicitud.Motivo}\nNo cancela pedidos ni consumo.";
        if (dialogos.Mostrar(this, "¿Cancelar esta reserva y dejar la mesa disponible? El historial se conserva.", "Confirmar cancelación", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;
        if (!FormularioAutorizar.Solicitar(this, repo, solicitud.OperacionId, "CancelarReserva", solicitud, resumen))
            return;
        ocupado = true;
        btnAtenderReserva.Enabled = btnCancelarReserva.Enabled = btnCerrar.Enabled = ctMotivo.Enabled = false;
        try
        {
            await DiarioOperaciones.Ejecutar(repo, "CancelarReserva", solicitud.OperacionId, solicitud, () => repo.CancelarReserva(solicitud));
            ocupado = false;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            lblMensaje.Text = MensajesReserva.Error(ex) + " Revisa las operaciones pendientes antes de repetir.";
        }
        finally
        {
            ocupado = false;
            if (!IsDisposed)
                btnCerrar.Enabled = true;
        }
    }
}
