using System.Data;
using Microsoft.Data.SqlClient;
using Restaurante.Backend.Dominio;

namespace Restaurante.Backend.Data.Repositorios;
public sealed partial class RepositorioRestaurante
{
    internal static bool TieneCuentasOperativas(SqlConnection c, SqlTransaction? t)
    {
        using var q = Consulta(c, t, "SELECT CASE WHEN EXISTS(SELECT 1 FROM dbo.VersionEsquema WHERE Version=10) AND COL_LENGTH(N'dbo.CuentasPedido',N'NombreConfirmado') IS NOT NULL AND COL_LENGTH(N'dbo.CuentasPedido',N'Version') IS NOT NULL AND COL_LENGTH(N'dbo.AbonosPedido',N'AlcanceSnapshot') IS NOT NULL AND OBJECT_ID(N'dbo.AjustesCuenta') IS NOT NULL AND OBJECT_ID(N'dbo.MovimientosCuenta') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM (VALUES(N'CuentasPedido',N'MesaId'),(N'CuentasPedido',N'Operativa'),(N'CuentasPedido',N'Activa'),(N'CuentasPedido',N'Liquidada'),(N'CuentasPedido',N'NombreConfirmado'),(N'CuentasPedido',N'Version'),(N'AbonosPedido',N'AlcanceSnapshot'),(N'AbonosPedido',N'SaldoAntes'),(N'AbonosPedido',N'SaldoDespues'),(N'AjustesCuenta',N'CuentaId'),(N'AjustesCuenta',N'OperacionId'),(N'AjustesCuenta',N'Original'),(N'AjustesCuenta',N'Descuento'),(N'AjustesCuenta',N'Tipo'),(N'AjustesCuenta',N'Porcentaje'),(N'AjustesCuenta',N'Motivo'),(N'AjustesCuenta',N'UsuarioId'),(N'AjustesCuenta',N'AutorizadorId'),(N'AjustesCuenta',N'Vigente'),(N'AjustesCuenta',N'Fecha'),(N'MovimientosCuenta',N'OperacionId'),(N'MovimientosCuenta',N'CuentaOrigenId'),(N'MovimientosCuenta',N'CuentaDestinoId'),(N'MovimientosCuenta',N'DetalleId'),(N'MovimientosCuenta',N'Cantidad'),(N'MovimientosCuenta',N'Accion'),(N'MovimientosCuenta',N'Detalle')) x(Tabla,Columna) WHERE COL_LENGTH(N'dbo.'+Tabla,Columna) IS NULL) AND (SELECT COUNT(*) FROM sys.index_columns WHERE object_id=OBJECT_ID(N'dbo.AbonosPedido') AND index_id=(SELECT index_id FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.AbonosPedido') AND name=N'UX_Abono_Incierto') AND key_ordinal>0)=2 AND EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.CuentasPedido') AND name=N'IX_Cuenta_MesaOperativa' AND is_disabled=0) THEN 1 ELSE 0 END;");
        return (int)q.ExecuteScalar()! == 1 && TieneCobranzaParcial(c, t);
    }

    private static void ExigirCuentasOperativas(SqlConnection c, SqlTransaction? t)
    {
        if (!TieneCuentasOperativas(c, t))
            throw new InvalidOperationException("Las cuentas identificadas requieren esquema 010 completo. No se actualiza automáticamente.");
    }

    public bool CuentasOperativasDisponibles()
    {
        using var c = AbrirConexion();
        SesionEn(c, null);
        using var q = Consulta(c, null, "SELECT COUNT(*) FROM dbo.VersionEsquema WHERE Version=10;");
        if ((int)q.ExecuteScalar()! == 0)
            return false;
        ExigirCuentasOperativas(c, null);
        return true;
    }

    private static Mesa MesaEn(SqlConnection c, SqlTransaction t, int id)
    {
        using var q = Consulta(c, t, "SELECT Numero,Capacidad,Estado FROM dbo.Mesas WITH(UPDLOCK,HOLDLOCK) WHERE Id=@m;", ("@m", id));
        using var r = q.ExecuteReader();
        if (!r.Read())
            throw new InvalidOperationException("La mesa ya no existe.");
        return new(id, r.GetInt32(0), r.GetInt32(1), r.GetString(2));
    }

