using System.Data;
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Restaurante.Backend.Dominio;

namespace Restaurante.Backend.Data.Repositorios;
public sealed partial class RepositorioRestaurante
{
    internal static bool TieneCobranzaParcial(SqlConnection c, SqlTransaction? t)
    {
        using var q = Consulta(c, t, "SELECT CASE WHEN EXISTS(SELECT 1 FROM dbo.VersionEsquema WHERE Version=9) AND NOT EXISTS(SELECT 1 FROM (VALUES(N'CuentasPedido',N'Id'),(N'CuentasPedido',N'PedidoId'),(N'CuentasPedido',N'Nombre'),(N'CuentasPedido',N'Original'),(N'CuentasPedido',N'Descuento'),(N'CuentasPedido',N'Total'),(N'CuentasPedido',N'OperacionId'),(N'UnidadesCuenta',N'CuentaId'),(N'UnidadesCuenta',N'DetalleId'),(N'UnidadesCuenta',N'Cantidad'),(N'AbonosPedido',N'OperacionId'),(N'AbonosPedido',N'PedidoId'),(N'AbonosPedido',N'CuentaId'),(N'AbonosPedido',N'Estado'),(N'AbonosPedido',N'Metodo'),(N'AbonosPedido',N'Importe'),(N'AbonosPedido',N'Entregado'),(N'AbonosPedido',N'Cambio'),(N'AbonosPedido',N'Receptor'),(N'AbonosPedido',N'Identificacion'),(N'AbonosPedido',N'Referencia'),(N'AbonosPedido',N'UsuarioId'),(N'AbonosPedido',N'Fecha'),(N'AbonosPedido',N'SolicitudOriginal'),(N'AbonosPedido',N'ConciliacionId'),(N'ReglaFrecuente',N'Id'),(N'ReglaFrecuente',N'Activa'),(N'ReglaFrecuente',N'VisitasMinimas'),(N'ReglaFrecuente',N'Porcentaje'),(N'ReglaFrecuente',N'Desde'),(N'ReglaFrecuente',N'Hasta'),(N'AjustesPedido',N'Porcentaje'),(N'AjustesPedido',N'Promocion')) x(Tabla,Columna) WHERE COL_LENGTH(N'dbo.'+Tabla,Columna) IS NULL) AND EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.AbonosPedido') AND name=N'UX_Abono_Incierto' AND is_unique=1 AND is_disabled=0) THEN 1 ELSE 0 END;");
        return (int)q.ExecuteScalar()! == 1;
    }

    private static void ExigirCobranzaParcial(SqlConnection c, SqlTransaction? t)
    {
        if (!TieneCobranzaParcial(c, t))
            throw new InvalidOperationException("Se requiere esquema 009 completo. No se actualizará automáticamente.");
    }

    public bool CobranzaParcialDisponible()
    {
        using var c = AbrirConexion();
        SesionEn(c, null);
        return CobranzaHabilitadaEn(c, null);
    }

    private static bool CobranzaHabilitadaEn(SqlConnection c, SqlTransaction? t)
    {
        using var q = Consulta(c, t, "SELECT COUNT(*) FROM dbo.VersionEsquema WHERE Version=9;");
        if ((int)q.ExecuteScalar()! == 0)
            return false;
        ExigirCobranzaParcial(c, t);
        return true;
    }

    private static bool HayCobranzaNueva(SqlConnection c, SqlTransaction t, int pedido)
    {
        if (!CobranzaHabilitadaEn(c, t))
            return false;
        using var q = Consulta(c, t, "SELECT (SELECT COUNT(*) FROM dbo.CuentasPedido WHERE PedidoId=@p)+(SELECT COUNT(*) FROM dbo.AbonosPedido WHERE PedidoId=@p AND Estado IN(N'Confirmado',N'Incierto'));", ("@p", pedido));
        return (int)q.ExecuteScalar()! > 0;
    }

