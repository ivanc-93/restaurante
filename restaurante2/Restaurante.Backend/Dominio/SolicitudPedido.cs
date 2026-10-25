namespace Restaurante.Backend.Dominio;

public sealed class SolicitudPedido
{
    public int MesaId { get; init; }
    public int? ClienteId { get; init; }
    public IReadOnlyList<DetallePedidoSolicitud> Detalles { get; init; } = [];
}