    private static int? PedidoEnMesa(SqlConnection c, SqlTransaction t, int mesa)
    {
        using var q = Consulta(c, t, "SELECT Id FROM dbo.Pedidos WITH(UPDLOCK,HOLDLOCK) WHERE MesaId=@m AND Estado IN(N'Abierto',N'En preparación');", ("@m", mesa));
        return q.ExecuteScalar()is int p ? p : null;
    }

    private static bool FinanzasLegadas(SqlConnection c, SqlTransaction t, int pedido)
    {
        using var q = Consulta(c, t, "SELECT (SELECT COUNT(*) FROM dbo.CuentasPedido WHERE PedidoId=@p AND Operativa=0)+(SELECT COUNT(*) FROM dbo.PagosManuales WHERE PedidoId=@p AND Estado IN(N'Confirmado',N'Incierto'))+(SELECT COUNT(*) FROM dbo.AbonosPedido WHERE PedidoId=@p AND CuentaId IS NULL AND Estado IN(N'Confirmado',N'Incierto'));", ("@p", pedido));
        return (int)q.ExecuteScalar()! > 0;
    }

    private static IReadOnlyList<CuentaOperativa> CuentasEn(SqlConnection c, SqlTransaction t, int mesa, int? pedido)
    {
        var datos = new List<(int Id, string Nombre, bool Confirmada, bool Cerrada, decimal Original, decimal Descuento, decimal Pagado, string Version)>();
        using (var q = Consulta(c, t, "SELECT x.Id,x.Nombre,x.NombreConfirmado,x.Liquidada,x.Original,x.Descuento,COALESCE((SELECT SUM(a.Importe) FROM dbo.AbonosPedido a WHERE a.CuentaId=x.Id AND a.Estado=N'Confirmado'),0),x.Version FROM dbo.CuentasPedido x WHERE x.Activa=1 AND ((@p IS NOT NULL AND x.PedidoId=@p) OR (@p IS NULL AND x.MesaId=@m AND x.PedidoId IS NULL AND x.Operativa=1)) ORDER BY x.Id;", ("@p", pedido), ("@m", mesa)))
        using (var r = q.ExecuteReader())
            while (r.Read())
                datos.Add((r.GetInt32(0), r.GetString(1), r.GetBoolean(2), r.GetBoolean(3), r.GetDecimal(4), r.GetDecimal(5), r.GetDecimal(6), Convert.ToBase64String((byte[])r[7])));
        var cuentas = new List<CuentaOperativa>();
        foreach (var d in datos)
        {
            var lineas = new List<DetalleCuenta>();
            using var q = Consulta(c, t, "SELECT d.Id,d.NombreProducto,d.TipoProducto,u.Cantidad,d.PrecioUnitario,u.Cantidad*d.PrecioUnitario FROM dbo.UnidadesCuenta u JOIN dbo.DetallesPedido d ON d.Id=u.DetalleId WHERE u.CuentaId=@c ORDER BY d.Id;", ("@c", d.Id));
            using var r = q.ExecuteReader();
            while (r.Read())
                lineas.Add(new(r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetInt32(3), r.GetDecimal(4), r.GetDecimal(5)));
            cuentas.Add(new(d.Id, d.Nombre, d.Confirmada, d.Cerrada, d.Original, d.Descuento, d.Pagado, d.Version, lineas));
        }

        return cuentas;
    }

