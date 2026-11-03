#nullable enable
namespace Restaurante.Frontend.Formularios;
partial class FormularioComprobante
{
    private System.ComponentModel.IContainer? components;
    private TextBox ctContenido = null!;
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        ctContenido = new TextBox()
        {
            Name = "ctContenido"
        };
        SuspendLayout();
        ctContenido.Dock = DockStyle.Fill;
        ctContenido.Multiline = true;
        ctContenido.ReadOnly = true;
        ctContenido.ScrollBars = ScrollBars.Both;
        ctContenido.BackColor = Color.White;
        ctContenido.Font = new Font("Segoe UI", 11);
        ctContenido.AccessibleName = "Comprobante interno académico";
        Controls.Add(ctContenido);
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(720, 650);
        MinimumSize = new Size(450, 350);
        StartPosition = FormStartPosition.CenterParent;
        Text = "Consultar comprobante interno";
        ResumeLayout(false);
    }
}
