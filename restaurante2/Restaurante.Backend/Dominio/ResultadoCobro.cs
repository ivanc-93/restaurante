namespace Restaurante.Backend.Dominio;
public sealed record ResultadoCobro(Guid OperacionId,string Estado,string? NumeroComprobante,decimal Total,decimal Cambio);