    public ServicioMesa ConsultarServicioMesa(int mesaId)
    {
        using var c = AbrirConexion();
        using var t = c.BeginTransaction(IsolationLevel.Serializable);
        SesionEn(c, t);
        ExigirCuentasOperativas(c, t);
        var mesa = MesaEn(c, t, mesaId);
        var pedido = PedidoEnMesa(c, t, mesaId);
        CuentaRevision? revision = null;
        if (pedido.HasValue)
        {
            ExigirLecturaPedido(c, t, pedido.Value);
            revision = LeerCuenta(c, t, pedido.Value);
        }

        var cuentas = CuentasEn(c, t, mesaId, pedido).ToList();
        var operativa = !pedido.HasValue || !FinanzasLegadas(c, t, pedido.Value);
        if (cuentas.Count == 0)
        {
            var lineas = new List<DetalleCuenta>();
            if (pedido.HasValue)
            {
                using var q = Consulta(c, t, "SELECT Id,NombreProducto,TipoProducto,Cantidad,PrecioUnitario,Cantidad*PrecioUnitario FROM dbo.DetallesPedidoVigentes WHERE PedidoId=@p ORDER BY Id;", ("@p", pedido));
                using var r = q.ExecuteReader();
                while (r.Read())
                    lineas.Add(new(r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetInt32(3), r.GetDecimal(4), r.GetDecimal(5)));
            }

            var pagado = pedido.HasValue ? CobranzaEn(c, t, revision!).Pagado : 0;
            cuentas.Add(new(0, "Cuenta general", true, false, revision?.ImporteOriginal ?? 0, revision?.Descuento ?? 0, pagado, "", lineas));
        }

        t.Commit();
        return new(mesa, pedido, revision?.Version, operativa, cuentas);
    }

    public IReadOnlyList<CuentaOperativa> ConsultarCuentasPedido(int pedidoId)
    {
        var mesa = MesaDePedido(pedidoId);
        using var c = AbrirConexion();
        using var t = c.BeginTransaction(IsolationLevel.Serializable);
        SesionEn(c, t);
        ExigirCuentasOperativas(c, t);
        MesaEn(c, t, mesa);
        ExigirLecturaPedido(c, t, pedidoId);
        var resultado = CuentasEn(c, t, mesa, pedidoId);
        t.Commit();
        return resultado;
    }

    private static void VersionPedidoMesa(SqlConnection c, SqlTransaction t, int? actual, int? esperado, string? version)
    {
        if (actual != esperado)
            throw new InvalidOperationException("La mesa cambió de pedido. Actualiza antes de continuar.");
        if (actual.HasValue)
            ExigirVersion(LeerCuenta(c, t, actual.Value), version ?? "");
    }

    private static int NuevaCuentaEn(SqlConnection c, SqlTransaction t, int mesa, int? pedido, string nombre, bool confirmada, Guid operacion, decimal original = 0, decimal descuento = 0)
    {
        using var q = Consulta(c, t, "INSERT dbo.CuentasPedido(MesaId,PedidoId,Nombre,Original,Descuento,OperacionId,Operativa,Activa,Liquidada,NombreConfirmado) OUTPUT INSERTED.Id VALUES(@m,@p,@n,@o,@d,@op,1,1,0,@confirmada);", ("@m", mesa), ("@p", pedido), ("@n", nombre), ("@o", original), ("@d", descuento), ("@op", operacion), ("@confirmada", confirmada));
        return (int)q.ExecuteScalar()!;
    }

    private static int GarantizarGeneral(SqlConnection c, SqlTransaction t, int mesa, int? pedido, Guid op)
    {
        var existentes = CuentasEn(c, t, mesa, pedido);
        if (existentes.Count > 0)
            return existentes[0].Id;
        if (pedido.HasValue && FinanzasLegadas(c, t, pedido.Value))
            throw new InvalidOperationException("El pedido conserva pagos o separación anterior. Continúa por su flujo original; no se redistribuirán automáticamente.");
        var cuenta = pedido.HasValue ? LeerCuenta(c, t, pedido.Value) : null;
        var id = NuevaCuentaEn(c, t, mesa, pedido, "Cuenta general", true, op, cuenta?.ImporteOriginal ?? 0, cuenta?.Descuento ?? 0);
        if (pedido.HasValue)
            Ejecutar(c, t, "INSERT dbo.UnidadesCuenta(CuentaId,DetalleId,Cantidad) SELECT @c,Id,Cantidad FROM dbo.DetallesPedidoVigentes WHERE PedidoId=@p;", ("@c", id), ("@p", pedido));
        return id;
    }

