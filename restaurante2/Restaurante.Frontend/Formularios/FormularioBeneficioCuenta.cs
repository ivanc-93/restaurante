using Restaurante.Backend.Dominio;

namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioBeneficioCuenta : Form
{
    private readonly IRepositorioRestaurante repo = null!;
    private readonly CuentaRevision pedido = null!;
    private readonly int? cuentaId;
    private bool ocupado;
    public FormularioBeneficioCuenta() => InitializeComponent();
    public FormularioBeneficioCuenta(IRepositorioRestaurante repo, CuentaRevision pedido, CuentaOperativa? cuenta) : this()
    {
        this.repo = repo;
        this.pedido = pedido;
        cuentaId = cuenta?.Id;
        lblResumen.Text = $"Pedido #{pedido.PedidoId} · {(cuenta is null ? "Toda la mesa (todas las cuentas deben estar sin pagos)" : cuenta.Etiqueta)}\nOriginal del alcance: {MonedaGuatemala.Formatear(cuenta?.Original ?? pedido.ImporteOriginal)}\nReemplaza el ajuste del alcance seleccionado. No modifica cuentas pagadas ni precios históricos.";
        FormClosing += (_, e) => e.Cancel = ocupado;
        cbTipo.SelectedIndexChanged += TipoBeneficio_Cambio;
    }

    private async void TipoBeneficio_Cambio(object? sender, EventArgs e)
    {
        if (ocupado)
            return;
        Restaurante.Frontend.Infraestructura.ContextoInterfaz.AsegurarActual();
        ocupado = true;
        cbTipo.Enabled = btnAplicar.Enabled = false;
        bool valido = true;
        try
        {
            nudPorcentaje.Enabled = cbTipo.Text == "Descuento";
            nudPorcentaje.Value = cbTipo.Text switch
            {
                "Cortesía" => 100,
                "Restablecer" => 0,
                _ => Math.Clamp(nudPorcentaje.Value, 5, 50)};
            if (cbTipo.Text == "Frecuente")
            {
                var regla = await Task.Run(repo.ConsultarReglaFrecuente);
                if (!regla.Activa)
                    throw new InvalidOperationException("No hay una promoción frecuente activa. Elige otro tipo de beneficio o consulta a gerencia.");
                nudPorcentaje.Value = regla.Porcentaje;
            }

            lblMensaje.Text = cbTipo.Text == "Cortesía" ? "100 % de este alcance, con autorización. No simula efectivo." : cbTipo.Text == "Restablecer" ? "Retira el descuento de este alcance con motivo y autorización." : "Descuento de 5 a 50 %; no acumulable. Se comprobará elegibilidad al aplicar.";
        }
        catch (Exception ex)
        {
            valido = false;
            lblMensaje.Text = MensajesReserva.Error(ex);
        }
        finally
        {
            ocupado = false;
            cbTipo.Enabled = true;
            btnAplicar.Enabled = valido;
        }
    }

    private async void Aplicar_Click(object? sender, EventArgs e)
    {
        if (ocupado)
            return;
        try
        {
            ReglasRestaurante.Texto(ctMotivo.Text, "Motivo", 400);
            var pct = cbTipo.Text == "Cortesía" ? 100 : cbTipo.Text == "Restablecer" ? 0 : nudPorcentaje.Value;
            if (cbTipo.Text is "Descuento" or "Frecuente" && (pct < 5 || pct > 50))
                throw new ArgumentException("El descuento debe estar entre 5 y 50 %. Usa Cortesía para el 100 %.");
            var s = new SolicitudBeneficioCuenta(Guid.NewGuid(), pedido.PedidoId, pedido.Version, pedido.Total, cuentaId, cbTipo.Text, pct, ctMotivo.Text.Trim());
            if (!FormularioAutorizar.Solicitar(this, repo, s.OperacionId, "BeneficioCuenta", s, lblResumen.Text + $"\n{s.Tipo}: {s.Porcentaje}%\nMotivo: {s.Motivo}"))
                return;
            ocupado = true;
            btnAplicar.Enabled = false;
            await DiarioOperaciones.Ejecutar(repo, "BeneficioCuenta", s.OperacionId, s, () => repo.AplicarBeneficioCuenta(s));
            ocupado = false;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            lblMensaje.Text = MensajesReserva.Error(ex);
            btnAplicar.Enabled = DiarioOperaciones.Listar(repo).Count == 0;
        }
        finally
        {
            ocupado = false;
        }
    }
}
