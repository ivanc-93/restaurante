namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioProductoCuenta
{
    private Label lblResumen = null!, lblMensaje = null!;
    private DataGridView dgvProductos = null!;
    private ComboBox cbAccion = null!, cbDestino = null!;
    private NumericUpDown nudCantidad = null!;
    private TextBox ctMotivo = null!;
    private CheckBox chkSinPreparacion = null!;
    private Button btnAplicar = null!;
    private void InitializeComponent()
    {
        var p = DisenoFormulario.Preparar(this, "Productos de una cuenta");
        lblResumen = Componentes.Etiqueta("Mover o corregir");
        lblResumen.Name = nameof(lblResumen);
        lblResumen.MaximumSize = new Size(550, 0);
        p.Controls.Add(lblResumen);
        dgvProductos = new()
        {
            Name = nameof(dgvProductos),
            Width = 520,
            Height = 180,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        p.Controls.Add(dgvProductos);
        cbAccion = DisenoFormulario.Opciones(p, "Acción");
        cbAccion.Name = nameof(cbAccion);
        cbDestino = DisenoFormulario.Opciones(p, "Cuenta de destino (solo traslado)");
        cbDestino.Name = nameof(cbDestino);
        p.Controls.Add(Componentes.Etiqueta("Unidades"));
        nudCantidad = new()
        {
            Name = nameof(nudCantidad),
            Minimum = 1,
            Maximum = 50,
            Value = 1
        };
        p.Controls.Add(nudCantidad);
        ctMotivo = DisenoFormulario.Campo(p, "Motivo obligatorio");
        ctMotivo.Name = nameof(ctMotivo);
        ctMotivo.MaxLength = 400;
        chkSinPreparacion = new()
        {
            Name = nameof(chkSinPreparacion),
            Text = "Declaro que no hubo preparación ni consumo",
            AutoSize = true
        };
        p.Controls.Add(chkSinPreparacion);
        btnAplicar = DisenoFormulario.Boton(p, "Confirmar traslado", Aplicar_Click);
        btnAplicar.Name = nameof(btnAplicar);
        lblMensaje = DisenoFormulario.Mensaje(p);
        cbAccion.SelectedIndexChanged += (_, _) =>
        {
            var corr = cbAccion.Text == "Corregir captura";
            cbDestino.Enabled = !corr;
            chkSinPreparacion.Visible = corr;
            btnAplicar.Text = corr ? "Solicitar corrección de unidades" : "Mover unidades a la cuenta seleccionada";
        };
    }
}
