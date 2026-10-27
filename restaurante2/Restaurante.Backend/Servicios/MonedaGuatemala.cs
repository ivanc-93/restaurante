using System.Globalization;
namespace Restaurante.Backend.Servicios;
public static class MonedaGuatemala
{
    public static string Formatear(decimal importe) => "Q " + importe.ToString("N2",CultureInfo.InvariantCulture);
}
