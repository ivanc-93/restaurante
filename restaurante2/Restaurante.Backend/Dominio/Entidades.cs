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
    public virtual decimal CalcularImporte(int cantidad) => Precio * cantidad;
    public override string ToString() => $"{Nombre} · {Precio:C}";
}

public sealed class Plato(int id, string nombre, string categoria, decimal precio)
    : ElementoMenu(id, nombre, categoria, precio)
{
    public override string Tipo => "Plato";
}

public sealed class Bebida(int id, string nombre, string categoria, decimal precio, bool alcoholica)
    : ElementoMenu(id, nombre, categoria, precio)
{
    public bool Alcoholica { get; } = alcoholica;
    public override string Tipo => "Bebida";
    public override decimal CalcularImporte(int cantidad) =>
        Precio * cantidad;
}

public sealed record Mesa(int Id, int Numero, int Capacidad, string Estado)
{
    public override string ToString() => $"Mesa {Numero} · {Capacidad} lugares";
}

public sealed record Cliente(int Id, string Nombre, string? Telefono, string? Correo)
{
    public override string ToString() => Nombre;
}

public sealed record PedidoResumen(int Id, int NumeroMesa, string Cliente, DateTime Fecha,
    string Estado, decimal Total, string? NumeroFactura);
public sealed record DetallePedidoResumen(string Producto, string Tipo, int Cantidad,
    decimal PrecioUnitario, decimal Importe);
public sealed record PedidoCreado(int Id, decimal Total, string? NumeroFactura);
public sealed record ResumenRestaurante(int MesasDisponibles, int PedidosAbiertos,
    decimal VentasDelDia, int ProductosActivos);
public sealed record VentaDiaria(DateTime Fecha, int Pedidos, decimal Total);
public sealed record PlatoVendido(string Nombre, int Cantidad, decimal Total);

public sealed class SolicitudPedido
{
    public int MesaId { get; init; }
    public int? ClienteId { get; init; }
    public IReadOnlyList<DetallePedidoSolicitud> Detalles { get; init; } = [];
}

public sealed record DetallePedidoSolicitud(int PlatoId, int Cantidad);
public sealed record UsuarioAutenticado(string Nombre, string Rol);
