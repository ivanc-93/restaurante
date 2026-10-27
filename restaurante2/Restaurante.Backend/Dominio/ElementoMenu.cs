namespace Restaurante.Backend.Dominio;

public abstract class ElementoMenu
{
    protected ElementoMenu(int id, string nombre, string categoria, decimal precio)
    {
        Id = id;
        Nombre = nombre;
        Categoria = categoria;
        Precio = precio;
    }

    public int Id { get; }
    public string Nombre { get; }
    public string Categoria { get; }
    public decimal Precio { get; }
    public abstract string Tipo { get; }
    public decimal CalcularImporte(int cantidad)
    {
        if (cantidad is < 1 or > 50) throw new ArgumentOutOfRangeException(nameof(cantidad));
        return Precio * cantidad;
    }
    public override string ToString() => $"{Nombre} · {Restaurante.Backend.Servicios.MonedaGuatemala.Formatear(Precio)}";
}
