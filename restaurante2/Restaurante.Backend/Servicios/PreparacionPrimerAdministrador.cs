namespace Restaurante.Backend.Servicios;

// The single-use secret remains in memory and is never exposed to the UI or serializers.
public sealed class PreparacionPrimerAdministrador : IDisposable
{
    private string? token;
    private readonly IConfiguracionConexion configuracion;
    public string RutaRespaldo { get; }
    internal PreparacionPrimerAdministrador(IConfiguracionConexion configuracion, string token, string rutaRespaldo)
    { this.configuracion = configuracion; this.token = token; RutaRespaldo = rutaRespaldo; }
    public void CrearAdministrador(string nombre, string usuario, string clave)
    {
        if (token is null) throw new InvalidOperationException("Prepare nuevamente la instalación con Windows.");
        new ServicioInstalacion(configuracion).CrearPrimerAdministrador(token, nombre, usuario, clave);
        token = null;
    }
    public void Dispose() => token = null;
}
