using Restaurante.Backend.Dominio;

namespace Restaurante.Backend.Servicios;

public interface IRepositorioRestaurante
{
    void CambiarMiContrasena(string anterior,string nueva);
    IReadOnlyList<UsuarioGestion> ListarUsuarios();
    IReadOnlyList<RegistroSeguridad> ConsultarAuditoriaSeguridad();
    void GuardarUsuario(int? id,string nombre,string usuario,string rol,bool activo,string? nuevaClave);
    void AutorizarOperacion(Guid id,string tipo,string solicitudJson,string usuario,string clave);
    ResultadoAutorizacion IntentarAutorizarOperacion(Guid id,string tipo,string solicitudJson,string usuario,string clave);
    bool RevocarAutorizacion(Guid id);
    decimal AplicarAjuste(SolicitudAjuste solicitud);
    UsuarioAutenticado? UsuarioActual { get; }
    string IdentificadorBase { get; }
    bool BaseEsPruebas { get; }
    void CerrarSesion();
    void ValidarEsquema();
    bool OperacionAplicada(Guid operacionId);
    CuentaRevision RevisarCuenta(int pedidoId);
    ResultadoCobro RegistrarCobro(SolicitudCobro solicitud);
    Guid? ObtenerPagoIncierto(int pedidoId);
    SolicitudCobro ConsultarSolicitudCobro(Guid operacionId);
    ResultadoCobro ConciliarTarjeta(Guid operacionIncierta, bool confirmado, string motivo, Guid operacionId);
    string GestionarMesa(SolicitudGestionMesa solicitud);
    string ConsultarComprobante(int pedidoId);
    IReadOnlyList<MovimientoOperacion> ReportarOperaciones(DateTime desde,DateTime hasta);
    UsuarioAutenticado? IniciarSesion(string usuario, string contrasena);
    ResumenRestaurante ObtenerResumen();
    IReadOnlyList<Mesa> ListarMesas();
    void CrearMesa(int numero, int capacidad);
    void ActualizarMesa(int id, int numero, int capacidad, string estado);
    void EliminarMesa(int id);
    IReadOnlyList<ElementoMenu> ListarMenu();
    IReadOnlyList<Cliente> ListarClientes();
    IReadOnlyList<Cliente> ListarClientesHistoricos();
    IReadOnlyList<PedidoResumen> ListarPedidos();
    PedidoResumen? ObtenerPedidoAbierto(int mesaId);
    IReadOnlyList<DetallePedidoResumen> ListarDetallesPedido(int pedidoId);
    PedidoCreado CrearPedido(SolicitudPedido solicitud);
    decimal AgregarProductoAPedido(int pedidoId, int menuId, int cantidad, Guid? operacionId = null);
    string FacturarPedido(int pedidoId);
    IReadOnlyList<VentaDiaria> ReportarVentas(DateTime desde, DateTime hasta);
    IReadOnlyList<PlatoVendido> ReportarPlatosMasVendidos(DateTime desde, DateTime hasta);
    DatosRestaurante ObtenerRestaurante();
    void GuardarRestaurante(string nombre, string ubicacion);
    void GuardarCliente(int? id, string nombre, string? telefono, string? correo);
    void DesactivarCliente(int id);
    void GuardarProducto(int? id, string nombre, string categoria, string tipo, decimal precio, bool alcoholica);
    void DesactivarProducto(int id);
    IReadOnlyList<VentaPeriodo> ReportarVentasPeriodo(DateTime desde, DateTime hasta, string agrupacion);
    IReadOnlyList<PedidoResumen> ReportarPedidos(DateTime desde, DateTime hasta, int? mesaId, int? clienteId);
}
