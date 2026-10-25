namespace Restaurante.Backend.Dominio;

public sealed record PedidoResumen(int Id, int NumeroMesa, string Cliente, DateTime Fecha,
    string Estado, decimal Total, string? NumeroFactura);
