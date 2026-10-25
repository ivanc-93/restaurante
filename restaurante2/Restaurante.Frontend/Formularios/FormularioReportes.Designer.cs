#nullable enable
namespace Restaurante.Frontend.Formularios;
partial class FormularioReportes
{
    private System.ComponentModel.IContainer? components;
    private DateTimePicker inicio=null!;
    private DateTimePicker fin=null!;
    private ComboBox agrupacion=null!;
    private ComboBox mesa=null!;
    private ComboBox cliente=null!;
    private Button consultar=null!;
    private DataGridView ventas=null!;
    private DataGridView platos=null!;
    private DataGridView pedidos=null!;
    private FlowLayoutPanel filtros=null!;
    private TabControl pestanas=null!;
    private TabPage ventasTab=null!;
    private TabPage platosTab=null!;
    private TabPage pedidosTab=null!;
    private Label total=null!;
    private Label desdeLabel=null!;
    private Label hastaLabel=null!;
    private Label mesaLabel=null!;
    private Label clienteLabel=null!;
    protected override void Dispose(bool disposing)
    {
        if(disposing) components?.Dispose();
        base.Dispose(disposing);
    }
    private void InitializeComponent()
    {
        components=new System.ComponentModel.Container(); inicio=new DateTimePicker(); fin=new DateTimePicker();
        agrupacion=new ComboBox(); mesa=new ComboBox(); cliente=new ComboBox(); consultar=new Button();
        ventas=new DataGridView(); platos=new DataGridView(); pedidos=new DataGridView(); filtros=new FlowLayoutPanel();
        pestanas=new TabControl(); ventasTab=new TabPage(); platosTab=new TabPage(); pedidosTab=new TabPage(); total=new Label();
        desdeLabel=new Label(); hastaLabel=new Label(); mesaLabel=new Label(); clienteLabel=new Label();
        ((System.ComponentModel.ISupportInitialize)ventas).BeginInit(); ((System.ComponentModel.ISupportInitialize)platos).BeginInit(); ((System.ComponentModel.ISupportInitialize)pedidos).BeginInit();
        SuspendLayout();
        filtros.Dock=DockStyle.Top; filtros.Height=110; filtros.AutoScroll=true;
        desdeLabel.Text="Desde"; desdeLabel.AutoSize=true; hastaLabel.Text="Hasta"; hastaLabel.AutoSize=true;
        inicio.Format=DateTimePickerFormat.Short; inicio.Width=120; fin.Format=DateTimePickerFormat.Short; fin.Width=120;
        agrupacion.DropDownStyle=ComboBoxStyle.DropDownList; agrupacion.Items.AddRange(new object[]{"Día","Semana","Mes"}); agrupacion.Width=100;
        agrupacion.SelectedIndex=0;
        mesaLabel.Text="Mesa"; mesaLabel.AutoSize=true; mesa.Width=130; mesa.DropDownStyle=ComboBoxStyle.DropDownList; mesa.DisplayMember="Nombre";
        clienteLabel.Text="Cliente"; clienteLabel.AutoSize=true;
        cliente.Width=180; cliente.DropDownStyle=ComboBoxStyle.DropDownList; cliente.DisplayMember="Nombre";
        consultar.Text="Consultar"; consultar.Size=new Size(110,32); consultar.Click+=Consultar_Click;
        inicio.ValueChanged+=Consultar_Click; fin.ValueChanged+=Consultar_Click; agrupacion.SelectedIndexChanged+=Consultar_Click;
        mesa.SelectedIndexChanged+=Consultar_Click; cliente.SelectedIndexChanged+=Consultar_Click;
        filtros.Controls.AddRange(new Control[]{desdeLabel,inicio,hastaLabel,fin,agrupacion,mesaLabel,mesa,clienteLabel,cliente,consultar});
        pestanas.Dock=DockStyle.Fill; ventasTab.Text="Ventas facturadas"; platosTab.Text="Platos más vendidos"; pedidosTab.Text="Pedidos por mesa y cliente";
        ventas.Dock=DockStyle.Fill; platos.Dock=DockStyle.Fill; pedidos.Dock=DockStyle.Fill;
        ventasTab.Controls.Add(ventas); platosTab.Controls.Add(platos); pedidosTab.Controls.Add(pedidos);
        pestanas.TabPages.AddRange(new TabPage[]{ventasTab,platosTab,pedidosTab});
        total.Dock=DockStyle.Bottom; total.Height=45; total.Text="Seleccione un intervalo";
        Controls.Add(pestanas); Controls.Add(filtros); Controls.Add(total);
        AutoScaleDimensions=new SizeF(7F,15F); AutoScaleMode=AutoScaleMode.Font;
        ClientSize=new Size(1100,680); MinimumSize=new Size(900,550); StartPosition=FormStartPosition.CenterParent;
        Text="Reportes del restaurante";
        ((System.ComponentModel.ISupportInitialize)ventas).EndInit(); ((System.ComponentModel.ISupportInitialize)platos).EndInit(); ((System.ComponentModel.ISupportInitialize)pedidos).EndInit();
        ResumeLayout(false);
    }
}
