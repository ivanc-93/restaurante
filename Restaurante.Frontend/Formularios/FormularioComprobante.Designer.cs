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
        ctContenido.TabStop = false;
        ctContenido.ScrollBars = ScrollBars.Vertical;
        ctContenido.WordWrap = true;
        ctContenido.BorderStyle = BorderStyle.None;
        ctContenido.BackColor = Color.White;
        ctContenido.Font = new Font("Consolas", 11);
        ctContenido.AccessibleName = "Comprobante interno académico";
        var raiz = new TableLayoutPanel
        {
            Name = "tlpDocumento",
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(20),
            BackColor = Componentes.Fondo
        };
        raiz.ColumnStyles.Add(new(SizeType.Percent, 100));
        raiz.RowStyles.Add(new(SizeType.AutoSize));
        raiz.RowStyles.Add(new(SizeType.Percent, 100));
        raiz.RowStyles.Add(new(SizeType.AutoSize));
        var titulo = Componentes.Etiqueta("SABORES · Comprobante académico · GTQ", 16, true);
        titulo.ForeColor = Componentes.Verde;
        titulo.Dock = DockStyle.Top;
        titulo.Margin = new Padding(0, 0, 0, 15);
        raiz.Controls.Add(titulo, 0, 0);
        var papel = new Panel
        {
            Name = "pnlDocumento",
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(16)
        };
        papel.Controls.Add(ctContenido);
        raiz.Controls.Add(papel, 0, 1);
        var pie = new FlowLayoutPanel
        {
            Name = "flpDocumento",
            Dock = DockStyle.Top,
            AutoSize = true
        };
        var copiar = Componentes.Boton("Copiar contenido", false);
        copiar.Name = "btnCopiarComprobante";
        copiar.Click += (_, _) =>
        {
            if (!string.IsNullOrWhiteSpace(ctContenido.Text))
                Clipboard.SetText(ctContenido.Text);
        };
        var volver = Componentes.Boton("Volver", false);
        volver.Name = "btnVolver";
        volver.Click += (_, _) => Close();
        pie.Controls.AddRange([copiar, volver]);
        raiz.Controls.Add(pie, 0, 2);
        Controls.Add(raiz);
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(720, 650);
        MinimumSize = new Size(450, 350);
        StartPosition = FormStartPosition.CenterParent;
        Text = "Consultar comprobante interno";
        ResumeLayout(false);
    }
}