    private static CuentaOperativa CuentaEditable(SqlConnection c, SqlTransaction t, int mesa, int? pedido, int id, string? version = null)
    {
        var cuenta = CuentasEn(c, t, mesa, pedido).SingleOrDefault(x => x.Id == id) ?? throw new InvalidOperationException("La cuenta no pertenece a esta mesa/pedido o fue retirada.");
        using var q = Consulta(c, t, "SELECT Operativa FROM dbo.CuentasPedido WHERE Id=@id;", ("@id", id));
        if (!(bool)q.ExecuteScalar()!)
            throw new InvalidOperationException("Esta cuenta conserva la política anterior.");
        if (version is not null && cuenta.Version != version)
            throw new InvalidOperationException("La cuenta cambió. Actualiza antes de continuar.");
        using var pago = Consulta(c, t, "SELECT COUNT(*) FROM dbo.AbonosPedido WHERE CuentaId=@c AND Estado IN(N'Confirmado',N'Incierto');", ("@c", id));
        if (cuenta.Liquidada || (int)pago.ExecuteScalar()! > 0)
            throw new InvalidOperationException("Esta cuenta está pagada o tiene un abono protegido. Agrega otra cuenta para consumo nuevo; no se alterará el recibo anterior.");
        return cuenta;
    }

    private static void MovimientoCuenta(SqlConnection c, SqlTransaction t, Guid op, int? origen, int? destino, int? detalle, int? cantidad, string accion, string texto) => Ejecutar(c, t, "INSERT dbo.MovimientosCuenta(OperacionId,CuentaOrigenId,CuentaDestinoId,DetalleId,Cantidad,Accion,Detalle) VALUES(@op,@o,@d,@l,@n,@a,@txt);", ("@op", op), ("@o", origen), ("@d", destino), ("@l", detalle), ("@n", cantidad), ("@a", accion), ("@txt", texto));
    private static void SincronizarCuentas(SqlConnection c, SqlTransaction t, int pedido)
    {
        using (var q = Consulta(c, t, "SELECT COUNT(*) FROM dbo.DetallesPedidoVigentes d WHERE d.PedidoId=@p AND d.Cantidad<>COALESCE((SELECT SUM(u.Cantidad) FROM dbo.UnidadesCuenta u JOIN dbo.CuentasPedido x ON x.Id=u.CuentaId WHERE u.DetalleId=d.Id AND x.PedidoId=@p AND x.Operativa=1 AND x.Activa=1),0);", ("@p", pedido)))
            if ((int)q.ExecuteScalar()! > 0)
                throw new InvalidOperationException("Las unidades no coinciden con el consumo vigente; no se aplicó la operación.");
        using (var protegido = Consulta(c, t, "SELECT COUNT(*) FROM dbo.CuentasPedido x WHERE x.PedidoId=@p AND x.Activa=1 AND (x.Liquidada=1 OR EXISTS(SELECT 1 FROM dbo.AbonosPedido a WHERE a.CuentaId=x.Id AND a.Estado IN(N'Confirmado',N'Incierto'))) AND x.Original<>COALESCE((SELECT SUM(u.Cantidad*d.PrecioUnitario) FROM dbo.UnidadesCuenta u JOIN dbo.DetallesPedido d ON d.Id=u.DetalleId WHERE u.CuentaId=x.Id),0);", ("@p", pedido)))
            if ((int)protegido.ExecuteScalar()! > 0)
                throw new InvalidOperationException("No se alterará el alcance de una cuenta protegida.");
        Ejecutar(c, t, "UPDATE x SET Original=COALESCE((SELECT SUM(u.Cantidad*d.PrecioUnitario) FROM dbo.UnidadesCuenta u JOIN dbo.DetallesPedido d ON d.Id=u.DetalleId WHERE u.CuentaId=x.Id),0) FROM dbo.CuentasPedido x WHERE x.PedidoId=@p AND x.Operativa=1 AND x.Activa=1 AND x.Original<>COALESCE((SELECT SUM(u.Cantidad*d.PrecioUnitario) FROM dbo.UnidadesCuenta u JOIN dbo.DetallesPedido d ON d.Id=u.DetalleId WHERE u.CuentaId=x.Id),0);UPDATE dbo.Pedidos SET ImporteOriginal=(SELECT COALESCE(SUM(Original),0) FROM dbo.CuentasPedido WHERE PedidoId=@p AND Activa=1),Descuento=(SELECT COALESCE(SUM(Descuento),0) FROM dbo.CuentasPedido WHERE PedidoId=@p AND Activa=1),Total=(SELECT COALESCE(SUM(Total),0) FROM dbo.CuentasPedido WHERE PedidoId=@p AND Activa=1) WHERE Id=@p;", ("@p", pedido));
    }

