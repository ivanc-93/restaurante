using Restaurante.Backend.Servicios;
namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioPrimerAdministrador:Form
{
    private readonly ServicioInstalacion servicio=null!;private bool ocupado;
    public FormularioPrimerAdministrador()=>InitializeComponent();
    public FormularioPrimerAdministrador(IConfiguracionConexion cfg):this(){servicio=new(cfg);FormClosing+=(_,e)=>e.Cancel=ocupado;}
    private async void Guardar_Click(object? sender,EventArgs e)
    {
        if(ocupado)return;
        if(!ValidacionCredenciales.Campo(token,mensaje,"el token de instalación")||!ValidacionCredenciales.Campo(nombre,mensaje,"el nombre")||!ValidacionCredenciales.Campo(usuario,mensaje,"el usuario")||!ValidacionCredenciales.Nueva(clave,mensaje,repetir))return;
        var t=token.Text;var n=nombre.Text;var u=usuario.Text;var c=clave.Text;ocupado=true;guardar.Enabled=false;
        try{await Task.Run(()=>servicio.CrearPrimerAdministrador(t,n,u,c));ocupado=false;DialogResult=DialogResult.OK;}
        catch(Exception ex){mensaje.Text=ex.Message;}
        finally{token.Clear();clave.Clear();repetir.Clear();ocupado=false;guardar.Enabled=true;if(DialogResult==DialogResult.OK)Close();}
    }
}
