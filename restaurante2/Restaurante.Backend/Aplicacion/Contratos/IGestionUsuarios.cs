using Restaurante.Backend.Dominio;

namespace Restaurante.Backend.Aplicacion.Contratos;
/// <summary>Contrato de capacidad; la implementación SQL conserva sus guardas de seguridad.</summary>
public interface IGestionUsuarios : ICodigosEmergencia
{
    IReadOnlyList<UsuarioGestion> ListarUsuarios();
    IReadOnlyList<RegistroSeguridad> ConsultarAuditoriaSeguridad();
    void GuardarUsuario(int? id, string nombre, string usuario, string rol, bool activo, string? nuevaClave);
}
