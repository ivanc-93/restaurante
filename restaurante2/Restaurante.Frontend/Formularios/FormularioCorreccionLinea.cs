using Restaurante.Backend.Dominio;
using Restaurante.Backend.Servicios;
using Restaurante.Frontend.Servicios;
namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioCorreccionLinea:Form
{
    private readonly IRepositorioRestaurante repo=null!;private readonly CuentaRevision cuenta=null!;private bool ocupado;
    public FormularioCorreccionLinea()=>InitializeComponent();
    public FormularioCorreccionLinea(IRepositorioRestaurante repo,CuentaRevision cuenta):this()
    {
        this.repo=repo;this.cuenta=cuenta;FormClosing+=(_,e)=>e.Cancel=ocupado;
        Shown+=async(_,_)=>{
            ocupado=true;aplicar.Enabled=false;
            try{lineas.DataSource=(await Task.Run(()=>repo.ListarLineasCorregibles(cuenta.PedidoId))).Where(x=>x.CantidadVigente>0).ToList();mensaje.Text="Solo antes de preparación. La captura original y cada corrección permanecen en el historial.";}
            catch(Exception ex){mensaje.Text=ex.Message;}
            finally{ocupado=false;aplicar.Enabled=lineas.Items.Count>0&&cuenta.Estado=="Abierto";}
        };
    }
    private async void Aplicar_Click(object? sender,EventArgs e)
    {
        if(ocupado||lineas.SelectedItem is not LineaPedidoCorregible linea)return;
        try
        {
            ReglasRestaurante.Texto(motivo.Text,"Motivo",500);
            if(!declaracion.Checked||retirar.Value>linea.CantidadVigente)throw new ArgumentException("Confirma que no hubo preparación y retira como máximo las unidades vigentes.");
            var s=new SolicitudCorreccionLinea(Guid.NewGuid(),cuenta.PedidoId,linea.Id,(int)retirar.Value,cuenta.Version,cuenta.Total,motivo.Text,true);
            var detalle=$"Pedido #{cuenta.PedidoId} · {linea.Producto}\nOriginal: {linea.CantidadOriginal}; vigente: {linea.CantidadVigente}; retirar: {s.CantidadRetirada}\nImporte retirado: {MonedaGuatemala.Formatear(s.CantidadRetirada*linea.PrecioUnitario)}\nMotivo: {s.Motivo}. Se invalidará el descuento previo y deberá revisarse de nuevo.";
            if(!FormularioAutorizar.Solicitar(this,repo,s.OperacionId,"CorreccionLinea",s,detalle))return;
            ocupado=true;aplicar.Enabled=false;
            await DiarioOperaciones.Ejecutar(repo,"Corregir",s.OperacionId,s,()=>repo.CorregirLinea(s));
            ocupado=false;DialogResult=DialogResult.OK;Close();
        }
        catch(Exception ex){mensaje.Text=ex.Message+" Si quedó pendiente, recupera desde Inicio con el mismo identificador.";}
        finally{ocupado=false;if(!IsDisposed)aplicar.Enabled=DiarioOperaciones.Listar(repo).Count==0&&cuenta.Estado=="Abierto";}
    }
}
