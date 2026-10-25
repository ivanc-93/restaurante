using Restaurante.Frontend.Servicios;
using Restaurante.Backend.Dominio;
using Restaurante.Backend.Servicios;
namespace Restaurante.Frontend.Formularios;

public sealed partial class FormularioDetalleMesa : Form
{
    private readonly IDialogosUsuario _dialogos = new DialogosUsuario();
    private readonly IRepositorioRestaurante _repositorio = null!;
    private readonly Mesa _mesa = null!;
    private PedidoResumen? _pedido;
    public FormularioDetalleMesa() => InitializeComponent();
    public FormularioDetalleMesa(IRepositorioRestaurante repositorio, Mesa mesa, IDialogosUsuario? dialogos = null) : this()
    {
        _repositorio = repositorio;
        _mesa = mesa;
        _dialogos = dialogos ?? new DialogosUsuario();
        Text = $"Mesa {mesa.Numero} | Pedido";
        Componentes.EstilizarTabla(_detalles);
        Shown += async (_, _) => await RecargarSeguroAsync();
    }
    private async Task RecargarSeguroAsync()
    {
        _agregar.Enabled = _facturar.Enabled = false;
        try
        {
            var datos = await Task.Run(() =>
            {
                var pedido = _repositorio.ObtenerPedidoAbierto(_mesa.Id);
                var productos = _repositorio.ListarMenu().ToList();
                var detalles = pedido is null ? new List<DetallePedidoResumen>() : _repositorio.ListarDetallesPedido(pedido.Id).ToList();
                return (pedido, productos, detalles);
            });
            _pedido = datos.pedido;
            _productos.DataSource = datos.productos;
            _detalles.DataSource = datos.detalles;
            _resumen.Text = _pedido is null ? "Sin pedido abierto. Cree uno desde Pedidos y facturas." : $"Pedido #{_pedido.Id} · {_pedido.Estado} · {_pedido.Cliente}";
            _total.Text = _pedido is null ? "Sin pedido" : $"Total: {_pedido.Total:C}";
            _agregar.Enabled = _pedido is not null && datos.productos.Count > 0;
            _facturar.Enabled = _pedido is not null && datos.detalles.Count > 0;
        }
        catch (Exception ex)
        {
            _dialogos.Mostrar(this, ex.Message, "No se pudo cargar la mesa", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    private async void Agregar_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_pedido is null)
                throw new InvalidOperationException("No hay un pedido abierto para esta mesa.");
            if (_productos.SelectedItem is not ElementoMenu producto)
                throw new InvalidOperationException("Selecciona un plato o bebida.");

            var pedidoId = _pedido.Id;
            var cantidad = (int)_cantidad.Value;
            _agregar.Enabled = _facturar.Enabled = false;
            var total = await Task.Run(() => _repositorio.AgregarProductoAPedido(pedidoId, producto.Id, cantidad));
            _cantidad.Value = 1;
            await RecargarSeguroAsync();
            _total.Text = $"Total acumulado: {total:C}";
        }
        catch (Exception ex)
        {
            _dialogos.Mostrar(this, ex.Message, "No se pudo agregar el producto",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            await RecargarSeguroAsync();
        }
    }

    private async void Facturar_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_pedido is null)
                throw new InvalidOperationException("No hay un pedido abierto para facturar.");
            var confirmacion = _dialogos.Mostrar(this,
                $"¿Los comensales se retiran y deseas facturar el pedido #{_pedido.Id} por {_pedido.Total:C}?",
                "Facturar pedido y liberar mesa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmacion != DialogResult.Yes) return;

            var pedidoId = _pedido.Id;
            _agregar.Enabled = _facturar.Enabled = false;
            var factura = await Task.Run(() => _repositorio.FacturarPedido(pedidoId));
            _dialogos.Mostrar(this, $"Factura {factura} emitida. La mesa {_mesa.Numero} quedó disponible.",
                "Pedido facturado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            _dialogos.Mostrar(this, ex.Message, "No se pudo facturar el pedido",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            await RecargarSeguroAsync();
        }
    }
}
