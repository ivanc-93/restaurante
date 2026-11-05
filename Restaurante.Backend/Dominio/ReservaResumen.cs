namespace Restaurante.Backend.Dominio;
public sealed record ReservaResumen(int Id, int MesaId, int NumeroMesa, string Nombre, string Estado, DateTime Fecha, int UsuarioId, string Version);
