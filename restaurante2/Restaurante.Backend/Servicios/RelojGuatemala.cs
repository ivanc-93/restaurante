namespace Restaurante.Backend.Servicios;
public static class RelojGuatemala
{
    public static DateTime Hoy => DateTime.UtcNow.AddHours(-6).Date;
    public static DateTime InicioUtc(DateTime fechaGuatemala) => fechaGuatemala.Date.AddHours(6);
}
