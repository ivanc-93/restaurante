using Restaurante.Backend.Dominio;

namespace Restaurante.Backend.Servicios;

public interface IRepositorioRestaurante
{
    UsuarioAutenticado? IniciarSesion(string usuario, string contrasena);
    ResumenRestaurante ObtenerResumen();
    IReadOnlyList<Mesa> ListarMesas();
    void CrearMesa(int numero, int capacidad);
    void ActualizarMesa(int id, int numero, int capacidad, string estado);
    void EliminarMesa(int id);
    IReadOnlyList<ElementoMenu> ListarMenu();
    IReadOnlyList<Cliente> ListarClientes();
    IReadOnlyList<PedidoResumen> ListarPedidos();
    PedidoResumen? ObtenerPedidoAbierto(int mesaId);
    IReadOnlyList<DetallePedidoResumen> ListarDetallesPedido(int pedidoId);
    PedidoCreado CrearPedido(SolicitudPedido solicitud);
    decimal AgregarProductoAPedido(int pedidoId, int menuId, int cantidad);
    string FacturarPedido(int pedidoId);
    IReadOnlyList<VentaDiaria> ReportarVentas(DateTime desde, DateTime hasta);
    IReadOnlyList<PlatoVendido> ReportarPlatosMasVendidos();
}
