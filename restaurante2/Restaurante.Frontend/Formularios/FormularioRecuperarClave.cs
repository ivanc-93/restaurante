using System.Security.Cryptography;
using Restaurante.Backend.Dominio;
using Restaurante.Backend.Servicios;
using Restaurante.Frontend.Servicios;
namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioRecuperarClave:Form
{
    private readonly IRepositorioRestaurante repo;private readonly DiarioRecuperacion diario;private Guid operacion=Guid.NewGuid();private string recibo=Convert.ToHexString(RandomNumberGenerator.GetBytes(32));private bool ocupado,incierto,terminado;
    public FormularioRecuperarClave(IRepositorioRestaurante repositorio,string? carpetaComprobante=null)
    {
        repo=repositorio;diario=new(repo.IdentificadorBase,carpetaComprobante);InitializeComponent();
        try{if(diario.Leer() is {} pendiente){operacion=pendiente.Id;recibo=pendiente.Recibo;incierto=true;mensaje.Text="Existe una recuperación pendiente en este perfil. Comprueba su resultado antes de repetir.";Estado(false);}}
        catch(Exception){incierto=true;Estado(false);consultar.Enabled=false;mensaje.Text="No se pudo leer el comprobante protegido de recuperación. Solicita asistencia antes de repetir.";}
        FormClosing+=(_,e)=>e.Cancel=ocupado;
        FormClosed+=(_,_)=>{LimpiarSecretos();recibo="";};
    }
    private void LimpiarSecretos(){foreach(var campo in new[]{claveAdministrador,codigo,nueva,confirmacion})campo.Clear();mostrar.Checked=false;}
    private void Estado(bool valor){ocupado=valor;foreach(var c in new Control[]{metodo,usuario,administrador,claveAdministrador,codigo,nueva,confirmacion,mostrar,cancelar})c.Enabled=!valor&&!terminado&&!incierto;recuperar.Enabled=!valor&&!incierto&&!terminado&&metodo.SelectedIndex<2;consultar.Enabled=!valor&&incierto;UseWaitCursor=valor;if(!valor){cancelar.Enabled=true;administrador.Enabled=claveAdministrador.Enabled=metodo.SelectedIndex==0&&!incierto&&!terminado;codigo.Enabled=metodo.SelectedIndex==1&&!incierto&&!terminado;}}
    private async void Recuperar_Click(object? sender,EventArgs e)
    {
        if(ocupado||incierto||terminado)return;
        if(!ValidacionCredenciales.Campo(usuario,mensaje,"el usuario")||!ValidacionCredenciales.Nueva(nueva,mensaje,confirmacion))return;
        var modo=metodo.SelectedIndex;
        if(modo==0&&(!ValidacionCredenciales.Campo(administrador,mensaje,"el usuario del Administrador")||!ValidacionCredenciales.Campo(claveAdministrador,mensaje,"la contraseña vigente del Administrador")))return;
        if(modo==1&&!ValidacionCredenciales.Campo(codigo,mensaje,"el código de emergencia"))return;
        var u=usuario.Text;var a=administrador.Text;var c=claveAdministrador.Text;var k=codigo.Text;var n=nueva.Text;Estado(true);mensaje.Text="Verificando recuperación…";
        string texto;bool correcto=false,dudoso=false;
        try{diario.Guardar(operacion,recibo);var r=await Task.Run(()=>modo==0?repo.RecuperarConAdministrador(operacion,recibo,u,a,c,n):repo.RecuperarConCodigo(operacion,recibo,u,k,n)).ConfigureAwait(false);texto=r.Mensaje;correcto=r.Correcto;diario.Borrar();}
        catch(Microsoft.Data.SqlClient.SqlException){dudoso=true;texto="No se pudo confirmar el resultado con SQL. Usa Comprobar resultado antes de repetir. No se ha confirmado el restablecimiento.";}
        catch(InvalidOperationException ex){texto=ex.Message;}
        catch(Exception ex){dudoso=true;System.Diagnostics.Trace.TraceError("Recuperación {0}: {1}",operacion,ex.GetType().Name);texto="No se pudo confirmar el resultado. Compruébalo antes de repetir.";}
        DisenoFormulario.EnInterfaz(this,()=>{terminado=correcto;incierto=dudoso;mensaje.Text=texto;LimpiarSecretos();Estado(false);});
    }
    private async void Consultar_Click(object? sender,EventArgs e)
    {
        if(ocupado)return;Estado(true);
        string texto;bool correcto=false,dudoso=false;
        try{var r=await Task.Run(()=>repo.ConsultarRecuperacion(operacion,recibo)).ConfigureAwait(false);texto=r.Mensaje;correcto=r.Correcto;if(correcto)diario.Borrar();else texto+=" Puedes reintentar con el mismo identificador y las credenciales de recuperación originales.";}
        catch(Exception ex){dudoso=true;System.Diagnostics.Trace.TraceError("Consultar recuperación {0}: {1}",operacion,ex.GetType().Name);texto="No se pudo consultar SQL. Conserva esta ventana y comprueba la conexión.";}
        DisenoFormulario.EnInterfaz(this,()=>{terminado=correcto;incierto=dudoso;mensaje.Text=texto;Estado(false);});
    }
}
