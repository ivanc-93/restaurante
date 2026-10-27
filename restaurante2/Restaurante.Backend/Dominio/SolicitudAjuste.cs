namespace Restaurante.Backend.Dominio;
public sealed record SolicitudAjuste(Guid OperacionId,int PedidoId,string Version,decimal TotalEsperado,
    string Tipo,decimal Descuento,string Motivo);
