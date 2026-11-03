namespace Restaurante.Backend.Aplicacion;
public sealed record EstadoInstalacionInicial(bool TieneUsuarios, bool Completada, bool TieneToken, bool AutorizadoWindows)
{
    public bool Pendiente => !TieneUsuarios && !Completada;
}
