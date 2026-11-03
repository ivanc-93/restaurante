namespace Restaurante.Backend.Dominio;
public sealed record SolicitudCuentaMesa(Guid OperacionId, int MesaId, int? PedidoId, string? VersionPedido, int CuentaId, string? VersionCuenta, string Accion, string Nombre, bool NombreConfirmado);
