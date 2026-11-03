using Restaurante.Backend.Dominio;

namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioDetalleMesa
{
    private ResumenCobranza? _cobranzaCuentas;
    private ServicioMesa? _servicio;
    private CuentaRevision? _revisionServicio;
    private int _cuentaSeleccionada;
    private bool cambiandoCuentas;
    private CuentaOperativa? CuentaSeleccionada() => cbCuentasServicio.SelectedItem as CuentaOperativa;
    private void AplicarServicio(ServicioMesa? servicio, CuentaRevision? revision, ResumenCobranza? cobranza)
    {
        _servicio = servicio;
        _revisionServicio = revision;
        _cobranzaCuentas = cobranza;
        cambiandoCuentas = true;
        try
        {
            var habilitado = servicio?.Operativa == true;
            cbCuentasServicio.Visible = btnCrearCuenta.Visible = btnNombreCuenta.Visible = btnMoverCuenta.Visible = btnBeneficioCuenta.Visible = habilitado;
            dgvCuentasServicio.Visible = habilitado && servicio!.Cuentas.Count > 1;
            tlpIzquierda.RowStyles[0].Height = dgvCuentasServicio.Visible ? 120 * DeviceDpi / 96f : 0;
            if (dgvCuentasServicio.Visible)
            {
                dgvCuentasServicio.DataSource = servicio!.Cuentas.Select(x => new { Id = x.Id, Cuenta = $"{x.Nombre} · #{x.Id}", Consumo = x.Original, Descuento = x.Descuento, Pagado = x.Pagado, Saldo = x.Saldo, Estado = x.Liquidada ? "Liquidada" : x.Pagado > 0 ? "Parcial" : "Abierta" }).ToList();
                dgvCuentasServicio.Columns["Id"].Visible = false;
            }

            btnPendienteMesa.Visible = _recuperacionPendiente;
            btnPendienteMesa.Enabled = true;
            if (habilitado)
            {
                var lista = servicio!.Cuentas.ToList();
                var todos = new CuentaOperativa(-1, "Toda la mesa", true, false, lista.Sum(x => x.Original), lista.Sum(x => x.Descuento), lista.Sum(x => x.Pagado), "", []);
                cbCuentasServicio.DataSource = new[]
                {
                    todos
                }.Concat(lista).ToList();
                cbCuentasServicio.DisplayMember = "Etiqueta";
                cbCuentasServicio.SelectedItem = lista.FirstOrDefault(x => x.Id == _cuentaSeleccionada) ?? (lista.Count == 1 ? lista[0] : todos);
            }
        }
        finally
        {
            cambiandoCuentas = false;
        }

        CambiarCuentaServicio();
    }

    private void CambiarCuentaServicio()
    {
        if (cambiandoCuentas || _servicio?.Operativa != true)
            return;
        var cuenta = CuentaSeleccionada();
        if (cuenta is null)
            return;
        _cuentaSeleccionada = cuenta.Id;
        var concreta = cuenta.Id >= 0;
        var pendientes = DiarioOperaciones.ParaPedido(_repositorio, _mesa.Id, _pedido?.Id);
        _recuperacionPendiente = pendientes.Count > 0;
        var cobroProtegido = cuenta.Id > 0 && (_cobranzaCuentas?.Abonos.Any(a => a.CuentaId == cuenta.Id && a.Estado is "Confirmado" or "Incierto") == true);
        // Las cuentas preparadas antes del pedido no requieren consultar una cobranza inexistente.
        var editable = concreta && !cuenta.Liquidada && !cobroProtegido;
        var lineas = concreta ? cuenta.Productos : _servicio.Cuentas.SelectMany(x => x.Productos).GroupBy(x => x.Id).Select(g => new DetalleCuenta(g.Key, g.First().Producto, g.First().Tipo, g.Sum(x => x.Cantidad), g.First().PrecioUnitario, g.Sum(x => x.Importe))).ToList();
        dgvDetalles.DataSource = lineas.ToList();
        if (dgvDetalles.Columns.Contains("Id"))
            dgvDetalles.Columns["Id"].Visible = false;
        lblTotal.Text = $"{(concreta ? cuenta.Nombre : "Toda la mesa")} · consumo {FormatoPresentacion.Moneda(cuenta.Original)}\nAjustes {FormatoPresentacion.Moneda(cuenta.Descuento)} · pagado {FormatoPresentacion.Moneda(cuenta.Pagado)}\nSaldo {FormatoPresentacion.Moneda(cuenta.Saldo)}";
        btnAgregarProducto.Enabled = flpCatalogo.Enabled = editable && !_recuperacionPendiente && _menu.Count > 0;
        btnCrearCuenta.Enabled = !_recuperacionPendiente;
        btnNombreCuenta.Enabled = editable && cuenta.Id >= 0 && !_recuperacionPendiente;
        btnMoverCuenta.Enabled = editable && cuenta.Id > 0 && cuenta.Productos.Count > 0 && !_recuperacionPendiente;
        btnBeneficioCuenta.Enabled = _pedido is not null && !_recuperacionPendiente && (concreta ? editable && cuenta.Original > 0 : _servicio.Cuentas.All(x => !x.Liquidada && x.Pagado == 0));
        btnRevisarCobro.Enabled = _pedido is not null && !_recuperacionPendiente && (concreta ? cuenta.Original > 0 : true);
        btnRevisarCobro.Text = concreta ? "Cobrar esta cuenta" : "Revisar cobros de la mesa";
        btnGestionar.Enabled = !_recuperacionPendiente;
        btnPendienteMesa.Visible = _recuperacionPendiente;
        btnPendienteMesa.Enabled = true;
        lblEstado.Text = _recuperacionPendiente ? "Hay una operación pendiente de confirmar. Revísala aquí; cerrar la ventana no cancela ni libera la mesa." : !concreta ? "Vista de toda la mesa. Selecciona una cuenta antes de agregar productos." : !editable ? "Cuenta protegida por pago o liquidación. Agrega otra cuenta para consumo nuevo." : $"Agregar al pedido registra el producto en: {cuenta.Nombre}.";
        if (Controls.Find("btnOpcionesComanda", true).FirstOrDefault()is Button opciones)
        {
            opciones.Enabled = !_recuperacionPendiente;
            if (opciones.ContextMenuStrip is { } menu)
            {
                menu.Items[0].Enabled = editable && _pedido?.Estado == "Abierto";
                menu.Items[1].Enabled = _pedido?.Estado == "Abierto";
            }
        }
    }

    private async Task GestionNombre(bool crear)
    {
        if (_ocupado || _cargando || _recuperacionPendiente || _servicio is null)
            return;
        var seleccion = CuentaSeleccionada();
        if (!crear && seleccion is not { Id: >= 0 })
            return;
        _ocupado = true;
        try
        {
            using var f = new FormularioNombreCuenta(_repositorio, _servicio, crear ? null : seleccion);
            if (f.ShowDialog(this) == DialogResult.OK)
                _cuentaSeleccionada = f.CuentaId;
        }
        finally
        {
            _ocupado = false;
            await RecargarSeguroAsync();
        }
    }

    private async void CrearCuenta_Click(object? sender, EventArgs e) => await GestionNombre(true);
    private async void NombreCuenta_Click(object? sender, EventArgs e) => await GestionNombre(false);
    private async void MoverCuenta_Click(object? sender, EventArgs e) => await EditarProductosCuenta(false);
    private async Task EditarProductosCuenta(bool corregir)
    {
        if (_ocupado || _cargando || _recuperacionPendiente || _servicio is null || _revisionServicio is null || CuentaSeleccionada()is not { Id: > 0 } seleccion)
            return;
        _ocupado = true;
        try
        {
            using var f = new FormularioProductoCuenta(_repositorio, _servicio, seleccion, _revisionServicio, corregir);
            f.ShowDialog(this);
        }
        finally
        {
            _ocupado = false;
            await RecargarSeguroAsync();
        }
    }

    private async void BeneficioCuenta_Click(object? sender, EventArgs e)
    {
        if (_ocupado || _cargando || _recuperacionPendiente || _revisionServicio is null)
            return;
        _ocupado = true;
        try
        {
            using var f = new FormularioBeneficioCuenta(_repositorio, _revisionServicio, CuentaSeleccionada()is { Id: >= 0 } c ? c : null);
            f.ShowDialog(this);
        }
        finally
        {
            _ocupado = false;
            await RecargarSeguroAsync();
        }
    }

    private async void PendienteMesa_Click(object? sender, EventArgs e)
    {
        if (_ocupado || _cargando)
            return;
        _ocupado = true;
        try
        {
            using var f = new FormularioRecuperacion(_repositorio, _mesa.Id, _pedido?.Id);
            f.ShowDialog(this);
        }
        finally
        {
            _ocupado = false;
            await RecargarSeguroAsync();
        }
    }

    private void EstiloResumenCuentas()
    {
        foreach (var b in new[]
        {
            btnCrearCuenta,
            btnNombreCuenta,
            btnMoverCuenta,
            btnBeneficioCuenta,
            btnPendienteMesa
        }

        )
        {
            b.MinimumSize = Size.Empty;
            b.Padding = new Padding(10, 5, 10, 5);
            b.AutoSize = true;
            b.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        }

        if (dgvDetalles.Columns.Contains("Cantidad"))
        {
            dgvDetalles.Columns["Cantidad"].HeaderText = "Ud.";
            dgvDetalles.Columns["Cantidad"].MinimumWidth = 75;
        }

        if (!dgvCuentasServicio.Visible)
            return;
        dgvCuentasServicio.DefaultCellStyle.Font = new Font("Segoe UI", 9);
        dgvCuentasServicio.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        dgvCuentasServicio.DefaultCellStyle.Padding = new Padding(3);
        dgvCuentasServicio.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
        foreach (DataGridViewRow fila in dgvCuentasServicio.Rows)
        {
            fila.MinimumHeight = (int)(26 * DeviceDpi / 96f);
            fila.Height = (int)(30 * DeviceDpi / 96f);
            fila.Cells["Cuenta"].ToolTipText = Convert.ToString(fila.Cells["Cuenta"].Value);
        }

        foreach (var n in new[]
        {
            "Consumo",
            "Descuento",
            "Pagado",
            "Saldo"
        }

        )
            dgvCuentasServicio.Columns[n].DefaultCellStyle.Format = "'Q '#,##0.00";
    }
}
