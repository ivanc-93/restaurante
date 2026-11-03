using Restaurante.Backend.Servicios;

namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioReportes : Form
{
    private readonly IRepositorioRestaurante _repositorio = null!;
    private bool _listo;
    private bool _cargando;
    public FormularioReportes() => InitializeComponent();
    public FormularioReportes(IRepositorioRestaurante repositorio) : this()
    {
        _repositorio = repositorio;
        Componentes.EstilizarTabla(dgvVentas);
        Componentes.EstilizarTabla(dgvPlatos);
        Componentes.EstilizarTabla(dgvPedidos);
        dtpInicio.Value = RelojGuatemala.Hoy.AddDays(-30);
        dtpFin.Value = RelojGuatemala.Hoy;
        cbAgrupacion.SelectedIndex = 0;
        var tpOperaciones = new TabPage("Cobros, cancelaciones e incidencias")
        {
            Name = "tpOperaciones"
        };
        tpOperaciones.Controls.Add(new DataGridView { Name = "operaciones", Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoGenerateColumns = true });
        tpOperaciones.Controls.Add(new Label { Name = "resumenOperaciones", Dock = DockStyle.Bottom, AutoSize = true, MaximumSize = new Size(950, 0), Padding = new Padding(10) });
        tabPestanas.TabPages.Add(tpOperaciones);
        Pestana_Cambio(this, EventArgs.Empty);
        Shown += async (_, _) =>
        {
            flpFiltros.Enabled = false;
            try
            {
                var opciones = await Task.Run(() => (Mesas: _repositorio.ListarMesas(), Clientes: _repositorio.ListarClientesHistoricos()));
                if (IsDisposed || Disposing)
                    return;
                cbMesa.DataSource = new List<OpcionFiltro>
                {
                    new(null, "Todas las mesas")
                }.Concat(opciones.Mesas.Select(m => new OpcionFiltro(m.Id, $"Mesa {m.Numero}"))).ToList();
                cbCliente.DataSource = new List<OpcionFiltro>
                {
                    new(null, "Todos los clientes"),
                    new(0, "Público general")
                }.Concat(opciones.Clientes.Select(c => new OpcionFiltro(c.Id, c.Nombre))).ToList();
                _listo = true;
                await CargarAsync();
            }
            catch (Exception ex)
            {
                if (!IsDisposed && !Disposing)
                    MessageBox.Show(this, ex.Message, "Filtros de reportes", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (!IsDisposed)
                    flpFiltros.Enabled = true;
            }
        };
    }

    private void Pestana_Cambio(object? sender, EventArgs e)
    {
        cbAgrupacion.Visible = tabPestanas.SelectedIndex == 0;
        cbMesa.Visible = lblMesaLabel.Visible = cbCliente.Visible = lblClienteLabel.Visible = tabPestanas.SelectedIndex == 2;
        lblTotal.Visible = tabPestanas.SelectedIndex == 0;
    }

    private async void Consultar_Click(object? sender, EventArgs e) => await CargarAsync();
    private async Task CargarAsync()
    {
        if (_repositorio is null || !_listo || _cargando)
            return;
        _cargando = true;
        var desde = dtpInicio.Value;
        var hasta = dtpFin.Value;
        var grupo = cbAgrupacion.Text;
        int? mesaId = (cbMesa.SelectedItem as OpcionFiltro)?.Id;
        int? clienteId = (cbCliente.SelectedItem as OpcionFiltro)?.Id;
        flpFiltros.Enabled = false;
        UseWaitCursor = true;
        try
        {
            ReglasRestaurante.Fechas(desde, hasta);
            var datos = await Task.Run(() => (Ventas: _repositorio.ReportarVentasPeriodo(desde, hasta, grupo), Platos: _repositorio.ReportarPlatosMasVendidos(desde, hasta), Pedidos: _repositorio.ReportarPedidos(desde, hasta, mesaId, clienteId), Operaciones: _repositorio.ReportarOperaciones(desde, hasta)));
            if (IsDisposed || Disposing)
                return;
            dgvVentas.DataSource = datos.Ventas.ToList();
            dgvPlatos.DataSource = datos.Platos.ToList();
            dgvPedidos.DataSource = datos.Pedidos.ToList();
            if (dgvPlatos.Columns.Contains("Importe"))
                dgvPlatos.Columns["Importe"].HeaderText = "Consumo bruto (Q), antes de ajustes";
            if (dgvPlatos.Columns.Contains("Total"))
                dgvPlatos.Columns["Total"].HeaderText = "Consumo bruto (Q), antes de ajustes";
            ((DataGridView)tabPestanas.TabPages[3].Controls["operaciones"]!).DataSource = datos.Operaciones.ToList();
            decimal Suma(string concepto, string estado) => datos.Operaciones.Where(x => x.Concepto == concepto && x.Estado == estado).Sum(x => x.Importe);
            tabPestanas.TabPages[3].Controls["resumenOperaciones"]!.Text = $"Cobros confirmados: efectivo {FormatoPresentacion.Moneda(Suma("Pago Efectivo", "Confirmado"))} · tarjeta manual {FormatoPresentacion.Moneda(Suma("Pago Tarjeta", "Confirmado"))}\n" + $"Cancelado: {FormatoPresentacion.Moneda(Suma("Cancelar", "Cancelado"))} · Incidencias generadas: {FormatoPresentacion.Moneda(Suma("Incidencia", "Incidencia"))} · Legados sin pago registrado: {FormatoPresentacion.Moneda(Suma("Comprobante legado", "Pago no registrado"))}\n" + $"Descuentos vigentes: {FormatoPresentacion.Moneda(Suma("Descuento", "Vigente"))} · Cortesías vigentes: {FormatoPresentacion.Moneda(Suma("Cortesía", "Vigente"))}\n" + "No sumar todas las filas: comprobantes, cobros, ajustes e incidencias representan conceptos distintos. Tarjetas rechazadas o inciertas no son cobros confirmados.";
            lblTotal.Text = $"Comprobantes emitidos (no equivale a efectivo cobrado): {FormatoPresentacion.Moneda(datos.Ventas.Sum(x => x.Total))} · {datos.Ventas.Sum(x => x.Facturas)} facturas. Semana: lunes a domingo.";
            FormatoPresentacion.Aplicar(this);
        }
        catch (Exception ex)
        {
            if (!IsDisposed && !Disposing)
                MessageBox.Show(this, ex.Message, "Reportes", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _cargando = false;
            if (!IsDisposed)
            {
                flpFiltros.Enabled = true;
                UseWaitCursor = false;
            }
        }
    }
}
