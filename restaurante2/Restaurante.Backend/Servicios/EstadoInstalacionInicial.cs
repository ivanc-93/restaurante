namespace Restaurante.Backend.Servicios;

public sealed record EstadoInstalacionInicial(bool TieneUsuarios, bool Completada, bool TieneToken, bool AutorizadoWindows)
{
    public bool Pendiente => !TieneUsuarios && !Completada;
}
