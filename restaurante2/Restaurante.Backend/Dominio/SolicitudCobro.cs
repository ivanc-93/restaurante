namespace Restaurante.Backend.Dominio;
public sealed record SolicitudCobro(Guid OperacionId,int PedidoId,string Version,decimal TotalEsperado,
    bool PublicoGeneral,string Receptor,string? Identificacion,string? Direccion,string? Correo,
    string Metodo,decimal Entregado,string ResultadoTarjeta,string? Referencia,string? Observacion,
    bool DatosConfirmados,bool PagoConfirmado,bool CortesiaAutorizada=false);
