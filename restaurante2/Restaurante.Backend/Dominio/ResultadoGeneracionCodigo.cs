namespace Restaurante.Backend.Dominio;
public sealed record ResultadoGeneracionCodigo(EstadoGeneracionCodigo Estado,string Mensaje,string? Codigo=null)
{
    public bool Generado=>Estado==EstadoGeneracionCodigo.Generado;
}
