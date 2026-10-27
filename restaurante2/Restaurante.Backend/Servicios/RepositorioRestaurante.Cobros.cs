using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Restaurante.Backend.Dominio;

namespace Restaurante.Backend.Servicios;

public sealed partial class RepositorioRestaurante
{
    public ResultadoCobro RegistrarCobro(SolicitudCobro solicitud)
    {
        ValidarCobro(solicitud);
        var mesa=MesaDePedido(solicitud.PedidoId);
        var resultado=Mutacion(solicitud.OperacionId,"Cobro",solicitud,mesa,solicitud.Metodo=="Cortesía",(c,t,actor)=>
        {
            var cuenta=LeerCuenta(c,t,solicitud.PedidoId);ExigirAbierto(cuenta);ExigirVersion(cuenta,solicitud.Version,solicitud.TotalEsperado);
            if(cuenta.Detalles.Count==0||cuenta.Detalles.Sum(d=>d.Importe)!=cuenta.Total)throw new InvalidOperationException("Los detalles no coinciden con el total.");
            SinPagoPendiente(c,t,cuenta.PedidoId);
            if(solicitud.Metodo=="Cortesía" && cuenta.Total!=0)throw new ArgumentException("La cortesía solo cierra cuentas de total cero.");
            if(solicitud.Metodo!="Cortesía"&&cuenta.Total==0)throw new ArgumentException("Una cuenta de cero requiere cortesía autorizada.");
            if(solicitud.Metodo=="Efectivo"&&solicitud.Entregado<cuenta.Total)throw new ArgumentException("El efectivo es insuficiente.");
            if(solicitud.Metodo=="Tarjeta"&&solicitud.Entregado!=cuenta.Total)throw new ArgumentException("La tarjeta debe cubrir el total exacto.");
            var estado=solicitud.Metodo=="Tarjeta"?solicitud.ResultadoTarjeta:"Confirmado";
            var cambio=solicitud.Metodo=="Efectivo"?solicitud.Entregado-cuenta.Total:0;
            Ejecutar(c,t,"""
                INSERT dbo.PagosManuales(OperacionId,PedidoId,Metodo,Estado,Importe,Entregado,Cambio,Referencia,Observacion,UsuarioId,SolicitudOriginal)
                VALUES(@op,@p,@metodo,@estado,@importe,@entregado,@cambio,@ref,@obs,@actor,@original);
                """,("@op",solicitud.OperacionId),("@p",cuenta.PedidoId),("@metodo",solicitud.Metodo),("@estado",estado),("@importe",cuenta.Total),
                ("@entregado",solicitud.Entregado),("@cambio",cambio),("@ref",solicitud.Referencia?.Trim()),("@obs",solicitud.Observacion?.Trim()),("@actor",actor.Id),("@original",JsonSerializer.Serialize(solicitud)));
            string? numero=null;
            if(estado=="Confirmado")numero=EmitirComprobante(c,t,cuenta,solicitud,actor);
            Auditar(c,t,solicitud.OperacionId,actor,cuenta.PedidoId,mesa,"CobroManual",cuenta.Estado,estado,solicitud.Metodo+": resultado declarado por operador",cuenta.Total);
            return new ResultadoCobro(solicitud.OperacionId,estado,numero,cuenta.Total,cambio);
        });
        if(resultado.Estado!="Incierto")return resultado;
        // Read the current reconciliation result without locking/updating another operation row.
        using var conexion=AbrirConexion();
        using var consulta=Consulta(conexion,null,"SELECT pm.Estado,f.NumeroFactura FROM dbo.PagosManuales pm LEFT JOIN dbo.Facturas f ON f.PedidoId=pm.PedidoId WHERE pm.OperacionId=@op;",("@op",solicitud.OperacionId));
        using var lector=consulta.ExecuteReader();
        if(!lector.Read())throw new InvalidOperationException("No se pudo recuperar el estado del pago. Conserve la solicitud.");
        return resultado with{Estado=lector.GetString(0),NumeroComprobante=lector.IsDBNull(1)?null:lector.GetString(1)};
    }
    private static void ValidarCobro(SolicitudCobro s)
    {
        if(!s.DatosConfirmados)throw new ArgumentException("Confirme que revisó los datos de la cuenta y del receptor.");
        if(!s.PublicoGeneral)ReglasRestaurante.Texto(s.Receptor,"Nombre del receptor",120);
        if(s.PublicoGeneral && !string.IsNullOrWhiteSpace(s.Identificacion))throw new ArgumentException("Público general no debe incluir identificación; seleccione receptor identificado.");
        if(s.Identificacion?.Trim().Length>40||s.Direccion?.Trim().Length>250||s.Referencia?.Trim().Length>100||s.Observacion?.Trim().Length>250)
            throw new ArgumentException("Alguno de los campos supera su longitud máxima.");
        if(!s.PublicoGeneral && s.Identificacion is not null && s.Identificacion.Any(char.IsControl))throw new ArgumentException("La identificación contiene caracteres inválidos.");
        ReglasRestaurante.Cliente(s.PublicoGeneral?"Público general":s.Receptor,null,s.Correo);
        if(s.Entregado<0 || s.Entregado>9999999999.99m || decimal.Round(s.Entregado,2)!=s.Entregado)
            throw new ArgumentException("Importe entregado fuera de rango o con más de dos decimales.");
        if(s.Metodo is not ("Efectivo" or "Tarjeta" or "Cortesía"))throw new ArgumentException("Seleccione efectivo, tarjeta o cortesía autorizada.");
        if(s.Metodo=="Cortesía" && (!s.CortesiaAutorizada || s.Entregado!=0))throw new ArgumentException("Confirme cortesía de total cero.");
        if(s.Metodo=="Tarjeta")
        {
            if(s.ResultadoTarjeta is not ("Confirmado" or "Rechazado" or "Incierto"))throw new ArgumentException("Seleccione el resultado real del terminal externo.");
            if(s.ResultadoTarjeta=="Confirmado"&&!s.PagoConfirmado)throw new ArgumentException("El operador debe confirmar que comprobó el pago externo.");
            if(s.ResultadoTarjeta is "Confirmado" or "Incierto")ReglasRestaurante.Texto(s.Referencia,"Referencia del terminal o comprobante",100);
        }
        else if(!s.PagoConfirmado)throw new ArgumentException("Confirme el efectivo recibido o la cortesía autorizada.");
    }
    private static string EmitirComprobante(SqlConnection c,SqlTransaction t,CuentaRevision cuenta,SolicitudCobro s,UsuarioAutenticado actor)
    {
        using var q=Consulta(c,t,"""
          DECLARE @emitido TABLE(Numero nvarchar(30));
          INSERT dbo.Facturas(PedidoId,Fecha,Subtotal,Impuesto,Total,Legada,Receptor,Identificacion,Direccion,Correo,
            RestauranteHistorico,UbicacionHistorica,MesaHistorica,Operador,UsuarioId)
          OUTPUT INSERTED.NumeroFactura INTO @emitido
          SELECT @p,DATEADD(hour,-6,SYSUTCDATETIME()),@importe,0,@importe,0,@nombre,@ident,@dir,@correo,Nombre,Ubicacion,@mesa,@operador,@actor FROM dbo.Restaurante WHERE Id=1;
          SELECT Numero FROM @emitido;
          """,("@p",cuenta.PedidoId),("@importe",cuenta.Total),("@nombre",s.PublicoGeneral?"Público general":s.Receptor.Trim()),
          ("@ident",string.IsNullOrWhiteSpace(s.Identificacion)?null:s.Identificacion.Trim()),("@dir",s.Direccion?.Trim()),("@correo",s.Correo?.Trim()),
          ("@mesa",cuenta.NumeroMesa),("@operador",actor.Nombre),("@actor",actor.Id));
        var numero=q.ExecuteScalar() as string??throw new InvalidOperationException("Configure la información del restaurante.");
        Ejecutar(c,t,"UPDATE dbo.Pedidos SET Estado=N'Facturado' WHERE Id=@p; UPDATE dbo.Mesas SET Estado=N'En limpieza' WHERE Id=@m;",("@p",cuenta.PedidoId),("@m",cuenta.MesaId));
        return numero;
    }

