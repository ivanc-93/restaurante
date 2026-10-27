namespace Restaurante.Backend.Dominio;

public sealed record UsuarioAutenticado(string Nombre, string Rol, int Id = 0);
