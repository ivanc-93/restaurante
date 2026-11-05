#nullable enable
namespace Restaurante.Frontend.Formularios;
partial class FormularioReportes
{
    private System.ComponentModel.IContainer? components;
    private DateTimePicker dtpInicio = null!;
    private DateTimePicker dtpFin = null!;
    private ComboBox cbAgrupacion = null!;
    private ComboBox cbMesa = null!;
    private ComboBox cbCliente = null!;
    private Button btnConsultar = null!;
    private DataGridView dgvVentas = null!;
    private DataGridView dgvPlatos = null!;
    private DataGridView dgvPedidos = null!;
    private FlowLayoutPanel flpFiltros = null!;
    private TabControl tabPestanas = null!;
    private TabPage tpVentasTab = null!;
    private TabPage tpPlatosTab = null!;
    private TabPage tpPedidosTab = null!;
    private Label lblTotal = null!;
    private Label lblResumenOperaciones = null!;
    private DataGridView dgvOperaciones = null!;
    private Label lblDesdeLabel = null!;
    private Label lblHastaLabel = null!;
    private Label lblMesaLabel = null!;
    private Label lblClienteLabel = null!;
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        dtpInicio = new DateTimePicker()
        {
            Name = "dtpInicio"
        };
        dtpFin = new DateTimePicker()
        {
            Name = "dtpFin"
        };
        cbAgrupacion = new ComboBox()
        {
            Name = "cbAgrupacion"
        };
        cbMesa = new ComboBox()
        {
            Name = "cbMesa"
        };
        cbCliente = new ComboBox()
        {
            Name = "cbCliente"
        };
        btnConsultar = new Button()
        {
            Name = "btnConsultar"
        };
        dgvVentas = new DataGridView()
        {
            Name = "dgvVentas"
        };
        dgvPlatos = new DataGridView()
        {
            Name = "dgvPlatos"
        };
        dgvPedidos = new DataGridView()
        {
            Name = "dgvPedidos"
        };
        flpFiltros = new FlowLayoutPanel()
        {
            Name = "flpFiltros"
        };
        tabPestanas = new TabControl()
        {
            Name = "tabPestanas"
        };
        tpVentasTab = new TabPage()
        {
            Name = "tpVentasTab"
        };
        tpPlatosTab = new TabPage()
        {
            Name = "tpPlatosTab"
        };
        tpPedidosTab = new TabPage()
        {
            Name = "tpPedidosTab"
        };
        lblTotal = new Label()
        {
            Name = "lblTotal"
        };
        lblDesdeLabel = new Label()
        {
            Name = "lblDesdeLabel"
        };
        lblHastaLabel = new Label()
        {
            Name = "lblHastaLabel"
        };
        lblMesaLabel = new Label()
        {
            Name = "lblMesaLabel"
        };
        lblClienteLabel = new Label()
        {
            Name = "lblClienteLabel"
        };
        ((System.ComponentModel.ISupportInitialize)dgvVentas).BeginInit();
        ((System.ComponentModel.ISupportInitialize)dgvPlatos).BeginInit();
        ((System.ComponentModel.ISupportInitialize)dgvPedidos).BeginInit();
        SuspendLayout();
        flpFiltros.Dock = DockStyle.Top;
        flpFiltros.AutoSize = true;
        flpFiltros.Padding = new Padding(12);
        flpFiltros.AutoScroll = true;
        lblDesdeLabel.Text = "Desde";
        lblDesdeLabel.AutoSize = true;
        lblHastaLabel.Text = "Hasta";
        lblHastaLabel.AutoSize = true;
        dtpInicio.Format = DateTimePickerFormat.Short;
        dtpInicio.Width = 120;
        dtpFin.Format = DateTimePickerFormat.Short;
        dtpFin.Width = 120;
        cbAgrupacion.DropDownStyle = ComboBoxStyle.DropDownList;
        cbAgrupacion.Items.AddRange(new object[] { "Día", "Semana", "Mes" });
        cbAgrupacion.Width = 100;
        cbAgrupacion.SelectedIndex = 0;
        lblMesaLabel.Text = "Mesa";
        lblMesaLabel.AutoSize = true;
        cbMesa.Width = 130;
        cbMesa.DropDownStyle = ComboBoxStyle.DropDownList;
        cbMesa.DisplayMember = "Nombre";
        lblClienteLabel.Text = "Cliente";
        lblClienteLabel.AutoSize = true;
        cbCliente.Width = 180;
        cbCliente.DropDownStyle = ComboBoxStyle.DropDownList;
        cbCliente.DisplayMember = "Nombre";
        btnConsultar.Text = "Consultar";
        btnConsultar.Size = new Size(110, 32);
        btnConsultar.Click += Consultar_Click;
        flpFiltros.Controls.AddRange(new Control[] { lblDesdeLabel, dtpInicio, lblHastaLabel, dtpFin, cbAgrupacion, lblMesaLabel, cbMesa, lblClienteLabel, cbCliente, btnConsultar });
        tabPestanas.SelectedIndexChanged += Pestana_Cambio;
        tabPestanas.Dock = DockStyle.Fill;
        tpVentasTab.Text = "Ventas facturadas";
        tpPlatosTab.Text = "Platos más vendidos";
        tpPedidosTab.Text = "Pedidos por mesa y cliente";
        dgvVentas.Dock = DockStyle.Fill;
        dgvPlatos.Dock = DockStyle.Fill;
        dgvPedidos.Dock = DockStyle.Fill;
        tpVentasTab.Controls.Add(dgvVentas);
        tpPlatosTab.Controls.Add(dgvPlatos);
        tpPedidosTab.Controls.Add(dgvPedidos);
        tabPestanas.TabPages.AddRange(new TabPage[] { tpVentasTab, tpPlatosTab, tpPedidosTab });
        lblTotal.Dock = DockStyle.Bottom;
        lblTotal.Height = 45;
        lblTotal.Text = "Seleccione un intervalo";
        Controls.Add(tabPestanas);
        Controls.Add(flpFiltros);
        Controls.Add(lblTotal);
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1100, 680);
        MinimumSize = new Size(900, 550);
        StartPosition = FormStartPosition.CenterParent;
        Text = "Reportes del restaurante";
        ((System.ComponentModel.ISupportInitialize)dgvVentas).EndInit();
        ((System.ComponentModel.ISupportInitialize)dgvPlatos).EndInit();
        ((System.ComponentModel.ISupportInitialize)dgvPedidos).EndInit();
        ResumeLayout(false);
        FormatoPresentacion.Aplicar(this);
    }
}
