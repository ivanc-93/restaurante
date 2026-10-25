namespace Restaurante.Backend.Dominio;

public sealed class Plato(int id, string nombre, string categoria, decimal precio)
    : ElementoMenu(id, nombre, categoria, precio)
{
    public override string Tipo => "Plato";
}
