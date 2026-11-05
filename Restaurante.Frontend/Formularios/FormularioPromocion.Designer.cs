namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioPromocion
{
    private CheckBox chkActiva = null!;
    private NumericUpDown nudVisitas = null!, nudPorcentaje = null!;
    private DateTimePicker dtDesde = null!, dtHasta = null!;
    private Label lblMensaje = null!;
    private Button btnGuardar = null!;
    private void InitializeComponent()
    {
        var p = DisenoFormulario.Preparar(this, "Promoción de clientes frecuentes");
        p.Controls.Add(Componentes.Etiqueta("Cuenta completa, no acumulable. Las visitas cuentan pedidos distintos facturados y pagados, no abonos. Cada aplicación requiere gerencia. Sin costos no se garantiza margen."));
        chkActiva = new()
        {
            Name = nameof(chkActiva),
            Text = "Activar promoción",
            AutoSize = true
        };
        p.Controls.Add(chkActiva);
        p.Controls.Add(Componentes.Etiqueta("Visitas completadas mínimas"));
        nudVisitas = new()
        {
            Name = nameof(nudVisitas),
            Minimum = 1,
            Maximum = 10000,
            Value = 1
        };
        p.Controls.Add(nudVisitas);
        p.Controls.Add(Componentes.Etiqueta("Descuento de 5 a 50 %"));
        nudPorcentaje = new()
        {
            Name = nameof(nudPorcentaje),
            Minimum = 5,
            Maximum = 50,
            Value = 5,
            DecimalPlaces = 2
        };
        p.Controls.Add(nudPorcentaje);
        p.Controls.Add(Componentes.Etiqueta("Vigencia desde / hasta (Guatemala)"));
        dtDesde = new()
        {
            Name = nameof(dtDesde),
            Format = DateTimePickerFormat.Short
        };
        dtHasta = new()
        {
            Name = nameof(dtHasta),
            Format = DateTimePickerFormat.Short
        };
        p.Controls.AddRange([dtDesde, dtHasta]);
        btnGuardar = DisenoFormulario.Boton(p, "Guardar condiciones", Guardar_Click);
        btnGuardar.Name = nameof(btnGuardar);
        lblMensaje = DisenoFormulario.Mensaje(p);
    }
}
