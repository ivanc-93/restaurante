using System.Text.Json;
using Microsoft.Data.SqlClient;
using Restaurante.Backend.Dominio;

namespace Restaurante.Backend.Data.Repositorios;
public sealed partial class RepositorioRestaurante
{
    private static bool EsCuentaOperativa(SqlConnection c, SqlTransaction t, int id)
    {
        if (!TieneCuentasOperativas(c, t))
            return false;
        using var q = Consulta(c, t, "SELECT Operativa FROM dbo.CuentasPedido WHERE Id=@id;", ("@id", id));
        return q.ExecuteScalar()is bool b && b;
    }

    private static void CapturarAlcanceAbono(SqlConnection c, SqlTransaction t, SolicitudAbono s, string estado)
    {
        var cuenta = CuentasEn(c, t, MesaDePedidoEn(c, t, s.PedidoId), s.PedidoId).Single(x => x.Id == s.CuentaId);
        var antes = cuenta.Saldo + (estado == "Confirmado" ? s.Importe : 0);
        var despues = cuenta.Saldo;
        var alcance = new AlcanceAbono(cuenta.Nombre, cuenta.Original, cuenta.Descuento, cuenta.Total, antes, despues, cuenta.Productos);
        Ejecutar(c, t, "UPDATE dbo.AbonosPedido SET AlcanceSnapshot=@json,SaldoAntes=@a,SaldoDespues=@d WHERE OperacionId=@id;", ("@json", JsonSerializer.Serialize(alcance)), ("@a", antes), ("@d", despues), ("@id", s.OperacionId));
    }

    private static int MesaDePedidoEn(SqlConnection c, SqlTransaction t, int pedido)
    {
        using var q = Consulta(c, t, "SELECT MesaId FROM dbo.Pedidos WHERE Id=@p;", ("@p", pedido));
        return (int)q.ExecuteScalar()!;
    }

    private static string TextoAlcanceAbono(SqlConnection c, Guid id)
    {
        if (!TieneCuentasOperativas(c, null))
            return "";
        using var q = Consulta(c, null, "SELECT AlcanceSnapshot,SaldoAntes,SaldoDespues FROM dbo.AbonosPedido WHERE OperacionId=@id;", ("@id", id));
        using var r = q.ExecuteReader();
        if (!r.Read() || r.IsDBNull(0))
            return "\r\nRegistro anterior: alcance conservado por importe, sin asignación de productos inventada.";
        var a = JsonSerializer.Deserialize<AlcanceAbono>(r.GetString(0))!;
        return $"\r\n\r\nCUENTA: {a.Cuenta}\r\nConsumo original: {MonedaGuatemala.Formatear(a.Original)}\r\nAjustes: {MonedaGuatemala.Formatear(a.Descuento)}\r\nTotal de la cuenta: {MonedaGuatemala.Formatear(a.Total)}\r\nSaldo antes: {MonedaGuatemala.Formatear(r.GetDecimal(1))}\r\nSaldo después del resultado registrado: {MonedaGuatemala.Formatear(r.GetDecimal(2))}\r\nPRODUCTOS DEL ALCANCE PROTEGIDO\r\n" + string.Join("\r\n", a.Productos.Select(d => $"{d.Cantidad} × {d.Producto} · {MonedaGuatemala.Formatear(d.Importe)}")) + "\r\nEl abono cubre importe sobre esta cuenta; no asigna automáticamente unidades individuales como pagadas.";
    }
}
