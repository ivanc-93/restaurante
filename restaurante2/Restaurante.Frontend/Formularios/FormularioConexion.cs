using Restaurante.Backend.Servicios;
using Restaurante.Frontend.Servicios;
namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioConexion:Form
{
    private readonly IConfiguracionConexion configuracion=null!;private bool ocupado;
    public FormularioConexion()=>InitializeComponent();
    public FormularioConexion(IConfiguracionConexion cfg):this(){configuracion=cfg;FormClosing+=(_,e)=>e.Cancel=ocupado;}
    private void Probar_Click(object? sender,EventArgs e)=>Ejecutar(false);
    private void Guardar_Click(object? sender,EventArgs e)=>Ejecutar(true);
    private async void Ejecutar(bool guardarDestino)
    {
        if(ocupado)return;
        if(string.IsNullOrWhiteSpace(servidor.Text)||string.IsNullOrWhiteSpace(baseDatos.Text)){mensaje.Text="Escriba servidor y base de datos.";return;}
        var host=servidor.Text;var db=baseDatos.Text;var port=(int)puerto.Value;var user=autenticacion.SelectedIndex==0?"":sqlUsuario.Text;var pass=autenticacion.SelectedIndex==0?"":sqlClave.Text;var trust=certificado.Checked;var u=admin.Text;var clave=adminClave.Text;
        ocupado=true;guardar.Enabled=probar.Enabled=false;
        var validandoEsquema=false;
        mensaje.Text="Comprobando conexión…";
        try
        {
            var temporal=new ConfiguracionConexionTemporal(ConfiguracionConexion.CrearCadena(host,port,db,user,pass,trust));
            var repo=new RepositorioRestaurante(temporal);
            await Task.Run(()=>{using var c=temporal.CrearConexion();c.Open();});
            validandoEsquema=true;
            await Task.Run(()=>repo.ValidarEsquema());
            validandoEsquema=false;
            mensaje.Text="Conexión correcta. Esquema compatible con esta versión de Sabores.";
            if(!guardarDestino)return;
            if(await Task.Run(()=>new ServicioInstalacion(temporal).NecesitaAdministrador()))
            {
                using var inicial=new FormularioPrimerAdministrador(temporal);
                if(inicial.ShowDialog(this)!=DialogResult.OK)return;
            }
            else
            {
                var actor=await Task.Run(()=>repo.IniciarSesion(u,clave));
                if(actor?.Rol!="Administrador")throw new UnauthorizedAccessException("Para guardar, autentique un administrador de Sabores de la base elegida.");
                if(actor.CambioObligatorio)
                {using var cambio=new FormularioCambiarClave(repo);if(cambio.ShowDialog(this)!=DialogResult.OK)return;mensaje.Text="Contraseña actualizada. Introduzca la nueva contraseña y vuelva a guardar.";return;}
            }
            if(MessageBox.Show(this,"Cambiar el destino cierra las sesiones. Las solicitudes pendientes permanecen vinculadas a su base y perfil anteriores; no se borran ni se trasladan. ¿Guardar este destino?","Confirmar conexión",MessageBoxButtons.YesNo,MessageBoxIcon.Warning)!=DialogResult.Yes)return;
            await Task.Run(()=>configuracion.ProbarYGuardar(host,port,db,user,pass,trust));ocupado=false;DialogResult=DialogResult.OK;
        }
        catch(Exception ex){mensaje.Text=ErroresConexion.Describir(ex,validandoEsquema)+" La configuración anterior se conserva."+ErroresConexion.Registrar(ex);}
        finally{adminClave.Clear();sqlClave.Clear();ocupado=false;guardar.Enabled=probar.Enabled=true;if(DialogResult==DialogResult.OK)Close();}
    }
}
