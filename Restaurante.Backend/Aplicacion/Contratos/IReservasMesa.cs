using Restaurante.Backend.Dominio;

namespace Restaurante.Backend.Aplicacion.Contratos;
public interface IReservasMesa
{
    bool ReservasDisponibles() => false;
    IReadOnlyList<ReservaResumen> ListarReservasActivas() => [];
    ReservaResumen? ConsultarReservaMesa(int mesaId) => throw new NotSupportedException();
    ReservaResumen ReservarMesa(SolicitudReserva solicitud) => throw new NotSupportedException();
    PedidoCreado AtenderReserva(SolicitudAtencionReserva solicitud) => throw new NotSupportedException();
    string CancelarReserva(SolicitudCancelacionReserva solicitud) => throw new NotSupportedException();
}
