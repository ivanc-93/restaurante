#nullable enable
namespace Restaurante.Frontend.Formularios;
partial class FormularioRecuperacion
{
    private System.ComponentModel.IContainer? components;
    private ListBox lstLista = null!;
    private TextBox ctDetalle = null!;
    private Button btnRecuperar = null!;
    private Button btnDescartar = null!;
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
        lstLista = new ListBox()
        {
            Name = "lstLista"
        };
        ctDetalle = new TextBox()
        {
            Name = "ctDetalle"
        };
        btnRecuperar = new Button()
        {
            Name = "btnRecuperar"
        };
        btnDescartar = new Button()
        {
            Name = "btnDescartar"
        };
        tlpEstructura = new TableLayoutPanel()
        {
            Name = "tlpEstructura"
        };
        SuspendLayout();
        tlpEstructura.Dock = DockStyle.Fill;
        tlpEstructura.Padding = new Padding(16);
        tlpEstructura.ColumnCount = 1;
        tlpEstructura.RowCount = 4;
        tlpEstructura.RowStyles.Add(new RowStyle(SizeType.Percent, 30));
        tlpEstructura.RowStyles.Add(new RowStyle(SizeType.Percent, 70));
        tlpEstructura.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tlpEstructura.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        lstLista.Dock = DockStyle.Fill;
        lstLista.DisplayMember = "Tipo";
        lstLista.SelectedIndexChanged += Seleccion_Cambio;
        ctDetalle.Dock = DockStyle.Fill;
        ctDetalle.Multiline = true;
        ctDetalle.ReadOnly = true;
        ctDetalle.ScrollBars = ScrollBars.Vertical;
        btnRecuperar.Text = "Recuperar la misma operación";
        btnRecuperar.AutoSize = true;
        btnRecuperar.Dock = DockStyle.Fill;
        btnRecuperar.Click += Recuperar_Click;
        btnDescartar.Text = "Abandonar solicitud sin aplicar (queda registrada en SQL)";
        btnDescartar.AutoSize = true;
        btnDescartar.Dock = DockStyle.Fill;
        btnDescartar.Click += Descartar_Click;
        tlpEstructura.Controls.Add(lstLista, 0, 0);
        tlpEstructura.Controls.Add(ctDetalle, 0, 1);
        tlpEstructura.Controls.Add(btnRecuperar, 0, 2);
        tlpEstructura.Controls.Add(btnDescartar, 0, 3);
        Controls.Add(tlpEstructura);
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(760, 580);
        MinimumSize = new Size(500, 380);
        StartPosition = FormStartPosition.CenterParent;
        Text = "Recuperación de operaciones locales";
        ResumeLayout(false);
    }
}
