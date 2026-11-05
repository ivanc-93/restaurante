using Restaurante.Backend.Dominio;

namespace Restaurante.Backend.Aplicacion.Contratos;
public interface ICuentasOperativas
{
    bool CuentasOperativasDisponibles() => false;
    ServicioMesa ConsultarServicioMesa(int mesaId) => throw new NotSupportedException();
    IReadOnlyList<CuentaOperativa> ConsultarCuentasPedido(int pedidoId) => throw new NotSupportedException();
    int GestionarCuentaMesa(SolicitudCuentaMesa s) => throw new NotSupportedException();
    ResultadoProductoCuenta AgregarProductoCuenta(SolicitudProductoCuenta s) => throw new NotSupportedException();
    string TraspasarCuenta(SolicitudTraspasoCuenta s) => throw new NotSupportedException();
    decimal AplicarBeneficioCuenta(SolicitudBeneficioCuenta s) => throw new NotSupportedException();
    decimal CorregirCuenta(SolicitudCorreccionCuenta s) => throw new NotSupportedException();
}
