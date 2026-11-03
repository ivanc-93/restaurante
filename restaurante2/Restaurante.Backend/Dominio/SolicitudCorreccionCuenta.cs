namespace Restaurante.Backend.Dominio;
public sealed record SolicitudCorreccionCuenta(Guid OperacionId, int PedidoId, string VersionPedido, decimal TotalEsperado, int CuentaId, int DetalleId, int Cantidad, string Motivo, bool SinPreparacionDeclarada);
