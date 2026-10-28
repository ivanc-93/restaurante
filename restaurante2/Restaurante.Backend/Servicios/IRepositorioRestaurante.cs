using Restaurante.Backend.Dominio;

namespace Restaurante.Backend.Servicios;

public interface IRepositorioRestaurante
{
    bool CorreccionesDisponibles() => false;
    IReadOnlyList<LineaPedidoCorregible> ListarLineasCorregibles(int pedidoId) => throw new NotSupportedException();
    decimal CorregirLinea(SolicitudCorreccionLinea s) => throw new NotSupportedException();
    string MarcarPreparacion(int pedidoId,Guid operacionId) => throw new NotSupportedException();
    IReadOnlyList<IncidenciaResumen> ListarIncidencias() => throw new NotSupportedException();
    string ResolverIncidencia(SolicitudResolucionIncidencia s) => throw new NotSupportedException();
    bool RecuperacionDisponible() => false;
    void AbandonarOperacion(Guid id) => throw new NotSupportedException("El repositorio no admite abandono seguro.");
    bool NecesitaCodigoRecuperacion();
    ResultadoGeneracionCodigo IntentarGenerarCodigoRecuperacion(string claveActual)=>new(EstadoGeneracionCodigo.NoDisponible,"Este repositorio no admite generación guiada de códigos.");
    string GenerarCodigoRecuperacion(string claveActual);
    ResultadoRecuperacion IntentarConfirmarCodigoRecuperacion(string codigo)=>new(false,"Este repositorio no admite confirmación guiada del código.");
    void ConfirmarCodigoRecuperacion(string codigo);
    ResultadoRecuperacion RecuperarConAdministrador(Guid operacion,string recibo,string objetivo,string administrador,string claveAdministrador,string nueva);
    ResultadoRecuperacion RecuperarConCodigo(Guid operacion,string recibo,string usuario,string codigo,string nueva);
    ResultadoRecuperacion ConsultarRecuperacion(Guid operacion,string recibo);
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
