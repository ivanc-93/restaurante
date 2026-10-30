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
    private TextBox buscarCatalogo = null!;
    private Label tituloCatalogo = null!, estadoEdicion = null!;
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
        Font=Componentes.Fuente;AutoScaleDimensions=new SizeF(96,96);AutoScaleMode=AutoScaleMode.Dpi;
        BackColor=Componentes.Fondo;
        layout.Dock=DockStyle.Fill; layout.Padding=new Padding(15); layout.ColumnCount=2; layout.RowCount=2;
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,65F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,35F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute,86F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent,100F));
        modo.DropDownStyle=ComboBoxStyle.DropDownList; modo.Items.AddRange(new object[]{"Clientes","Productos"});
        modo.Width=220; modo.SelectedIndexChanged+=Modo_Cambio;
        tabla.Dock=DockStyle.Fill; tabla.ReadOnly=true; tabla.AllowUserToAddRows=false; tabla.AutoGenerateColumns=true;
        campos.Dock=DockStyle.Fill; campos.FlowDirection=FlowDirection.TopDown; campos.WrapContents=false; campos.AutoScroll=true;
        campos.BackColor=Color.White;campos.Padding=new Padding(18);campos.Margin=new Padding(18,0,0,0);
        estadoEdicion=new Label{Text="Nuevo registro",AutoSize=true,Font=new Font("Segoe UI",18,FontStyle.Bold),Margin=new Padding(0,0,0,16)};
        nombreLabel.AutoSize=true; telefonoLabel.AutoSize=true; correoLabel.AutoSize=true; categoriaLabel.AutoSize=true; ubicacionLabel.AutoSize=true; tipoLabel.AutoSize=true; precioLabel.AutoSize=true;
        nombre.Width=260; nombre.MaxLength=120; nombreLabel.Text="Nombre (obligatorio)";
        telefono.Width=260; telefono.MaxLength=30; telefonoLabel.Text="Teléfono del cliente";
        correo.Width=260; correo.MaxLength=150; correoLabel.Text="Correo del cliente";
        categoria.Width=260; categoria.MaxLength=60; categoriaLabel.Text="Categoría del producto";
        ubicacion.Width=260; ubicacion.MaxLength=250; ubicacionLabel.Text="Ubicación del restaurante";
        tipo.Width=260; tipo.DropDownStyle=ComboBoxStyle.DropDownList; tipo.Items.AddRange(new object[]{"Plato","Bebida"}); tipoLabel.Text="Tipo de producto";
        precio.Width=260; precio.DecimalPlaces=2; precio.Maximum=99999999.99M; precioLabel.Text="Precio (cero = cortesía)";
        alcoholica.Text="Bebida alcohólica"; alcoholica.AutoSize=true;
        nuevo.Text="Nuevo / Limpiar"; nuevo.Size=new Size(260,48); nuevo.Click+=Nuevo_Click;
        editar.Text="Editar seleccionado"; editar.Size=new Size(260,32); editar.Click+=Seleccionar_Click;
        guardar.Text="Guardar cambios"; guardar.Size=new Size(260,48); guardar.Click+=Guardar_Click;
        desactivar.Text="Desactivar seleccionado"; desactivar.Size=new Size(260,32); desactivar.Click+=Desactivar_Click;
        foreach(var boton in new[]{nuevo,editar,guardar,desactivar}){Componentes.EstilizarBoton(boton,boton==guardar);boton.AutoSize=false;boton.Height=48;boton.Margin=new Padding(0,10,0,0);}
        desactivar.ForeColor=Color.FromArgb(151,45,43);Iconos.Aplicar(desactivar,"user-minus");Iconos.Aplicar(guardar,"save");Iconos.Aplicar(nuevo,"plus");Iconos.Aplicar(editar,"pencil");
        campos.Controls.AddRange(new Control[]{estadoEdicion,nombreLabel,nombre,telefonoLabel,telefono,correoLabel,correo,categoriaLabel,categoria,tipoLabel,tipo,precioLabel,precio,alcoholica,ubicacionLabel,ubicacion,guardar,nuevo,editar,desactivar});
        tituloCatalogo=new Label{Text="Clientes y productos",AutoSize=true,Font=new Font("Segoe UI",24,FontStyle.Bold),Margin=new Padding(0,0,24,0)};
        var cabecera=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoSize=true,WrapContents=true};cabecera.Controls.Add(tituloCatalogo);cabecera.Controls.Add(modo);
        buscarCatalogo=new TextBox{Name="buscarCatalogo",Dock=DockStyle.Fill,PlaceholderText="Buscar por nombre, teléfono o correo",AccessibleName="Buscar en el catálogo",Margin=new Padding(0,0,0,12)};
        buscarCatalogo.TextChanged+=(_,_)=>AplicarFiltro();
        var listado=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=2};listado.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));listado.RowStyles.Add(new RowStyle(SizeType.Absolute,50));listado.RowStyles.Add(new RowStyle(SizeType.Percent,100));listado.Controls.Add(buscarCatalogo,0,0);listado.Controls.Add(tabla,0,1);
        tabla.CellDoubleClick+=(_,_)=>Seleccionar_Click(this,EventArgs.Empty);
        tabla.DataBindingComplete+=(_,_)=>
        {
            if(tabla.Columns.Contains("Id"))tabla.Columns["Id"].Visible=false;
            if(tabla.Columns.Contains("Telefono"))tabla.Columns["Telefono"].HeaderText="Teléfono";
            foreach(DataGridViewRow fila in tabla.Rows)fila.MinimumHeight=46;
        };
        layout.Controls.Add(cabecera,0,0); layout.SetColumnSpan(cabecera,2);
        layout.Controls.Add(listado,0,1); layout.Controls.Add(campos,1,1);
        campos.SizeChanged+=(_,_)=>{var ancho=Math.Max(140,campos.ClientSize.Width-campos.Padding.Horizontal-SystemInformation.VerticalScrollBarWidth-8);foreach(Control campo in campos.Controls){if(campo is Label or CheckBox)campo.MaximumSize=new Size(ancho,0);else campo.Width=ancho;}};
        Controls.Add(layout);
        ClientSize=new Size(1180,720); MinimumSize=new Size(850,520); StartPosition=FormStartPosition.CenterParent;
        Text="Clientes, productos y restaurante";
        ((System.ComponentModel.ISupportInitialize)tabla).EndInit();
        ((System.ComponentModel.ISupportInitialize)precio).EndInit();
        ResumeLayout(false);
        FormatoPresentacion.Aplicar(this);
    }
}
