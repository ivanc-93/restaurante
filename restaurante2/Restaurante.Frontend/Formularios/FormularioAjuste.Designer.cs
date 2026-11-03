namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioAjuste
{
    private Label lblResumen = null!;
    private Label lblImporte = null!;
    private Label lblMensaje = null!;
    private ComboBox cbTipo = null!;
    private NumericUpDown nudDescuento = null!;
    private TextBox ctMotivo = null!;
    private Button btnAplicar = null!;
    private void InitializeComponent()
    {
        var flpP = DisenoFormulario.Preparar(this, "Descuento o cortesía antes del cobro");
        lblResumen = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(500, 0),
            Name = "lblResumen"
        };
        flpP.Controls.Add(lblResumen);
        cbTipo = DisenoFormulario.Opciones(flpP, "Tipo de ajuste", "Descuento", "Cortesía");
        lblImporte = new Label
        {
            Name = "lblImporte",
            Text = "Descuento en Q sobre el importe original",
            AutoSize = true
        };
        flpP.Controls.Add(lblImporte);
        nudDescuento = new NumericUpDown
        {
            DecimalPlaces = 2,
            Maximum = 9999999999.99m,
            Width = 250,
            Name = "nudDescuento"
        };
        flpP.Controls.Add(nudDescuento);
        ctMotivo = DisenoFormulario.Campo(flpP, "Motivo obligatorio");
        btnAplicar = DisenoFormulario.Boton(flpP, "Solicitar y aplicar ajuste", Aplicar_Click);
        lblMensaje = DisenoFormulario.Mensaje(flpP);
        cbTipo.SelectedIndexChanged += (_, _) =>
        {
            nudDescuento.Enabled = cbTipo.Text != "Cortesía";
        };
    }
}
