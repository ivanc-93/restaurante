namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioResolverIncidencia
{
    private Label lblResumen = null!;
    private Label lblMensaje = null!;
    private ComboBox cbDesenlace = null!;
    private ComboBox cbMetodo = null!;
    private TextBox ctMotivo = null!;
    private TextBox ctReceptor = null!;
    private TextBox ctIdentificacion = null!;
    private TextBox ctDireccion = null!;
    private TextBox ctCorreo = null!;
    private TextBox ctReferencia = null!;
    private NumericUpDown nudEntregado = null!;
    private CheckBox chkPublico = null!;
    private CheckBox chkConfirmacion = null!;
    private Button btnResolver = null!;
    private void InitializeComponent()
    {
        var flpP = DisenoFormulario.Preparar(this, "Resolver incidencia");
        lblResumen = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(600, 0),
            Name = "lblResumen"
        };
        flpP.Controls.Add(lblResumen);
        cbDesenlace = DisenoFormulario.Opciones(flpP, "Desenlace", "Cobro posterior", "Pérdida", "Condonación");
        ctMotivo = DisenoFormulario.Campo(flpP, "Motivo y evidencia del desenlace");
        chkPublico = new CheckBox
        {
            Text = "Comprobante a público general",
            Checked = true,
            AutoSize = true,
            Name = "chkPublico"
        };
        flpP.Controls.Add(chkPublico);
        ctReceptor = DisenoFormulario.Campo(flpP, "Nombre del receptor identificado");
        ctIdentificacion = DisenoFormulario.Campo(flpP, "Identificación (sin validación SAT)");
        ctDireccion = DisenoFormulario.Campo(flpP, "Dirección");
        ctCorreo = DisenoFormulario.Campo(flpP, "Correo opcional");
        cbMetodo = DisenoFormulario.Opciones(flpP, "Pago manual confirmado; sin conexión bancaria", "Efectivo", "Tarjeta");
        flpP.Controls.Add(new Label { Text = "Recibido en Q (tarjeta: importe exacto)", AutoSize = true });
        nudEntregado = new NumericUpDown
        {
            DecimalPlaces = 2,
            Maximum = 9999999999.99m,
            Width = 300,
            Name = "nudEntregado"
        };
        flpP.Controls.Add(nudEntregado);
        ctReferencia = DisenoFormulario.Campo(flpP, "Referencia externa si es tarjeta");
        chkConfirmacion = new CheckBox
        {
            Text = "Revisé los datos; si hay cobro, comprobé su recepción completa",
            AutoSize = true,
            Name = "chkConfirmacion"
        };
        flpP.Controls.Add(chkConfirmacion);
        flpP.Controls.Add(new Label { AutoSize = false, Size = new Size(1, 32), TabStop = false });
        void Actualizar()
        {
            var pago = cbDesenlace.Text == "Cobro posterior";
            chkPublico.Enabled = cbMetodo.Enabled = nudEntregado.Enabled = ctReferencia.Enabled = pago;
            ctReceptor.Enabled = ctIdentificacion.Enabled = ctDireccion.Enabled = ctCorreo.Enabled = pago && !chkPublico.Checked;
        }

        chkPublico.CheckedChanged += (_, _) => Actualizar();
        cbDesenlace.SelectedIndexChanged += (_, _) => Actualizar();
        Actualizar();
        btnResolver = DisenoFormulario.Boton(flpP, "Registrar resolución", Resolver_Click);
        var btnCerrar = DisenoFormulario.Boton(flpP, "Cerrar", (_, _) => Close());
        CancelButton = btnCerrar;
        lblMensaje = DisenoFormulario.Mensaje(flpP);
    }
}
