using System.Net.Mail;

namespace Restaurante.Backend.Servicios;

public static class ReglasRestaurante
{
    public static string Texto(string? valor, string campo, int maximo)
    {
        var texto = valor?.Trim() ?? "";
        if (texto.Length == 0 || texto.Length > maximo)
            throw new ArgumentException($"{campo}: ingrese entre 1 y {maximo} caracteres.");
        return texto;
    }

    public static void Cliente(string nombre, string? telefono, string? correo)
    {
        Texto(nombre, "Nombre", 120);
        if (telefono?.Trim().Length > 30) throw new ArgumentException("Teléfono: máximo 30 caracteres.");
        if (!string.IsNullOrWhiteSpace(correo) &&
            (correo.Trim().Length > 150 || !MailAddress.TryCreate(correo.Trim(), out var direccion) || direccion.Address != correo.Trim()))
            throw new ArgumentException("Ingrese un correo válido de hasta 150 caracteres.");
    }

    public static void Producto(string nombre, string categoria, string tipo, decimal precio, bool alcoholica)
    {
        Texto(nombre, "Nombre", 120);
        Texto(categoria, "Categoría", 60);
        if (tipo is not ("Plato" or "Bebida")) throw new ArgumentException("Seleccione Plato o Bebida.");
        if (alcoholica && tipo != "Bebida") throw new ArgumentException("Solo una bebida puede ser alcohólica.");
        if (precio < 0 || precio > 99999999.99m || decimal.Round(precio, 2) != precio)
            throw new ArgumentException("Precio: de 0 a 99,999,999.99 con máximo dos decimales.");
    }

    public static (DateTime Desde, DateTime HastaExclusivo) Fechas(DateTime desde, DateTime hasta)
    {
        if (desde.Date > hasta.Date) throw new ArgumentException("La fecha inicial no puede superar la final.");
        if (hasta.Date == DateTime.MaxValue.Date) throw new ArgumentException("Fecha final fuera de rango.");
        return (desde.Date, hasta.Date.AddDays(1));
    }

    public static DateTime InicioPeriodo(DateTime fecha, string agrupacion) => agrupacion switch
    {
        "Día" => fecha.Date,
        "Semana" => fecha.Date.AddDays(-(((int)fecha.DayOfWeek + 6) % 7)),
        "Mes" => new DateTime(fecha.Year, fecha.Month, 1),
        _ => throw new ArgumentException("Agrupación inválida.")
    };
}
