namespace Restaurante.Backend.Dominio;

public sealed record Mesa(int Id, int Numero, int Capacidad, string Estado)
{
    public override string ToString() => $"Mesa {Numero} · {Capacidad} lugares";
}
