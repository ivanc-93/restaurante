#nullable enable
namespace Restaurante.Frontend.Formularios;
partial class FormularioDetalleMesa
{
    private System.ComponentModel.IContainer? components;
    private Label _resumen = null!;
    private Label _total = null!;
    private Label _estado = null!;
    private DataGridView _detalles = null!;
    private ComboBox _productos = null!;
    private ComboBox _clientes = null!;
    private ComboBox _categoria = null!;
    private ComboBox _tipo = null!;
    private TextBox _buscar = null!;
    private NumericUpDown _cantidad = null!;
    private Button _agregar = null!;
    private Button _facturar = null!;
    private FlowLayoutPanel _catalogo = null!;
    private TableLayoutPanel layout = null!;
    private TableLayoutPanel izquierda = null!;
    private TableLayoutPanel derecha = null!;
    private FlowLayoutPanel acciones = null!;
    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        _resumen = new Label(); _total = new Label(); _estado = new Label();
        _detalles = new DataGridView(); _productos = new ComboBox(); _cantidad = new NumericUpDown();
        _agregar = new Button(); _facturar = new Button(); _clientes = new ComboBox();
        _categoria = new ComboBox(); _tipo = new ComboBox(); _buscar = new TextBox();
        _catalogo = new FlowLayoutPanel(); layout = new TableLayoutPanel();
        izquierda = new TableLayoutPanel(); derecha = new TableLayoutPanel(); acciones = new FlowLayoutPanel();
        ((System.ComponentModel.ISupportInitialize)_detalles).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_cantidad).BeginInit(); SuspendLayout();
        layout.Dock = DockStyle.Fill; layout.Padding = new Padding(16); layout.ColumnCount = 2; layout.RowCount = 2;
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60)); layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _resumen.Text = "Cargando mesa…"; _resumen.AutoSize = true; _resumen.Margin = new Padding(0,0,0,16);
        _resumen.Font = new Font("Segoe UI", 15, FontStyle.Bold);
        layout.Controls.Add(_resumen,0,0); layout.SetColumnSpan(_resumen,2);
        izquierda.Dock = DockStyle.Fill; izquierda.ColumnCount = 1; izquierda.RowCount = 3; izquierda.Margin = new Padding(0,0,12,0);
        izquierda.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        izquierda.RowStyles.Add(new RowStyle(SizeType.AutoSize)); izquierda.RowStyles.Add(new RowStyle(SizeType.Percent,100)); izquierda.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var filtros = new FlowLayoutPanel { Dock=DockStyle.Fill, AutoSize=true, WrapContents=true };
        _buscar.PlaceholderText = "Buscar producto"; _buscar.Width = 180;
        _categoria.DropDownStyle=ComboBoxStyle.DropDownList; _categoria.Width=150;
        _tipo.DropDownStyle=ComboBoxStyle.DropDownList; _tipo.Width=110; _tipo.Items.AddRange(new object[]{"Todos","Plato","Bebida"}); _tipo.SelectedIndex=0;
        filtros.Controls.AddRange(new Control[]{_buscar,_categoria,_tipo});
        _buscar.TextChanged += Filtro_Cambio; _categoria.SelectedIndexChanged += Filtro_Cambio; _tipo.SelectedIndexChanged += Filtro_Cambio;
        _catalogo.Dock=DockStyle.Fill; _catalogo.AutoScroll=true; _catalogo.WrapContents=true; _catalogo.BackColor=Color.White;
        acciones.Dock=DockStyle.Fill; acciones.AutoSize=true; acciones.WrapContents=true;
        _productos.DropDownStyle=ComboBoxStyle.DropDownList; _productos.DisplayMember="Nombre"; _productos.Width=220;
        _cantidad.Minimum=1; _cantidad.Maximum=50; _cantidad.Value=1; _cantidad.Width=70; _cantidad.AccessibleName="Cantidad a agregar";
        _agregar.Text="Agregar producto"; _agregar.AutoSize=true; _agregar.Enabled=false; _agregar.Click+=Agregar_Click;
        Iconos.Aplicar(_agregar,"plus");
        acciones.Controls.AddRange(new Control[]{_productos,_cantidad,_agregar});
        izquierda.Controls.Add(filtros,0,0); izquierda.Controls.Add(_catalogo,0,1); izquierda.Controls.Add(acciones,0,2);
        derecha.Dock=DockStyle.Fill; derecha.ColumnCount=1; derecha.RowCount=6; derecha.BackColor=Color.White; derecha.Padding=new Padding(12);
        derecha.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        derecha.RowStyles.Add(new RowStyle(SizeType.AutoSize)); derecha.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        derecha.RowStyles.Add(new RowStyle(SizeType.Percent,100)); derecha.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        derecha.RowStyles.Add(new RowStyle(SizeType.AutoSize)); derecha.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var clienteLabel=new Label { Text="Cliente (antes del primer producto)", AutoSize=true };
        _clientes.DropDownStyle=ComboBoxStyle.DropDownList; _clientes.Dock=DockStyle.Fill; _clientes.AccessibleName="Cliente del pedido";
        _detalles.Dock=DockStyle.Fill; _detalles.AutoGenerateColumns=true; _detalles.ReadOnly=true; _detalles.AllowUserToAddRows=false;
        _estado.Text="Preparando catálogo…"; _estado.AutoSize=true; _estado.MaximumSize=new Size(360,0); _estado.ForeColor=Componentes.Secundario;
        _total.Text="Sin pedido"; _total.AutoSize=true; _total.Font=new Font("Segoe UI",18,FontStyle.Bold); _total.Margin=new Padding(0,12,0,12);
        _facturar.Text="Facturar y liberar mesa"; _facturar.Dock=DockStyle.Fill; _facturar.AutoSize=true; _facturar.Enabled=false; _facturar.Click+=Facturar_Click;
        _facturar.BackColor=Componentes.Verde; _facturar.ForeColor=Color.White; Iconos.Aplicar(_facturar,"receipt-text");
        derecha.Controls.Add(clienteLabel,0,0); derecha.Controls.Add(_clientes,0,1); derecha.Controls.Add(_detalles,0,2);
        derecha.Controls.Add(_estado,0,3); derecha.Controls.Add(_total,0,4); derecha.Controls.Add(_facturar,0,5);
        layout.Controls.Add(izquierda,0,1); layout.Controls.Add(derecha,1,1); Controls.Add(layout);
        AutoScaleDimensions=new SizeF(7F,15F); AutoScaleMode=AutoScaleMode.Font;
        ClientSize=new Size(1120,660); MinimumSize=new Size(680,400); StartPosition=FormStartPosition.CenterParent; Text="Mesa y pedido";
        ((System.ComponentModel.ISupportInitialize)_detalles).EndInit(); ((System.ComponentModel.ISupportInitialize)_cantidad).EndInit();
        ResumeLayout(false); FormatoPresentacion.Aplicar(this);
    }
}
