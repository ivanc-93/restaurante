namespace Restaurante.Backend.Dominio;
public sealed record LineaPedidoCorregible(int Id,string Producto,int CantidadOriginal,int CantidadVigente,decimal PrecioUnitario);
