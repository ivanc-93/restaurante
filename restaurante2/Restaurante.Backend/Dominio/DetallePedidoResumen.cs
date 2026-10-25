namespace Restaurante.Backend.Dominio;

public sealed record DetallePedidoResumen(string Producto, string Tipo, int Cantidad,
    decimal PrecioUnitario, decimal Importe);
