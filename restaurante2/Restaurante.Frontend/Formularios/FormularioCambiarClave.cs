using Restaurante.Backend.Servicios;
namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioCambiarClave:Form
{
    private readonly IRepositorioRestaurante repo=null!;private bool ocupado;
    public FormularioCambiarClave()=>InitializeComponent();
    public FormularioCambiarClave(IRepositorioRestaurante repo):this(){this.repo=repo;FormClosing+=(_,e)=>e.Cancel=ocupado;}
    private async void Guardar_Click(object? sender,EventArgs e)
    {
        if(ocupado)return;
        if(!ValidacionCredenciales.Campo(actual,mensaje,"la contraseña actual")||!ValidacionCredenciales.Nueva(nueva,mensaje,repetir,actual))return;
        var a=actual.Text;var n=nueva.Text;ocupado=true;guardar.Enabled=false;
        bool terminado=false;
        try{await Task.Run(()=>repo.CambiarMiContrasena(a,n));terminado=true;}
        catch(Exception ex){mensaje.Text=ex.Message;}
        finally{ocupado=false;guardar.Enabled=true;actual.Clear();nueva.Clear();repetir.Clear();}
        if(terminado){DialogResult=DialogResult.OK;Close();}
    }
}
