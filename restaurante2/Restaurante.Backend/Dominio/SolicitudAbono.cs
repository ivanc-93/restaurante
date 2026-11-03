namespace Restaurante.Backend.Dominio;
public sealed record SolicitudAbono(Guid OperacionId, int PedidoId, string Version, decimal TotalEsperado, int? CuentaId, decimal SaldoEsperado, decimal Importe, SolicitudCobro Datos);
