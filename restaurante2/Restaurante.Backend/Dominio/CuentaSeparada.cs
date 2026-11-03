namespace Restaurante.Backend.Dominio;
public sealed record CuentaSeparada(int Id, string Nombre, decimal Original, decimal Descuento, decimal Total, decimal Pagado, decimal Saldo, bool Operativa = false, bool Liquidada = false, bool NombreConfirmado = true);
