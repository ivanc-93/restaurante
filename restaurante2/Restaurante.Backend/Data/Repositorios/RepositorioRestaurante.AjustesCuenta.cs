using Microsoft.Data.SqlClient;
using Restaurante.Backend.Dominio;

namespace Restaurante.Backend.Data.Repositorios;
public sealed partial class RepositorioRestaurante
{
    private static int JefeDeOperacion(SqlConnection c, SqlTransaction t, UsuarioAutenticado actor, Guid id)
    {
        if (actor.Rol is "Administrador" or "Gerente")
            return actor.Id;
        using var q = Consulta(c, t, "SELECT AutorizadorId FROM dbo.AutorizacionesOperacion WHERE OperacionId=@op;", ("@op", id));
        return (int)q.ExecuteScalar()!;
    }

    private static void ExigirFrecuenciaEn(SqlConnection c, SqlTransaction t, int? cliente, decimal porcentaje)
    {
        if (!cliente.HasValue)
            throw new ArgumentException("Selecciona el cliente del pedido para la promoción frecuente.");
        using var q = Consulta(c, t, "SELECT COUNT(*) FROM dbo.ReglaFrecuente r WHERE r.Id=1 AND r.Activa=1 AND r.Porcentaje=@pct AND CONVERT(date,DATEADD(hour,-6,SYSUTCDATETIME())) BETWEEN r.Desde AND r.Hasta AND r.VisitasMinimas<=(SELECT COUNT(*) FROM dbo.Pedidos p JOIN dbo.Clientes cli ON cli.Id=p.ClienteId AND cli.Activo=1 JOIN dbo.Facturas f ON f.PedidoId=p.Id WHERE p.ClienteId=@cliente AND p.Estado=N'Facturado' AND f.Legada=0 AND f.Total>0 AND (EXISTS(SELECT 1 FROM dbo.PagosManuales pm WHERE pm.PedidoId=p.Id AND pm.Estado=N'Confirmado' AND pm.Importe=f.Total) OR COALESCE((SELECT SUM(a.Importe) FROM dbo.AbonosPedido a WHERE a.PedidoId=p.Id AND a.Estado=N'Confirmado'),0)=f.Total));", ("@pct", porcentaje), ("@cliente", cliente));
        if ((int)q.ExecuteScalar()! != 1)
            throw new InvalidOperationException("El cliente o porcentaje no cumple la promoción vigente.");
    }

    private static void LiquidarCuentasResueltas(SqlConnection c, SqlTransaction t, int pedido)
    {
        Ejecutar(c, t, "UPDATE x SET Liquidada=1 FROM dbo.CuentasPedido x WHERE x.PedidoId=@p AND x.Operativa=1 AND x.Activa=1 AND x.Liquidada=0 AND (x.Original>0 OR EXISTS(SELECT 1 FROM dbo.Pedidos p WHERE p.Id=x.PedidoId AND p.Estado=N'Facturado')) AND x.Total=COALESCE((SELECT SUM(Importe) FROM dbo.AbonosPedido a WHERE a.CuentaId=x.Id AND a.Estado=N'Confirmado'),0) AND NOT EXISTS(SELECT 1 FROM dbo.AbonosPedido a WHERE a.CuentaId=x.Id AND a.Estado=N'Incierto');", ("@p", pedido));
    }

