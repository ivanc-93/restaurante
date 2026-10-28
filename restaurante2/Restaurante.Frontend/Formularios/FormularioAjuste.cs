using Restaurante.Backend.Dominio;
using Restaurante.Backend.Servicios;
using Restaurante.Frontend.Servicios;
namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioAjuste:Form
{
    private readonly IRepositorioRestaurante repo=null!;private readonly CuentaRevision cuenta=null!;private bool ocupado;
    public FormularioAjuste()=>InitializeComponent();
    public FormularioAjuste(IRepositorioRestaurante repo,CuentaRevision cuenta):this(){this.repo=repo;this.cuenta=cuenta;resumen.Text=$"Mesa {cuenta.NumeroMesa} · Pedido {cuenta.PedidoId}\nOriginal {MonedaGuatemala.Formatear(cuenta.ImporteOriginal)} · total actual {MonedaGuatemala.Formatear(cuenta.Total)}\nAgregar otro producto invalida el ajuste y exige nueva revisión.";FormClosing+=(_,e)=>e.Cancel=ocupado;}
    private async void Aplicar_Click(object? sender,EventArgs e)
    {
        if(ocupado)return;
        try
        {
            ReglasRestaurante.Texto(motivo.Text,"Motivo",500);
            var s=new SolicitudAjuste(Guid.NewGuid(),cuenta.PedidoId,cuenta.Version,cuenta.Total,tipo.Text,tipo.Text=="Cortesía"?cuenta.ImporteOriginal:descuento.Value,motivo.Text);
            ValidacionAjuste.Comprobar(s.Tipo,s.Descuento,cuenta.ImporteOriginal);
            if(!FormularioAutorizar.Solicitar(this,repo,s.OperacionId,"Ajuste",s,resumen.Text+$"\n{s.Tipo}: {MonedaGuatemala.Formatear(s.Descuento)}\nMotivo: {s.Motivo}"))return;
            ocupado=true;aplicar.Enabled=false;
            await DiarioOperaciones.Ejecutar(repo,"Ajuste",s.OperacionId,s,()=>repo.AplicarAjuste(s));ocupado=false;DialogResult=DialogResult.OK;
        }
        catch(Exception ex){mensaje.Text=ex.Message+" Si hay una solicitud pendiente, recupérela desde Inicio antes de repetir.";aplicar.Enabled=DiarioOperaciones.Listar(repo).Count==0;}
        finally{ocupado=false;if(DialogResult==DialogResult.OK)Close();}
    }
}
