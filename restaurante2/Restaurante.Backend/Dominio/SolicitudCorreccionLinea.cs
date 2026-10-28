namespace Restaurante.Backend.Dominio;
public sealed record SolicitudCorreccionLinea(Guid OperacionId,int PedidoId,int DetalleId,int CantidadRetirada,string Version,decimal TotalEsperado,string Motivo,bool SinPreparacionDeclarada);
