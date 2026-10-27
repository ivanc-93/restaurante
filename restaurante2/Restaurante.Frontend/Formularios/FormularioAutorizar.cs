using System.Text.Json;
using Restaurante.Backend.Servicios;
namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioAutorizar:Form
{
    private readonly IRepositorioRestaurante repo=null!;private readonly Guid id;private readonly string tipo="",json="";private bool ocupado;
    public FormularioAutorizar()=>InitializeComponent();
    public FormularioAutorizar(IRepositorioRestaurante repo,Guid id,string tipo,object solicitud,string detalle):this()
    {this.repo=repo;this.id=id;this.tipo=tipo;json=JsonSerializer.Serialize(solicitud,solicitud.GetType());resumen.Text=detalle+"\nAutoriza únicamente estos datos durante dos minutos. La sesión del mesero no cambia.";FormClosing+=(_,e)=>e.Cancel=ocupado;}
    private async void Autorizar_Click(object? sender,EventArgs e)
    {
        if(ocupado)return;var u=usuario.Text;var c=clave.Text;ocupado=true;autorizar.Enabled=false;
        try{await Task.Run(()=>repo.AutorizarOperacion(id,tipo,json,u,c));ocupado=false;DialogResult=DialogResult.OK;}
        catch(Exception ex){mensaje.Text=ex.Message;}
        finally{clave.Clear();ocupado=false;autorizar.Enabled=true;if(DialogResult==DialogResult.OK)Close();}
    }
    public static bool Solicitar(IWin32Window owner,IRepositorioRestaurante repo,Guid id,string tipo,object solicitud,string detalle)
    {
        if(repo.UsuarioActual?.Rol is "Administrador" or "Gerente")return true;
        using var f=new FormularioAutorizar(repo,id,tipo,solicitud,detalle);return f.ShowDialog(owner)==DialogResult.OK;
    }
}
