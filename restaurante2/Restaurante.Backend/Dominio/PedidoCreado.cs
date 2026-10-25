namespace Restaurante.Backend.Dominio;

public sealed record PedidoCreado(int Id, decimal Total, string? NumeroFactura);
