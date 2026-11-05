using Restaurante.Backend.Dominio;

namespace Restaurante.Backend.Data.Repositorios;
public sealed partial class RepositorioRestaurante
{
    public decimal AplicarBeneficio(SolicitudBeneficio s)
    {
        ReglasRestaurante.Texto(s.Motivo, "Motivo", 400);
        if (s.Tipo is not ("Descuento" or "Frecuente" or "Cortesía") || s.Tipo == "Cortesía" && s.Porcentaje != 100 || s.Tipo != "Cortesía" && (s.Porcentaje < 5 || s.Porcentaje > 50) || decimal.Round(s.Porcentaje, 2) != s.Porcentaje)
            throw new ArgumentException("Descuento: de 5 a 50 %. Cortesía: exactamente 100 %.");
        return Mutacion(s.OperacionId, "Beneficio", s, MesaDePedido(s.PedidoId), true, (c, t, actor) =>
        {
            ExigirCobranzaParcial(c, t);
            var cuenta = LeerCuenta(c, t, s.PedidoId);
            ExigirAbierto(cuenta);
            ExigirVersion(cuenta, s.Version, s.TotalEsperado);
            SinPagoPendiente(c, t, s.PedidoId);
            if (s.Tipo == "Frecuente")
            {
                if (!cuenta.ClienteId.HasValue)
                    throw new ArgumentException("La promoción requiere un cliente identificado en el pedido.");
                using var elegible = Consulta(c, t, "SELECT COUNT(*) FROM dbo.ReglaFrecuente r WHERE r.Id=1 AND r.Activa=1 AND r.Porcentaje=@pct AND CONVERT(date,DATEADD(hour,-6,SYSUTCDATETIME())) BETWEEN r.Desde AND r.Hasta AND r.VisitasMinimas<=(SELECT COUNT(*) FROM dbo.Pedidos p JOIN dbo.Clientes cli ON cli.Id=p.ClienteId AND cli.Activo=1 JOIN dbo.Facturas f ON f.PedidoId=p.Id WHERE p.ClienteId=@cliente AND p.Estado=N'Facturado' AND f.Legada=0 AND f.Total>0 AND (EXISTS(SELECT 1 FROM dbo.PagosManuales pm WHERE pm.PedidoId=p.Id AND pm.Estado=N'Confirmado' AND pm.Importe=f.Total) OR COALESCE((SELECT SUM(a.Importe) FROM dbo.AbonosPedido a WHERE a.PedidoId=p.Id AND a.Estado=N'Confirmado'),0)=f.Total));", ("@pct", s.Porcentaje), ("@cliente", cuenta.ClienteId));
                if ((int)elegible.ExecuteScalar()! != 1)
                    throw new InvalidOperationException("El cliente no cumple la promoción vigente o sus condiciones cambiaron.");
            }

            var descuento = decimal.Round(cuenta.ImporteOriginal * s.Porcentaje / 100, 2, MidpointRounding.AwayFromZero);
            var tipo = s.Tipo == "Cortesía" ? "Cortesía" : "Descuento";
            if (tipo != "Cortesía" && descuento >= cuenta.ImporteOriginal)
                throw new ArgumentException("El redondeo dejaría la cuenta en cero. Usa una cortesía explícita autorizada; no un descuento ordinario.");
            var jefe = actor.Id;
            if (actor.Rol is not ("Gerente" or "Administrador"))
            {
                using var q = Consulta(c, t, "SELECT AutorizadorId FROM dbo.AutorizacionesOperacion WHERE OperacionId=@op;", ("@op", s.OperacionId));
                jefe = (int)q.ExecuteScalar()!;
            }

            Ejecutar(c, t, "UPDATE dbo.AjustesPedido SET Vigente=0 WHERE PedidoId=@p;UPDATE dbo.Pedidos SET Descuento=@d,Total=ImporteOriginal-@d WHERE Id=@p;INSERT dbo.AjustesPedido(PedidoId,OperacionId,ImporteOriginal,Descuento,TotalFinal,Tipo,Motivo,UsuarioId,AutorizadorId,Porcentaje,Promocion)VALUES(@p,@op,@o,@d,@total,@tipo,@motivo,@u,@jefe,@pct,@promo);", ("@p", s.PedidoId), ("@op", s.OperacionId), ("@o", cuenta.ImporteOriginal), ("@d", descuento), ("@total", cuenta.ImporteOriginal - descuento), ("@tipo", tipo), ("@motivo", s.Motivo.Trim()), ("@u", actor.Id), ("@jefe", jefe), ("@pct", s.Porcentaje), ("@promo", s.Tipo == "Frecuente"));
            Auditar(c, t, s.OperacionId, actor, s.PedidoId, cuenta.MesaId, "Beneficio", cuenta.Total.ToString(), (cuenta.ImporteOriginal - descuento).ToString(), $"{s.Tipo} {s.Porcentaje}%: {s.Motivo}", descuento);
            return cuenta.ImporteOriginal - descuento;
        });
    }
}
