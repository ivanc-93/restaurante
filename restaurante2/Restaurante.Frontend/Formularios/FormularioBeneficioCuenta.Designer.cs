namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioBeneficioCuenta
{
    private Label lblResumen = null!, lblMensaje = null!;
    private ComboBox cbTipo = null!;
    private NumericUpDown nudPorcentaje = null!;
    private TextBox ctMotivo = null!;
    private Button btnAplicar = null!;
    private void InitializeComponent()
    {
        var p = DisenoFormulario.Preparar(this, "Beneficio del alcance seleccionado");
        lblResumen = Componentes.Etiqueta("Cuenta o toda la mesa");
        lblResumen.Name = nameof(lblResumen);
        lblResumen.MaximumSize = new Size(500, 0);
        p.Controls.Add(lblResumen);
        cbTipo = DisenoFormulario.Opciones(p, "Ajuste autorizado", "Descuento", "Frecuente", "Cortesía", "Restablecer");
        cbTipo.Name = nameof(cbTipo);
        p.Controls.Add(Componentes.Etiqueta("Porcentaje (descuento 5–50 %; cortesía 100 %)"));
        nudPorcentaje = new()
        {
            Name = nameof(nudPorcentaje),
            Minimum = 0,
            Maximum = 100,
            Value = 5,
            DecimalPlaces = 2
        };
        p.Controls.Add(nudPorcentaje);
        ctMotivo = DisenoFormulario.Campo(p, "Motivo obligatorio");
        ctMotivo.Name = nameof(ctMotivo);
        ctMotivo.MaxLength = 400;
        btnAplicar = DisenoFormulario.Boton(p, "Solicitar y aplicar a este alcance", Aplicar_Click);
        btnAplicar.Name = nameof(btnAplicar);
        lblMensaje = DisenoFormulario.Mensaje(p);
        lblMensaje.Name = nameof(lblMensaje);
    }
}
