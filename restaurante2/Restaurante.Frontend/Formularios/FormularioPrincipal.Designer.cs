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
        estructura.Dock = DockStyle.Fill;
        estructura.ColumnCount = 2;
        estructura.RowCount = 2;
        estructura.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 225F));
        estructura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        estructura.RowStyles.Add(new RowStyle(SizeType.Absolute, 55F));
        estructura.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        lateral.Dock = DockStyle.Fill;
        marca.Text = "S  sabores";
        marca.Font = new Font("Segoe UI",21F,FontStyle.Bold);
        marca.Dock = DockStyle.Top;
        marca.Height = 65;
        menu.Dock = DockStyle.Fill;
        menu.FlowDirection = FlowDirection.TopDown;
        menu.WrapContents = false;
        menu.AutoScroll = true;
        usuarioLabel.Dock = DockStyle.Bottom;
        usuarioLabel.Height = 45;
        salir.Text = "Cerrar sesión";
        Iconos.Aplicar(salir, "log-out");
        salir.Dock = DockStyle.Bottom;
        salir.Height = 42;
        salir.Click += Salir_Click;
        lateral.Controls.Add(menu);
        lateral.Controls.Add(marca);
        lateral.Controls.Add(usuarioLabel);
        lateral.Controls.Add(salir);
        _titulo.Dock = DockStyle.Fill;
        _titulo.TextAlign = ContentAlignment.MiddleLeft;
        _titulo.Padding = new Padding(20,0,0,0);
        _contenido.Dock = DockStyle.Fill;
        _contenido.AutoScroll = true;
        _contenido.Padding = new Padding(20);
        estructura.Controls.Add(lateral,0,0);
        estructura.SetRowSpan(lateral,2);
        estructura.Controls.Add(_titulo,1,0);
        estructura.Controls.Add(_contenido,1,1);
        Controls.Add(estructura);
        AutoScaleDimensions = new SizeF(7F,15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1260,800);
        MinimumSize = new Size(650,380);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Sabores | Gestión de restaurante";
        ResumeLayout(false);
        FormatoPresentacion.Aplicar(this);
    }
}
