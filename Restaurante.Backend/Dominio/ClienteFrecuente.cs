namespace Restaurante.Backend.Dominio;
public sealed record ClienteFrecuente(int ClienteId, int Visitas, bool Elegible, decimal Porcentaje);
