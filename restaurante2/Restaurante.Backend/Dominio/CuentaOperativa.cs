namespace Restaurante.Backend.Dominio;
public sealed record CuentaOperativa(int Id, string Nombre, bool NombreConfirmado, bool Liquidada, decimal Original, decimal Descuento, decimal Pagado, string Version, IReadOnlyList<DetalleCuenta> Productos)
{
    public decimal Total => Original - Descuento;
    public decimal Saldo => Total - Pagado;
    public string Etiqueta => Id < 0 ? "Toda la mesa" : $"{Nombre}{(Id > 0 ? " · #" + Id : "")} · {(Liquidada ? "Liquidada" : Pagado > 0 ? "Pago parcial" : "Abierta")} · {MonedaGuatemala.Formatear(Saldo)}";
}
