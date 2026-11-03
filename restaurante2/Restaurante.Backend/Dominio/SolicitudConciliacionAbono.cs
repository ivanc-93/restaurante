namespace Restaurante.Backend.Dominio;
public sealed record SolicitudConciliacionAbono(Guid OperacionId, int PedidoId, string Version, decimal TotalEsperado, Guid AbonoId, bool Confirmado, string Motivo);
