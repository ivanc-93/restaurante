namespace Restaurante.Backend.Dominio;
public sealed record SolicitudGestionMesa(Guid OperacionId,int MesaId,int? PedidoId,string? Version,
    string Accion,string Motivo,bool SinPreparacionDeclarada);
