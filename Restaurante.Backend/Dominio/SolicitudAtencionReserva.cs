namespace Restaurante.Backend.Dominio;
// Solicitud independiente: no modifica el JSON ni el hash de aperturas antiguas pendientes.
public sealed record SolicitudAtencionReserva(Guid OperacionId, int ReservaId, int MesaId, string Version, int? ClienteId, IReadOnlyList<DetallePedidoSolicitud> Detalles);
