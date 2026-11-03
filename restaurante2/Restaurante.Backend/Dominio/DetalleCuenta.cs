namespace Restaurante.Backend.Dominio;
public sealed record DetalleCuenta(int Id, string Producto, string Tipo, int Cantidad, decimal PrecioUnitario, decimal Importe);