    public decimal AplicarBeneficioCuenta(SolicitudBeneficioCuenta s)
    {
        ReglasRestaurante.Texto(s.Motivo, "Motivo del beneficio", 400);
        if (s.Tipo is not ("Descuento" or "Frecuente" or "Cortesía" or "Restablecer") || s.Tipo == "Restablecer" && s.Porcentaje != 0 || s.Tipo == "Cortesía" && s.Porcentaje != 100 || s.Tipo is "Descuento" or "Frecuente" && (s.Porcentaje < 5 || s.Porcentaje > 50) || decimal.Round(s.Porcentaje, 2) != s.Porcentaje)
            throw new ArgumentException("Descuento 5–50 %, cortesía 100 % o restablecimiento 0 %.");
        return Mutacion(s.OperacionId, "BeneficioCuenta", s, MesaDePedido(s.PedidoId), true, (c, t, actor) =>
        {
            ExigirCuentasOperativas(c, t);
            var pedido = LeerCuenta(c, t, s.PedidoId);
            ExigirAbierto(pedido);
            ExigirVersion(pedido, s.VersionPedido, s.TotalEsperado);
            if (FinanzasLegadas(c, t, s.PedidoId))
                throw new InvalidOperationException("El pedido conserva su flujo financiero anterior.");
            var ids = CuentasEn(c, t, pedido.MesaId, pedido.PedidoId).Where(x => s.CuentaId.HasValue ? x.Id == s.CuentaId.Value : x.Original > 0).Select(x => x.Id).ToArray();
            if (ids.Length == 0)
                throw new ArgumentException("Selecciona una cuenta con consumo.");
            var cuentas = ids.Select(id => CuentaEditable(c, t, pedido.MesaId, pedido.PedidoId, id)).ToArray();
            if (s.Tipo == "Frecuente")
                ExigirFrecuenciaEn(c, t, pedido.ClienteId, s.Porcentaje);
            var original = cuentas.Sum(x => x.Original);
            var totalDescuento = decimal.Round(original * s.Porcentaje / 100, 2, MidpointRounding.AwayFromZero);
            var descuentos = cuentas.Select(x => original == 0 ? 0 : decimal.Truncate(totalDescuento * x.Original / original * 100) / 100).ToArray();
            var residuo = (int)((totalDescuento - descuentos.Sum()) * 100);
            for (int i = 0; i < residuo; i++)
                descuentos[i % descuentos.Length] += .01m;
            var jefe = JefeDeOperacion(c, t, actor, s.OperacionId);
            for (int i = 0; i < cuentas.Length; i++)
            {
                var x = cuentas[i];
                if (x.Original == 0 && s.Tipo != "Restablecer")
                    throw new ArgumentException("Una cuenta vacía no necesita descuento ni cortesía.");
                if (s.Tipo is "Descuento" or "Frecuente" && descuentos[i] >= x.Original)
                    throw new ArgumentException("El redondeo dejaría una cuenta en cero. Utiliza cortesía explícita para esa cuenta.");
                Ejecutar(c, t, "UPDATE dbo.AjustesCuenta SET Vigente=0 WHERE CuentaId=@c;UPDATE dbo.CuentasPedido SET Descuento=@d,Liquidada=CASE WHEN @tipo=N'Cortesía' THEN 1 ELSE 0 END WHERE Id=@c;INSERT dbo.AjustesCuenta(CuentaId,OperacionId,Original,Descuento,Tipo,Porcentaje,Motivo,UsuarioId,AutorizadorId)VALUES(@c,@op,@o,@d,@tipo,@pct,@motivo,@u,@j);", ("@c", x.Id), ("@op", s.OperacionId), ("@o", x.Original), ("@d", descuentos[i]), ("@tipo", s.Tipo), ("@pct", s.Porcentaje), ("@motivo", s.Motivo.Trim()), ("@u", actor.Id), ("@j", jefe));
                MovimientoCuenta(c, t, s.OperacionId, x.Id, x.Id, null, null, "Beneficio", $"{x.Nombre}: {s.Tipo} {s.Porcentaje}% · descuento {descuentos[i]} · {s.Motivo}");
            }

            Ejecutar(c, t, "UPDATE dbo.AjustesPedido SET Vigente=0 WHERE PedidoId=@p;", ("@p", s.PedidoId));
            SincronizarCuentas(c, t, s.PedidoId);
            var actual = LeerCuenta(c, t, s.PedidoId);
            LiquidarCuentasResueltas(c, t, s.PedidoId);
            var cob = CobranzaEn(c, t, actual);
            if (cob.Saldo == 0 && actual.ImporteOriginal > 0 && !cob.Abonos.Any(a => a.Estado == "Incierto"))
                EmitirComprobante(c, t, actual, new(s.OperacionId, s.PedidoId, actual.Version, actual.Total, true, "Público general", null, null, null, "Cortesía", 0, "Confirmado", null, null, true, true, true), actor);
            Auditar(c, t, s.OperacionId, actor, s.PedidoId, pedido.MesaId, "BeneficioCuenta", pedido.Total.ToString(), actual.Total.ToString(), $"{s.Tipo} {s.Porcentaje}% · Cuenta {(s.CuentaId?.ToString() ?? "toda la mesa")}: {s.Motivo}", totalDescuento);
            return actual.Total;
        });
    }

