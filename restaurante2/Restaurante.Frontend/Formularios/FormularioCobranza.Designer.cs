#nullable enable
namespace Restaurante.Frontend.Formularios;
partial class FormularioCobranza
{
    private Label lblResumen = null!, lblCuenta = null!, lblCambio = null!, lblMensaje = null!, lblMotivo = null!, lblResultado = null!, lblReferencia = null!;
    private Panel pnlCuerpo = null!;
    private FlowLayoutPanel flpPago = null!;
    private ComboBox cbCuenta = null!, cbMetodo = null!, cbResultado = null!;
    private NumericUpDown nudImporte = null!, nudEntregado = null!;
    private TextBox ctReceptor = null!, ctIdentificacion = null!, ctReferencia = null!, ctMotivo = null!;
    private CheckBox chkPublico = null!, chkDatos = null!, chkPago = null!;
    private Button btnUsarRecibido = null!, btnRevisarPendiente = null!;
    private Button btnConfirmar = null!, btnDividir = null!, btnVolver = null!, btnConciliarConfirmado = null!, btnConciliarRechazado = null!, btnCortesia = null!;
    private DataGridView dgvProductos = null!, dgvAbonos = null!;
    private void InitializeComponent()
    {
        SuspendLayout();
        Font = Componentes.Fuente;
        BackColor = Componentes.Fondo;
        AutoScaleDimensions = new SizeF(96, 96);
        AutoScaleMode = AutoScaleMode.Dpi;
        Text = "Sabores | Cuentas y abonos · GTQ";
        ClientSize = new Size(1180, 760);
        MinimumSize = new Size(800, 550);
        StartPosition = FormStartPosition.CenterParent;
        var raiz = new TableLayoutPanel
        {
            Name = "tlpCobranza",
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(16)
        };
        raiz.RowStyles.Add(new(SizeType.AutoSize));
        raiz.RowStyles.Add(new(SizeType.Percent, 100));
        raiz.RowStyles.Add(new(SizeType.AutoSize));
        raiz.ColumnStyles.Add(new(SizeType.Percent, 100));
        lblResumen = Componentes.Etiqueta("Cargando cuenta…", 12, true);
        lblResumen.Name = nameof(lblResumen);
        lblResumen.Dock = DockStyle.Top;
        raiz.Controls.Add(lblResumen, 0, 0);
        pnlCuerpo = new Panel
        {
            Name = nameof(pnlCuerpo),
            Dock = DockStyle.Fill
        };
        var columnas = new TableLayoutPanel
        {
            Name = "tlpColumnas",
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        columnas.ColumnStyles.Add(new(SizeType.Percent, 52));
        columnas.ColumnStyles.Add(new(SizeType.Percent, 48));
        columnas.RowStyles.Add(new(SizeType.Percent, 100));
        pnlCuerpo.Controls.Add(columnas);
        var izquierda = new TableLayoutPanel
        {
            Name = "tlpHistorial",
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(0, 0, 12, 0)
        };
        izquierda.ColumnStyles.Add(new(SizeType.Percent, 100));
        izquierda.RowStyles.Add(new(SizeType.Percent, 100));
        izquierda.RowStyles.Add(new(SizeType.AutoSize));
        var tabs = new TabControl
        {
            Name = "tabConsumoAbonos",
            Dock = DockStyle.Fill
        };
        var consumo = new TabPage("Productos");
        var historial = new TabPage("Abonos y recibos");
        dgvProductos = new()
        {
            Name = nameof(dgvProductos),
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        consumo.Controls.Add(dgvProductos);
        dgvAbonos = new()
        {
            Name = nameof(dgvAbonos),
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells
        };
        historial.Controls.Add(dgvAbonos);
        tabs.TabPages.AddRange([consumo, historial]);
        izquierda.Controls.Add(tabs, 0, 0);
        var recibos = new FlowLayoutPanel
        {
            Name = "flpRecibos",
            Dock = DockStyle.Top,
            AutoSize = true
        };
        var btnRecibo = Componentes.Boton("Ver recibo", false);
        btnRecibo.Name = "btnRecibo";
        btnRecibo.Click += Recibo_Click;
        var btnFinal = Componentes.Boton("Resumen final", false);
        btnFinal.Name = "btnFinal";
        btnFinal.Click += Final_Click;
        foreach (var b in new[]
        {
            btnRecibo,
            btnFinal
        }

        )
        {
            b.Font = new Font("Segoe UI", 10);
            b.Margin = new Padding(0, 4, 8, 4);
        }

        btnRevisarPendiente = Componentes.Boton("Revisar pendiente", false);
        btnRevisarPendiente.Name = nameof(btnRevisarPendiente);
        btnRevisarPendiente.Click += Pendiente_Click;
        recibos.Controls.AddRange([btnRecibo, btnFinal, btnRevisarPendiente]);
        izquierda.Controls.Add(recibos, 0, 1);
        columnas.Controls.Add(izquierda, 0, 0);
        var derecha = new Panel
        {
            Name = "pnlPago",
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = Color.White
        };
        flpPago = new()
        {
            Name = nameof(flpPago),
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(12)
        };
        derecha.Controls.Add(flpPago);
        columnas.Controls.Add(derecha, 1, 0);
        void Agregar(Control c)
        {
            c.Margin = new Padding(0, 4, 0, 6);
            flpPago.Controls.Add(c);
        }

        Agregar(Componentes.Etiqueta("Registrar pago", 12, true));
        Agregar(Componentes.Etiqueta("Cuenta a pagar"));
        cbCuenta = new()
        {
            Name = nameof(cbCuenta),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cbCuenta.SelectedIndexChanged += (_, _) => CambiarCuenta();
        Agregar(cbCuenta);
        lblCuenta = Componentes.Etiqueta("");
        lblCuenta.Name = nameof(lblCuenta);
        Agregar(lblCuenta);
        btnDividir = Componentes.Boton("Separar cuentas", false);
        btnDividir.Name = nameof(btnDividir);
        btnDividir.Click += Dividir_Click;
        Agregar(btnDividir);
        Agregar(Componentes.Etiqueta("Importe que se aplicará (Q)"));
        nudImporte = new()
        {
            Name = nameof(nudImporte),
            DecimalPlaces = 2,
            Maximum = 9999999999.99m
        };
        nudImporte.ValueChanged += (_, _) => CambiarPago();
        Agregar(nudImporte);
        Agregar(Componentes.Etiqueta("Método manual"));
        cbMetodo = new()
        {
            Name = nameof(cbMetodo),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cbMetodo.Items.AddRange(["Efectivo", "Tarjeta"]);
        cbMetodo.SelectedIndex = 0;
        cbMetodo.SelectedIndexChanged += (_, _) => CambiarPago();
        Agregar(cbMetodo);
        Agregar(Componentes.Etiqueta("Efectivo recibido / importe externo (Q)"));
        nudEntregado = new()
        {
            Name = nameof(nudEntregado),
            DecimalPlaces = 2,
            Maximum = 9999999999.99m
        };
        nudEntregado.ValueChanged += (_, _) => CambiarPago();
        Agregar(nudEntregado);
        btnUsarRecibido = Componentes.Boton("Usar efectivo recibido como importe", false);
        btnUsarRecibido.Name = nameof(btnUsarRecibido);
        btnUsarRecibido.Visible = false;
        btnUsarRecibido.Click += UsarRecibido_Click;
        Agregar(btnUsarRecibido);
        lblCambio = Componentes.Etiqueta("");
        lblCambio.Name = nameof(lblCambio);
        Agregar(lblCambio);
        lblResultado = Componentes.Etiqueta("Resultado declarado del terminal");
        lblResultado.Name = nameof(lblResultado);
        Agregar(lblResultado);
        cbResultado = new()
        {
            Name = nameof(cbResultado),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cbResultado.Items.AddRange(["Confirmado", "Rechazado", "Incierto"]);
        cbResultado.SelectedIndex = 0;
        Agregar(cbResultado);
        lblReferencia = Componentes.Etiqueta("Referencia externa (nunca tarjeta, CVV o PIN)");
        lblReferencia.Name = nameof(lblReferencia);
        Agregar(lblReferencia);
        ctReferencia = new()
        {
            Name = nameof(ctReferencia),
            MaxLength = 100
        };
        Agregar(ctReferencia);
        Agregar(Componentes.Etiqueta("Datos del receptor del abono", 10, true));
        chkPublico = new()
        {
            Name = nameof(chkPublico),
            Text = "Público general",
            Checked = true,
            AutoSize = true
        };
        Agregar(chkPublico);
        ctReceptor = new()
        {
            AccessibleName = "Nombre del receptor",
            Name = nameof(ctReceptor),
            PlaceholderText = "Nombre del receptor",
            MaxLength = 120
        };
        Agregar(ctReceptor);
        ctIdentificacion = new()
        {
            AccessibleName = "Identificación del receptor (opcional)",
            Name = nameof(ctIdentificacion),
            PlaceholderText = "Identificación opcional",
            MaxLength = 40
        };
        Agregar(ctIdentificacion);
        chkPublico.CheckedChanged += (_, _) =>
        {
            ctReceptor.Visible = ctIdentificacion.Visible = ctReceptor.Enabled = ctIdentificacion.Enabled = !chkPublico.Checked;
            if (chkPublico.Checked)
                ctIdentificacion.Clear();
        };
        ctReceptor.Visible = ctIdentificacion.Visible = ctReceptor.Enabled = ctIdentificacion.Enabled = false;
        chkDatos = new()
        {
            Name = nameof(chkDatos),
            Text = "Revisé cuenta, importe y receptor",
            AutoSize = true
        };
        Agregar(chkDatos);
        chkPago = new()
        {
            Name = nameof(chkPago),
            Text = "Confirmo recepción / verificación manual del pago",
            AutoSize = true
        };
        Agregar(chkPago);
        btnConfirmar = Componentes.Boton("Confirmar abono");
        btnConfirmar.Name = nameof(btnConfirmar);
        btnConfirmar.Click += Confirmar_Click;
        btnCortesia = Componentes.Boton("Cerrar cortesía autorizada", false);
        btnCortesia.Name = nameof(btnCortesia);
        btnCortesia.Click += Cortesia_Click;
        Agregar(btnCortesia);
        // La conciliación permanece fuera del panel de pago que se deshabilita si está incierto.
        var conciliacion = new FlowLayoutPanel
        {
            Name = "flpConciliacion",
            Dock = DockStyle.Bottom,
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };
        lblMotivo = Componentes.Etiqueta("Evidencia para resolver tarjeta incierta");
        ctMotivo = new()
        {
            Name = nameof(ctMotivo),
            MaxLength = 500,
            Multiline = true,
            Height = 70,
            Width = 360
        };
        btnConciliarConfirmado = Componentes.Boton("Confirmar pago externo", false);
        btnConciliarRechazado = Componentes.Boton("Declarar pago rechazado", false);
        btnConciliarConfirmado.Name = nameof(btnConciliarConfirmado);
        btnConciliarRechazado.Name = nameof(btnConciliarRechazado);
        btnConciliarConfirmado.Click += Conciliar_Click;
        btnConciliarRechazado.Click += Conciliar_Click;
        conciliacion.Controls.AddRange([lblMotivo, ctMotivo, btnConciliarConfirmado, btnConciliarRechazado]);
        derecha.Controls.Add(conciliacion);
        var pie = new TableLayoutPanel
        {
            Name = "tlpPie",
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 3
        };
        pie.ColumnStyles.Add(new(SizeType.Percent, 100));
        pie.ColumnStyles.Add(new(SizeType.AutoSize));
        pie.ColumnStyles.Add(new(SizeType.AutoSize));
        lblMensaje = Componentes.Etiqueta("Comprobantes académicos. Sin SAT, FEL ni conexión bancaria.");
        lblMensaje.Name = nameof(lblMensaje);
        lblMensaje.Dock = DockStyle.Fill;
        btnVolver = Componentes.Boton("Volver", false);
        btnVolver.Name = nameof(btnVolver);
        btnVolver.Click += (_, _) => Close();
        pie.Controls.Add(lblMensaje, 0, 0);
        pie.Controls.Add(btnConfirmar, 1, 0);
        pie.Controls.Add(btnVolver, 2, 0);
        raiz.Controls.Add(pnlCuerpo, 0, 1);
        raiz.Controls.Add(pie, 0, 2);
        Controls.Add(raiz);
        foreach (Control c in flpPago.Controls)
            c.Font = new Font("Segoe UI", 10, c is Button ? FontStyle.Bold : FontStyle.Regular);
        btnConfirmar.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        lblMensaje.Font = new Font("Segoe UI", 10);
        derecha.SizeChanged += (_, _) =>
        {
            var ancho = Math.Max(200, derecha.ClientSize.Width - 45);
            foreach (Control c in flpPago.Controls)
            {
                c.Width = ancho;
                if (c is Label or CheckBox)
                    c.MaximumSize = new Size(ancho, 0);
            }

            lblMensaje.MaximumSize = new Size(Math.Max(250, ClientSize.Width - 650), 0);
        };
        ResumeLayout(true);
    }
}
