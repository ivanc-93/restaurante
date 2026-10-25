#nullable enable
namespace Restaurante.Frontend.Formularios;
partial class FormularioDetalleMesa
{
    private System.ComponentModel.IContainer? components;
    private Label _resumen = null!;
    private Label _total = null!;
    private DataGridView _detalles = null!;
    private ComboBox _productos = null!;
    private NumericUpDown _cantidad = null!;
    private Button _agregar = null!;
    private Button _facturar = null!;
    private TableLayoutPanel layout = null!;
    private FlowLayoutPanel acciones = null!;
    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        _resumen = new Label();
        _total = new Label();
        _detalles = new DataGridView();
        _productos = new ComboBox();
        _cantidad = new NumericUpDown();
        _agregar = new Button();
        _facturar = new Button();
        layout = new TableLayoutPanel();
        acciones = new FlowLayoutPanel();
        ((System.ComponentModel.ISupportInitialize)_detalles).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_cantidad).BeginInit();
        SuspendLayout();
        layout.Dock = DockStyle.Fill;
        layout.Padding = new Padding(20);
        layout.ColumnCount = 1;
        layout.RowCount = 4;
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));
        _resumen.Text = "Detalle de la mesa";
        _resumen.Dock = DockStyle.Fill;
        _detalles.Dock = DockStyle.Fill;
        _detalles.AutoGenerateColumns = true;
        _detalles.ReadOnly = true;
        _detalles.AllowUserToAddRows = false;
        acciones.Dock = DockStyle.Fill;
        acciones.AutoScroll = true;
        _productos.DropDownStyle = ComboBoxStyle.DropDownList;
        _productos.DisplayMember = "Nombre";
        _productos.Width = 260;
        _cantidad.Minimum = 1;
        _cantidad.Maximum = 50;
        _cantidad.Value = 1;
        _cantidad.Width = 75;
        _agregar.Enabled = false;
        _agregar.Text = "Agregar producto";
        _agregar.Size = new Size(145, 36);
        _agregar.Click += Agregar_Click;
        _facturar.Enabled = false;
        _facturar.Text = "Facturar y liberar mesa";
        _facturar.Size = new Size(195, 36);
        _facturar.Click += Facturar_Click;
        acciones.Controls.AddRange(new Control[] {_productos,_cantidad,_agregar,_facturar});
        _total.Dock = DockStyle.Fill;
        _total.Font = new Font("Georgia", 17F, FontStyle.Bold);
        layout.Controls.Add(_resumen,0,0);
        layout.Controls.Add(_detalles,0,1);
        layout.Controls.Add(acciones,0,2);
        layout.Controls.Add(_total,0,3);
        Controls.Add(layout);
        AutoScaleDimensions = new SizeF(7F,15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(940,600);
        MinimumSize = new Size(700,500);
        StartPosition = FormStartPosition.CenterParent;
        Text = "Detalle de mesa";
        ((System.ComponentModel.ISupportInitialize)_detalles).EndInit();
        ((System.ComponentModel.ISupportInitialize)_cantidad).EndInit();
        ResumeLayout(false);
    }
}
