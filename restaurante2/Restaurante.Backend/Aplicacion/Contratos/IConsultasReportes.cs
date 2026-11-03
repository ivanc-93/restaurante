using Restaurante.Backend.Dominio;

namespace Restaurante.Backend.Aplicacion.Contratos;
/// <summary>Contrato de capacidad; la implementación SQL conserva sus guardas de seguridad.</summary>
public interface IConsultasReportes
{
    IReadOnlyList<Mesa> ListarMesas();
    IReadOnlyList<Cliente> ListarClientesHistoricos();
    IReadOnlyList<VentaPeriodo> ReportarVentasPeriodo(DateTime desde, DateTime hasta, string agrupacion);
    IReadOnlyList<PlatoVendido> ReportarPlatosMasVendidos(DateTime desde, DateTime hasta);
    IReadOnlyList<PedidoResumen> ReportarPedidos(DateTime desde, DateTime hasta, int? mesaId, int? clienteId);
    IReadOnlyList<MovimientoOperacion> ReportarOperaciones(DateTime desde, DateTime hasta);
    IReadOnlyList<VentaDiaria> ReportarVentas(DateTime desde, DateTime hasta);
}
