using Restaurante.Backend.Dominio;
using Restaurante.Frontend.Servicios;

namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioAjuste : Form
{
    private readonly IRepositorioRestaurante repo = null!;
    private readonly CuentaRevision cuenta = null!;
    private bool ocupado;
    private bool porcentual;
    public FormularioAjuste() => InitializeComponent();
    public FormularioAjuste(IRepositorioRestaurante repo, CuentaRevision cuenta) : this()
    {
        this.repo = repo;
        this.cuenta = cuenta;
        lblResumen.Text = $"Mesa {cuenta.NumeroMesa} · Pedido {cuenta.PedidoId}\nOriginal {MonedaGuatemala.Formatear(cuenta.ImporteOriginal)} · total actual {MonedaGuatemala.Formatear(cuenta.Total)}\nAgregar otro producto invalida el ajuste y exige nueva revisión.";
        porcentual = repo.CobranzaParcialDisponible();
        if (porcentual)
        {
            lblImporte.Text = "Porcentaje sobre el consumo original (5 a 50 %)";
            nudDescuento.Minimum = 5;
            nudDescuento.Maximum = 50;
            nudDescuento.Value = 5;
            cbTipo.Items.Add("Frecuente");
            lblResumen.Text += "\nNo acumulable: reemplaza el ajuste vigente. No permitido después de dividir o abonar.";
            if (cuenta.ClienteId.HasValue)
            {
                var f = repo.ConsultarFrecuencia(cuenta.ClienteId.Value);
                lblResumen.Text += $"\nCliente: {f.Visitas} visitas completadas. Promoción {(f.Elegible ? "disponible" : "no disponible")}.";
                if (f.Elegible)
                    nudDescuento.Value = f.Porcentaje;
            }
        }

        if (porcentual)
            cbTipo.SelectedIndexChanged += (_, _) =>
            {
                nudDescuento.Enabled = cbTipo.Text == "Descuento";
                lblImporte.Text = cbTipo.Text == "Cortesía" ? "Cortesía de 100 % sobre todo el consumo original" : "Porcentaje sobre consumo original (5 a 50 %)";
                if (cbTipo.Text == "Frecuente")
                    nudDescuento.Value = repo.ConsultarReglaFrecuente().Porcentaje;
            };
        FormClosing += (_, e) => e.Cancel = ocupado;
    }

    private async void Aplicar_Click(object? sender, EventArgs e)
    {
        if (ocupado)
            return;
        try
        {
            ReglasRestaurante.Texto(ctMotivo.Text, "Motivo", 500);
            if (porcentual)
            {
                var b = new SolicitudBeneficio(Guid.NewGuid(), cuenta.PedidoId, cuenta.Version, cuenta.Total, cbTipo.Text, cbTipo.Text == "Cortesía" ? 100 : nudDescuento.Value, ctMotivo.Text);
                if (!FormularioAutorizar.Solicitar(this, repo, b.OperacionId, "Beneficio", b, $"Pedido #{cuenta.PedidoId} · {b.Tipo} {b.Porcentaje}%\nMotivo: {b.Motivo}"))
                    return;
                ocupado = true;
                btnAplicar.Enabled = false;
                await DiarioOperaciones.Ejecutar(repo, "Beneficio", b.OperacionId, b, () => repo.AplicarBeneficio(b));
                ocupado = false;
                DialogResult = DialogResult.OK;
                return;
            }

            var s = new SolicitudAjuste(Guid.NewGuid(), cuenta.PedidoId, cuenta.Version, cuenta.Total, cbTipo.Text, cbTipo.Text == "Cortesía" ? cuenta.ImporteOriginal : nudDescuento.Value, ctMotivo.Text);
            ValidacionAjuste.Comprobar(s.Tipo, s.Descuento, cuenta.ImporteOriginal);
            if (!FormularioAutorizar.Solicitar(this, repo, s.OperacionId, "Ajuste", s, lblResumen.Text + $"\n{s.Tipo}: {MonedaGuatemala.Formatear(s.Descuento)}\nMotivo: {s.Motivo}"))
                return;
            ocupado = true;
            btnAplicar.Enabled = false;
            await DiarioOperaciones.Ejecutar(repo, "Ajuste", s.OperacionId, s, () => repo.AplicarAjuste(s));
            ocupado = false;
            DialogResult = DialogResult.OK;
        }
        catch (Exception ex)
        {
            lblMensaje.Text = ex.Message + " Si hay una solicitud pendiente, recupérela desde Inicio antes de repetir.";
            btnAplicar.Enabled = DiarioOperaciones.Listar(repo).Count == 0;
        }
        finally
        {
            ocupado = false;
            if (DialogResult == DialogResult.OK)
                Close();
        }
    }
}
