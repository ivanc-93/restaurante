#nullable enable
namespace Restaurante.Frontend.Formularios;
partial class FormularioPrincipal
{
    private System.ComponentModel.IContainer? components;
    private Panel pnlContenido = null!;
    private Label lblTitulo = null!;
    private FlowLayoutPanel flpNavegacion = null!;
    private Label lblUsuarioLabel = null!;
    private Button btnCerrarSesion = null!;
    private TableLayoutPanel tlpEstructura = null!;
    private Panel pnlLateral = null!;
    private Label lblMarca = null!;
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        pnlContenido = new Panel()
        {
            Name = "pnlContenido"
        };
        lblTitulo = new Label()
        {
            Name = "lblTitulo"
        };
        flpNavegacion = new FlowLayoutPanel()
        {
            Name = "flpNavegacion"
        };
        lblUsuarioLabel = new Label()
        {
            Name = "lblUsuarioLabel"
        };
        btnCerrarSesion = new Button()
        {
            Name = "btnCerrarSesion"
        };
        tlpEstructura = new TableLayoutPanel()
        {
            Name = "tlpEstructura"
        };
        pnlLateral = new Panel()
        {
            Name = "pnlLateral"
        };
        lblMarca = new Label()
        {
            Name = "lblMarca"
        };
        SuspendLayout();
        Font = Componentes.Fuente;
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoScaleDimensions = new SizeF(96F, 96F);
        tlpEstructura.Dock = DockStyle.Fill;
        tlpEstructura.ColumnCount = 2;
        tlpEstructura.RowCount = 2;
        tlpEstructura.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 245F));
        tlpEstructura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpEstructura.RowStyles.Add(new RowStyle(SizeType.Absolute, 55F));
        tlpEstructura.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        pnlLateral.Dock = DockStyle.Fill;
        pnlLateral.BackColor = Componentes.Verde;
        pnlLateral.Padding = new Padding(12, 12, 12, 8);
        lblMarca.Text = "sabores";
        lblMarca.ForeColor = Color.White;
        lblMarca.Font = new Font("Segoe UI", 21F, FontStyle.Bold);
        lblMarca.Dock = DockStyle.Top;
        lblMarca.Height = 80;
        flpNavegacion.Dock = DockStyle.Fill;
        flpNavegacion.FlowDirection = FlowDirection.TopDown;
        flpNavegacion.WrapContents = false;
        flpNavegacion.AutoScroll = true;
        lblUsuarioLabel.Dock = DockStyle.Bottom;
        lblUsuarioLabel.Height = 60;
        lblUsuarioLabel.ForeColor = Color.White;
        lblUsuarioLabel.Font = new Font("Segoe UI", 11);
        btnCerrarSesion.Text = "Cerrar sesión";
        Iconos.Aplicar(btnCerrarSesion, "log-out");
        btnCerrarSesion.Dock = DockStyle.Bottom;
        btnCerrarSesion.Height = 42;
        btnCerrarSesion.BackColor = Componentes.Verde;
        btnCerrarSesion.ForeColor = Color.White;
        btnCerrarSesion.FlatStyle = FlatStyle.Flat;
        btnCerrarSesion.FlatAppearance.BorderSize = 0;
        btnCerrarSesion.Font = new Font("Segoe UI", 12);
        Iconos.Aplicar(btnCerrarSesion, "log-out");
        btnCerrarSesion.Click += Salir_Click;
        pnlLateral.Controls.Add(flpNavegacion);
        pnlLateral.Controls.Add(lblMarca);
        pnlLateral.Controls.Add(lblUsuarioLabel);
        pnlLateral.Controls.Add(btnCerrarSesion);
        lblTitulo.Dock = DockStyle.Fill;
        lblTitulo.TextAlign = ContentAlignment.MiddleLeft;
        lblTitulo.Padding = new Padding(20, 0, 0, 0);
        lblTitulo.BackColor = Color.White;
        lblTitulo.ForeColor = Componentes.Secundario;
        pnlContenido.Dock = DockStyle.Fill;
        pnlContenido.AutoScroll = true;
        pnlContenido.Padding = new Padding(20);
        tlpEstructura.Controls.Add(pnlLateral, 0, 0);
        tlpEstructura.SetRowSpan(pnlLateral, 2);
        tlpEstructura.Controls.Add(lblTitulo, 1, 0);
        tlpEstructura.Controls.Add(pnlContenido, 1, 1);
        Controls.Add(tlpEstructura);
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(1260, 800);
        MinimumSize = new Size(650, 380);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Sabores | Gestión de restaurante";
        ResumeLayout(false);
        FormatoPresentacion.Aplicar(this);
        flpNavegacion.SizeChanged += (_, _) =>
        {
            foreach (Control elemento in flpNavegacion.Controls)
                elemento.Width = Math.Max(160, flpNavegacion.ClientSize.Width - 20);
        };
    }
}
