#nullable enable
namespace Restaurante.Frontend.Formularios;
partial class FormularioDivision
{
    private Label lblResumen = null!, lblMensaje = null!;
    private NumericUpDown nudPersonas = null!;
    private ComboBox cbModo = null!;
    private DataGridView dgvPartes = null!, dgvUnidades = null!;
    private Button btnGuardar = null!;
    private void InitializeComponent()
    {
        SuspendLayout();
        Font = Componentes.Fuente;
        AutoScaleDimensions = new SizeF(96, 96);
        AutoScaleMode = AutoScaleMode.Dpi;
        Text = "Sabores | Separar cuentas";
        ClientSize = new Size(900, 650);
        MinimumSize = new Size(650, 450);
        StartPosition = FormStartPosition.CenterParent;
        var raiz = new TableLayoutPanel
        {
            Name = "tlpDivision",
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            ColumnCount = 1,
            RowCount = 6
        };
        raiz.ColumnStyles.Add(new(SizeType.Percent, 100));
        foreach (var tipo in new[]
        {
            SizeType.AutoSize,
            SizeType.AutoSize,
            SizeType.Percent,
            SizeType.Percent,
            SizeType.AutoSize,
            SizeType.AutoSize
        }

        )
            raiz.RowStyles.Add(new(tipo, tipo == SizeType.Percent ? 50 : 0));
        lblResumen = Componentes.Etiqueta("Separar antes de cobrar");
        lblResumen.Name = nameof(lblResumen);
        lblResumen.Dock = DockStyle.Top;
        raiz.Controls.Add(lblResumen, 0, 0);
        var opciones = new FlowLayoutPanel
        {
            Name = "flpOpciones",
            AutoSize = true,
            Dock = DockStyle.Top
        };
        opciones.Controls.Add(Componentes.Etiqueta("Número de cuentas"));
        nudPersonas = new()
        {
            Name = nameof(nudPersonas),
            Minimum = 2,
            Maximum = 20,
            Value = 2,
            Width = 70
        };
        nudPersonas.ValueChanged += (_, _) => CrearPartes();
        opciones.Controls.Add(nudPersonas);
        cbModo = new()
        {
            Name = nameof(cbModo),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 300
        };
        cbModo.Items.AddRange(["Por importes", "Por unidades de productos"]);
        cbModo.SelectedIndex = 0;
        opciones.Controls.Add(cbModo);
        raiz.Controls.Add(opciones, 0, 1);
        dgvPartes = new()
        {
            Name = nameof(dgvPartes),
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        dgvUnidades = new()
        {
            Name = nameof(dgvUnidades),
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        raiz.Controls.Add(dgvPartes, 0, 2);
        raiz.Controls.Add(dgvUnidades, 0, 3);
        cbModo.SelectedIndexChanged += (_, _) =>
        {
            dgvUnidades.Visible = cbModo.SelectedIndex == 1;
            raiz.RowStyles[2].Height = cbModo.SelectedIndex == 1 ? 50 : 100;
            raiz.RowStyles[3].Height = cbModo.SelectedIndex == 1 ? 50 : 0;
            dgvPartes.Columns.Cast<DataGridViewColumn>().Where(c => c.Name == "Original").ToList().ForEach(c => c.ReadOnly = cbModo.SelectedIndex == 1);
        };
        dgvUnidades.Visible = false;
        raiz.RowStyles[2].Height = 100;
        raiz.RowStyles[3].Height = 0;
        lblMensaje = Componentes.Etiqueta("En modo productos, escribe el número de cuenta para cada unidad. Un producto compartido puede dividirse por importe.");
        lblMensaje.Name = nameof(lblMensaje);
        lblMensaje.Dock = DockStyle.Top;
        raiz.Controls.Add(lblMensaje, 0, 4);
        var pie = new FlowLayoutPanel
        {
            Name = "flpPie",
            AutoSize = true,
            Dock = DockStyle.Top
        };
        btnGuardar = Componentes.Boton("Confirmar cuentas");
        btnGuardar.Name = nameof(btnGuardar);
        btnGuardar.Click += Guardar_Click;
        var btnVolver = Componentes.Boton("Volver", false);
        btnVolver.Name = "btnVolver";
        btnVolver.Click += (_, _) => Close();
        pie.Controls.AddRange([btnGuardar, btnVolver]);
        raiz.Controls.Add(pie, 0, 5);
        Controls.Add(raiz);
        SizeChanged += (_, _) =>
        {
            lblResumen.MaximumSize = lblMensaje.MaximumSize = new Size(Math.Max(200, ClientSize.Width - 50), 0);
        };
        ResumeLayout(true);
    }
}
