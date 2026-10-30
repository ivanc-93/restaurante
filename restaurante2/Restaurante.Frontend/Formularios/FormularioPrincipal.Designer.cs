#nullable enable
namespace Restaurante.Frontend.Formularios;
partial class FormularioPrincipal
{
    private System.ComponentModel.IContainer? components;
    private Panel _contenido = null!;
    private Label _titulo = null!;
    private FlowLayoutPanel menu = null!;
    private Label usuarioLabel = null!;
    private Button salir = null!;
    private TableLayoutPanel estructura = null!;
    private Panel lateral = null!;
    private Label marca = null!;
    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        _contenido = new Panel();
        _titulo = new Label();
        menu = new FlowLayoutPanel();
        usuarioLabel = new Label();
        salir = new Button();
        estructura = new TableLayoutPanel();
        lateral = new Panel();
        marca = new Label();
        SuspendLayout();
        Font = Componentes.Fuente;
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoScaleDimensions = new SizeF(96F,96F);
        estructura.Dock = DockStyle.Fill;
        estructura.ColumnCount = 2;
        estructura.RowCount = 2;
        estructura.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 245F));
        estructura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        estructura.RowStyles.Add(new RowStyle(SizeType.Absolute, 55F));
        estructura.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        lateral.Dock = DockStyle.Fill;
        lateral.BackColor = Componentes.Verde;
        lateral.Padding = new Padding(12,12,12,8);
        marca.Text = "sabores";
        marca.ForeColor = Color.White;
        marca.Font = new Font("Segoe UI",21F,FontStyle.Bold);
        marca.Dock = DockStyle.Top;
        marca.Height = 80;
        menu.Dock = DockStyle.Fill;
        menu.FlowDirection = FlowDirection.TopDown;
        menu.WrapContents = false;
        menu.AutoScroll = true;
        usuarioLabel.Dock = DockStyle.Bottom;
        usuarioLabel.Height = 60;
        usuarioLabel.ForeColor = Color.White;
        usuarioLabel.Font = new Font("Segoe UI",11);
        salir.Text = "Cerrar sesión";
        Iconos.Aplicar(salir, "log-out");
        salir.Dock = DockStyle.Bottom;
        salir.Height = 42;
        salir.BackColor=Componentes.Verde;salir.ForeColor=Color.White;salir.FlatStyle=FlatStyle.Flat;
        salir.FlatAppearance.BorderSize=0;salir.Font=new Font("Segoe UI",12);
        Iconos.Aplicar(salir,"log-out");
        salir.Click += Salir_Click;
        lateral.Controls.Add(menu);
        lateral.Controls.Add(marca);
        lateral.Controls.Add(usuarioLabel);
        lateral.Controls.Add(salir);
        _titulo.Dock = DockStyle.Fill;
        _titulo.TextAlign = ContentAlignment.MiddleLeft;
        _titulo.Padding = new Padding(20,0,0,0);
        _titulo.BackColor=Color.White;_titulo.ForeColor=Componentes.Secundario;
        _contenido.Dock = DockStyle.Fill;
        _contenido.AutoScroll = true;
        _contenido.Padding = new Padding(20);
        estructura.Controls.Add(lateral,0,0);
        estructura.SetRowSpan(lateral,2);
        estructura.Controls.Add(_titulo,1,0);
        estructura.Controls.Add(_contenido,1,1);
        Controls.Add(estructura);
        AutoScaleDimensions = new SizeF(96F,96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(1260,800);
        MinimumSize = new Size(650,380);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Sabores | Gestión de restaurante";
        ResumeLayout(false);
        FormatoPresentacion.Aplicar(this);
        menu.SizeChanged+=(_,_)=>{foreach(Control elemento in menu.Controls)elemento.Width=Math.Max(160,menu.ClientSize.Width-20);};
    }
}
