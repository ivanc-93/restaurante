#nullable enable
namespace Restaurante.Frontend.Formularios;
partial class FormularioCatalogos
{
    private System.ComponentModel.IContainer? components;
    private ComboBox modo = null!;
    private DataGridView tabla = null!;
    private TextBox nombre = null!;
    private TextBox telefono = null!;
    private TextBox correo = null!;
    private TextBox categoria = null!;
    private TextBox ubicacion = null!;
    private ComboBox tipo = null!;
    private NumericUpDown precio = null!;
    private CheckBox alcoholica = null!;
    private Button nuevo = null!;
    private Button editar = null!;
    private Button guardar = null!;
    private Button desactivar = null!;
    private FlowLayoutPanel campos = null!;
    private TableLayoutPanel layout = null!;
    private Label nombreLabel = null!;
    private Label telefonoLabel = null!;
    private Label correoLabel = null!;
    private Label categoriaLabel = null!;
    private Label ubicacionLabel = null!;
    private Label tipoLabel = null!;
    private Label precioLabel = null!;
    protected override void Dispose(bool disposing)
    {
        if(disposing) components?.Dispose();
        base.Dispose(disposing);
    }
    private void InitializeComponent()
    {
        components=new System.ComponentModel.Container();
        modo=new ComboBox(); tabla=new DataGridView(); nombre=new TextBox(); telefono=new TextBox();
        correo=new TextBox(); categoria=new TextBox(); ubicacion=new TextBox(); tipo=new ComboBox();
        precio=new NumericUpDown(); alcoholica=new CheckBox(); nuevo=new Button(); editar=new Button();
        guardar=new Button(); desactivar=new Button(); campos=new FlowLayoutPanel(); layout=new TableLayoutPanel();
        nombreLabel=new Label(); telefonoLabel=new Label(); correoLabel=new Label(); categoriaLabel=new Label();
        ubicacionLabel=new Label(); tipoLabel=new Label(); precioLabel=new Label();
        ((System.ComponentModel.ISupportInitialize)tabla).BeginInit();
        ((System.ComponentModel.ISupportInitialize)precio).BeginInit();
        SuspendLayout();
        layout.Dock=DockStyle.Fill; layout.Padding=new Padding(15); layout.ColumnCount=2; layout.RowCount=2;
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,65F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,35F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute,45F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent,100F));
        modo.DropDownStyle=ComboBoxStyle.DropDownList; modo.Items.AddRange(new object[]{"Clientes","Productos"});
        modo.Width=250; modo.SelectedIndexChanged+=Modo_Cambio;
        tabla.Dock=DockStyle.Fill; tabla.ReadOnly=true; tabla.AllowUserToAddRows=false; tabla.AutoGenerateColumns=true;
        campos.Dock=DockStyle.Fill; campos.FlowDirection=FlowDirection.TopDown; campos.WrapContents=false; campos.AutoScroll=true;
        nombreLabel.AutoSize=true; telefonoLabel.AutoSize=true; correoLabel.AutoSize=true; categoriaLabel.AutoSize=true; ubicacionLabel.AutoSize=true; tipoLabel.AutoSize=true; precioLabel.AutoSize=true;
        nombre.Width=260; nombre.MaxLength=120; nombreLabel.Text="Nombre (obligatorio)";
        telefono.Width=260; telefono.MaxLength=30; telefonoLabel.Text="Teléfono del cliente";
        correo.Width=260; correo.MaxLength=150; correoLabel.Text="Correo del cliente";
        categoria.Width=260; categoria.MaxLength=60; categoriaLabel.Text="Categoría del producto";
        ubicacion.Width=260; ubicacion.MaxLength=250; ubicacionLabel.Text="Ubicación del restaurante";
        tipo.Width=260; tipo.DropDownStyle=ComboBoxStyle.DropDownList; tipo.Items.AddRange(new object[]{"Plato","Bebida"}); tipoLabel.Text="Tipo de producto";
        precio.Width=260; precio.DecimalPlaces=2; precio.Maximum=99999999.99M; precioLabel.Text="Precio (cero = cortesía)";
        alcoholica.Text="Bebida alcohólica"; alcoholica.AutoSize=true;
        nuevo.Text="Nuevo / Limpiar"; nuevo.Size=new Size(260,32); nuevo.Click+=Nuevo_Click;
        editar.Text="Editar seleccionado"; editar.Size=new Size(260,32); editar.Click+=Seleccionar_Click;
        guardar.Text="Guardar"; guardar.Size=new Size(260,36); guardar.Click+=Guardar_Click;
        desactivar.Text="Desactivar seleccionado"; desactivar.Size=new Size(260,32); desactivar.Click+=Desactivar_Click;
        campos.Controls.AddRange(new Control[]{nombreLabel,nombre,telefonoLabel,telefono,correoLabel,correo,categoriaLabel,categoria,tipoLabel,tipo,precioLabel,precio,alcoholica,ubicacionLabel,ubicacion,nuevo,editar,guardar,desactivar});
        layout.Controls.Add(modo,0,0); layout.SetColumnSpan(modo,2);
        layout.Controls.Add(tabla,0,1); layout.Controls.Add(campos,1,1);
        Controls.Add(layout); AutoScaleDimensions=new SizeF(7F,15F); AutoScaleMode=AutoScaleMode.Font;
        ClientSize=new Size(1040,780); MinimumSize=new Size(950,650); StartPosition=FormStartPosition.CenterParent;
        Text="Clientes, productos y restaurante";
        ((System.ComponentModel.ISupportInitialize)tabla).EndInit();
        ((System.ComponentModel.ISupportInitialize)precio).EndInit();
        ResumeLayout(false);
        FormatoPresentacion.Aplicar(this);
    }
}
