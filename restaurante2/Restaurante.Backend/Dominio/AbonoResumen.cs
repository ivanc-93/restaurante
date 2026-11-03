namespace Restaurante.Backend.Dominio;
public sealed record AbonoResumen(Guid OperacionId, int? CuentaId, string Estado, string Metodo, decimal Importe, decimal Entregado, decimal Cambio, string Receptor, DateTime Fecha);
