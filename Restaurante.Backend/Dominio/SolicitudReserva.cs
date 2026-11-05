namespace Restaurante.Backend.Dominio;
public sealed record SolicitudReserva(Guid OperacionId, int MesaId, string Nombre);
