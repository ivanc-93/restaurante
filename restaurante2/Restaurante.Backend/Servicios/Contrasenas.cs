using System.Security.Cryptography;
using System.Text;
namespace Restaurante.Backend.Servicios;

public static class Contrasenas
{
    public const int Iteraciones = 600000;
    public static (string Hash,string Sal) Crear(string clave)
    {
        if(string.IsNullOrWhiteSpace(clave)||clave.Length<12||clave.Length>128)
            throw new ArgumentException("La contraseña debe tener entre 12 y 128 caracteres.");
        var sal=RandomNumberGenerator.GetBytes(24);
        return(Convert.ToBase64String(Rfc2898DeriveBytes.Pbkdf2(clave,sal,Iteraciones,HashAlgorithmName.SHA256,32)),Convert.ToBase64String(sal));
    }
    public static bool Verificar(string clave,string hash,string sal,string algoritmo,int iteraciones)
    {
        try
        {
            var calculado=algoritmo=="PBKDF2-SHA256" && iteraciones>=100000 && iteraciones<=2000000
                ? Rfc2898DeriveBytes.Pbkdf2(clave,Convert.FromBase64String(sal),iteraciones,HashAlgorithmName.SHA256,32)
                : algoritmo=="SHA256" ? SHA256.HashData(Encoding.UTF8.GetBytes($"{sal}:{clave}")) : [];
            var esperado=algoritmo=="SHA256"?Convert.FromHexString(hash):Convert.FromBase64String(hash);
            return calculado.Length==32 && CryptographicOperations.FixedTimeEquals(calculado,esperado);
        }
        catch(FormatException){return false;}
    }
}
