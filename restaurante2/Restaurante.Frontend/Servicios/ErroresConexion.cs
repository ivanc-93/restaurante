using Microsoft.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace Restaurante.Frontend.Servicios;

internal static class ErroresConexion
{
    internal static string Describir(Exception error, bool validandoEsquema = false)
    {
        var cadena = Cadena(error).ToArray();
        if (cadena.Any(e => e is DllNotFoundException or BadImageFormatException or FileLoadException))
            return "No se pudo cargar un componente local de conexión SQL. Compile y ejecute desde una ruta corta fuera de OneDrive; compruebe las dependencias de Microsoft.Data.SqlClient y la arquitectura del equipo.";
        var numeros = cadena.OfType<SqlException>().SelectMany(e => e.Errors.Cast<SqlError>()).Select(e => e.Number).ToArray();
        if (numeros.Contains(4060) || numeros.Contains(916))
            return "No se puede abrir la base de datos indicada. Puede no existir o su cuenta puede no tener acceso. Revise el nombre y solicite al responsable de SQL que compruebe los permisos.";
        if (numeros.Any(n => n is 18456 or 18452 or 229 or 230) || cadena.Any(e => e is UnauthorizedAccessException))
            return "No se pudo autorizar el acceso. Revise la autenticación y las credenciales; solicite los permisos necesarios. Para guardar una conexión existente necesita un administrador de Sabores.";
        if (numeros.Any(n => n is -2 or 258) || cadena.Any(e => e is TimeoutException || e is System.ComponentModel.Win32Exception w && w.NativeErrorCode == 258))
            return "Se agotó el tiempo de espera al conectar con SQL Server. Compruebe que el servidor esté iniciado y accesible e inténtelo nuevamente.";
        if (validandoEsquema && (cadena.Any(e => e is InvalidOperationException) || numeros.Any(n => n is 208 or 207)))
            return "La base tiene un esquema incompatible o incompleto. Solicite una revisión y una actualización con respaldo. Sabores no crea ni migra bases automáticamente.";
        if (numeros.Any(n => n is -1 or 0 or 2 or 26 or 40 or 53 or 64 or 233 or 10060 or 10061 or 11001))
            return "No se pudo conectar con el servidor o la instancia de SQL Server. Revise el nombre, que el servicio esté iniciado y el acceso de red. Consulte el diagnóstico si el problema continúa.";
        if (numeros.Length > 0)
            return "SQL Server rechazó la operación de conexión o validación. Solicite al responsable que revise el diagnóstico y la configuración del servidor.";
        if (cadena.Any(e => e is IOException))
            return "No se pudo leer o guardar la configuración local. Compruebe los permisos y el espacio disponible e inténtelo nuevamente.";
        return "No fue posible completar la configuración de conexión. Revise los datos e inténtelo nuevamente; consulte el diagnóstico si continúa.";
    }

    internal static string Registrar(Exception error)
    {
        var referencia = Guid.NewGuid().ToString("N");
        try
        {
            var carpeta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SaboresRestaurante", "Diagnosticos");
            Directory.CreateDirectory(carpeta);
            // Never record Message, Data or SQL text: these may contain secrets or user input.
            var registro = new
            {
                Referencia = referencia, FechaUtc = DateTime.UtcNow,
                Arquitectura = RuntimeInformation.ProcessArchitecture.ToString(),
                RutaAplicacionLongitud = AppContext.BaseDirectory.Length,
                Errores = Cadena(error).Select(e => new
                {
                    Tipo = e.GetType().FullName, e.HResult,
                    CodigoNativo = e is System.ComponentModel.Win32Exception win32 ? (int?)win32.NativeErrorCode : null,
                    Metodos = new System.Diagnostics.StackTrace(e, false).GetFrames()?.Select(f =>
                        f.GetMethod()?.DeclaringType?.FullName + "." + f.GetMethod()?.Name).ToArray(),
                    CodigosSql = e is SqlException sql ? sql.Errors.Cast<SqlError>().Select(s => new { s.Number, s.State, s.Class }).ToArray() : null
                }).ToArray()
            };
            File.AppendAllText(Path.Combine(carpeta, "conexion-" + DateTime.UtcNow.ToString("yyyyMMdd") + ".jsonl"), JsonSerializer.Serialize(registro) + Environment.NewLine);
            return " Diagnóstico: " + referencia + " (carpeta Diagnosticos de SaboresRestaurante en los datos locales de Windows).";
        }
        catch (Exception) { return " No se pudo guardar el diagnóstico local. Referencia: " + referencia + "."; }
    }

    private static IEnumerable<Exception> Cadena(Exception error)
    {
        yield return error;
        if (error is AggregateException aggregate)
        {
            foreach (var inner in aggregate.InnerExceptions)
                foreach (var item in Cadena(inner)) yield return item;
        }
        else if (error.InnerException is not null)
            foreach (var item in Cadena(error.InnerException)) yield return item;
    }
}
