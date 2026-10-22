using Restaurante.Backend.Dominio;
using Restaurante.Backend.Servicios;

namespace Restaurante.Frontend.Formularios;

public sealed class FormularioDetalleMesa : Form
{
    private readonly IRepositorioRestaurante _repositorio;
    private readonly Mesa _mesa;
    private readonly Label _resumen = new() { AutoSize = true, ForeColor = Componentes.Secundario };
    private readonly Label _total = new() { AutoSize = true, Font = new Font("Georgia", 17, FontStyle.Bold), ForeColor = Componentes.Verde };
    private readonly DataGridView _detalles = new() { Dock = DockStyle.Fill, AutoGenerateColumns = true };
    private readonly ComboBox _productos = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        DisplayMember = nameof(ElementoMenu.Nombre),
        Font = Componentes.Fuente,
        Width = 280
    };
    private readonly NumericUpDown _cantidad = new()
    {
        Minimum = 1,
        Maximum = 50,
        Value = 1,
        Width = 80,
        Font = Componentes.Fuente
    };
    private readonly Button _agregar = Componentes.Boton("Agregar producto");
    private readonly Button _facturar = Componentes.Boton("Facturar y liberar mesa");
    private PedidoResumen? _pedido;

    public FormularioDetalleMesa(IRepositorioRestaurante repositorio, Mesa mesa)
    {
        _repositorio = repositorio;
        _mesa = mesa;
        Text = $"Mesa {_mesa.Numero} | Detalle del pedido";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(850, 590);
        Size = new Size(980, 680);
        BackColor = Componentes.Fondo;
        CrearInterfaz();
        CargarPedido();
    }

    private void CrearInterfaz()
    {
        var principal = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(22),
            ColumnCount = 1,
            RowCount = 5,
            BackColor = Componentes.Fondo
        };
        principal.RowStyles.Add(new RowStyle(SizeType.Absolute, 75));
        principal.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        principal.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        principal.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
        principal.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        Controls.Add(principal);

        principal.Controls.Add(new Label
        {
            Text = $"Mesa {_mesa.Numero}  ·  {_mesa.Capacidad} lugares",
            Dock = DockStyle.Fill,
            Font = new Font("Georgia", 21, FontStyle.Bold),
            ForeColor = Componentes.Tinta,
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 0);
        principal.Controls.Add(_resumen, 0, 1);
        Componentes.EstilizarTabla(_detalles);
        principal.Controls.Add(_detalles, 0, 2);

        var agregarFila = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 12, 0, 0)
        };
        agregarFila.Controls.Add(Componentes.Etiqueta("Producto", 9, true));
        agregarFila.Controls.Add(_productos);
        agregarFila.Controls.Add(Componentes.Etiqueta("Cantidad", 9, true));
        agregarFila.Controls.Add(_cantidad);
        _agregar.Width = 160;
        _agregar.Height = 34;
        _agregar.Margin = new Padding(0, 2, 0, 0);
        agregarFila.Controls.Add(_agregar);
        principal.Controls.Add(agregarFila, 0, 3);

        var pie = new Panel { Dock = DockStyle.Fill };
        pie.Controls.Add(_total);
        _total.Location = new Point(2, 14);
        _facturar.Width = 230;
        _facturar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _facturar.Location = new Point(Width - _facturar.Width - 70, 4);
        pie.Resize += (_, _) => _facturar.Left = pie.ClientSize.Width - _facturar.Width;
        pie.Controls.Add(_facturar);
        principal.Controls.Add(pie, 0, 4);

        _agregar.Click += (_, _) => AgregarProducto();
        _facturar.Click += (_, _) => FacturarPedido();
    }

    private void CargarPedido()
    {
        _pedido = _repositorio.ObtenerPedidoAbierto(_mesa.Id);
        _productos.DataSource = _repositorio.ListarMenu().ToList();
        if (_pedido is null)
        {
            _resumen.Text = "No hay un pedido abierto para esta mesa. Crea un pedido desde Pedidos y facturas.";
            _detalles.DataSource = null;
            _total.Text = "Sin pedido abierto";
            _agregar.Enabled = false;
            _facturar.Enabled = false;
            _productos.Enabled = false;
            _cantidad.Enabled = false;
            return;
        }

        _resumen.Text = $"Pedido #{_pedido.Id} · {_pedido.Estado} · Iniciado {_pedido.Fecha:g} · Cliente: {_pedido.Cliente}";
        _detalles.DataSource = _repositorio.ListarDetallesPedido(_pedido.Id).ToList();
        _total.Text = $"Total acumulado: {_pedido.Total:C}";
        _agregar.Enabled = _productos.Items.Count > 0;
        _facturar.Enabled = _pedido.Total > 0;
        _productos.Enabled = true;
        _cantidad.Enabled = true;
    }

    private void AgregarProducto()
    {
        try
        {
            if (_pedido is null)
                throw new InvalidOperationException("No hay un pedido abierto para esta mesa.");
            if (_productos.SelectedItem is not ElementoMenu producto)
                throw new InvalidOperationException("Selecciona un plato o bebida.");

            var total = _repositorio.AgregarProductoAPedido(_pedido.Id, producto.Id, (int)_cantidad.Value);
            _cantidad.Value = 1;
            CargarPedido();
            _total.Text = $"Total acumulado: {total:C}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "No se pudo agregar el producto",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void FacturarPedido()
    {
        try
        {
            if (_pedido is null)
                throw new InvalidOperationException("No hay un pedido abierto para facturar.");
            var confirmacion = MessageBox.Show(this,
                $"¿Los comensales se retiran y deseas facturar el pedido #{_pedido.Id} por {_pedido.Total:C}?",
                "Facturar pedido y liberar mesa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmacion != DialogResult.Yes) return;

            var factura = _repositorio.FacturarPedido(_pedido.Id);
            MessageBox.Show(this, $"Factura {factura} emitida. La mesa {_mesa.Numero} quedó disponible.",
                "Pedido facturado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "No se pudo facturar el pedido",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            CargarPedido();
        }
    }
}
