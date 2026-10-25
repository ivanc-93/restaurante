namespace Restaurante.Backend.Dominio;

public sealed record ResumenRestaurante(int MesasDisponibles, int PedidosAbiertos,
    decimal VentasDelDia, int ProductosActivos);