    public decimal CorregirCuenta(SolicitudCorreccionCuenta s)
    {
        ReglasRestaurante.Texto(s.Motivo, "Motivo de corrección", 400);
        if (!s.SinPreparacionDeclarada || s.Cantidad <= 0)
            throw new ArgumentException("Declara que no hubo preparación ni consumo e indica unidades positivas.");
        return Mutacion(s.OperacionId, "CorreccionCuenta", s, MesaDePedido(s.PedidoId), true, (c, t, actor) =>
        {
            ExigirCuentasOperativas(c, t);
            var pedido = LeerCuenta(c, t, s.PedidoId);
            ExigirAbierto(pedido);
            ExigirVersion(pedido, s.VersionPedido, s.TotalEsperado);
            if (pedido.Estado != "Abierto")
                throw new InvalidOperationException("Ya se declaró preparación. No se retira consumo mediante corrección.");
            var cuenta = CuentaEditable(c, t, pedido.MesaId, s.PedidoId, s.CuentaId);
            var linea = cuenta.Productos.SingleOrDefault(x => x.Id == s.DetalleId);
            if (linea is null || s.Cantidad > linea.Cantidad)
                throw new ArgumentException("Las unidades no pertenecen a esta cuenta.");
            using var q = Consulta(c, t, "SELECT Cantidad FROM dbo.DetallesPedidoVigentes WHERE Id=@d AND PedidoId=@p;", ("@d", s.DetalleId), ("@p", s.PedidoId));
            var anterior = (int)q.ExecuteScalar()!;
            var jefe = JefeDeOperacion(c, t, actor, s.OperacionId);
            Ejecutar(c, t, "INSERT dbo.CorreccionesLinea(DetalleId,OperacionId,CantidadRetirada,CantidadAnterior,CantidadPosterior,Motivo,UsuarioId,AutorizadorId)VALUES(@d,@op,@n,@a,@b,@motivo,@u,@j);IF (SELECT Cantidad FROM dbo.UnidadesCuenta WHERE CuentaId=@c AND DetalleId=@d)=@n DELETE dbo.UnidadesCuenta WHERE CuentaId=@c AND DetalleId=@d;ELSE UPDATE dbo.UnidadesCuenta SET Cantidad=Cantidad-@n WHERE CuentaId=@c AND DetalleId=@d;UPDATE dbo.AjustesCuenta SET Vigente=0 WHERE CuentaId=@c;UPDATE dbo.CuentasPedido SET Descuento=0 WHERE Id=@c;UPDATE dbo.AjustesPedido SET Vigente=0 WHERE PedidoId=@p;", ("@d", s.DetalleId), ("@op", s.OperacionId), ("@n", s.Cantidad), ("@a", anterior), ("@b", anterior - s.Cantidad), ("@motivo", s.Motivo.Trim()), ("@u", actor.Id), ("@j", jefe), ("@c", cuenta.Id), ("@p", s.PedidoId));
            SincronizarCuentas(c, t, s.PedidoId);
            MovimientoCuenta(c, t, s.OperacionId, cuenta.Id, null, s.DetalleId, s.Cantidad, "Corregir", s.Motivo.Trim());
            Auditar(c, t, s.OperacionId, actor, s.PedidoId, pedido.MesaId, "CorreccionCuenta", anterior.ToString(), (anterior - s.Cantidad).ToString(), s.Motivo.Trim(), linea.PrecioUnitario * s.Cantidad);
            return LeerCuenta(c, t, s.PedidoId).Total;
        });
    }
}
