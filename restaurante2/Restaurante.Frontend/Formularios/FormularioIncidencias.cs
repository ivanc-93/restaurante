using Restaurante.Backend.Dominio;
using Restaurante.Backend.Servicios;
namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioIncidencias:Form
{
    private readonly IRepositorioRestaurante repo=null!;private bool ocupado;
    public FormularioIncidencias()=>InitializeComponent();
    public FormularioIncidencias(IRepositorioRestaurante repo):this(){this.repo=repo;Shown+=async(_,_)=>await Cargar();FormClosing+=(_,e)=>e.Cancel=ocupado;resolver.Visible=repo.UsuarioActual?.Rol is "Administrador" or "Gerente";}
    private async Task Cargar()
    {
        if(ocupado||repo is null)return;ocupado=true;resolver.Enabled=false;filtro.Enabled=false;
        try{var todas=filtro.SelectedIndex==1;var datos=await Task.Run(repo.ListarIncidencias);tabla.DataSource=datos.Where(x=>todas||x.Estado=="Pendiente").ToList();FormatoPresentacion.Aplicar(this);tabla.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.DisplayedCells;
            var orden=new[]{"PedidoId","Estado","Desenlace","Importe","Mesa","Cliente","Motivo"};for(var i=0;i<orden.Length;i++)if(tabla.Columns.Contains(orden[i]))tabla.Columns[orden[i]].DisplayIndex=i;mensaje.Text="El importe conserva el saldo de origen. Revisa Estado y Desenlace; una pérdida o condonación no equivale a dinero cobrado.";}
        catch(Exception ex){mensaje.Text=ex.Message;}
        finally{ocupado=false;resolver.Enabled=true;filtro.Enabled=true;}
    }
    private async void Resolver_Click(object? sender,EventArgs e)
    {
        if(ocupado)return;
        if(tabla.CurrentRow?.DataBoundItem is not IncidenciaResumen {Estado:"Pendiente"} incidencia){mensaje.Text="Selecciona una incidencia pendiente. Las resueltas conservan su historial y no admiten otro desenlace.";return;}
        ocupado=true;
        try{var cuenta=await Task.Run(()=>repo.RevisarCuenta(incidencia.PedidoId));using var f=new FormularioResolverIncidencia(repo,cuenta);f.ShowDialog(this);}
        catch(Exception ex){mensaje.Text=ex.Message;}
        finally{ocupado=false;await Cargar();}
    }
}
