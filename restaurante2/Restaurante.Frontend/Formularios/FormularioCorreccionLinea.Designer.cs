namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioCorreccionLinea
{
    private ComboBox cbLineas = null!;
    private NumericUpDown nudRetirar = null!;
    private TextBox ctMotivo = null!;
    private CheckBox chkDeclaracion = null!;
    private Button btnAplicar = null!;
    private Label lblMensaje = null!;
    private void InitializeComponent()
    {
        var flpP = DisenoFormulario.Preparar(this, "Corregir captura de comanda");
        flpP.Controls.Add(new Label { Text = "Selecciona una línea. La corrección requiere Gerente/Administrador y no elimina la captura original.", AutoSize = true, MaximumSize = new Size(600, 0) });
        cbLineas = DisenoFormulario.Opciones(flpP, "Producto");
        cbLineas.DisplayMember = "Producto";
        cbLineas.FormattingEnabled = true;
        cbLineas.Format += (_, e) =>
        {
            if (e.ListItem is Restaurante.Backend.Dominio.LineaPedidoCorregible l)
                e.Value = $"{l.Producto} · {l.CantidadVigente} vigentes de {l.CantidadOriginal} capturadas";
        };
        flpP.Controls.Add(new Label { Text = "Unidades a retirar", AutoSize = true });
        nudRetirar = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 50,
            Value = 1,
            Name = "nudRetirar"
        };
        flpP.Controls.Add(nudRetirar);
        chkDeclaracion = new CheckBox
        {
            Text = "Confirmo que no hubo preparación ni consumo de estas unidades",
            AutoSize = true,
            Name = "chkDeclaracion"
        };
        flpP.Controls.Add(chkDeclaracion);
        ctMotivo = DisenoFormulario.Campo(flpP, "Motivo de la corrección");
        btnAplicar = DisenoFormulario.Boton(flpP, "Solicitar autorización y corregir", Aplicar_Click);
        var btnCerrar = DisenoFormulario.Boton(flpP, "Cerrar", (_, _) => Close());
        CancelButton = btnCerrar;
        lblMensaje = DisenoFormulario.Mensaje(flpP);
    }
}
