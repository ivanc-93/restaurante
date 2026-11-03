using System.Text.Json;
using Restaurante.Backend.Dominio;

namespace Restaurante.Frontend.Presentacion;
internal static class PresentacionPendiente
{
    public static string Resumen(OperacionPendienteLocal p)
    {
        if (p.Tipo == "Ilegible")
            return $"Registro local ilegible: {p.Datos}\r\nSe conserva para revisión técnica. Los otros registros pueden revisarse. No intentes repetir su operación sin comprobar SQL.";
        using var doc = JsonDocument.Parse(p.Datos);
        var r = doc.RootElement;
        if (r.ValueKind != JsonValueKind.Object)
            return $"Acción: {p.Tipo} · Pedido: {r} · Identificador: {p.Id}. Se recuperará la misma solicitud.";
        string V(string n) => r.TryGetProperty(n, out var v) ? v.ToString() : "No aplica";
        if (p.Tipo is "Reservar" or "CancelarReserva" or "AtenderReserva")
            return $"Acción: {p.Tipo}\r\nMesa: {V("MesaId")} · Reserva: {V("ReservaId")}\r\nTitular: {V("Nombre")}\r\nMotivo: {V("Motivo")}\r\nIdentificador: {p.Id}\r\nSe recuperan exactamente los datos originales; no se crea otra operación.";
        var importe = r.TryGetProperty("TotalEsperado", out var total) ? MonedaGuatemala.Formatear(total.GetDecimal()) : "Se comprobará en SQL";
        return $"Acción: {p.Tipo}\r\nPedido: {V("PedidoId")} · Mesa: {V("MesaId")}\r\nImporte: {importe}\r\nMotivo: {V("Motivo")}\r\nIdentificador de recuperación: {p.Id}\r\nSe conservarán exactamente los datos originales. Si SQL ya aplicó la solicitud, solo se recupera su resultado.";
    }

    public static (string Tipo, object Datos)? Autorizacion(OperacionPendienteLocal p) => p.Tipo switch
    {
        "Beneficio" => ("Beneficio", JsonSerializer.Deserialize<SolicitudBeneficio>(p.Datos)!),
        "ConciliarAbono" => ("ConciliarAbono", JsonSerializer.Deserialize<SolicitudConciliacionAbono>(p.Datos)!),
        "CancelarReserva" => ("CancelarReserva", JsonSerializer.Deserialize<SolicitudCancelacionReserva>(p.Datos)!),
        "Ajuste" => ("Ajuste", JsonSerializer.Deserialize<SolicitudAjuste>(p.Datos)!),
        "Gestionar" => ("GestionMesa", JsonSerializer.Deserialize<SolicitudGestionMesa>(p.Datos)!),
        "Conciliar" => ("ConciliarTarjeta", JsonSerializer.Deserialize<SolicitudConciliacion>(p.Datos)!),
        "Corregir" => ("CorreccionLinea", JsonSerializer.Deserialize<SolicitudCorreccionLinea>(p.Datos)!),
        _ => null
    };
}
