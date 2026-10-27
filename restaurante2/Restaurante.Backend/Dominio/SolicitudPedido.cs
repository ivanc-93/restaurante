namespace Restaurante.Backend.Dominio;

public sealed class SolicitudPedido
{
    public Guid OperacionId { get; init; } = Guid.NewGuid();
    public int MesaId { get; init; }
    public int? ClienteId { get; init; }
    public IReadOnlyList<DetallePedidoSolicitud> Detalles { get; init; } = [];
}