    public int GestionarCuentaMesa(SolicitudCuentaMesa s)
    {
        if (s.Accion is not ("Crear" or "Renombrar" or "Retirar"))
            throw new ArgumentException("Acción de cuenta inválida.");
        if (s.Accion != "Retirar")
        {
            ReglasRestaurante.Texto(s.Nombre, "Nombre de cuenta", 80);
            if (s.Nombre.Any(char.IsControl))
                throw new ArgumentException("El nombre no admite saltos de línea ni caracteres de control.");
        }

        return Mutacion(s.OperacionId, "CuentaMesa", s, s.MesaId, false, (c, t, actor) =>
        {
            ExigirCuentasOperativas(c, t);
            var mesa = MesaEn(c, t, s.MesaId);
            var pedido = PedidoEnMesa(c, t, s.MesaId);
            VersionPedidoMesa(c, t, pedido, s.PedidoId, s.VersionPedido);
            if (pedido.HasValue && FinanzasLegadas(c, t, pedido.Value))
                throw new InvalidOperationException("No se convierten cuentas con pagos o división anterior.");
            if (!pedido.HasValue && mesa.Estado is not ("Disponible" or "Reservada"))
                throw new InvalidOperationException("La mesa no está lista para preparar cuentas.");
            int id;
            string nombreAnterior = "Sin cuenta";
            if (s.Accion == "Crear")
            {
                GarantizarGeneral(c, t, s.MesaId, pedido, s.OperacionId);
                var actuales = CuentasEn(c, t, s.MesaId, pedido);
                if (actuales.Count >= 20)
                    throw new ArgumentException("La mesa admite hasta veinte cuentas activas.");
                if (actuales.Count == 1 && actuales[0].Nombre == "Cuenta general" && actuales[0].Original == 0)
                {
                    id = actuales[0].Id;
                    nombreAnterior = actuales[0].Nombre;
                    Ejecutar(c, t, "UPDATE dbo.CuentasPedido SET Nombre=@n,NombreConfirmado=@confirmada WHERE Id=@id;", ("@n", s.Nombre.Trim()), ("@confirmada", s.NombreConfirmado), ("@id", id));
                }
                else
                    id = NuevaCuentaEn(c, t, s.MesaId, pedido, s.Nombre.Trim(), s.NombreConfirmado, s.OperacionId);
            }
            else
            {
                var objetivo = s.CuentaId == 0 && s.Accion == "Renombrar" ? GarantizarGeneral(c, t, s.MesaId, pedido, s.OperacionId) : s.CuentaId;
                var cuenta = CuentaEditable(c, t, s.MesaId, pedido, objetivo, s.CuentaId == 0 ? null : s.VersionCuenta);
                id = cuenta.Id;
                nombreAnterior = cuenta.Nombre;
                if (s.Accion == "Retirar")
                {
                    if (cuenta.Original != 0 || cuenta.Descuento != 0 || cuenta.Productos.Count > 0)
                        throw new InvalidOperationException("Solo puedes retirar una cuenta vacía sin pagos ni ajustes.");
                    Ejecutar(c, t, "UPDATE dbo.CuentasPedido SET Activa=0 WHERE Id=@id;", ("@id", id));
                }
                else
                    Ejecutar(c, t, "UPDATE dbo.CuentasPedido SET Nombre=@n,NombreConfirmado=@confirmada WHERE Id=@id;", ("@n", s.Nombre.Trim()), ("@confirmada", s.NombreConfirmado), ("@id", id));
            }

            if (pedido.HasValue)
                Ejecutar(c, t, "UPDATE dbo.Pedidos SET Total=Total WHERE Id=@p;", ("@p", pedido));
            MovimientoCuenta(c, t, s.OperacionId, null, id, null, null, s.Accion, $"{nombreAnterior} → {s.Nombre.Trim()}");
            Auditar(c, t, s.OperacionId, actor, pedido, s.MesaId, "CuentaMesa", nombreAnterior, s.Accion, "Cuenta operativa; no crea consumo", 0);
            return id;
        });
    }

