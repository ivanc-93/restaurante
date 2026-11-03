#nullable enable
namespace Restaurante.Frontend.Formularios;
partial class FormularioGestionMesa
{
    private System.ComponentModel.IContainer? components;
    private Label lblResumen = null!;
    private ComboBox cbAccion = null!;
    private ComboBox cbResultado = null!;
    private TextBox ctMotivo = null!;
    private CheckBox chkDeclarado = null!;
    private CheckBox chkVerificado = null!;
    private Button btnAplicar = null!;
    private FlowLayoutPanel flpCampos = null!;
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        lblResumen = new Label()
        {
            Name = "lblResumen"
        };
        cbAccion = new ComboBox()
        {
            Name = "cbAccion"
        };
        cbResultado = new ComboBox()
        {
            Name = "cbResultado"
        };
        ctMotivo = new TextBox()
        {
            Name = "ctMotivo"
        };
        chkDeclarado = new CheckBox()
        {
            Name = "chkDeclarado"
        };
        chkVerificado = new CheckBox()
        {
            Name = "chkVerificado"
        };
        btnAplicar = new Button()
        {
            Name = "btnAplicar"
        };
        flpCampos = new FlowLayoutPanel()
        {
            Name = "flpCampos"
        };
        SuspendLayout();
        flpCampos.Dock = DockStyle.Fill;
        flpCampos.FlowDirection = FlowDirection.TopDown;
        flpCampos.WrapContents = false;
        flpCampos.AutoScroll = true;
        flpCampos.Padding = new Padding(20);
        lblResumen.AutoSize = true;
        lblResumen.MaximumSize = new Size(480, 0);
        cbAccion.Width = 430;
        cbAccion.DropDownStyle = ComboBoxStyle.DropDownList;
        cbAccion.SelectedIndexChanged += Accion_Cambio;
        cbResultado.Width = 430;
        cbResultado.DropDownStyle = ComboBoxStyle.DropDownList;
        cbResultado.Items.AddRange(new object[] { "Confirmado", "Rechazado" });
        ctMotivo.Width = 430;
        ctMotivo.Height = 100;
        ctMotivo.Multiline = true;
        ctMotivo.MaxLength = 500;
        ctMotivo.AccessibleName = "Motivo y evidencia obligatorios";
        chkDeclarado.Text = "Declaro que no hubo preparación ni consumo";
        chkDeclarado.AutoSize = true;
        chkVerificado.Text = "Comprobé manualmente la evidencia y el resultado";
        chkVerificado.AutoSize = true;
        btnAplicar.Text = "Confirmar gestión";
        btnAplicar.AutoSize = true;
        btnAplicar.Enabled = false;
        btnAplicar.Click += Aplicar_Click;
        flpCampos.Controls.Add(lblResumen);
        flpCampos.Controls.Add(cbAccion);
        flpCampos.Controls.Add(new Label { Text = "Resultado externo (solo conciliación)", AutoSize = true });
        flpCampos.Controls.Add(cbResultado);
        flpCampos.Controls.Add(new Label { Text = "Motivo / evidencia (obligatorio)", AutoSize = true });
        flpCampos.Controls.Add(ctMotivo);
        flpCampos.Controls.Add(chkDeclarado);
        flpCampos.Controls.Add(chkVerificado);
        flpCampos.Controls.Add(btnAplicar);
        Controls.Add(flpCampos);
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(540, 530);
        MinimumSize = new Size(480, 420);
        StartPosition = FormStartPosition.CenterParent;
        Text = "Gestionar cierre de mesa";
        ResumeLayout(false);
    }
}
