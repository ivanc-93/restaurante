#nullable enable
namespace Restaurante.Frontend.Formularios;
partial class FormularioAcceso
{
    private System.ComponentModel.IContainer? components;
    private TextBox ctUsuario = null!, ctContrasena = null!;
    private Label lblMensaje = null!, lblTitulo = null!, lblUsuarioEtiqueta = null!, lblClaveEtiqueta = null!, lblAyuda = null!;
    private Button btnEntrarSistema = null!, btnConfigurarConexion = null!;
    private CheckBox chkMostrarContrasena = null!;
    private TableLayoutPanel tlpEstructura = null!;
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        SuspendLayout();
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = new Font("Segoe UI", 10F);
        BackColor = Color.FromArgb(246, 247, 243);
        Text = "Sabores | Acceso";
        ClientSize = new Size(480, 610);
        MinimumSize = new Size(360, 360);
        StartPosition = FormStartPosition.CenterScreen;
        tlpEstructura = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            Padding = new Padding(24, 12, 24, 0),
            AutoScroll = true,
            Name = "tlpEstructura"
        };
        tlpEstructura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (var i = 0; i < 6; i++)
            tlpEstructura.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        lblAyuda = new Label
        {
            Text = "SABORES  ·  Gestión de restaurante",
            AutoSize = true,
            ForeColor = Color.FromArgb(49, 86, 62),
            Margin = new Padding(0, 0, 0, 6),
            Name = "lblAyuda"
        };
        lblTitulo = new Label
        {
            Text = "Iniciar sesión",
            AutoSize = true,
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 4),
            Name = "lblTitulo"
        };
        lblUsuarioEtiqueta = new Label
        {
            Text = "Usuario",
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 6),
            Name = "lblUsuarioEtiqueta"
        };
        ctUsuario = new TextBox
        {
            Dock = DockStyle.Top,
            MaxLength = 50,
            TabIndex = 0,
            AccessibleName = "Usuario de Sabores",
            Margin = new Padding(0, 0, 0, 6),
            Name = "ctUsuario"
        };
        lblClaveEtiqueta = new Label
        {
            Text = "Contraseña",
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 6),
            Name = "lblClaveEtiqueta"
        };
        ctContrasena = new TextBox
        {
            Dock = DockStyle.Top,
            MaxLength = 128,
            UseSystemPasswordChar = true,
            TabIndex = 1,
            AccessibleName = "Contraseña de Sabores",
            Margin = new Padding(0, 0, 0, 4),
            Name = "ctContrasena"
        };
        chkMostrarContrasena = new CheckBox
        {
            Text = "Mostrar contraseña",
            AutoSize = true,
            TabIndex = 2,
            Margin = new Padding(0, 0, 0, 4),
            Name = "chkMostrarContrasena"
        };
        chkMostrarContrasena.CheckedChanged += (_, _) => ctContrasena.UseSystemPasswordChar = !chkMostrarContrasena.Checked;
        lblMensaje = new Label
        {
            Text = "Usa tu cuenta individual para continuar.",
            AutoSize = true,
            MinimumSize = new Size(0, 24),
            ForeColor = Color.FromArgb(92, 103, 98),
            Margin = new Padding(0, 4, 0, 10),
            Name = "lblMensaje"
        };
        btnEntrarSistema = new Button
        {
            Text = "Entrar al sistema",
            AutoSize = true,
            Dock = DockStyle.Top,
            MinimumSize = new Size(0, 36),
            BackColor = Color.FromArgb(49, 86, 62),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            TabIndex = 3,
            Margin = new Padding(0, 0, 0, 6),
            Name = "btnEntrarSistema"
        };
        btnEntrarSistema.Click += Entrar_Click;
        btnConfigurarConexion = new Button
        {
            Text = "Configurar o recuperar conexión",
            AutoSize = true,
            Dock = DockStyle.Top,
            MinimumSize = new Size(0, 36),
            BackColor = Color.White,
            ForeColor = Color.FromArgb(49, 86, 62),
            FlatStyle = FlatStyle.Flat,
            TabIndex = 4,
            Margin = new Padding(0, 0, 0, 4),
            Name = "btnConfigurarConexion"
        };
        btnConfigurarConexion.Click += Conexion_Click;
        var tlpCabeceraClave = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 0, 0, 4),
            TabIndex = 2,
            Name = "tlpCabeceraClave"
        };
        tlpCabeceraClave.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        tlpCabeceraClave.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        tlpCabeceraClave.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        chkMostrarContrasena.Margin = new Padding(0);
        lblClaveEtiqueta.Margin = new Padding(0);
        tlpCabeceraClave.Controls.Add(lblClaveEtiqueta, 0, 0);
        tlpCabeceraClave.Controls.Add(chkMostrarContrasena, 1, 0);
        Control[] controles = [lblAyuda, lblTitulo, lblUsuarioEtiqueta, ctUsuario, tlpCabeceraClave, ctContrasena];
        for (var i = 0; i < controles.Length; i++)
            tlpEstructura.Controls.Add(controles[i], 0, i);
        var tlpPie = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(24, 8, 24, 12),
            Name = "tlpPie"
        };
        tlpPie.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        var lnkRecuperar = new LinkLabel
        {
            Text = "¿Olvidaste tu contraseña?",
            AutoSize = true,
            TabIndex = 4,
            Margin = new Padding(0, 6, 0, 10),
            LinkColor = Color.FromArgb(49, 86, 62),
            Name = "lnkRecuperar"
        };
        lnkRecuperar.LinkClicked += RecuperarClave_Click;
        btnConfigurarConexion.TabIndex = 5;
        tlpPie.Controls.Add(lblMensaje, 0, 0);
        tlpPie.Controls.Add(btnEntrarSistema, 0, 1);
        tlpPie.Controls.Add(lnkRecuperar, 0, 2);
        tlpPie.Controls.Add(btnConfigurarConexion, 0, 3);
        tlpPie.SizeChanged += (_, _) => lblMensaje.MaximumSize = new Size(Math.Max(180, tlpPie.ClientSize.Width - tlpPie.Padding.Horizontal), 0);
        var tlpContenedor = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Name = "tlpContenedor"
        };
        tlpContenedor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        tlpContenedor.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        tlpContenedor.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tlpContenedor.Controls.Add(tlpEstructura, 0, 0);
        tlpContenedor.Controls.Add(tlpPie, 0, 1);
        tlpEstructura.SizeChanged += (_, _) =>
        {
            var ancho = Math.Max(180, tlpEstructura.ClientSize.Width - tlpEstructura.Padding.Horizontal - SystemInformation.VerticalScrollBarWidth);
            foreach (var label in tlpEstructura.Controls.OfType<Label>())
                label.MaximumSize = new Size(ancho, 0);
        };
        Controls.Add(tlpContenedor);
        AcceptButton = btnEntrarSistema;
        ResumeLayout(true);
    }
}