    public Guid? ObtenerPagoIncierto(int pedidoId)
    {
        ExigirSesion();using var c=AbrirConexion();using var q=Consulta(c,null,"SELECT OperacionId FROM dbo.PagosManuales WHERE PedidoId=@p AND Estado=N'Incierto';",("@p",pedidoId));
        return q.ExecuteScalar() is Guid id?id:null;
    }
    public SolicitudCobro ConsultarSolicitudCobro(Guid operacionId)
    {
        ExigirSesion();using var c=AbrirConexion();using var q=Consulta(c,null,"SELECT SolicitudOriginal FROM dbo.PagosManuales WHERE OperacionId=@op;",("@op",operacionId));
        return q.ExecuteScalar() is string json?JsonSerializer.Deserialize<SolicitudCobro>(json)!:throw new InvalidOperationException("No se encontró el registro del pago.");
    }
    public ResultadoCobro ConciliarTarjeta(Guid operacionIncierta,bool confirmado,string motivo,Guid operacionId)
    {
        motivo=ReglasRestaurante.Texto(motivo,"Motivo y evidencia de conciliación",500);
        ExigirSesion(true);
        int pedidoId;
        using(var c=AbrirConexion())using(var q=Consulta(c,null,"SELECT PedidoId FROM dbo.PagosManuales WHERE OperacionId=@op;",("@op",operacionIncierta)))
            pedidoId=q.ExecuteScalar() is int p?p:throw new InvalidOperationException("La operación no existe.");
        var mesa=MesaDePedido(pedidoId);
        return Mutacion(operacionId,"ConciliarTarjeta",new{operacionIncierta,confirmado,motivo},mesa,true,(c,t,actor)=>
        {
            SolicitudCobro original;
            using(var q=Consulta(c,t,"SELECT SolicitudOriginal,Estado FROM dbo.PagosManuales WITH(UPDLOCK,HOLDLOCK) WHERE OperacionId=@op;",("@op",operacionIncierta)))
            using(var r=q.ExecuteReader())
            {
                if(!r.Read()||r.GetString(1)!="Incierto")throw new InvalidOperationException("El pago ya no está pendiente de conciliación.");
                original=JsonSerializer.Deserialize<SolicitudCobro>(r.GetString(0))!;
            }
            var cuenta=LeerCuenta(c,t,pedidoId);ExigirAbierto(cuenta);ExigirVersion(cuenta,original.Version,original.TotalEsperado);
            var estado=confirmado?"Confirmado":"Rechazado";
            Ejecutar(c,t,"UPDATE dbo.PagosManuales SET Estado=@estado,Fecha=SYSUTCDATETIME() WHERE OperacionId=@op;",("@estado",estado),("@op",operacionIncierta));
            var numero=confirmado?EmitirComprobante(c,t,cuenta,original,actor):null;
            var resultado=new ResultadoCobro(operacionIncierta,estado,numero,cuenta.Total,0);
            Auditar(c,t,operacionId,actor,pedidoId,mesa,"ConciliarTarjeta","Incierto",estado,motivo,cuenta.Total);
            return resultado;
        });
    }

