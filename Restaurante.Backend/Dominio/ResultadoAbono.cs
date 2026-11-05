namespace Restaurante.Backend.Dominio;
public sealed record ResultadoAbono(Guid OperacionId, string Estado, decimal Aplicado, decimal Cambio, decimal Saldo, string? Comprobante);
