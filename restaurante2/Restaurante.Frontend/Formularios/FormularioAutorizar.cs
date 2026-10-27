using System.Text.Json;
using Restaurante.Backend.Dominio;
using Restaurante.Backend.Servicios;
namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioAutorizar:Form
{
    private readonly IRepositorioRestaurante repo=null!;private readonly Guid id;private readonly string tipo="",json="";private bool ocupado,cerrando;
    public ResultadoAutorizacion? Resultado {get;private set;}
    public string? AdvertenciaCierre {get;private set;}
    public FormularioAutorizar()=>InitializeComponent();
    public FormularioAutorizar(IRepositorioRestaurante repo,Guid id,string tipo,object solicitud,string detalle):this()
    {
        this.repo=repo;this.id=id;this.tipo=tipo;json=JsonSerializer.Serialize(solicitud,solicitud.GetType());resumen.Text=detalle+"\nAutoriza únicamente estos datos durante dos minutos. La sesión del mesero no cambia.";
        FormClosing+=(_,e)=>{if(ocupado){e.Cancel=true;return;}if(!cerrando&&DialogResult!=DialogResult.OK){e.Cancel=true;Cancelar_Click(this,EventArgs.Empty);}};
    }
    private void Procesando(bool valor)
    {ocupado=valor;usuario.Enabled=clave.Enabled=mostrarClave.Enabled=autorizar.Enabled=cancelar.Enabled=!valor;autorizar.Text=valor?"Comprobando…":"Autorizar esta operación";UseWaitCursor=valor;}
    private void EnInterfaz(Action accion)
    {if(IsDisposed)return;if(InvokeRequired)Invoke(accion);else accion();}
    private async void Autorizar_Click(object? sender,EventArgs e)
    {
        if(ocupado)return;Resultado=null;
        if(string.IsNullOrWhiteSpace(usuario.Text)){mensaje.Text="Escribe el usuario del gerente o administrador.";usuario.Focus();return;}
        if(string.IsNullOrEmpty(clave.Text)){mensaje.Text="Escribe la contraseña vigente de esa cuenta.";clave.Focus();return;}
        var u=usuario.Text;var c=clave.Text;Procesando(true);mensaje.Text="Comprobando cuenta y solicitud…";
        ResultadoAutorizacion? resultado=null;string texto;
        try{resultado=await Task.Run(()=>repo.IntentarAutorizarOperacion(id,tipo,json,u,c)).ConfigureAwait(false);texto=resultado.Mensaje;}
        catch(Microsoft.Data.SqlClient.SqlException){texto="No se pudo consultar la base de datos. No se confirmó la autorización. Comprueba la conexión antes de reintentar.";}
        catch(Exception ex)
        {
            System.Diagnostics.Trace.TraceError("Fallo técnico de autorización {0}: {1}",id,ex.GetType().Name);
            texto=$"No se confirmó la autorización por un error técnico ({ex.GetType().Name}). Conserva la solicitud y solicita asistencia; no repitas la gestión manualmente.";
        }
        EnInterfaz(()=>{
            Resultado=resultado;mensaje.Text=texto;clave.Clear();mostrarClave.Checked=false;Procesando(false);
            if(Resultado?.Autorizada==true){cerrando=true;DialogResult=DialogResult.OK;Close();}
            else{if(Resultado?.Estado==EstadoAutorizacion.OperacionAplicada)autorizar.Enabled=false;clave.Focus();}
        });
    }
    private async void Cancelar_Click(object? sender,EventArgs e)
    {
        if(ocupado)return;Procesando(true);
        try{if(repo is not null)await Task.Run(()=>repo.RevocarAutorizacion(id)).ConfigureAwait(false);}
        catch(Exception ex)
        {
            System.Diagnostics.Trace.TraceError("No se confirmó revocación de autorización {0}: {1}",id,ex.GetType().Name);
            AdvertenciaCierre="Se canceló la ventana y no se ejecutó la gestión. No se pudo confirmar en SQL la revocación de una aprobación previa, si existía. Comprueba la conexión y el resultado original antes de reintentar.";
        }
        finally{EnInterfaz(()=>{clave.Clear();mostrarClave.Checked=false;Procesando(false);cerrando=true;DialogResult=DialogResult.Cancel;Close();});}
    }
    public static bool Solicitar(IWin32Window owner,IRepositorioRestaurante repo,Guid id,string tipo,object solicitud,string detalle)
    {
        if(repo.UsuarioActual?.Rol is "Administrador" or "Gerente")return true;
        using var f=new FormularioAutorizar(repo,id,tipo,solicitud,detalle);var resultado=f.ShowDialog(owner);
        if(f.AdvertenciaCierre is not null)MessageBox.Show(owner,f.AdvertenciaCierre,"Autorización cancelada",MessageBoxButtons.OK,MessageBoxIcon.Warning);
        return resultado==DialogResult.OK;
    }
}
