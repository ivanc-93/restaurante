namespace Restaurante.Backend.Dominio;
public sealed record SolicitudBeneficioCuenta(Guid OperacionId, int PedidoId, string VersionPedido, decimal TotalEsperado, int? CuentaId, string Tipo, decimal Porcentaje, string Motivo);
