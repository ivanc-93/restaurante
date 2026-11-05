namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioNombreCuenta
{
    private Label lblResumen = null!, lblMensaje = null!;
    private TextBox ctNombre = null!;
    private CheckBox chkConfirmado = null!;
    private Button btnGuardar = null!, btnRetirar = null!;
    private void InitializeComponent()
    {
        var p = DisenoFormulario.Preparar(this, "Cuenta de la mesa");
        lblResumen = Componentes.Etiqueta("Identifica esta cuenta");
        lblResumen.Name = nameof(lblResumen);
        lblResumen.MaximumSize = new Size(500, 0);
        p.Controls.Add(lblResumen);
        ctNombre = DisenoFormulario.Campo(p, "Nombre (por ejemplo Ana o Familia Pérez)");
        ctNombre.Name = nameof(ctNombre);
        ctNombre.MaxLength = 80;
        chkConfirmado = new()
        {
            Name = nameof(chkConfirmado),
            AutoSize = true,
            MaximumSize = new Size(500, 0),
            Text = "Confirmo este nombre para identificar la cuenta al cobrar"
        };
        p.Controls.Add(chkConfirmado);
        btnGuardar = DisenoFormulario.Boton(p, "Guardar cuenta", Guardar_Click);
        btnGuardar.Name = nameof(btnGuardar);
        btnRetirar = DisenoFormulario.Boton(p, "Retirar cuenta vacía (conserva historial)", Retirar_Click);
        btnRetirar.Name = nameof(btnRetirar);
        lblMensaje = DisenoFormulario.Mensaje(p);
        lblMensaje.Name = nameof(lblMensaje);
        AcceptButton = btnGuardar;
    }
}
