using Restaurante.Backend.Dominio;

namespace Restaurante.Backend.Aplicacion.Contratos;
/// <summary>Solo las operaciones de custodia del código; no expone usuarios ni ventas.</summary>
public interface ICodigosEmergencia
{
    ResultadoGeneracionCodigo IntentarGenerarCodigoRecuperacion(string claveActual) => new(EstadoGeneracionCodigo.NoDisponible, "Este repositorio no admite generación guiada de códigos.");
    ResultadoRecuperacion IntentarConfirmarCodigoRecuperacion(string codigo) => new(false, "Este repositorio no admite confirmación guiada del código.");
}
