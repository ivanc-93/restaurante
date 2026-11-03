#nullable enable
namespace Restaurante.Frontend.Formularios;
partial class FormularioCobro
{
    private System.ComponentModel.IContainer? components;
    private TextBox ctDetalle = null!;
    private ComboBox cbCliente = null!;
    private CheckBox chkPublico = null!;
    private CheckBox chkOpcionales = null!;
    private TextBox ctReceptor = null!;
    private TextBox ctIdentificacion = null!;
    private TextBox ctDireccion = null!;
    private TextBox ctCorreo = null!;
    private ComboBox cbMetodo = null!;
    private ComboBox cbResultadoTarjeta = null!;
    private NumericUpDown nudEntregado = null!;
    private TextBox ctReferencia = null!;
    private TextBox ctObservacion = null!;
    private CheckBox chkDatosConfirmados = null!;
    private CheckBox chkPagoConfirmado = null!;
    private Label lblSaldo = null!;
    private Label lblMensaje = null!;
    private Button btnConfirmar = null!;
    private TableLayoutPanel tlpEstructura = null!;
    private FlowLayoutPanel flpCampos = null!;
    private TableLayoutPanel tlpPie = null!;
    private DataGridView dgvTablaCuenta = null!;
    private Label lblTotalCuenta = null!;
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        ctDetalle = new TextBox()
        {
            Name = "ctDetalle"
        };
        cbCliente = new ComboBox()
        {
            Name = "cbCliente"
        };
        chkPublico = new CheckBox()
        {
            Name = "chkPublico"
        };
        chkOpcionales = new CheckBox()
        {
            Name = "chkOpcionales"
        };
        ctReceptor = new TextBox()
        {
            Name = "ctReceptor"
        };
        ctIdentificacion = new TextBox()
        {
            Name = "ctIdentificacion"
        };
        ctDireccion = new TextBox()
        {
            Name = "ctDireccion"
        };
        ctCorreo = new TextBox()
        {
            Name = "ctCorreo"
        };
        cbMetodo = new ComboBox()
        {
            Name = "cbMetodo"
        };
        cbResultadoTarjeta = new ComboBox()
        {
            Name = "cbResultadoTarjeta"
        };
        nudEntregado = new NumericUpDown()
        {
            Name = "nudEntregado"
        };
        ctReferencia = new TextBox()
        {
            Name = "ctReferencia"
        };
        ctObservacion = new TextBox()
        {
            Name = "ctObservacion"
        };
        chkDatosConfirmados = new CheckBox()
        {
            Name = "chkDatosConfirmados"
        };
        chkPagoConfirmado = new CheckBox()
        {
            Name = "chkPagoConfirmado"
        };
        lblSaldo = new Label()
        {
            Name = "lblSaldo"
        };
        lblMensaje = new Label()
        {
            Name = "lblMensaje"
        };
        btnConfirmar = new Button()
        {
            Name = "btnConfirmar"
        };
        tlpEstructura = new TableLayoutPanel()
        {
            Name = "tlpEstructura"
        };
        flpCampos = new FlowLayoutPanel()
        {
            Name = "flpCampos"
        };
        tlpPie = new TableLayoutPanel()
        {
            Name = "tlpPie"
        };
        ((System.ComponentModel.ISupportInitialize)nudEntregado).BeginInit();
        SuspendLayout();
        Font = Componentes.Fuente;
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoScaleDimensions = new SizeF(96, 96);
        tlpEstructura.Dock = DockStyle.Fill;
        tlpEstructura.ColumnCount = 2;
        tlpEstructura.RowCount = 2;
        tlpEstructura.Padding = new Padding(16);
        tlpEstructura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48));
        tlpEstructura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52));
        tlpEstructura.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        tlpEstructura.RowStyles.Add(new RowStyle(SizeType.Absolute, 145));
        ctDetalle.Dock = DockStyle.Fill;
        ctDetalle.Multiline = true;
        ctDetalle.ReadOnly = true;
        ctDetalle.ScrollBars = ScrollBars.Vertical;
        ctDetalle.BackColor = Color.White;
        ctDetalle.AccessibleName = "Detalle y total de la cuenta";
        ctDetalle.TabStop = false;
        flpCampos.Dock = DockStyle.Fill;
        flpCampos.FlowDirection = FlowDirection.TopDown;
        flpCampos.WrapContents = false;
        flpCampos.AutoScroll = true;
        flpCampos.Padding = new Padding(12);
        flpCampos.BackColor = Color.White;
        flpCampos.Margin = new Padding(14, 0, 0, 0);
        dgvTablaCuenta = new DataGridView
        {
            Name = "tablaCuenta",
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            TabStop = false
        };
        Componentes.EstilizarTabla(dgvTablaCuenta);
        dgvTablaCuenta.Columns.Add(new DataGridViewTextBoxColumn { Name = "Producto", DataPropertyName = "Producto", HeaderText = "Producto", FillWeight = 200 });
        dgvTablaCuenta.Columns.Add(new DataGridViewTextBoxColumn { Name = "Cantidad", DataPropertyName = "Cantidad", HeaderText = "Cant.", FillWeight = 55 });
        dgvTablaCuenta.Columns.Add(new DataGridViewTextBoxColumn { Name = "Unitario", DataPropertyName = "PrecioUnitario", HeaderText = "Unitario", FillWeight = 90, DefaultCellStyle = new DataGridViewCellStyle { Format = "'Q '#,##0.00" } });
        dgvTablaCuenta.Columns.Add(new DataGridViewTextBoxColumn { Name = "Importe", DataPropertyName = "Importe", HeaderText = "Total", FillWeight = 90 });
        lblTotalCuenta = new Label
        {
            Text = "Total",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Segoe UI", 24, FontStyle.Bold),
            ForeColor = Componentes.Verde,
            Name = "lblTotalCuenta"
        };
        var tlpCuentaPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(16),
            ColumnCount = 1,
            RowCount = 3,
            Name = "tlpCuentaPanel"
        };
        tlpCuentaPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        tlpCuentaPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
        tlpCuentaPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        tlpCuentaPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
        tlpCuentaPanel.Controls.Add(new Label { Text = "Detalle del pedido", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 22, FontStyle.Bold) }, 0, 0);
        tlpCuentaPanel.Controls.Add(dgvTablaCuenta, 0, 1);
        tlpCuentaPanel.Controls.Add(lblTotalCuenta, 0, 2);
        ctDetalle.Visible = false;
        components.Add(ctDetalle);
        flpCampos.Controls.Add(new Label { Text = "Cobro manual", Font = new Font("Segoe UI", 22, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 0, 0, 14) });
        cbCliente.DropDownStyle = ComboBoxStyle.DropDownList;
        cbCliente.Width = 330;
        cbCliente.AccessibleName = "Copiar datos de cliente del catálogo";
        chkPublico.Text = "Público general / sin identificación";
        chkPublico.AutoSize = true;
        chkPublico.CheckedChanged += Datos_Cambio;
        chkOpcionales.Text = "Agregar dirección, correo u observación (opcional)";
        chkOpcionales.AutoSize = true;
        chkOpcionales.CheckedChanged += Datos_Cambio;
        ctReceptor.Width = 330;
        ctReceptor.MaxLength = 120;
        ctReceptor.AccessibleName = "Nombre o razón social del receptor";
        ctIdentificacion.Width = 330;
        ctIdentificacion.MaxLength = 40;
        ctIdentificacion.AccessibleName = "NIT u otra identificación opcional";
        ctDireccion.Width = 330;
        ctDireccion.MaxLength = 250;
        ctDireccion.AccessibleName = "Dirección opcional";
        ctCorreo.Width = 330;
        ctCorreo.MaxLength = 150;
        ctCorreo.AccessibleName = "Correo opcional";
        cbMetodo.DropDownStyle = ComboBoxStyle.DropDownList;
        cbMetodo.Width = 330;
        cbMetodo.Items.AddRange(new object[] { "Efectivo", "Tarjeta" });
        cbMetodo.SelectedIndexChanged += Datos_Cambio;
        cbResultadoTarjeta.DropDownStyle = ComboBoxStyle.DropDownList;
        cbResultadoTarjeta.Width = 330;
        cbResultadoTarjeta.Items.AddRange(new object[] { "Confirmado", "Rechazado", "Incierto" });
        cbResultadoTarjeta.AccessibleName = "Resultado declarado del pago externo";
        cbResultadoTarjeta.SelectedIndexChanged += Datos_Cambio;
        nudEntregado.DecimalPlaces = 2;
        nudEntregado.Maximum = 9999999999.99M;
        nudEntregado.Width = 330;
        nudEntregado.ValueChanged += Datos_Cambio;
        nudEntregado.AccessibleName = "Importe entregado o pagado externamente";
        ctReferencia.Width = 330;
        ctReferencia.MaxLength = 100;
        ctReferencia.AccessibleName = "Referencia externa sin datos de tarjeta";
        ctObservacion.Width = 330;
        ctObservacion.MaxLength = 250;
        ctObservacion.AccessibleName = "Observación del pago";
        chkDatosConfirmados.Text = "Revisé productos, importe y datos del receptor";
        chkDatosConfirmados.AutoSize = true;
        chkPagoConfirmado.Text = "Confirmo recepción del efectivo o comprobación manual del pago";
        chkPagoConfirmado.AutoSize = true;
        flpCampos.Controls.Add(new Label { Text = "Copiar cliente existente (opcional)", AutoSize = true });
        flpCampos.Controls.Add(cbCliente);
        flpCampos.Controls.Add(chkPublico);
        flpCampos.Controls.Add(new Label { Name = "lblReceptor", Text = "Nombre del receptor", AutoSize = true });
        flpCampos.Controls.Add(ctReceptor);
        flpCampos.Controls.Add(new Label { Name = "lblIdentificacion", Text = "NIT / identificación (opcional, sin validación SAT)", AutoSize = true });
        flpCampos.Controls.Add(ctIdentificacion);
        flpCampos.Controls.Add(chkOpcionales);
        flpCampos.Controls.Add(new Label { Name = "lblDireccion", Text = "Dirección (opcional)", AutoSize = true });
        flpCampos.Controls.Add(ctDireccion);
        flpCampos.Controls.Add(new Label { Name = "lblCorreo", Text = "Correo (opcional)", AutoSize = true });
        flpCampos.Controls.Add(ctCorreo);
        flpCampos.Controls.Add(new Label { Text = "Método de pago manual", AutoSize = true });
        flpCampos.Controls.Add(cbMetodo);
        flpCampos.Controls.Add(new Label { Name = "lblResultado", Text = "Resultado del terminal externo (solo tarjeta)", AutoSize = true });
        flpCampos.Controls.Add(cbResultadoTarjeta);
        flpCampos.Controls.Add(new Label { Text = "Entregado en efectivo / importe externo", AutoSize = true });
        flpCampos.Controls.Add(nudEntregado);
        flpCampos.Controls.Add(new Label { Name = "lblReferencia", Text = "Referencia externa (sin número de tarjeta)", AutoSize = true });
        flpCampos.Controls.Add(ctReferencia);
        flpCampos.Controls.Add(new Label { Name = "lblObservacion", Text = "Observación", AutoSize = true });
        flpCampos.Controls.Add(ctObservacion);
        flpCampos.Controls.Add(chkDatosConfirmados);
        flpCampos.Controls.Add(chkPagoConfirmado);
        chkDatosConfirmados.CheckedChanged += Datos_Cambio;
        chkPagoConfirmado.CheckedChanged += Datos_Cambio;
        tlpPie.Dock = DockStyle.Fill;
        tlpPie.ColumnCount = 1;
        tlpPie.RowCount = 3;
        tlpPie.AutoSize = false;
        tlpPie.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        tlpPie.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tlpPie.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tlpPie.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        lblSaldo.AutoSize = true;
        lblSaldo.Font = new Font("Segoe UI", 13, FontStyle.Bold);
        lblMensaje.AutoSize = true;
        lblMensaje.AutoEllipsis = true;
        lblMensaje.MaximumSize = new Size(850, 70);
        lblMensaje.Text = "Comprobante interno académico — no válido como documento tributario. Sin conexión SAT ni bancaria.";
        btnConfirmar.Text = "Confirmar cobro";
        btnConfirmar.AutoSize = false;
        btnConfirmar.Height = 50;
        btnConfirmar.Dock = DockStyle.Top;
        btnConfirmar.Click += Confirmar_Click;
        Componentes.EstilizarBoton(btnConfirmar, true);
        Iconos.Aplicar(btnConfirmar, "check");
        tlpPie.Controls.Add(lblSaldo, 0, 0);
        tlpPie.Controls.Add(lblMensaje, 0, 1);
        tlpPie.Controls.Add(btnConfirmar, 0, 2);
        tlpEstructura.Controls.Add(tlpCuentaPanel, 0, 0);
        tlpEstructura.Controls.Add(flpCampos, 1, 0);
        tlpEstructura.Controls.Add(tlpPie, 0, 1);
        tlpEstructura.SetColumnSpan(tlpPie, 2);
        Controls.Add(tlpEstructura);
        ClientSize = new Size(1220, 730);
        MinimumSize = new Size(800, 520);
        StartPosition = FormStartPosition.CenterParent;
        Text = "Sabores | Revisar y cobrar · GTQ";
        ((System.ComponentModel.ISupportInitialize)nudEntregado).EndInit();
        ResumeLayout(false);
    }
}
