namespace Restaurante.Backend.Dominio;
public sealed record SolicitudDivision(Guid OperacionId, int PedidoId, string Version, decimal TotalEsperado, IReadOnlyList<ParteCuenta> Partes, IReadOnlyList<AsignacionProducto> Productos);
