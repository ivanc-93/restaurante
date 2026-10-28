namespace Restaurante.Backend.Servicios;
public static class ValidacionAjuste
{
    public static void Comprobar(string tipo,decimal descuento,decimal original)
    {
        if(tipo is not("Descuento" or "Cortesía")||descuento<0||decimal.Round(descuento,2)!=descuento)
            throw new ArgumentException("Indique un ajuste no negativo con hasta dos decimales.");
        if(descuento>original||tipo=="Cortesía"&&descuento!=original||tipo=="Descuento"&&descuento==original)
            throw new ArgumentException("El descuento parcial debe ser menor al consumo original. Para cubrirlo completo seleccione Cortesía.");
    }
}