    public ResultadoProductoCuenta AgregarProductoCuenta(SolicitudProductoCuenta s)
    {
        if (s.MenuId <= 0 || s.Cantidad is < 1 or > 50)
            throw new ArgumentException("Selecciona producto y cantidad de 1 a 50.");
        return Mutacion<ResultadoProductoCuenta>(s.OperacionId, "ProductoCuenta", s, s.MesaId, false, (c, t, actor) =>
        {
            ExigirCuentasOperativas(c, t);
            var mesa = MesaEn(c, t, s.MesaId);
            var pedido = PedidoEnMesa(c, t, s.MesaId);
            VersionPedidoMesa(c, t, pedido, s.PedidoId, s.VersionPedido);
            if (pedido.HasValue && FinanzasLegadas(c, t, pedido.Value))
                throw new InvalidOperationException("No se altera un pedido con cobranza anterior.");
            var id = s.CuentaId == 0 ? GarantizarGeneral(c, t, s.MesaId, pedido, s.OperacionId) : s.CuentaId;
            var seleccion = CuentaEditable(c, t, s.MesaId, pedido, id, s.CuentaId > 0 ? s.VersionCuenta : null);
            if (!pedido.HasValue)
            {
                if (mesa.Estado == "Reservada")
                {
                    if (!s.ReservaId.HasValue)
                        throw new InvalidOperationException("Abre la reserva para atenderla.");
                    ExigirReservaActiva(c, t, s.MesaId, s.ReservaId.Value, s.VersionReserva ?? "");
                }
                else if (mesa.Estado != "Disponible")
                    throw new InvalidOperationException("La mesa ya no está disponible.");
                var creado = CrearPedidoConConsumo(c, t, actor, s.MesaId, s.ClienteId, [new(s.MenuId, s.Cantidad)], s.OperacionId, mesa.Estado);
                pedido = creado.Id;
                Ejecutar(c, t, "UPDATE dbo.CuentasPedido SET PedidoId=@p WHERE MesaId=@m AND PedidoId IS NULL AND Operativa=1 AND Activa=1;INSERT dbo.UnidadesCuenta(CuentaId,DetalleId,Cantidad) SELECT @c,Id,Cantidad FROM dbo.DetallesPedidoVigentes WHERE PedidoId=@p;", ("@p", pedido), ("@m", s.MesaId), ("@c", id));
                if (mesa.Estado == "Reservada")
                    Ejecutar(c, t, "UPDATE dbo.ReservasMesa SET Estado=N'Atendida',PedidoId=@p,OperacionCierre=@op,UsuarioCierre=@u,FechaCierre=DATEADD(hour,-6,SYSUTCDATETIME()),MotivoCierre=N'Primer producto en cuenta identificada' WHERE Id=@r;", ("@p", pedido), ("@op", s.OperacionId), ("@u", actor.Id), ("@r", s.ReservaId));
            }
            else
            {
                ExigirAbierto(LeerCuenta(c, t, pedido.Value));
                var linea = InsertarLineaAsignable(c, t, pedido.Value, s.MenuId, s.Cantidad);
                Ejecutar(c, t, "INSERT dbo.UnidadesCuenta VALUES(@c,@d,@n);", ("@c", id), ("@d", linea.Id), ("@n", s.Cantidad));
            }

            Ejecutar(c, t, "UPDATE dbo.AjustesCuenta SET Vigente=0 WHERE CuentaId=@c;UPDATE dbo.CuentasPedido SET Descuento=0 WHERE Id=@c;UPDATE dbo.AjustesPedido SET Vigente=0 WHERE PedidoId=@p;", ("@c", id), ("@p", pedido.Value));
            SincronizarCuentas(c, t, pedido.Value);
            MovimientoCuenta(c, t, s.OperacionId, null, id, null, s.Cantidad, "Agregar", "Producto incorporado a la cuenta seleccionada");
            Auditar(c, t, s.OperacionId, actor, pedido, s.MesaId, "ProductoCuenta", mesa.Estado, "Ocupada", "Consumo asignado a cuenta identificada", 0);
            return new(pedido.Value, id, LeerCuenta(c, t, pedido.Value).Total);
        });
    }

