using Restaurante.Backend.Dominio;
namespace Restaurante.Backend.Servicios;
public sealed partial class RepositorioRestaurante
{
    public decimal AplicarAjuste(SolicitudAjuste s)
    {
        var motivo=ReglasRestaurante.Texto(s.Motivo,"Motivo",500);
        if(s.Tipo is not("Descuento" or "Cortesía")||s.Descuento<0||decimal.Round(s.Descuento,2)!=s.Descuento)
            throw new ArgumentException("Indique descuento no negativo con dos decimales.");
        var mesa=MesaDePedido(s.PedidoId);
        return Mutacion(s.OperacionId,"Ajuste",s,mesa,true,(c,t,actor)=>
        {
            var cuenta=LeerCuenta(c,t,s.PedidoId);ExigirAbierto(cuenta);ExigirVersion(cuenta,s.Version,s.TotalEsperado);SinPagoPendiente(c,t,s.PedidoId);
            ValidacionAjuste.Comprobar(s.Tipo,s.Descuento,cuenta.ImporteOriginal);
            var jefe=actor.Id;
            if(actor.Rol is not("Administrador" or "Gerente"))
            {using var q=Consulta(c,t,"SELECT AutorizadorId FROM dbo.AutorizacionesOperacion WHERE OperacionId=@op;",("@op",s.OperacionId));jefe=(int)q.ExecuteScalar()!;}
            Ejecutar(c,t,"UPDATE dbo.AjustesPedido SET Vigente=0 WHERE PedidoId=@p;UPDATE dbo.Pedidos SET Descuento=@d,Total=ImporteOriginal-@d WHERE Id=@p;INSERT dbo.AjustesPedido(PedidoId,OperacionId,ImporteOriginal,Descuento,TotalFinal,Tipo,Motivo,UsuarioId,AutorizadorId)VALUES(@p,@op,@o,@d,@total,@tipo,@motivo,@actor,@jefe);",
                ("@p",s.PedidoId),("@op",s.OperacionId),("@o",cuenta.ImporteOriginal),("@d",s.Descuento),("@total",cuenta.ImporteOriginal-s.Descuento),("@tipo",s.Tipo),("@motivo",motivo),("@actor",actor.Id),("@jefe",jefe));
            Auditar(c,t,s.OperacionId,actor,s.PedidoId,mesa,s.Tipo,cuenta.Total.ToString(),(cuenta.ImporteOriginal-s.Descuento).ToString(),motivo,s.Descuento);
            return cuenta.ImporteOriginal-s.Descuento;
        });
    }
}