    public string GestionarMesa(SolicitudGestionMesa s)
    {
        var motivo=ReglasRestaurante.Texto(s.Motivo,"Motivo",500);
        if(s.Accion is not ("Cancelar" or "Incidencia" or "Corregir estado" or "Confirmar limpieza"))throw new ArgumentException("Acción no permitida.");
        return Mutacion(s.OperacionId,"GestionMesa",s,s.MesaId,s.Accion!="Confirmar limpieza",(c,t,actor)=>
        {
            using var q=Consulta(c,t,"SELECT Estado FROM dbo.Mesas WHERE Id=@m;",("@m",s.MesaId));var estado=q.ExecuteScalar() as string??throw new InvalidOperationException("La mesa no existe.");
            using var a=Consulta(c,t,"SELECT COUNT(*) FROM dbo.Pedidos WHERE MesaId=@m AND Estado IN(N'Abierto',N'En preparación');",("@m",s.MesaId));var activos=(int)a.ExecuteScalar()!;
            if(s.Accion is "Corregir estado" or "Confirmar limpieza")
            {
                if(activos!=0||s.PedidoId.HasValue)throw new InvalidOperationException("La mesa tiene un pedido activo. Resuelva la cuenta primero.");
                if(s.Accion=="Confirmar limpieza"&&estado!="En limpieza"||s.Accion=="Corregir estado"&&estado!="Ocupada")
                    throw new InvalidOperationException("El estado cambió; actualice la mesa.");
                var nuevo=s.Accion=="Confirmar limpieza"?"Disponible":"En limpieza";
                Ejecutar(c,t,"UPDATE dbo.Mesas SET Estado=@estado WHERE Id=@m;",("@estado",nuevo),("@m",s.MesaId));
                Auditar(c,t,s.OperacionId,actor,null,s.MesaId,s.Accion,estado,nuevo,motivo,0);return nuevo;
            }
            if(!s.PedidoId.HasValue)throw new ArgumentException("Seleccione el pedido a resolver.");
            var cuenta=LeerCuenta(c,t,s.PedidoId.Value);
            if(cuenta.MesaId!=s.MesaId)throw new InvalidOperationException("El pedido no pertenece a la mesa.");
            ExigirAbierto(cuenta);ExigirVersion(cuenta,s.Version??"");SinPagoPendiente(c,t,cuenta.PedidoId);
            using(var f=Consulta(c,t,"SELECT COUNT(*) FROM dbo.Facturas WHERE PedidoId=@p;",("@p",cuenta.PedidoId)))
                if((int)f.ExecuteScalar()!>0)throw new InvalidOperationException("Existe un comprobante. No está habilitada su corrección financiera.");
            if(s.Accion=="Cancelar"&&!s.SinPreparacionDeclarada)throw new InvalidOperationException("Solo puede cancelar declarando que no hubo preparación ni consumo. Si los hubo, registre incidencia pendiente de resolución.");
            var posterior=s.Accion=="Cancelar"?"Cancelado":"Incidencia";
            Ejecutar(c,t,"UPDATE dbo.Pedidos SET Estado=@estado WHERE Id=@p; UPDATE dbo.Mesas SET Estado=N'En limpieza' WHERE Id=@m;",("@estado",posterior),("@p",cuenta.PedidoId),("@m",s.MesaId));
            if(s.Accion=="Incidencia")Ejecutar(c,t,"INSERT dbo.IncidenciasConsumo(PedidoId,ImportePendiente,Motivo,OperacionId,UsuarioId)VALUES(@p,@total,@motivo,@op,@actor);",
                ("@p",cuenta.PedidoId),("@total",cuenta.Total),("@motivo",motivo),("@op",s.OperacionId),("@actor",actor.Id));
            Auditar(c,t,s.OperacionId,actor,cuenta.PedidoId,s.MesaId,s.Accion,cuenta.Estado,posterior,motivo,cuenta.Total);
            return posterior;
        });
    }

