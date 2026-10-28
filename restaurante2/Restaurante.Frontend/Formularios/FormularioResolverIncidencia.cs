using Restaurante.Backend.Dominio;
using Restaurante.Backend.Servicios;
using Restaurante.Frontend.Servicios;
namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioResolverIncidencia:Form
{
    private readonly IRepositorioRestaurante repo=null!;private readonly CuentaRevision cuenta=null!;private bool ocupado;
    public FormularioResolverIncidencia()=>InitializeComponent();
    public FormularioResolverIncidencia(IRepositorioRestaurante repo,CuentaRevision cuenta):this(){this.repo=repo;this.cuenta=cuenta;resumen.Text=$"Pedido #{cuenta.PedidoId} · Saldo completo: {MonedaGuatemala.Formatear(cuenta.Total)}. No admite pagos parciales.";entregado.Value=cuenta.Total;FormClosing+=(_,e)=>e.Cancel=ocupado;}
    private async void Resolver_Click(object? sender,EventArgs e)
    {
        if(ocupado)return;
        try
        {
            ReglasRestaurante.Texto(motivo.Text,"Motivo y evidencia",500);
            if(!confirmacion.Checked)throw new ArgumentException("Confirma que revisaste los datos y el desenlace real.");
            var id=Guid.NewGuid();SolicitudCobro? cobro=null;
            if(desenlace.Text=="Cobro posterior")
            {
                if(!publico.Checked)ReglasRestaurante.Cliente(receptor.Text,null,correo.Text);
                if(metodo.Text=="Tarjeta")ReglasRestaurante.Texto(referencia.Text,"Referencia del pago confirmado",100);
                if(metodo.Text=="Efectivo"&&entregado.Value<cuenta.Total||metodo.Text=="Tarjeta"&&entregado.Value!=cuenta.Total)throw new ArgumentException("Revisa el efectivo recibido o el importe exacto de tarjeta.");
                cobro=new(id,cuenta.PedidoId,cuenta.Version,cuenta.Total,publico.Checked,receptor.Text,publico.Checked?null:identificacion.Text,publico.Checked?null:direccion.Text,publico.Checked?null:correo.Text,metodo.Text,entregado.Value,metodo.Text=="Tarjeta"?"Confirmado":"",referencia.Text,"Cobro posterior de incidencia",true,true);
            }
            var s=new SolicitudResolucionIncidencia(id,cuenta.PedidoId,desenlace.Text,motivo.Text,cobro);
            if(MessageBox.Show(this,$"Resolver como {s.Desenlace} por {MonedaGuatemala.Formatear(cuenta.Total)}. No cambiará el estado actual de la mesa. ¿Confirmar?","Resolver incidencia",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)return;
            ocupado=true;resolver.Enabled=false;
            var resultado=await DiarioOperaciones.Ejecutar(repo,"Resolver",id,s,()=>repo.ResolverIncidencia(s));
            MessageBox.Show(this,resultado,"Resolución registrada");ocupado=false;DialogResult=DialogResult.OK;Close();
        }
        catch(Exception ex){mensaje.Text=ex.Message+" Si hay resultado incierto, recupera la misma solicitud desde Inicio.";}
        finally{ocupado=false;if(!IsDisposed)resolver.Enabled=DiarioOperaciones.Listar(repo).Count==0;}
    }
}
