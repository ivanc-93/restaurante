namespace Restaurante.Backend.Dominio;
public sealed record SolicitudResolucionIncidencia(Guid OperacionId,int PedidoId,string Desenlace,string Motivo,SolicitudCobro? Cobro);
