namespace Restaurante.Backend.Dominio;

public sealed class Bebida(int id, string nombre, string categoria, decimal precio, bool alcoholica)
    : ElementoMenu(id, nombre, categoria, precio)
{
    public bool Alcoholica { get; } = alcoholica;
    public override string Tipo => "Bebida";

}
