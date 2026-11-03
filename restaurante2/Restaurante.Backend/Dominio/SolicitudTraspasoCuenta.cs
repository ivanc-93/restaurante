namespace Restaurante.Backend.Dominio;
public sealed record SolicitudTraspasoCuenta(Guid OperacionId, int PedidoId, string VersionPedido, int CuentaOrigenId, int CuentaDestinoId, int DetalleId, int Cantidad, string Motivo);
