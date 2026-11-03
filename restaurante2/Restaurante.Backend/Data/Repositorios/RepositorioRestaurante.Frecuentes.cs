using Restaurante.Backend.Dominio;

namespace Restaurante.Backend.Data.Repositorios;
public sealed partial class RepositorioRestaurante
{
    public ReglaFrecuente ConsultarReglaFrecuente()
    {
        using var c = AbrirConexion();
        SesionEn(c, null);
        ExigirCobranzaParcial(c, null);
        using var q = Consulta(c, null, "SELECT Activa,VisitasMinimas,Porcentaje,Desde,Hasta FROM dbo.ReglaFrecuente WHERE Id=1;");
        using var r = q.ExecuteReader();
        if (!r.Read())
            throw new InvalidOperationException("Falta configuración de promociones.");
        return new(r.GetBoolean(0), r.GetInt32(1), r.GetDecimal(2), r.GetDateTime(3), r.GetDateTime(4));
    }

    public string GuardarReglaFrecuente(SolicitudReglaFrecuente s)
    {
        var a = s.Regla;
        if (a is null || a.VisitasMinimas is < 1 or > 10000 || a.Porcentaje < 5 || a.Porcentaje > 50 || decimal.Round(a.Porcentaje, 2) != a.Porcentaje || a.Hasta.Date < a.Desde.Date)
            throw new ArgumentException("Revisa visitas mínimas, porcentaje de 5 a 50 y vigencia.");
        ExigirSesion(true);
        return Mutacion(s.OperacionId, "ReglaFrecuente", s, 0, true, (c, t, actor) =>
        {
            ExigirCobranzaParcial(c, t);
            Ejecutar(c, t, "UPDATE dbo.ReglaFrecuente SET Activa=@a,VisitasMinimas=@v,Porcentaje=@p,Desde=@d,Hasta=@h WHERE Id=1;", ("@a", a.Activa), ("@v", a.VisitasMinimas), ("@p", a.Porcentaje), ("@d", a.Desde.Date), ("@h", a.Hasta.Date));
            Seguridad(c, t, actor.Id, null, "ReglaFrecuente", $"Activa={a.Activa}; visitas={a.VisitasMinimas}; porcentaje={a.Porcentaje}; vigencia={a.Desde:yyyy-MM-dd}/{a.Hasta:yyyy-MM-dd}");
            return "Promoción guardada. No se aplica automáticamente ni se acumula con otros ajustes.";
        });
    }

    public ClienteFrecuente ConsultarFrecuencia(int clienteId)
    {
        ExigirSesion();
        var regla = ConsultarReglaFrecuente();
        using var c = AbrirConexion();
        using var q = Consulta(c, null, "SELECT COUNT(*) FROM dbo.Pedidos p JOIN dbo.Clientes cli ON cli.Id=p.ClienteId AND cli.Activo=1 JOIN dbo.Facturas f ON f.PedidoId=p.Id WHERE p.ClienteId=@c AND p.Estado=N'Facturado' AND f.Legada=0 AND f.Total>0 AND (EXISTS(SELECT 1 FROM dbo.PagosManuales pm WHERE pm.PedidoId=p.Id AND pm.Estado=N'Confirmado' AND pm.Importe=f.Total) OR COALESCE((SELECT SUM(a.Importe) FROM dbo.AbonosPedido a WHERE a.PedidoId=p.Id AND a.Estado=N'Confirmado'),0)=f.Total);", ("@c", clienteId));
        var visitas = (int)q.ExecuteScalar()!;
        var hoy = DateTime.UtcNow.AddHours(-6).Date;
        return new(clienteId, visitas, regla.Activa && visitas >= regla.VisitasMinimas && hoy >= regla.Desde.Date && hoy <= regla.Hasta.Date, regla.Porcentaje);
    }
}
