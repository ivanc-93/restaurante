#nullable enable
namespace Restaurante.Frontend.Formularios;
partial class FormularioMesa
{
    private System.ComponentModel.IContainer? components;
    private NumericUpDown nudNumero = null!;
    private NumericUpDown nudCapacidad = null!;
    private ComboBox cbEstado = null!;
    private Button btnAceptar = null!;
    private Button btnCancelar = null!;
    private Label lblNumeroEtiqueta = null!;
    private Label lblCapacidadEtiqueta = null!;
    private Label lblEstadoEtiqueta = null!;
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        nudNumero = new NumericUpDown()
        {
            Name = "nudNumero"
        };
        nudCapacidad = new NumericUpDown()
        {
            Name = "nudCapacidad"
        };
        cbEstado = new ComboBox()
        {
            Name = "cbEstado"
        };
        btnAceptar = new Button()
        {
            Name = "btnAceptar"
        };
        btnCancelar = new Button()
        {
            Name = "btnCancelar"
        };
        lblNumeroEtiqueta = new Label()
        {
            Name = "lblNumeroEtiqueta"
        };
        lblCapacidadEtiqueta = new Label()
        {
            Name = "lblCapacidadEtiqueta"
        };
        lblEstadoEtiqueta = new Label()
        {
            Name = "lblEstadoEtiqueta"
        };
        ((System.ComponentModel.ISupportInitialize)nudNumero).BeginInit();
        ((System.ComponentModel.ISupportInitialize)nudCapacidad).BeginInit();
        SuspendLayout();
        lblNumeroEtiqueta.Text = "Número de mesa";
        lblNumeroEtiqueta.SetBounds(25, 22, 340, 24);
        nudNumero.SetBounds(25, 50, 340, 30);
        nudNumero.Minimum = 1;
        nudNumero.Maximum = int.MaxValue;
        nudNumero.Value = 1;
        lblCapacidadEtiqueta.Text = "Capacidad (personas)";
        lblCapacidadEtiqueta.SetBounds(25, 95, 340, 24);
        nudCapacidad.SetBounds(25, 122, 340, 30);
        nudCapacidad.Minimum = 1;
        nudCapacidad.Maximum = 100;
        nudCapacidad.Value = 4;
        lblEstadoEtiqueta.Text = "Estado";
        lblEstadoEtiqueta.SetBounds(25, 168, 340, 24);
        cbEstado.SetBounds(25, 195, 340, 30);
        cbEstado.DropDownStyle = ComboBoxStyle.DropDownList;
        cbEstado.Items.AddRange(new object[] { "Disponible", "Ocupada", "Reservada", "En limpieza" });
        cbEstado.SelectedIndex = 0;
        btnAceptar.Text = "Guardar";
        btnAceptar.SetBounds(215, 260, 150, 42);
        btnAceptar.DialogResult = DialogResult.OK;
        btnCancelar.Text = "Cancelar";
        btnCancelar.SetBounds(25, 260, 150, 42);
        btnCancelar.DialogResult = DialogResult.Cancel;
        Controls.AddRange(new Control[] { lblNumeroEtiqueta, nudNumero, lblCapacidadEtiqueta, nudCapacidad, lblEstadoEtiqueta, cbEstado, btnAceptar, btnCancelar });
        AcceptButton = btnAceptar;
        CancelButton = btnCancelar;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(400, 335);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Mesa";
        ((System.ComponentModel.ISupportInitialize)nudNumero).EndInit();
        ((System.ComponentModel.ISupportInitialize)nudCapacidad).EndInit();
        ResumeLayout(false);
        FormatoPresentacion.Aplicar(this);
    }
}
