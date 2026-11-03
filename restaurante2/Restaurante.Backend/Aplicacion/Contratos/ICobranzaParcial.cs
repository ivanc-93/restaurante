using Restaurante.Backend.Dominio;

namespace Restaurante.Backend.Aplicacion.Contratos;
public interface ICobranzaParcial
{
    decimal AplicarBeneficio(SolicitudBeneficio s) => throw new NotSupportedException();
    bool CobranzaParcialDisponible() => false;
    ResumenCobranza ConsultarCobranza(int pedidoId) => throw new NotSupportedException();
    string DividirCuenta(SolicitudDivision s) => throw new NotSupportedException();
    ResultadoAbono RegistrarAbono(SolicitudAbono s) => throw new NotSupportedException();
    ResultadoAbono ConciliarAbono(SolicitudConciliacionAbono s) => throw new NotSupportedException();
    string ConsultarReciboAbono(Guid id) => throw new NotSupportedException();
    ReglaFrecuente ConsultarReglaFrecuente() => throw new NotSupportedException();
    string GuardarReglaFrecuente(SolicitudReglaFrecuente s) => throw new NotSupportedException();
    ClienteFrecuente ConsultarFrecuencia(int clienteId) => throw new NotSupportedException();
}