    private static ResumenCobranza CobranzaEn(SqlConnection c, SqlTransaction t, CuentaRevision cuenta)
    {
        var cuentas = new List<CuentaSeparada>();
        var abonos = new List<AbonoResumen>();
        using (var q = Consulta(c, t, "SELECT Id,Nombre,Original,Descuento,Total,COALESCE((SELECT SUM(Importe) FROM dbo.AbonosPedido a WHERE a.CuentaId=x.Id AND Estado=N'Confirmado'),0) FROM dbo.CuentasPedido x WHERE PedidoId=@p ORDER BY Id;", ("@p", cuenta.PedidoId)))
        using (var r = q.ExecuteReader())
            while (r.Read())
                cuentas.Add(new(r.GetInt32(0), r.GetString(1), r.GetDecimal(2), r.GetDecimal(3), r.GetDecimal(4), r.GetDecimal(5), r.GetDecimal(4) - r.GetDecimal(5)));
        using (var q = Consulta(c, t, "SELECT OperacionId,CuentaId,Estado,Metodo,Importe,Entregado,Cambio,Receptor,DATEADD(hour,-6,Fecha) FROM dbo.AbonosPedido WHERE PedidoId=@p ORDER BY Fecha,OperacionId;", ("@p", cuenta.PedidoId)))
        using (var r = q.ExecuteReader())
            while (r.Read())
                abonos.Add(new(r.GetGuid(0), r.IsDBNull(1) ? null : r.GetInt32(1), r.GetString(2), r.GetString(3), r.GetDecimal(4), r.GetDecimal(5), r.GetDecimal(6), r.GetString(7), r.GetDateTime(8)));
        using var anterior = Consulta(c, t, "SELECT COALESCE(SUM(Importe),0) FROM dbo.PagosManuales WHERE PedidoId=@p AND Estado=N'Confirmado';", ("@p", cuenta.PedidoId));
        var pagado = abonos.Where(a => a.Estado == "Confirmado").Sum(a => a.Importe) + (decimal)anterior.ExecuteScalar()!;
        return new(cuenta, pagado, cuenta.Total - pagado, cuentas.Count > 0 || abonos.Any(a => a.Estado is "Confirmado" or "Incierto"), cuentas, abonos);
    }

    public ResumenCobranza ConsultarCobranza(int pedidoId)
    {
        var mesa = MesaDePedido(pedidoId);
        using var c = AbrirConexion();
        using var t = c.BeginTransaction(IsolationLevel.Serializable);
        SesionEn(c, t);
        using (var q = Consulta(c, t, "SELECT Id FROM dbo.Mesas WITH(UPDLOCK,HOLDLOCK) WHERE Id=@m;", ("@m", mesa)))
            q.ExecuteScalar();
        ExigirLecturaPedido(c, t, pedidoId);
        ExigirCobranzaParcial(c, t);
        var resultado = CobranzaEn(c, t, LeerCuenta(c, t, pedidoId));
        t.Commit();
        return resultado;
    }

    public string DividirCuenta(SolicitudDivision s)
    {
        if (s.Partes is null || s.Partes.Count is < 2 or > 20 || s.Productos is null)
            throw new ArgumentException("Define entre 2 y 20 cuentas.");
        foreach (var p in s.Partes)
        {
            ReglasRestaurante.Texto(p.Nombre, "Nombre de cuenta", 80);
            if (p.Original <= 0 || decimal.Round(p.Original, 2) != p.Original)
                throw new ArgumentException("Cada parte debe ser positiva y tener dos decimales.");
        }

        return Mutacion(s.OperacionId, "DividirCuenta", s, MesaDePedido(s.PedidoId), false, (c, t, actor) =>
        {
            ExigirCobranzaParcial(c, t);
            var cuenta = LeerCuenta(c, t, s.PedidoId);
            ExigirAbierto(cuenta);
            ExigirVersion(cuenta, s.Version, s.TotalEsperado);
            SinPagoPendiente(c, t, s.PedidoId);
            if (cuenta.Total <= 0)
                throw new InvalidOperationException("Una cortesía total se cierra sin dividir ni simular abonos.");
            if (s.Partes.Sum(p => p.Original) != cuenta.ImporteOriginal)
                throw new ArgumentException("La distribución debe cubrir todo el consumo original.");
            if (s.Productos.Count > 0)
            {
                if (s.Productos.Any(p => p.Cuenta < 0 || p.Cuenta >= s.Partes.Count || p.Cantidad <= 0) || s.Productos.GroupBy(p => (p.Cuenta, p.DetalleId)).Any(g => g.Count() > 1))
                    throw new ArgumentException("Asignación de unidades inválida o repetida.");
                var lineas = new Dictionary<int, (int Cantidad, decimal Precio)>();
                using (var q = Consulta(c, t, "SELECT Id,Cantidad,PrecioUnitario FROM dbo.DetallesPedidoVigentes WHERE PedidoId=@p;", ("@p", s.PedidoId)))
                using (var r = q.ExecuteReader())
                    while (r.Read())
                        lineas.Add(r.GetInt32(0), (r.GetInt32(1), r.GetDecimal(2)));
                if (s.Productos.Any(p => !lineas.ContainsKey(p.DetalleId)) || lineas.Any(l => s.Productos.Where(p => p.DetalleId == l.Key).Sum(p => p.Cantidad) != l.Value.Cantidad))
                    throw new ArgumentException("Asigna exactamente todas las unidades vigentes del pedido.");
                for (int i = 0; i < s.Partes.Count; i++)
                    if (s.Productos.Where(p => p.Cuenta == i).Sum(p => p.Cantidad * lineas[p.DetalleId].Precio) != s.Partes[i].Original)
                        throw new ArgumentException("Las unidades no coinciden con el importe de la cuenta.");
            }

            var descuentos = s.Partes.Select(p => decimal.Truncate(cuenta.Descuento * p.Original / cuenta.ImporteOriginal * 100) / 100).ToArray();
            var centavos = (int)((cuenta.Descuento - descuentos.Sum()) * 100);
            for (int i = 0; i < centavos; i++)
                descuentos[i % descuentos.Length] += .01m;
            for (int i = 0; i < s.Partes.Count; i++)
            {
                using var q = Consulta(c, t, "INSERT dbo.CuentasPedido(PedidoId,Nombre,Original,Descuento,OperacionId)OUTPUT INSERTED.Id VALUES(@p,@n,@o,@d,@op);", ("@p", s.PedidoId), ("@n", s.Partes[i].Nombre.Trim()), ("@o", s.Partes[i].Original), ("@d", descuentos[i]), ("@op", s.OperacionId));
                var id = (int)q.ExecuteScalar()!;
                foreach (var p in s.Productos.Where(p => p.Cuenta == i))
                    Ejecutar(c, t, "INSERT dbo.UnidadesCuenta VALUES(@c,@d,@n);", ("@c", id), ("@d", p.DetalleId), ("@n", p.Cantidad));
            }

            Ejecutar(c, t, "UPDATE dbo.Pedidos SET Total=Total WHERE Id=@p;", ("@p", s.PedidoId));
            Auditar(c, t, s.OperacionId, actor, s.PedidoId, cuenta.MesaId, "DividirCuenta", "Única", "Separada", "Distribución completa antes del cobro", cuenta.Total);
            return "Cuentas separadas. El consumo se conserva; la distribución queda bloqueada para proteger los cobros.";
        });
    }

