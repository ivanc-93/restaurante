namespace Restaurante.Backend.Dominio;

public sealed record Cliente(int Id, string Nombre, string? Telefono, string? Correo)
{
    public override string ToString() => Nombre;
}
