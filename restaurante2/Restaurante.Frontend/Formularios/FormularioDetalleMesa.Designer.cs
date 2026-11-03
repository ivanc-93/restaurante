#nullable enable
namespace Restaurante.Frontend.Formularios;
partial class FormularioDetalleMesa
{
    private System.ComponentModel.IContainer? components;
    private Label lblResumen = null!;
    private Label lblTotal = null!;
    private Label lblEstado = null!;
    private DataGridView dgvDetalles = null!;
    private ComboBox cbProductos = null!;
    private ComboBox cbClientes = null!;
    private ComboBox cbCategoria = null!;
    private ComboBox cbTipo = null!;
    private TextBox ctBuscar = null!;
    private NumericUpDown nudCantidad = null!;
    private Button btnAgregarProducto = null!;
    private Button btnGestionar = null!;
    private Button btnRevisarCobro = null!;
    private FlowLayoutPanel flpCatalogo = null!;
    private TableLayoutPanel tlpLayout = null!;
    private TableLayoutPanel tlpIzquierda = null!;
    private TableLayoutPanel tlpDerecha = null!;
    private FlowLayoutPanel flpAcciones = null!;
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        lblResumen = new Label()
        {
            Name = "lblResumen"
        };
        lblTotal = new Label()
        {
            Name = "lblTotal"
        };
        lblEstado = new Label()
        {
            Name = "lblEstado"
        };
        dgvDetalles = new DataGridView()
        {
            Name = "dgvDetalles"
        };
        cbProductos = new ComboBox()
        {
            Name = "cbProductos"
        };
        nudCantidad = new NumericUpDown()
        {
            Name = "nudCantidad"
        };
        btnAgregarProducto = new Button()
        {
            Name = "btnAgregarProducto"
        };
        btnRevisarCobro = new Button()
        {
            Name = "btnRevisarCobro"
        };
        btnGestionar = new Button()
        {
            Name = "btnGestionar"
        };
        cbClientes = new ComboBox()
        {
            Name = "cbClientes"
        };
        cbCategoria = new ComboBox()
        {
            Name = "cbCategoria"
        };
        cbTipo = new ComboBox()
        {
            Name = "cbTipo"
        };
        ctBuscar = new TextBox()
        {
            Name = "ctBuscar"
        };
        flpCatalogo = new FlowLayoutPanel()
        {
            Name = "flpCatalogo"
        };
        tlpLayout = new TableLayoutPanel()
        {
            Name = "tlpLayout"
        };
        tlpIzquierda = new TableLayoutPanel()
        {
            Name = "tlpIzquierda"
        };
        tlpDerecha = new TableLayoutPanel()
        {
            Name = "tlpDerecha"
        };
        flpAcciones = new FlowLayoutPanel()
        {
            Name = "flpAcciones"
        };
        ((System.ComponentModel.ISupportInitialize)dgvDetalles).BeginInit();
        ((System.ComponentModel.ISupportInitialize)nudCantidad).BeginInit();
        SuspendLayout();
        Font = Componentes.Fuente;
        AutoScaleDimensions = new SizeF(96, 96);
        AutoScaleMode = AutoScaleMode.Dpi;
        tlpLayout.Dock = DockStyle.Fill;
        tlpLayout.Padding = new Padding(16);
        tlpLayout.ColumnCount = 2;
        tlpLayout.RowCount = 2;
        tlpLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        tlpLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        tlpLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tlpLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        lblResumen.Text = "Cargando mesa…";
        lblResumen.AutoSize = true;
        lblResumen.Margin = new Padding(0, 0, 0, 16);
        lblResumen.Font = new Font("Segoe UI", 15, FontStyle.Bold);
        tlpLayout.Controls.Add(lblResumen, 0, 0);
        tlpLayout.SetColumnSpan(lblResumen, 2);
        tlpIzquierda.Dock = DockStyle.Fill;
        tlpIzquierda.ColumnCount = 1;
        tlpIzquierda.RowCount = 3;
        tlpIzquierda.Margin = new Padding(0, 0, 12, 0);
        tlpIzquierda.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        tlpIzquierda.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tlpIzquierda.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        tlpIzquierda.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var flpFiltros = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = true,
            Name = "flpFiltros"
        };
        ctBuscar.PlaceholderText = "Buscar producto";
        ctBuscar.Width = 180;
        cbCategoria.DropDownStyle = ComboBoxStyle.DropDownList;
        cbCategoria.Width = 150;
        cbTipo.DropDownStyle = ComboBoxStyle.DropDownList;
        cbTipo.Width = 110;
        cbTipo.Items.AddRange(new object[] { "Todos", "Plato", "Bebida" });
        cbTipo.SelectedIndex = 0;
        flpFiltros.Controls.AddRange(new Control[] { ctBuscar, cbCategoria, cbTipo });
        ctBuscar.TextChanged += Filtro_Cambio;
        cbCategoria.SelectedIndexChanged += Filtro_Cambio;
        cbTipo.SelectedIndexChanged += Filtro_Cambio;
        flpCatalogo.Dock = DockStyle.Fill;
        flpCatalogo.AutoScroll = true;
        flpCatalogo.WrapContents = true;
        flpCatalogo.BackColor = Color.White;
        flpAcciones.Dock = DockStyle.Fill;
        flpAcciones.AutoSize = true;
        flpAcciones.WrapContents = true;
        cbProductos.DropDownStyle = ComboBoxStyle.DropDownList;
        cbProductos.DisplayMember = "Nombre";
        cbProductos.Width = 220;
        nudCantidad.Minimum = 1;
        nudCantidad.Maximum = 50;
        nudCantidad.Value = 1;
        nudCantidad.Width = 70;
        nudCantidad.AccessibleName = "Cantidad a agregar";
        btnAgregarProducto.Text = "Agregar producto";
        btnAgregarProducto.AutoSize = true;
        btnAgregarProducto.Enabled = false;
        btnAgregarProducto.Click += Agregar_Click;
        Iconos.Aplicar(btnAgregarProducto, "plus");
        flpAcciones.Controls.AddRange(new Control[] { cbProductos, nudCantidad, btnAgregarProducto });
        tlpIzquierda.Controls.Add(flpFiltros, 0, 0);
        tlpIzquierda.Controls.Add(flpCatalogo, 0, 1);
        tlpIzquierda.Controls.Add(flpAcciones, 0, 2);
        tlpDerecha.Dock = DockStyle.Fill;
        tlpDerecha.ColumnCount = 1;
        tlpDerecha.RowCount = 7;
        tlpDerecha.BackColor = Color.White;
        tlpDerecha.Padding = new Padding(12);
        tlpDerecha.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        tlpDerecha.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tlpDerecha.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tlpDerecha.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        tlpDerecha.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tlpDerecha.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tlpDerecha.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tlpDerecha.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var lblClienteLabel = new Label
        {
            Text = "Cliente (antes del primer producto)",
            AutoSize = true,
            Name = "lblClienteLabel"
        };
        cbClientes.DropDownStyle = ComboBoxStyle.DropDownList;
        cbClientes.Dock = DockStyle.Fill;
        cbClientes.AccessibleName = "Cliente del pedido";
        dgvDetalles.Dock = DockStyle.Fill;
        dgvDetalles.AutoGenerateColumns = true;
        dgvDetalles.ReadOnly = true;
        dgvDetalles.AllowUserToAddRows = false;
        lblEstado.Text = "Preparando catálogo…";
        lblEstado.AutoSize = true;
        lblEstado.MaximumSize = new Size(360, 0);
        lblEstado.ForeColor = Componentes.Secundario;
        lblTotal.Text = "Sin pedido";
        lblTotal.AutoSize = true;
        lblTotal.Font = new Font("Segoe UI", 18, FontStyle.Bold);
        lblTotal.Margin = new Padding(0, 12, 0, 12);
        btnRevisarCobro.Text = "Revisar y cobrar";
        btnRevisarCobro.Dock = DockStyle.Fill;
        btnRevisarCobro.AutoSize = true;
        btnRevisarCobro.Enabled = false;
        btnRevisarCobro.Click += Facturar_Click;
        btnRevisarCobro.BackColor = Componentes.Verde;
        btnRevisarCobro.ForeColor = Color.White;
        Iconos.Aplicar(btnRevisarCobro, "receipt-text");
        tlpDerecha.Controls.Add(lblClienteLabel, 0, 0);
        tlpDerecha.Controls.Add(cbClientes, 0, 1);
        tlpDerecha.Controls.Add(dgvDetalles, 0, 2);
        btnGestionar.Text = "Gestionar cierre de mesa";
        btnGestionar.AutoSize = true;
        btnGestionar.Dock = DockStyle.Fill;
        btnGestionar.Enabled = false;
        btnGestionar.Click += Gestionar_Click;
        var flpGestionAcciones = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = true,
            Name = "flpGestionAcciones"
        };
        var btnCorregir = new Button
        {
            Text = "Corregir unidades",
            AutoSize = true,
            Name = "btnCorregir"
        };
        btnCorregir.Click += Corregir_Click;
        var btnPreparar = new Button
        {
            Text = "Declarar preparación",
            AutoSize = true,
            Name = "btnPreparar"
        };
        btnPreparar.Click += Preparar_Click;
        flpGestionAcciones.Controls.AddRange(new Control[] { btnGestionar, btnCorregir, btnPreparar });
        tlpDerecha.Controls.Add(flpGestionAcciones, 0, 6);
        tlpDerecha.Controls.Add(lblEstado, 0, 3);
        tlpDerecha.Controls.Add(lblTotal, 0, 4);
        tlpDerecha.Controls.Add(btnRevisarCobro, 0, 5);
        tlpLayout.Controls.Add(tlpIzquierda, 0, 1);
        tlpLayout.Controls.Add(tlpDerecha, 1, 1);
        Controls.Add(tlpLayout);
        AutoScaleDimensions = new SizeF(96, 96);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(1220, 730);
        MinimumSize = new Size(850, 520);
        StartPosition = FormStartPosition.CenterParent;
        Text = "Sabores | Mesa y pedido";
        ((System.ComponentModel.ISupportInitialize)dgvDetalles).EndInit();
        ((System.ComponentModel.ISupportInitialize)nudCantidad).EndInit();
        ResumeLayout(false);
        FormatoPresentacion.Aplicar(this);
    }
}
