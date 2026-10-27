namespace Restaurante.Backend.Dominio;
public sealed record ResultadoAutorizacion(EstadoAutorizacion Estado,string Mensaje)
{
    public bool Autorizada=>Estado==EstadoAutorizacion.Autorizada;
}
