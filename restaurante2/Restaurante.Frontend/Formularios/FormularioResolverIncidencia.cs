using Restaurante.Backend.Dominio;
using Restaurante.Backend.Servicios;
using Restaurante.Frontend.Servicios;

namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioResolverIncidencia : Form
{
    private readonly IRepositorioRestaurante repo = null!;
    private readonly CuentaRevision cuenta = null!;
    private bool ocupado;
    public FormularioResolverIncidencia() => InitializeComponent();
    public FormularioResolverIncidencia(IRepositorioRestaurante repo, CuentaRevision cuenta) : this()
    {
        this.repo = repo;
        this.cuenta = cuenta;
        lblResumen.Text = $"Pedido #{cuenta.PedidoId} · Saldo completo: {MonedaGuatemala.Formatear(cuenta.Total)}. No admite pagos parciales.";
        nudEntregado.Value = cuenta.Total;
        FormClosing += (_, e) => e.Cancel = ocupado;
    }

    private async void Resolver_Click(object? sender, EventArgs e)
    {
        if (ocupado)
            return;
        try
        {
            ReglasRestaurante.Texto(ctMotivo.Text, "Motivo y evidencia", 500);
            if (!chkConfirmacion.Checked)
                throw new ArgumentException("Confirma que revisaste los datos y el desenlace real.");
            var id = Guid.NewGuid();
            SolicitudCobro? cobro = null;
            if (cbDesenlace.Text == "Cobro posterior")
            {
                if (!chkPublico.Checked)
                    ReglasRestaurante.Cliente(ctReceptor.Text, null, ctCorreo.Text);
                if (cbMetodo.Text == "Tarjeta")
                    ReglasRestaurante.Texto(ctReferencia.Text, "Referencia del pago confirmado", 100);
                if (cbMetodo.Text == "Efectivo" && nudEntregado.Value < cuenta.Total || cbMetodo.Text == "Tarjeta" && nudEntregado.Value != cuenta.Total)
                    throw new ArgumentException("Revisa el efectivo recibido o el importe exacto de tarjeta.");
                cobro = new(id, cuenta.PedidoId, cuenta.Version, cuenta.Total, chkPublico.Checked, ctReceptor.Text, chkPublico.Checked ? null : ctIdentificacion.Text, chkPublico.Checked ? null : ctDireccion.Text, chkPublico.Checked ? null : ctCorreo.Text, cbMetodo.Text, nudEntregado.Value, cbMetodo.Text == "Tarjeta" ? "Confirmado" : "", ctReferencia.Text, "Cobro posterior de incidencia", true, true);
            }

            var s = new SolicitudResolucionIncidencia(id, cuenta.PedidoId, cbDesenlace.Text, ctMotivo.Text, cobro);
            if (MessageBox.Show(this, $"Resolver como {s.Desenlace} por {MonedaGuatemala.Formatear(cuenta.Total)}. No cambiará el estado actual de la mesa. ¿Confirmar?", "Resolver incidencia", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            ocupado = true;
            btnResolver.Enabled = false;
            var resultado = await DiarioOperaciones.Ejecutar(repo, "Resolver", id, s, () => repo.ResolverIncidencia(s));
            MessageBox.Show(this, resultado, "Resolución registrada");
            ocupado = false;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            lblMensaje.Text = ex.Message + " Si hay resultado incierto, recupera la misma solicitud desde Inicio.";
        }
        finally
        {
            ocupado = false;
            if (!IsDisposed)
                btnResolver.Enabled = DiarioOperaciones.Listar(repo).Count == 0;
        }
    }
}
