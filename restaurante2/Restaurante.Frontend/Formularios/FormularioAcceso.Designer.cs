#nullable enable
namespace Restaurante.Frontend.Formularios;

partial class FormularioAcceso
{
    private System.ComponentModel.IContainer? components;
    private TextBox _usuario = null!;
    private TextBox _contrasena = null!;
    private Label _mensaje = null!;
    private Button entrar = null!;
    private Label titulo = null!;
    private Label usuarioEtiqueta = null!;
    private Label claveEtiqueta = null!;
    private Label ayuda = null!;
    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        _usuario = new TextBox();
        _contrasena = new TextBox();
        _mensaje = new Label();
        entrar = new Button();
        titulo = new Label();
        usuarioEtiqueta = new Label();
        claveEtiqueta = new Label();
        ayuda = new Label();
        SuspendLayout();
        titulo.Text = "Sabores | Iniciar sesión";
        titulo.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
        titulo.SetBounds(32, 25, 480, 45);
        usuarioEtiqueta.Text = "Usuario";
        usuarioEtiqueta.SetBounds(35, 90, 420, 24);
        _usuario.SetBounds(35, 116, 420, 30);
        _usuario.Text = "";
        _usuario.MaxLength = 50;
        claveEtiqueta.Text = "Contraseña";
        claveEtiqueta.SetBounds(35, 156, 420, 24);
        _contrasena.SetBounds(35, 182, 420, 30);
        _contrasena.UseSystemPasswordChar = true;
        _mensaje.SetBounds(35, 222, 420, 55);
        _mensaje.ForeColor = Color.Firebrick;
        entrar.Text = "Entrar al sistema";
        entrar.SetBounds(35, 282, 420, 42);
        entrar.BackColor = Color.FromArgb(49, 86, 62);
        entrar.ForeColor = Color.White;
        entrar.Click += Entrar_Click;
        ayuda.Text = "Utilice su cuenta individual de Sabores.";
        ayuda.SetBounds(35, 340, 420, 55);
        Controls.AddRange(new Control[] { titulo, usuarioEtiqueta, _usuario, claveEtiqueta, _contrasena, _mensaje, entrar, ayuda });
        AcceptButton = entrar;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(500, 420);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Sabores | Acceso";
        ResumeLayout(false);
        FormatoPresentacion.Aplicar(this);
        PerformLayout();
    }
}
