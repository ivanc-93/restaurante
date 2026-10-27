using Restaurante.Backend.Servicios;
namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioComprobante:Form
{
    public FormularioComprobante(){InitializeComponent();FormatoPresentacion.Aplicar(this);}
    public FormularioComprobante(IRepositorioRestaurante repo,int pedidoId):this()
    {Shown+=async(_,_)=>{try{contenido.Text=await Task.Run(()=>repo.ConsultarComprobante(pedidoId));}catch(Exception ex){contenido.Text=ex.Message;}};}
}
