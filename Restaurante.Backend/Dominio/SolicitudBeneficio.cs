namespace Restaurante.Backend.Dominio;
public sealed record SolicitudBeneficio(Guid OperacionId, int PedidoId, string Version, decimal TotalEsperado, string Tipo, decimal Porcentaje, string Motivo);