    public string TraspasarCuenta(SolicitudTraspasoCuenta s)
    {
        ReglasRestaurante.Texto(s.Motivo, "Motivo del traslado", 400);
        if (s.Cantidad <= 0 || s.CuentaOrigenId == s.CuentaDestinoId)
            throw new ArgumentException("Selecciona otra cuenta y una cantidad positiva.");
        return Mutacion(s.OperacionId, "TraspasoCuenta", s, MesaDePedido(s.PedidoId), false, (c, t, actor) =>
        {
            ExigirCuentasOperativas(c, t);
            var pedido = LeerCuenta(c, t, s.PedidoId);
            ExigirAbierto(pedido);
            ExigirVersion(pedido, s.VersionPedido);
            var origen = CuentaEditable(c, t, pedido.MesaId, s.PedidoId, s.CuentaOrigenId);
            var destino = CuentaEditable(c, t, pedido.MesaId, s.PedidoId, s.CuentaDestinoId);
            if (origen.Descuento != 0 || destino.Descuento != 0)
                throw new InvalidOperationException("Restablece los ajustes de ambas cuentas con autorización antes de mover productos.");
            var linea = origen.Productos.SingleOrDefault(d => d.Id == s.DetalleId);
            if (linea is null || s.Cantidad > linea.Cantidad)
                throw new ArgumentException("La cuenta no tiene esas unidades disponibles.");
            Ejecutar(c, t, "IF EXISTS(SELECT 1 FROM dbo.UnidadesCuenta WHERE CuentaId=@destino AND DetalleId=@d) UPDATE dbo.UnidadesCuenta SET Cantidad=Cantidad+@n WHERE CuentaId=@destino AND DetalleId=@d;ELSE INSERT dbo.UnidadesCuenta VALUES(@destino,@d,@n);IF (SELECT Cantidad FROM dbo.UnidadesCuenta WHERE CuentaId=@origen AND DetalleId=@d)=@n DELETE dbo.UnidadesCuenta WHERE CuentaId=@origen AND DetalleId=@d;ELSE UPDATE dbo.UnidadesCuenta SET Cantidad=Cantidad-@n WHERE CuentaId=@origen AND DetalleId=@d;", ("@origen", origen.Id), ("@destino", destino.Id), ("@d", s.DetalleId), ("@n", s.Cantidad));
            SincronizarCuentas(c, t, s.PedidoId);
            MovimientoCuenta(c, t, s.OperacionId, origen.Id, destino.Id, s.DetalleId, s.Cantidad, "Trasladar", s.Motivo.Trim());
            Auditar(c, t, s.OperacionId, actor, s.PedidoId, pedido.MesaId, "TraspasoCuenta", origen.Nombre, destino.Nombre, s.Motivo.Trim(), linea.PrecioUnitario * s.Cantidad);
            return "Unidades trasladadas. No se borró consumo ni cambió su preparación o precio.";
        });
    }
}
