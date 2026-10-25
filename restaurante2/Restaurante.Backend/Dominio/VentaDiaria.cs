namespace Restaurante.Backend.Dominio;

public sealed record VentaDiaria(DateTime Fecha, int Pedidos, decimal Total);
