using System.Text.RegularExpressions;

namespace Restaurante.Backend.Dominio.Reglas;
/// <summary>Evita confundir una referencia externa con un número completo de tarjeta.</summary>
public static class ReferenciaPago
{
    public static void Validar(string? texto)
    {
        if (string.IsNullOrEmpty(texto))
            return;
        foreach (Match coincidencia in Regex.Matches(texto, @"[0-9](?:[ -]?[0-9]){12,18}"))
        {
            var digitos = coincidencia.Value.Where(c => c is >= '0' and <= '9').Select(c => c - '0').ToArray();
            int suma = 0;
            bool duplicar = false;
            for (int i = digitos.Length - 1; i >= 0; i--)
            {
                int valor = digitos[i];
                if (duplicar)
                {
                    valor *= 2;
                    if (valor > 9)
                        valor -= 9;
                }

                suma += valor;
                duplicar = !duplicar;
            }

            if (suma % 10 == 0)
                throw new ArgumentException("No ingreses números completos de tarjeta. Utiliza una referencia de operación; nunca PAN, CVV ni PIN.");
        }
    }
}
