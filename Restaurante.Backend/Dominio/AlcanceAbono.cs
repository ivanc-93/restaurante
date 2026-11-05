namespace Restaurante.Backend.Dominio;
public sealed record AlcanceAbono(string Cuenta, decimal Original, decimal Descuento, decimal Total, decimal SaldoAntes, decimal SaldoDespues, IReadOnlyList<DetalleCuenta> Productos);
