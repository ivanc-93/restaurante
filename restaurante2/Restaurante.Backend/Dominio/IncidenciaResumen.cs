namespace Restaurante.Backend.Dominio;
public sealed record IncidenciaResumen(int PedidoId,int Mesa,string Cliente,decimal Importe,string Motivo,string Estado,string? Desenlace);