    public ResultadoAbono RegistrarAbono(SolicitudAbono s)
    {
        if (s.Datos is null || s.Datos.OperacionId != s.OperacionId || s.Datos.PedidoId != s.PedidoId || s.Datos.TotalEsperado != s.Importe || s.Datos.Version != s.Version)
            throw new ArgumentException("Los datos del abono no corresponden a la solicitud.");
        ValidacionCobro.Comprobar(s.Datos);
        if (s.Importe <= 0 || decimal.Round(s.Importe, 2) != s.Importe || s.Datos.Metodo is not ("Efectivo" or "Tarjeta"))
            throw new ArgumentException("El abono debe ser positivo, en efectivo o tarjeta.");
        var resultado = Mutacion(s.OperacionId, "Abono", s, MesaDePedido(s.PedidoId), false, (c, t, actor) =>
        {
            ExigirCobranzaParcial(c, t);
            var cuenta = LeerCuenta(c, t, s.PedidoId);
            ExigirAbierto(cuenta);
            ExigirVersion(cuenta, s.Version, s.TotalEsperado);
            using (var q = Consulta(c, t, "SELECT COUNT(*) FROM dbo.PagosManuales WHERE PedidoId=@p AND Estado IN(N'Confirmado',N'Incierto');", ("@p", s.PedidoId)))
                if ((int)q.ExecuteScalar()! > 0)
                    throw new InvalidOperationException("Resuelve primero el pago del flujo anterior.");
            var cob = CobranzaEn(c, t, cuenta);
            if (cob.Abonos.Any(a => a.Estado == "Incierto"))
                throw new InvalidOperationException("Existe una tarjeta incierta. Debe conciliarse antes de otro abono.");
            var saldo = cob.Cuentas.Count == 0 && !s.CuentaId.HasValue ? cob.Saldo : cob.Cuentas.SingleOrDefault(a => a.Id == s.CuentaId)?.Saldo ?? throw new ArgumentException("Selecciona una cuenta del pedido.");
            if (saldo != s.SaldoEsperado || s.Importe > saldo)
                throw new InvalidOperationException("El saldo cambió o el abono excede lo pendiente. Actualiza la cuenta.");
            if (s.Datos.Metodo == "Efectivo" && s.Datos.Entregado < s.Importe || s.Datos.Metodo == "Tarjeta" && s.Datos.Entregado != s.Importe)
                throw new ArgumentException("Revisa el importe entregado y el aplicado.");
            var estado = s.Datos.Metodo == "Tarjeta" ? s.Datos.ResultadoTarjeta : "Confirmado";
            var cambio = s.Datos.Metodo == "Efectivo" ? s.Datos.Entregado - s.Importe : 0;
            Ejecutar(c, t, "INSERT dbo.AbonosPedido(OperacionId,PedidoId,CuentaId,Estado,Metodo,Importe,Entregado,Cambio,Receptor,Identificacion,Referencia,UsuarioId,SolicitudOriginal)VALUES(@op,@p,@c,@e,@m,@i,@ent,@cam,@r,@id,@ref,@u,@json);", ("@op", s.OperacionId), ("@p", s.PedidoId), ("@c", s.CuentaId), ("@e", estado), ("@m", s.Datos.Metodo), ("@i", s.Importe), ("@ent", s.Datos.Entregado), ("@cam", cambio), ("@r", s.Datos.PublicoGeneral ? "Público general" : s.Datos.Receptor.Trim()), ("@id", s.Datos.Identificacion), ("@ref", s.Datos.Referencia), ("@u", actor.Id), ("@json", JsonSerializer.Serialize(s)));
            var pendiente = cob.Saldo - (estado == "Confirmado" ? s.Importe : 0);
            var numero = estado == "Confirmado" && pendiente == 0 ? FinalizarAbonos(c, t, cuenta, s.Datos, actor) : null;
            Ejecutar(c, t, "UPDATE dbo.Pedidos SET Total=Total WHERE Id=@p;", ("@p", s.PedidoId));
            Auditar(c, t, s.OperacionId, actor, s.PedidoId, cuenta.MesaId, "Abono", cuenta.Estado, estado, "Cobro parcial manual; recibo individual", s.Importe);
            return new ResultadoAbono(s.OperacionId, estado, estado == "Confirmado" ? s.Importe : 0, cambio, pendiente, numero);
        });
        // Recupera el estado posterior de una conciliación sin volver a ejecutar el abono.
        if (resultado.Estado == "Incierto")
        {
            using var c = AbrirConexion();
            using var q = Consulta(c, null, "SELECT a.Estado,f.NumeroFactura FROM dbo.AbonosPedido a LEFT JOIN dbo.Facturas f ON f.PedidoId=a.PedidoId WHERE a.OperacionId=@id;", ("@id", s.OperacionId));
            using var r = q.ExecuteReader();
            if (r.Read() && r.GetString(0) != "Incierto")
                return resultado with
                {
                    Estado = r.GetString(0),
                    Aplicado = r.GetString(0) == "Confirmado" ? s.Importe : 0,
                    Comprobante = r.IsDBNull(1) ? null : r.GetString(1)
                };
        }

        return resultado;
    }

