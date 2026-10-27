namespace Restaurante.Backend.Dominio;
public sealed record SolicitudIncorporacion(Guid OperacionId,int PedidoId,int MenuId,int Cantidad);
