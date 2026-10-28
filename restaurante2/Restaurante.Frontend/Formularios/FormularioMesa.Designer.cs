#nullable enable
namespace Restaurante.Frontend.Formularios;
partial class FormularioMesa
{
    private System.ComponentModel.IContainer? components;
    private NumericUpDown _numero = null!;
    private NumericUpDown _capacidad = null!;
    private ComboBox _estado = null!;
    private Button aceptar = null!;
    private Button cancelar = null!;
    private Label numeroEtiqueta = null!;
    private Label capacidadEtiqueta = null!;
    private Label estadoEtiqueta = null!;
    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        _numero = new NumericUpDown();
        _capacidad = new NumericUpDown();
        _estado = new ComboBox();
        aceptar = new Button();
        cancelar = new Button();
        numeroEtiqueta = new Label();
        capacidadEtiqueta = new Label();
        estadoEtiqueta = new Label();
        ((System.ComponentModel.ISupportInitialize)_numero).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_capacidad).BeginInit();
        SuspendLayout();
        numeroEtiqueta.Text = "Número de mesa";
        numeroEtiqueta.SetBounds(25, 22, 340, 24);
        _numero.SetBounds(25, 50, 340, 30);
        _numero.Minimum = 1;
        _numero.Maximum = int.MaxValue;
        _numero.Value = 1;
        capacidadEtiqueta.Text = "Capacidad (personas)";
        capacidadEtiqueta.SetBounds(25, 95, 340, 24);
        _capacidad.SetBounds(25, 122, 340, 30);
        _capacidad.Minimum = 1;
        _capacidad.Maximum = 100;
        _capacidad.Value = 4;
        estadoEtiqueta.Text = "Estado";
        estadoEtiqueta.SetBounds(25, 168, 340, 24);
        _estado.SetBounds(25, 195, 340, 30);
        _estado.DropDownStyle = ComboBoxStyle.DropDownList;
        _estado.Items.AddRange(new object[] { "Disponible", "Ocupada", "Reservada", "En limpieza" });
        _estado.SelectedIndex = 0;
        aceptar.Text = "Guardar";
        aceptar.SetBounds(215, 260, 150, 42);
        aceptar.DialogResult = DialogResult.OK;
        cancelar.Text = "Cancelar";
        cancelar.SetBounds(25, 260, 150, 42);
        cancelar.DialogResult = DialogResult.Cancel;
        Controls.AddRange(new Control[] {numeroEtiqueta, _numero, capacidadEtiqueta, _capacidad, estadoEtiqueta, _estado, aceptar, cancelar});
        AcceptButton = aceptar;
        CancelButton = cancelar;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(400, 335);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Mesa";
        ((System.ComponentModel.ISupportInitialize)_numero).EndInit();
        ((System.ComponentModel.ISupportInitialize)_capacidad).EndInit();
        ResumeLayout(false);
        FormatoPresentacion.Aplicar(this);
    }
}
