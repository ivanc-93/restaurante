using System.Text.Json;
using Restaurante.Backend.Dominio;

namespace Restaurante.Frontend.Infraestructura.Recuperacion;
internal static class AlcancePendiente
{
    internal static bool Afecta(OperacionPendienteLocal p, int mesaId, int? pedidoId)
    {
        if (p.Tipo == "Ilegible")
            return true;
        if (p.Tipo == "ReglaFrecuente")
            return false;
        try
        {
            using var d = JsonDocument.Parse(p.Datos);
            var r = d.RootElement;
            if (r.ValueKind == JsonValueKind.Number)
                return !pedidoId.HasValue || r.GetInt32() == pedidoId;
            if (r.TryGetProperty("PedidoId", out var pedido) && pedido.ValueKind == JsonValueKind.Number && pedido.GetInt32() > 0)
                return pedidoId.HasValue && pedido.GetInt32() == pedidoId;
            if (r.TryGetProperty("MesaId", out var mesa) && mesa.ValueKind == JsonValueKind.Number)
                return mesa.GetInt32() == mesaId;
        }
        catch (JsonException)
        {
            return true;
        }

        return true;
    }
}
