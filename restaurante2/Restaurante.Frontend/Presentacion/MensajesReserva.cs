namespace Restaurante.Frontend.Presentacion;
internal static class MensajesReserva
{
    internal static string Error(Exception error)
    {
        if (error is ArgumentException or UnauthorizedAccessException or InvalidOperationException)
            return error.Message;
        System.Diagnostics.Trace.TraceError("Reserva: fallo técnico {0}", error.GetType().Name);
        return "No se pudo confirmar la operación con SQL. Comprueba la conexión y revisa Recuperar operaciones antes de repetir.";
    }
}
