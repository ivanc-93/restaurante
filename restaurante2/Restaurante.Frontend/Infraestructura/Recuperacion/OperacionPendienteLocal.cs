namespace Restaurante.Frontend.Infraestructura.Recuperacion;
internal sealed record OperacionPendienteLocal(Guid Id, string Base, int UsuarioId, string Tipo, string Datos);
