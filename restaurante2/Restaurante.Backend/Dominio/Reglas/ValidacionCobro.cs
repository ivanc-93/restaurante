using Restaurante.Backend.Dominio;

namespace Restaurante.Backend.Dominio.Reglas;
/// <summary>Valida una solicitud manual sin depender de formularios ni de SQL.</summary>
public static class ValidacionCobro
{
    public static void Comprobar(SolicitudCobro s)
    {
        if (!s.DatosConfirmados)
            throw new ArgumentException("Confirme que revisó los datos de la cuenta y del receptor.");
        if (!s.PublicoGeneral)
            ReglasRestaurante.Texto(s.Receptor, "Nombre del receptor", 120);
        if (s.PublicoGeneral && !string.IsNullOrWhiteSpace(s.Identificacion))
            throw new ArgumentException("Público general no debe incluir identificación; seleccione receptor identificado.");
        if (s.Identificacion?.Trim().Length > 40 || s.Direccion?.Trim().Length > 250 || s.Referencia?.Trim().Length > 100 || s.Observacion?.Trim().Length > 250)
            throw new ArgumentException("Alguno de los campos supera su longitud máxima.");
        if (!s.PublicoGeneral && s.Identificacion is not null && s.Identificacion.Any(char.IsControl))
            throw new ArgumentException("La identificación contiene caracteres inválidos.");
        ReglasRestaurante.Cliente(s.PublicoGeneral ? "Público general" : s.Receptor, null, s.Correo);
        if (s.Entregado < 0 || s.Entregado > 9999999999.99m || decimal.Round(s.Entregado, 2) != s.Entregado)
            throw new ArgumentException("Importe entregado fuera de rango o con más de dos decimales.");
        if (s.Metodo is not ("Efectivo" or "Tarjeta" or "Cortesía"))
            throw new ArgumentException("Seleccione efectivo, tarjeta o cortesía autorizada.");
        if (s.Metodo == "Cortesía" && (!s.CortesiaAutorizada || s.Entregado != 0))
            throw new ArgumentException("Confirme cortesía de total cero.");
        if (s.Metodo == "Tarjeta")
        {
            if (s.ResultadoTarjeta is not ("Confirmado" or "Rechazado" or "Incierto"))
                throw new ArgumentException("Seleccione el resultado real del terminal externo.");
            if (s.ResultadoTarjeta == "Confirmado" && !s.PagoConfirmado)
                throw new ArgumentException("El operador debe confirmar que comprobó el pago externo.");
            if (s.ResultadoTarjeta is "Confirmado" or "Incierto")
                ReglasRestaurante.Texto(s.Referencia, "Referencia del terminal o comprobante", 100);
        }
        else if (!s.PagoConfirmado)
            throw new ArgumentException("Confirme el efectivo recibido o la cortesía autorizada.");
    }
}
