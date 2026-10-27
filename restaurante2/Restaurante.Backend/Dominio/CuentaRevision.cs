namespace Restaurante.Backend.Dominio;
public sealed record CuentaRevision(int PedidoId,int MesaId,int NumeroMesa,int? ClienteId,string Cliente,
    string Estado,decimal Total,string Version,IReadOnlyList<DetallePedidoResumen> Detalles);
