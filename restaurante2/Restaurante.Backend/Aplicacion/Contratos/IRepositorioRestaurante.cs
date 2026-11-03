using Restaurante.Backend.Dominio;

namespace Restaurante.Backend.Aplicacion.Contratos;
public interface IRepositorioRestaurante : IConsultasReportes, IGestionUsuarios, IReservasMesa, ICobranzaParcial
{
    bool CorreccionesDisponibles() => false;
    IReadOnlyList<LineaPedidoCorregible> ListarLineasCorregibles(int pedidoId) => throw new NotSupportedException();
    decimal CorregirLinea(SolicitudCorreccionLinea s) => throw new NotSupportedException();
    string MarcarPreparacion(int pedidoId, Guid operacionId) => throw new NotSupportedException();
    IReadOnlyList<IncidenciaResumen> ListarIncidencias() => throw new NotSupportedException();
    string ResolverIncidencia(SolicitudResolucionIncidencia s) => throw new NotSupportedException();
    bool RecuperacionDisponible() => false;
    void AbandonarOperacion(Guid id) => throw new NotSupportedException("El repositorio no admite abandono seguro.");
    bool NecesitaCodigoRecuperacion();
    string GenerarCodigoRecuperacion(string claveActual);
    void ConfirmarCodigoRecuperacion(string codigo);
    ResultadoRecuperacion RecuperarConAdministrador(Guid operacion, string recibo, string objetivo, string administrador, string claveAdministrador, string nueva);
    ResultadoRecuperacion RecuperarConCodigo(Guid operacion, string recibo, string usuario, string codigo, string nueva);
    ResultadoRecuperacion ConsultarRecuperacion(Guid operacion, string recibo);
    void CambiarMiContrasena(string anterior, string nueva);
    void AutorizarOperacion(Guid id, string tipo, string solicitudJson, string usuario, string clave);
    ResultadoAutorizacion IntentarAutorizarOperacion(Guid id, string tipo, string solicitudJson, string usuario, string clave);
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
    UsuarioAutenticado? IniciarSesion(string usuario, string contrasena);
    ResumenRestaurante ObtenerResumen();
    void CrearMesa(int numero, int capacidad);
    void ActualizarMesa(int id, int numero, int capacidad, string estado);
    void EliminarMesa(int id);
    IReadOnlyList<ElementoMenu> ListarMenu();
    IReadOnlyList<Cliente> ListarClientes();
    IReadOnlyList<PedidoResumen> ListarPedidos();
    PedidoResumen? ObtenerPedidoAbierto(int mesaId);
    IReadOnlyList<DetallePedidoResumen> ListarDetallesPedido(int pedidoId);
    PedidoCreado CrearPedido(SolicitudPedido solicitud);
    decimal AgregarProductoAPedido(int pedidoId, int menuId, int cantidad, Guid? operacionId = null);
    string FacturarPedido(int pedidoId);
    DatosRestaurante ObtenerRestaurante();
    void GuardarRestaurante(string nombre, string ubicacion);
    void GuardarCliente(int? id, string nombre, string? telefono, string? correo);
    void DesactivarCliente(int id);
    void GuardarProducto(int? id, string nombre, string categoria, string tipo, decimal precio, bool alcoholica);
    void DesactivarProducto(int id);
}
