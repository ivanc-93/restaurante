namespace Restaurante.Backend.Dominio;
public sealed record MovimientoOperacion(DateTime Fecha,int PedidoId,string Concepto,string Estado,decimal Importe,string Responsable,string Referencia);
