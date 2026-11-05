namespace Restaurante.Backend.Dominio;
public sealed record SolicitudCancelacionReserva(Guid OperacionId, int ReservaId, int MesaId, string Version, string Motivo);
