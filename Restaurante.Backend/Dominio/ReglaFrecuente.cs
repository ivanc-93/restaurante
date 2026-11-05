namespace Restaurante.Backend.Dominio;
public sealed record ReglaFrecuente(bool Activa, int VisitasMinimas, decimal Porcentaje, DateTime Desde, DateTime Hasta);
