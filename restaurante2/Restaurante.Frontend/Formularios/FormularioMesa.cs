using Restaurante.Backend.Dominio;

namespace Restaurante.Frontend.Formularios;

public sealed class FormularioMesa : Form
{
    private readonly NumericUpDown _numero = new()
    {
        Minimum = 1,
        Maximum = 10000,
        Width = 250,
        Font = Componentes.Fuente
    };
    private readonly NumericUpDown _capacidad = new()
    {
        Minimum = 1,
        Maximum = 100,
        Width = 250,
        Font = Componentes.Fuente
    };
    private readonly ComboBox _estado = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Width = 250,
        Font = Componentes.Fuente
    };

    public int NumeroMesa => (int)_numero.Value;
    public int Capacidad => (int)_capacidad.Value;
    public string EstadoMesa => _estado.SelectedItem?.ToString() ?? "Disponible";

    public FormularioMesa(Mesa? mesa = null, bool soloLectura = false, int siguienteNumero = 1)
    {
        Text = soloLectura ? "Detalle de mesa" : mesa is null ? "Nueva mesa" : "Editar mesa";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(410, soloLectura || mesa is null ? 290 : 335);
        BackColor = Componentes.Fondo;

        _estado.Items.AddRange(["Disponible", "Ocupada", "Reservada"]);
        _numero.Value = mesa?.Numero ?? Math.Clamp(siguienteNumero, 1, 10000);
        _capacidad.Value = mesa?.Capacidad ?? 4;
        _estado.SelectedItem = mesa?.Estado ?? "Disponible";

        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(25),
            BackColor = Color.White
        };
        Controls.Add(panel);
        var formulario = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = soloLectura || mesa is null ? 5 : 6
        };
        formulario.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        formulario.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
        formulario.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
        formulario.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
        if (mesa is not null && !soloLectura)
            formulario.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
        formulario.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        formulario.Controls.Add(new Label
        {
            Text = soloLectura ? $"Información de la mesa {mesa?.Numero}" :
                mesa is null ? "Registrar una mesa" : "Actualizar datos de la mesa",
            Font = new Font("Georgia", 15, FontStyle.Bold),
            ForeColor = Componentes.Tinta,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        });
        formulario.Controls.Add(CrearCampo("Número de mesa", _numero));
        formulario.Controls.Add(CrearCampo("Capacidad (personas)", _capacidad));
        formulario.Controls.Add(CrearCampo("Estado", _estado));

        if (soloLectura)
        {
            _numero.Enabled = false;
            _capacidad.Enabled = false;
            _estado.Enabled = false;
        }
        else if (mesa is not null)
        {
            formulario.Controls.Add(new Label
            {
                Text = "Las mesas con pedidos registrados no se pueden eliminar.",
                AutoSize = true,
                ForeColor = Componentes.Secundario,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            });
        }

        var acciones = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false
        };
        var aceptar = Componentes.Boton(soloLectura ? "Cerrar" : mesa is null ? "Crear mesa" : "Guardar cambios");
        aceptar.Width = 145;
        aceptar.DialogResult = DialogResult.OK;
        acciones.Controls.Add(aceptar);
        if (!soloLectura)
        {
            var cancelar = Componentes.Boton("Cancelar", false);
            cancelar.Width = 110;
            cancelar.DialogResult = DialogResult.Cancel;
            acciones.Controls.Add(cancelar);
            AcceptButton = aceptar;
            CancelButton = cancelar;
        }
        else
        {
            AcceptButton = aceptar;
        }
        formulario.Controls.Add(acciones);
        panel.Controls.Add(formulario);
    }

    private static Control CrearCampo(string texto, Control control)
    {
        var grupo = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        grupo.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        grupo.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var etiqueta = Componentes.Etiqueta(texto, 9, true);
        etiqueta.Margin = Padding.Empty;
        control.Dock = DockStyle.Fill;
        grupo.Controls.Add(etiqueta, 0, 0);
        grupo.Controls.Add(control, 0, 1);
        return grupo;
    }
}
