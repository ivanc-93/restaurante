namespace Restaurante.Backend.Dominio;
public sealed record ResumenCobranza(CuentaRevision Pedido, decimal Pagado, decimal Saldo, bool Congelada, IReadOnlyList<CuentaSeparada> Cuentas, IReadOnlyList<AbonoResumen> Abonos);
