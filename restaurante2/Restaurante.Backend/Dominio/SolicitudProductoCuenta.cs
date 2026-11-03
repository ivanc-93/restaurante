namespace Restaurante.Backend.Dominio;
public sealed record SolicitudProductoCuenta(Guid OperacionId, int MesaId, int? PedidoId, string? VersionPedido, int CuentaId, string? VersionCuenta, int MenuId, int Cantidad, int? ClienteId, int? ReservaId, string? VersionReserva);
