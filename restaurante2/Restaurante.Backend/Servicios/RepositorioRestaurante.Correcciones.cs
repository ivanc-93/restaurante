using System.Data;
using Microsoft.Data.SqlClient;
using Restaurante.Backend.Dominio;
namespace Restaurante.Backend.Servicios;
public sealed partial class RepositorioRestaurante
{
    private static bool TieneCorrecciones(SqlConnection c,SqlTransaction? t)
    {
        using var q=Consulta(c,t,"SELECT CASE WHEN OBJECT_ID(N'dbo.CorreccionesLinea',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.ResolucionesIncidencia',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.DetallesPedidoVigentes',N'V') IS NOT NULL THEN 1 ELSE 0 END;");
        return (int)q.ExecuteScalar()! ==1;
    }
    private static string TablaDetalles(SqlConnection c,SqlTransaction? t)=>TieneCorrecciones(c,t)?"dbo.DetallesPedidoVigentes":"dbo.DetallesPedido";
    private static void ExigirCorrecciones(SqlConnection c,SqlTransaction? t)
    {if(!TieneCorrecciones(c,t))throw new InvalidOperationException("Esta función necesita la actualización 007 respaldada. No se modificará automáticamente la base.");}
    public bool CorreccionesDisponibles(){using var c=AbrirConexion();SesionEn(c,null);return TieneCorrecciones(c,null);}
    public IReadOnlyList<LineaPedidoCorregible> ListarLineasCorregibles(int pedidoId)
    {
        using var c=AbrirConexion();using var t=c.BeginTransaction(IsolationLevel.Serializable);
        ExigirLecturaPedido(c,t,pedidoId);ExigirCorrecciones(c,t);
        using var q=Consulta(c,t,"SELECT d.Id,d.NombreProducto,d.Cantidad,d.Cantidad-COALESCE(x.Retirada,0),d.PrecioUnitario FROM dbo.DetallesPedido d OUTER APPLY(SELECT SUM(CantidadRetirada) Retirada FROM dbo.CorreccionesLinea WHERE DetalleId=d.Id)x WHERE d.PedidoId=@p ORDER BY d.Id;",("@p",pedidoId));
        var lista=new List<LineaPedidoCorregible>();using(var r=q.ExecuteReader())while(r.Read())lista.Add(new(r.GetInt32(0),r.GetString(1),r.GetInt32(2),r.GetInt32(3),r.GetDecimal(4)));t.Commit();return lista;
    }
    public string MarcarPreparacion(int pedidoId,Guid operacionId)
    {
        return Mutacion(operacionId,"Preparacion",new{pedidoId},MesaDePedido(pedidoId),false,(c,t,actor)=>{
            var cuenta=LeerCuenta(c,t,pedidoId);ExigirAbierto(cuenta);SinPagoPendiente(c,t,pedidoId);
            if(cuenta.Estado!="Abierto")throw new InvalidOperationException("La preparación ya fue declarada.");
            Ejecutar(c,t,"UPDATE dbo.Pedidos SET Estado=N'En preparación' WHERE Id=@p;",("@p",pedidoId));
            Auditar(c,t,operacionId,actor,pedidoId,cuenta.MesaId,"Preparacion",cuenta.Estado,"En preparación","Preparación declarada por operador",0);return "En preparación";
        });
    }
    public decimal CorregirLinea(SolicitudCorreccionLinea s)
    {
        ReglasRestaurante.Texto(s.Motivo,"Motivo",500);
        if(!s.SinPreparacionDeclarada||s.CantidadRetirada<=0)throw new ArgumentException("Declare que no hubo preparación ni consumo e indique unidades positivas a retirar.");
        return Mutacion(s.OperacionId,"CorreccionLinea",s,MesaDePedido(s.PedidoId),true,(c,t,actor)=>{
            ExigirCorrecciones(c,t);var cuenta=LeerCuenta(c,t,s.PedidoId);ExigirAbierto(cuenta);ExigirVersion(cuenta,s.Version,s.TotalEsperado);SinPagoPendiente(c,t,s.PedidoId);
            if(cuenta.Estado!="Abierto")throw new InvalidOperationException("Ya se declaró preparación. Conserve el consumo y registre una incidencia desde Gestionar cierre.");
            using var q=Consulta(c,t,"SELECT d.Cantidad-COALESCE((SELECT SUM(CantidadRetirada) FROM dbo.CorreccionesLinea WHERE DetalleId=d.Id),0),d.PrecioUnitario FROM dbo.DetallesPedido d WITH(UPDLOCK,HOLDLOCK) WHERE d.Id=@d AND d.PedidoId=@p;",("@d",s.DetalleId),("@p",s.PedidoId));
            int anterior;decimal precio;using(var r=q.ExecuteReader()){if(!r.Read())throw new ArgumentException("La línea no pertenece al pedido.");anterior=r.GetInt32(0);precio=r.GetDecimal(1);}
            if(s.CantidadRetirada>anterior)throw new ArgumentException("No retire más unidades de las vigentes.");
            var jefe=actor.Id;
            if(actor.Rol is not("Administrador" or "Gerente")){using var a=Consulta(c,t,"SELECT AutorizadorId FROM dbo.AutorizacionesOperacion WHERE OperacionId=@op;",("@op",s.OperacionId));jefe=(int)a.ExecuteScalar()!;}
            var total=cuenta.ImporteOriginal-s.CantidadRetirada*precio;
            Ejecutar(c,t,"INSERT dbo.CorreccionesLinea(DetalleId,OperacionId,CantidadRetirada,CantidadAnterior,CantidadPosterior,Motivo,UsuarioId,AutorizadorId)VALUES(@d,@op,@n,@a,@b,@motivo,@u,@j);UPDATE dbo.AjustesPedido SET Vigente=0 WHERE PedidoId=@p;UPDATE dbo.Pedidos SET ImporteOriginal=@total,Descuento=0,Total=@total WHERE Id=@p;",("@d",s.DetalleId),("@op",s.OperacionId),("@n",s.CantidadRetirada),("@a",anterior),("@b",anterior-s.CantidadRetirada),("@motivo",s.Motivo.Trim()),("@u",actor.Id),("@j",jefe),("@p",s.PedidoId),("@total",total));
            Auditar(c,t,s.OperacionId,actor,s.PedidoId,cuenta.MesaId,"CorreccionLinea",anterior.ToString(),(anterior-s.CantidadRetirada).ToString(),s.Motivo,s.CantidadRetirada*precio);return total;
        });
    }
    public IReadOnlyList<IncidenciaResumen> ListarIncidencias()
    {
        using var c=AbrirConexion();using var t=c.BeginTransaction(IsolationLevel.Serializable);var actor=SesionEn(c,t);ExigirCorrecciones(c,t);
        using var q=Consulta(c,t,"SELECT i.PedidoId,p.MesaHistorica,COALESCE(p.ClienteHistorico,N'Público general'),i.ImportePendiente,i.Motivo,CASE WHEN r.PedidoId IS NULL THEN N'Pendiente' ELSE N'Resuelta' END,r.Desenlace FROM dbo.IncidenciasConsumo i JOIN dbo.Pedidos p ON p.Id=i.PedidoId LEFT JOIN dbo.ResolucionesIncidencia r ON r.PedidoId=i.PedidoId WHERE @gerente=1 OR i.UsuarioId=@u ORDER BY i.Fecha DESC;",("@gerente",actor.Rol is "Administrador" or "Gerente"),("@u",actor.Id));
        var lista=new List<IncidenciaResumen>();using(var r=q.ExecuteReader())while(r.Read())lista.Add(new(r.GetInt32(0),r.GetInt32(1),r.GetString(2),r.GetDecimal(3),r.GetString(4),r.GetString(5),r.IsDBNull(6)?null:r.GetString(6)));t.Commit();return lista;
    }
    public string ResolverIncidencia(SolicitudResolucionIncidencia s)
    {
        ReglasRestaurante.Texto(s.Motivo,"Motivo y evidencia",500);
        if(s.Desenlace is not("Cobro posterior" or "Pérdida" or "Condonación"))throw new ArgumentException("Desenlace inválido.");
        if(s.Desenlace=="Cobro posterior")
        {
            if(s.Cobro is null||s.Cobro.OperacionId!=s.OperacionId||s.Cobro.PedidoId!=s.PedidoId)throw new ArgumentException("Revise el cobro de esta incidencia.");
            ValidarCobro(s.Cobro);
            if(s.Cobro.Metodo is not("Efectivo" or "Tarjeta")||s.Cobro.Metodo=="Tarjeta"&&s.Cobro.ResultadoTarjeta!="Confirmado")throw new ArgumentException("Resuelva únicamente con pago completo confirmado. Un resultado incierto conserva la incidencia pendiente.");
        }
        else if(s.Cobro is not null)throw new ArgumentException("Una pérdida o condonación no registra pago.");
        return Mutacion(s.OperacionId,"ResolverIncidencia",s,MesaDePedido(s.PedidoId),false,(c,t,actor)=>{
            actor=SesionEn(c,t,gerente:true);ExigirCorrecciones(c,t);var cuenta=LeerCuenta(c,t,s.PedidoId);
            using var q=Consulta(c,t,"SELECT i.ImportePendiente FROM dbo.IncidenciasConsumo i WITH(UPDLOCK,HOLDLOCK) WHERE i.PedidoId=@p AND NOT EXISTS(SELECT 1 FROM dbo.ResolucionesIncidencia WHERE PedidoId=i.PedidoId);",("@p",s.PedidoId));
            if(q.ExecuteScalar() is not decimal importe||cuenta.Estado!="Incidencia")throw new InvalidOperationException("La incidencia ya fue resuelta o cambió.");
            SinPagoPendiente(c,t,s.PedidoId);string resultado=s.Desenlace;
            if(s.Cobro is {} pago)
            {
                ExigirVersion(cuenta,pago.Version,pago.TotalEsperado);
                if(importe<=0)throw new InvalidOperationException("Un saldo cero no admite cobro en efectivo o tarjeta. Registra su desenlace sin inventar un pago.");
                if(cuenta.Detalles.Count==0||cuenta.Detalles.Sum(d=>d.Importe)!=cuenta.ImporteOriginal||cuenta.ImporteOriginal-cuenta.Descuento!=cuenta.Total)throw new InvalidOperationException("Los detalles no coinciden con el saldo de la incidencia.");
                if(importe!=cuenta.Total||pago.Metodo=="Tarjeta"&&pago.Entregado!=importe||pago.Metodo=="Efectivo"&&pago.Entregado<importe)throw new ArgumentException("El pago debe cubrir exactamente el saldo pendiente; revise el efectivo y cambio.");
                var cambio=pago.Metodo=="Efectivo"?pago.Entregado-importe:0;
                Ejecutar(c,t,"INSERT dbo.PagosManuales(OperacionId,PedidoId,Metodo,Estado,Importe,Entregado,Cambio,Referencia,Observacion,UsuarioId,SolicitudOriginal)VALUES(@op,@p,@metodo,N'Confirmado',@importe,@entregado,@cambio,@ref,@obs,@u,@json);",("@op",s.OperacionId),("@p",s.PedidoId),("@metodo",pago.Metodo),("@importe",importe),("@entregado",pago.Entregado),("@cambio",cambio),("@ref",pago.Referencia),("@obs",pago.Observacion),("@u",actor.Id),("@json",System.Text.Json.JsonSerializer.Serialize(pago)));
                resultado+=" · comprobante "+EmitirComprobante(c,t,cuenta,pago,actor,cerrarMesa:false);
            }
            Ejecutar(c,t,"INSERT dbo.ResolucionesIncidencia(PedidoId,OperacionId,Desenlace,Importe,Motivo,UsuarioId)VALUES(@p,@op,@d,@i,@m,@u);",("@p",s.PedidoId),("@op",s.OperacionId),("@d",s.Desenlace),("@i",importe),("@m",s.Motivo.Trim()),("@u",actor.Id));
            Auditar(c,t,s.OperacionId,actor,s.PedidoId,cuenta.MesaId,"ResolverIncidencia","Pendiente",s.Desenlace,s.Motivo,importe);return resultado;
        });
    }
}
