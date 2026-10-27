namespace Restaurante.Backend.Dominio;
public sealed record SolicitudConciliacion(Guid OperacionId,Guid OperacionIncierta,bool Confirmado,string Motivo);