    public string ConsultarComprobante(int pedidoId)
    {
        ExigirSesion();using var c=AbrirConexion();
        using var q=Consulta(c,null,"""
            SELECT f.NumeroFactura,f.Fecha,f.Legada,f.RestauranteHistorico,f.UbicacionHistorica,f.MesaHistorica,
              f.Receptor,f.Identificacion,f.Direccion,f.Correo,f.Total,f.Operador,
              pm.Metodo,pm.Importe,pm.Entregado,pm.Cambio,pm.Referencia
            FROM dbo.Facturas f LEFT JOIN dbo.PagosManuales pm ON pm.PedidoId=f.PedidoId AND pm.Estado=N'Confirmado' WHERE f.PedidoId=@p;
            """,("@p",pedidoId));
        var texto=new StringBuilder("Comprobante interno académico — no válido como documento tributario\r\n\r\n");
        using(var r=q.ExecuteReader())
        {
            if(!r.Read())throw new InvalidOperationException("El pedido no tiene comprobante emitido.");
            string D(int i)=>r.IsDBNull(i)?"No registrado":Convert.ToString(r.GetValue(i),System.Globalization.CultureInfo.InvariantCulture)!;
            texto.AppendLine($"{D(3)} | {D(4)}\r\nComprobante: {D(0)} · {r.GetDateTime(1):dd/MM/yyyy HH:mm}\r\nPedido: {pedidoId} · Mesa: {D(5)}\r\nReceptor: {D(6)}\r\nIdentificación: {D(7)}\r\nDirección: {D(8)}\r\nCorreo: {D(9)}");
            texto.AppendLine($"Total GTQ: {MonedaGuatemala.Formatear(r.GetDecimal(10))}\r\nResponsable: {D(11)}");
            if(r.GetBoolean(2))texto.AppendLine("LEGADO: datos conocidos al migrar; pago no registrado. No equivale a pago confirmado.");
            else texto.AppendLine($"Pago registrado por el operador: {D(12)}\r\nAplicado: {MonedaGuatemala.Formatear(r.GetDecimal(13))}\r\nEntregado: {MonedaGuatemala.Formatear(r.GetDecimal(14))}\r\nCambio: {MonedaGuatemala.Formatear(r.GetDecimal(15))}\r\nReferencia externa: {D(16)}");
        }
        texto.AppendLine("\r\nPRODUCTOS");
        foreach(var d in ListarDetallesPedido(pedidoId))texto.AppendLine($"{d.Cantidad} × {d.Producto} · {MonedaGuatemala.Formatear(d.PrecioUnitario)} = {MonedaGuatemala.Formatear(d.Importe)}");
        return texto.ToString();
    }
    public IReadOnlyList<MovimientoOperacion> ReportarOperaciones(DateTime desde,DateTime hasta)
    {
        ExigirSesion();var fechas=ReglasRestaurante.Fechas(desde,hasta);using var c=AbrirConexion();
        using var q=Consulta(c,null,"""
            SELECT DATEADD(hour,-6,pm.Fecha),pm.PedidoId,N'Pago '+pm.Metodo,pm.Estado,pm.Importe,u.Nombre,COALESCE(pm.Referencia,N'')
              FROM dbo.PagosManuales pm JOIN dbo.Usuarios u ON u.Id=pm.UsuarioId WHERE pm.Fecha>=@desdeUtc AND pm.Fecha<@hastaUtc
            UNION ALL
            SELECT DATEADD(hour,-6,a.Fecha),a.PedidoId,a.Accion,a.Posterior,a.Importe,u.Nombre,a.Motivo FROM dbo.AuditoriaOperaciones a JOIN dbo.Usuarios u ON u.Id=a.UsuarioId
              WHERE a.Accion IN(N'Cancelar',N'Incidencia') AND a.Fecha>=@desdeUtc AND a.Fecha<@hastaUtc
            UNION ALL
            SELECT f.Fecha,f.PedidoId,CASE WHEN f.Legada=1 THEN N'Comprobante legado' ELSE N'Comprobante interno' END,
              CASE WHEN f.Legada=1 THEN N'Pago no registrado' ELSE N'Emitido' END,f.Total,COALESCE(f.Operador,N'No registrado'),f.NumeroFactura
              FROM dbo.Facturas f WHERE f.Fecha>=@desde AND f.Fecha<@hasta ORDER BY 1;
            """,("@desde",fechas.Desde),("@hasta",fechas.HastaExclusivo),("@desdeUtc",RelojGuatemala.InicioUtc(desde)),("@hastaUtc",RelojGuatemala.InicioUtc(hasta).AddDays(1)));
        using var r=q.ExecuteReader();var resultado=new List<MovimientoOperacion>();
        while(r.Read())resultado.Add(new(r.GetDateTime(0),r.GetInt32(1),r.GetString(2),r.GetString(3),r.GetDecimal(4),r.GetString(5),r.GetString(6)));
        return resultado;
    }
}
