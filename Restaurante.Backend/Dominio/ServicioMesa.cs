namespace Restaurante.Backend.Dominio;
public sealed record ServicioMesa(Mesa Mesa, int? PedidoId, string? VersionPedido, bool Operativa, IReadOnlyList<CuentaOperativa> Cuentas);