    private static string FinalizarAbonos(SqlConnection c, SqlTransaction t, CuentaRevision cuenta, SolicitudCobro datos, UsuarioAutenticado actor) => EmitirComprobante(c, t, cuenta, datos with { PublicoGeneral = true, Receptor = "Público general", Identificacion = null, Direccion = null, Correo = null }, actor);
    public ResultadoAbono ConciliarAbono(SolicitudConciliacionAbono s)
    {
        ReglasRestaurante.Texto(s.Motivo, "Motivo y evidencia", 500);
        return Mutacion(s.OperacionId, "ConciliarAbono", s, MesaDePedido(s.PedidoId), true, (c, t, actor) =>
        {
            ExigirCobranzaParcial(c, t);
            var cuenta = LeerCuenta(c, t, s.PedidoId);
            ExigirAbierto(cuenta);
            ExigirVersion(cuenta, s.Version, s.TotalEsperado);
            SolicitudAbono original;
            using (var q = Consulta(c, t, "SELECT SolicitudOriginal FROM dbo.AbonosPedido WITH(UPDLOCK,HOLDLOCK) WHERE OperacionId=@op AND PedidoId=@p AND Estado=N'Incierto';", ("@op", s.AbonoId), ("@p", s.PedidoId)))
                original = q.ExecuteScalar()is string j ? JsonSerializer.Deserialize<SolicitudAbono>(j)! : throw new InvalidOperationException("El abono ya no está incierto.");
            var cob = CobranzaEn(c, t, cuenta);
            var pendiente = cob.Saldo - (s.Confirmado ? original.Importe : 0);
            if (pendiente < 0)
                throw new InvalidOperationException("El abono excedería el saldo.");
            var estado = s.Confirmado ? "Confirmado" : "Rechazado";
            Ejecutar(c, t, "UPDATE dbo.AbonosPedido SET Estado=@e,ConciliacionId=@id WHERE OperacionId=@op;UPDATE dbo.Pedidos SET Total=Total WHERE Id=@p;", ("@e", estado), ("@id", s.OperacionId), ("@op", s.AbonoId), ("@p", s.PedidoId));
            var numero = s.Confirmado && pendiente == 0 ? FinalizarAbonos(c, t, cuenta, original.Datos, actor) : null;
            Auditar(c, t, s.OperacionId, actor, s.PedidoId, cuenta.MesaId, "ConciliarAbono", "Incierto", estado, s.Motivo, original.Importe);
            return new ResultadoAbono(s.AbonoId, estado, s.Confirmado ? original.Importe : 0, 0, pendiente, numero);
        });
    }

