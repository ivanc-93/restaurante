using Restaurante.Backend.Dominio;

namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioCobranza : Form
{
    private readonly IRepositorioRestaurante repo = null!;
    private readonly int pedido;
    private ResumenCobranza estado = null!;
    private bool ocupado;
    private readonly int? cuentaInicial;
    private bool primeraCarga = true;
    private IReadOnlyList<CuentaOperativa> cuentasOperativas = [];
    public bool Finalizado { get; private set; }

    public FormularioCobranza() => InitializeComponent();
    public FormularioCobranza(IRepositorioRestaurante repo, int pedido, int? cuentaInicial = null) : this()
    {
        this.repo = repo;
        this.pedido = pedido;
        this.cuentaInicial = cuentaInicial;
        Shown += async (_, _) => await Ejecutar(Cargar);
        FormClosing += (_, e) => e.Cancel = ocupado;
    }

    private async Task Cargar()
    {
        if (InvokeRequired)
        {
            await Restaurante.Frontend.Infraestructura.ContextoInterfaz.Invocar(this, Cargar);
            return;
        }

        Restaurante.Frontend.Infraestructura.ContextoInterfaz.AsegurarActual();
        estado = await Task.Run(() => repo.ConsultarCobranza(pedido));
        var c = estado.Pedido;
        lblResumen.Text = $"Mesa {c.NumeroMesa} · Pedido #{pedido} · SALDO {M(estado.Saldo)}\nConsumo {M(c.ImporteOriginal)} − ajustes {M(c.Descuento)} = total {M(c.Total)} · pagado {M(estado.Pagado)}";
        dgvProductos.DataSource = c.Detalles.ToList();
        dgvAbonos.DataSource = estado.Abonos.ToList();
        cbCuenta.DataSource = estado.Cuentas.Count == 0 ? new List<CuentaSeparada>
        {
            new(0, "Cuenta general", c.ImporteOriginal, c.Descuento, c.Total, estado.Pagado, estado.Saldo)
        }

        : estado.Cuentas.ToList();
        cbCuenta.DisplayMember = "Nombre";
        cuentasOperativas = await Task.Run(() => repo.CuentasOperativasDisponibles() ? repo.ConsultarCuentasPedido(pedido) : Array.Empty<CuentaOperativa>());
        if (primeraCarga && cuentaInicial.HasValue)
        {
            cbCuenta.SelectedItem = estado.Cuentas.SingleOrDefault(x => x.Id == cuentaInicial.Value) ?? throw new InvalidOperationException("La cuenta seleccionada ya no está activa. Vuelve a la mesa para actualizar.");
        }

        primeraCarga = false;
        var incierto = !estado.Cuentas.Any(x => x.Operativa) && estado.Abonos.Any(a => a.Estado == "Incierto");
        btnDividir.Enabled = !estado.Congelada && c.Total > 0 && c.Estado is "Abierto" or "En preparación";
        btnConfirmar.Enabled = estado.Saldo > 0 && !incierto;
        btnCortesia.Visible = c.Total == 0 && c.Estado is "Abierto" or "En preparación";
        btnConciliarConfirmado.Visible = btnConciliarRechazado.Visible = ctMotivo.Visible = lblMotivo.Visible = incierto;
        flpPago.Enabled = !incierto && (estado.Saldo > 0 || c.Total == 0 && c.Estado is "Abierto" or "En preparación");
        lblMensaje.Text = incierto ? "Tarjeta incierta: no repetir el pago. Un gerente debe confirmar o rechazar con evidencia." : estado.Saldo == 0 ? "Cuenta liquidada. Consulta los recibos y el resumen final." : estado.Congelada ? "Cuenta protegida: no admite cambios de consumo ni ajustes después de dividir o abonar." : "Aplica descuentos antes de dividir o cobrar. Puedes registrar varios abonos, en efectivo o tarjeta.";
        btnConfirmar.Visible = c.Total > 0;
        if (c.Estado == "Facturado")
        {
            Finalizado = true;
            btnConfirmar.Enabled = btnDividir.Enabled = false;
        }

        CambiarCuenta();
        FormatoPresentacion.Aplicar(this);
        foreach (var nombre in new[]
        {
            "OperacionId",
            "CuentaId",
            "Entregado",
            "Cambio"
        }

        )
            if (dgvAbonos.Columns.Contains(nombre))
                dgvAbonos.Columns[nombre].Visible = false;
        if (dgvAbonos.Columns.Contains("Fecha"))
            dgvAbonos.Columns["Fecha"].DefaultCellStyle.Format = "dd/MM HH:mm";
        dgvAbonos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        dgvAbonos.DefaultCellStyle.Font = new Font("Segoe UI", 10);
        dgvAbonos.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        btnDividir.Visible = !estado.Congelada && !estado.Cuentas.Any(x => x.Operativa);
    }

    private static string M(decimal v) => MonedaGuatemala.Formatear(v);
    private void CambiarCuenta()
    {
        if (cbCuenta.SelectedItem is not CuentaSeparada c)
            return;
        nudImporte.Maximum = Math.Max(.01m, c.Saldo);
        nudImporte.Value = Math.Max(0, c.Saldo);
        var incierta = estado.Abonos.Any(a => a.Estado == "Incierto" && (!c.Operativa || a.CuentaId == c.Id));
        btnConfirmar.Enabled = c.Saldo > 0 && !incierta && !c.Liquidada && c.NombreConfirmado && estado.Pedido.Estado is "Abierto" or "En preparación";
        flpPago.Enabled = !incierta && (c.Saldo > 0 || !c.Operativa && estado.Pedido.Total == 0);
        btnConciliarConfirmado.Visible = btnConciliarRechazado.Visible = ctMotivo.Visible = lblMotivo.Visible = incierta;
        if (c.Operativa)
        {
            dgvProductos.DataSource = cuentasOperativas.SingleOrDefault(x => x.Id == c.Id)?.Productos.ToList();
            if (dgvProductos.Columns.Contains("Id"))
                dgvProductos.Columns["Id"].Visible = false;
            lblMensaje.Text = !c.NombreConfirmado ? "Confirma el nombre desde la mesa antes de cobrar esta cuenta." : incierta ? "Esta cuenta tiene tarjeta incierta; concilia con evidencia. Las otras cuentas conservan su operación." : c.Liquidada ? "Cuenta liquidada: sus recibos y consumo se conservan. Para un pedido nuevo de esta persona, crea otra cuenta." : "El abono protege esta cuenta. Otras cuentas abiertas pueden continuar su atención.";
        }

        lblCuenta.Text = $"Total {M(c.Total)} · pagado {M(c.Pagado)} · pendiente {M(c.Saldo)}";
        CambiarPago();
    }

    private void CambiarPago()
    {
        var tarjeta = cbMetodo.Text == "Tarjeta";
        cbResultado.Visible = ctReferencia.Visible = lblResultado.Visible = lblReferencia.Visible = tarjeta;
        nudEntregado.Enabled = !tarjeta;
        if (tarjeta)
            nudEntregado.Value = nudImporte.Value;
        lblCambio.Text = tarjeta ? "Registro manual; no se conecta al banco." : $"Cambio: {M(Math.Max(0, nudEntregado.Value - nudImporte.Value))}";
        btnConfirmar.Text = $"Confirmar abono de {M(nudImporte.Value)}";
        btnUsarRecibido.Visible = !tarjeta && nudEntregado.Value > 0 && nudEntregado.Value < nudImporte.Value;
        btnUsarRecibido.Text = $"Aplicar {M(nudEntregado.Value)} como abono parcial";
    }

    private void UsarRecibido_Click(object? sender, EventArgs e)
    {
        nudImporte.Value = Math.Min(nudEntregado.Value, nudImporte.Maximum);
        var saldo = (cbCuenta.SelectedItem as CuentaSeparada)?.Saldo ?? 0;
        lblMensaje.Text = $"Aplicarás {M(nudImporte.Value)}. Saldo previsto de esta cuenta: {M(saldo - nudImporte.Value)}. Revisa y confirma antes de registrar; aún no se ha cobrado.";
    }

    private async Task Ejecutar(Func<Task> accion)
    {
        if (InvokeRequired)
        {
            await Restaurante.Frontend.Infraestructura.ContextoInterfaz.Invocar(this, () => Ejecutar(accion));
            return;
        }

        Restaurante.Frontend.Infraestructura.ContextoInterfaz.AsegurarActual();
        if (ocupado)
            return;
        ocupado = true;
        UseWaitCursor = true;
        pnlCuerpo.Enabled = false;
        btnVolver.Enabled = false;
        try
        {
            await accion();
        }
        catch (Exception ex)
        {
            lblMensaje.Text = MensajesReserva.Error(ex) + " Si existe una operación pendiente, vuelve a Inicio para recuperarla.";
        }
        finally
        {
            ocupado = false;
            UseWaitCursor = false;
            pnlCuerpo.Enabled = true;
            btnVolver.Enabled = true;
            if (repo is not null && DiarioOperaciones.ParaPedido(repo, estado?.Pedido.MesaId ?? 0, pedido).Count > 0)
            {
                btnConfirmar.Enabled = btnDividir.Enabled = btnConciliarConfirmado.Enabled = btnConciliarRechazado.Enabled = false;
                lblMensaje.Text = "Operación pendiente: recupera la misma solicitud desde Inicio. No vuelvas a cobrar.";
            }
        }
    }

    private async void Confirmar_Click(object? sender, EventArgs e) => await Ejecutar(async () =>
    {
        if (!chkDatos.Checked || !chkPago.Checked && !(cbMetodo.Text == "Tarjeta" && cbResultado.Text is "Rechazado" or "Incierto"))
            throw new ArgumentException("Revisa los datos y confirma el efectivo recibido o el resultado del pago externo.");
        if (cbCuenta.SelectedItem is not CuentaSeparada c)
            throw new ArgumentException("Selecciona la cuenta.");
        var id = Guid.NewGuid();
        var datos = new SolicitudCobro(id, pedido, estado.Pedido.Version, nudImporte.Value, chkPublico.Checked, ctReceptor.Text, ctIdentificacion.Text, null, null, cbMetodo.Text, nudEntregado.Value, cbResultado.Text, ctReferencia.Text, null, true, chkPago.Checked);
        var s = new SolicitudAbono(id, pedido, estado.Pedido.Version, estado.Pedido.Total, c.Id == 0 ? null : c.Id, c.Saldo, nudImporte.Value, datos);
        ValidacionCobro.Comprobar(datos);
        if (s.Importe <= 0 || s.Importe > c.Saldo)
            throw new ArgumentException("El importe debe ser positivo y no exceder el saldo de la cuenta.");
        if (MessageBox.Show(this, $"{c.Nombre}\nAplicar {M(s.Importe)} mediante {datos.Metodo}.\n¿Confirmas los datos?", "Revisar abono", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;
        await DiarioOperaciones.Ejecutar(repo, "Abono", id, s, () => repo.RegistrarAbono(s));
        using (var recibo = new FormularioComprobante(await Task.Run(() => repo.ConsultarReciboAbono(id))))
            recibo.ShowDialog(this);
        await Cargar();
        chkDatos.Checked = chkPago.Checked = false;
    });
    private async void Pendiente_Click(object? sender, EventArgs e) => await Ejecutar(async () =>
    {
        using var f = new FormularioRecuperacion(repo, estado.Pedido.MesaId, pedido);
        f.ShowDialog(this);
        await Cargar();
    });
    private async void Dividir_Click(object? sender, EventArgs e) => await Ejecutar(async () =>
    {
        using var f = new FormularioDivision(repo, estado.Pedido);
        f.ShowDialog(this);
        await Cargar();
    });
    private async void Conciliar_Click(object? sender, EventArgs e) => await Ejecutar(async () =>
    {
        ReglasRestaurante.Texto(ctMotivo.Text, "Motivo y evidencia de conciliación", 500);
        var seleccion = cbCuenta.SelectedItem as CuentaSeparada;
        var a = estado.Abonos.Single(a => a.Estado == "Incierto" && (seleccion?.Operativa != true || a.CuentaId == seleccion.Id));
        var s = new SolicitudConciliacionAbono(Guid.NewGuid(), pedido, estado.Pedido.Version, estado.Pedido.Total, a.OperacionId, sender == btnConciliarConfirmado, ctMotivo.Text.Trim());
        if (!FormularioAutorizar.Solicitar(this, repo, s.OperacionId, "ConciliarAbono", s, $"Abono {a.OperacionId}\nImporte {M(a.Importe)}\nResultado {(s.Confirmado ? "Confirmado" : "Rechazado")}\nMotivo {s.Motivo}"))
            return;
        await DiarioOperaciones.Ejecutar(repo, "ConciliarAbono", s.OperacionId, s, () => repo.ConciliarAbono(s));
        await Cargar();
    });
    private async void Recibo_Click(object? sender, EventArgs e) => await Ejecutar(async () =>
    {
        if (dgvAbonos.CurrentRow?.DataBoundItem is not AbonoResumen a)
            throw new ArgumentException("Selecciona un abono del historial.");
        using var f = new FormularioComprobante(await Task.Run(() => repo.ConsultarReciboAbono(a.OperacionId)));
        f.ShowDialog(this);
    });
    private async void Final_Click(object? sender, EventArgs e) => await Ejecutar(async () =>
    {
        using var f = new FormularioComprobante(await Task.Run(() => repo.ConsultarComprobante(pedido)));
        f.ShowDialog(this);
    });
    private async void Cortesia_Click(object? sender, EventArgs e) => await Ejecutar(async () =>
    {
        using var f = new FormularioCobro(repo, estado.Pedido);
        f.ShowDialog(this);
        await Cargar();
    });
}