    public string ConsultarReciboAbono(Guid id)
    {
        using var c = AbrirConexion();
        SesionEn(c, null);
        ExigirCobranzaParcial(c, null);
        int pedido;
        using (var q = Consulta(c, null, "SELECT PedidoId FROM dbo.AbonosPedido WHERE OperacionId=@id;", ("@id", id)))
            pedido = q.ExecuteScalar()is int p ? p : throw new InvalidOperationException("No existe ese abono.");
        ExigirLecturaPedido(pedido);
        using var consulta = Consulta(c, null, "SELECT a.Estado,a.Metodo,a.Importe,a.Entregado,a.Cambio,a.Receptor,DATEADD(hour,-6,a.Fecha),COALESCE(x.Nombre,N'Cuenta general'),a.Referencia FROM dbo.AbonosPedido a LEFT JOIN dbo.CuentasPedido x ON x.Id=a.CuentaId WHERE a.OperacionId=@id;", ("@id", id));
        using var r = consulta.ExecuteReader();
        r.Read();
        return $"SABORES · RECIBO DE ABONO\r\nInterno académico — no válido como documento tributario\r\n\r\nRecibo: {id}\r\nPedido #{pedido} · {r.GetString(7)}\r\nFecha: {r.GetDateTime(6):dd/MM/yyyy HH:mm}\r\nReceptor: {r.GetString(5)}\r\nResultado: {r.GetString(0)}\r\nMétodo: {r.GetString(1)}\r\nImporte solicitado: {MonedaGuatemala.Formatear(r.GetDecimal(2))}\r\nAplicado: {MonedaGuatemala.Formatear(r.GetString(0) == "Confirmado" ? r.GetDecimal(2) : 0)}\r\nEntregado: {MonedaGuatemala.Formatear(r.GetDecimal(3))}\r\nCambio: {MonedaGuatemala.Formatear(r.GetDecimal(4))}\r\nReferencia: {(r.IsDBNull(8) ? "—" : r.GetString(8))}\r\n\r\nEste recibo no representa el consumo de otras personas ni el cierre de la mesa.";
    }

    private string ComprobanteAbonos(int pedidoId)
    {
        var resumen = ConsultarCobranza(pedidoId);
        using var c = AbrirConexion();
        using var q = Consulta(c, null, "SELECT NumeroFactura FROM dbo.Facturas WHERE PedidoId=@p;", ("@p", pedidoId));
        var numero = q.ExecuteScalar() as string ?? throw new InvalidOperationException("El comprobante final aún no está emitido.");
        var b = new StringBuilder($"SABORES · RESUMEN FINAL {numero}\r\nComprobante interno académico — no válido como documento tributario\r\nPedido #{pedidoId} · Mesa {resumen.Pedido.NumeroMesa}\r\nReceptores: según recibos individuales\r\n\r\n");
        foreach (var d in resumen.Pedido.Detalles)
            b.AppendLine($"{d.Cantidad} × {d.Producto} · {MonedaGuatemala.Formatear(d.Importe)}");
        b.AppendLine($"\r\nConsumo: {MonedaGuatemala.Formatear(resumen.Pedido.ImporteOriginal)}\r\nAjustes: {MonedaGuatemala.Formatear(resumen.Pedido.Descuento)}\r\nTotal: {MonedaGuatemala.Formatear(resumen.Pedido.Total)}\r\nPagado: {MonedaGuatemala.Formatear(resumen.Pagado)}\r\nSaldo: {MonedaGuatemala.Formatear(resumen.Saldo)}\r\n\r\nABONOS");
        foreach (var a in resumen.Abonos)
            b.AppendLine($"{a.Fecha:dd/MM HH:mm} · {a.Metodo} · {a.Estado} · {MonedaGuatemala.Formatear(a.Importe)} · {a.Receptor}\r\nRecibo {a.OperacionId}");
        return b.ToString();
    }
}
