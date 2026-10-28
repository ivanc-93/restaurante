using Microsoft.Data.SqlClient;
using Restaurante.Backend.Dominio;
using Restaurante.Backend.Servicios;
namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioUsuarios:Form
{
    private readonly IRepositorioRestaurante repo=null!;
    private int? id;
    private bool ocupado,resultadoIncierto;
    public FormularioUsuarios()=>InitializeComponent();
    public FormularioUsuarios(IRepositorioRestaurante repo):this()
    {
        this.repo=repo;
        Shown+=async(_,_)=>await Recargar();
        FormClosing+=(_,e)=>{if(ocupado)e.Cancel=true;};
        AcceptButton=guardar;
    }
    private void Ocupado(bool valor)
    {
        ocupado=valor;UseWaitCursor=valor;
        foreach(Control c in Controls)c.Enabled=!valor;
        if(!valor)guardar.Enabled=!resultadoIncierto;
    }
    private async Task Recargar()
    {
        if(ocupado)return;Ocupado(true);
        try{var datos=await Task.Run(repo.ListarUsuarios);if(!IsDisposed)lista.DataSource=datos.ToList();}
        catch(Exception ex){if(!IsDisposed)mensaje.Text=MensajeError(ex);}
        finally{if(!IsDisposed)Ocupado(false);}
    }
    private void Limpiar(){if(ocupado||resultadoIncierto)return;id=null;nombre.Clear();usuario.Clear();clave.Clear();confirmarClave.Clear();rol.SelectedIndex=0;activo.Checked=true;}
    private async void Auditoria_Click(object? sender,EventArgs e)
    {
        if(ocupado)return;Ocupado(true);
        try{var datos=await Task.Run(repo.ConsultarAuditoriaSeguridad);if(!IsDisposed){lista.DataSource=datos.ToList();mensaje.Text="Últimas 200 acciones de seguridad, hora de Guatemala.";}}
        catch(Exception ex){if(!IsDisposed)mensaje.Text=MensajeError(ex);}
        finally{if(!IsDisposed)Ocupado(false);}
    }
    private void Editar_Click(object? sender,EventArgs e)
    {
        if(ocupado||resultadoIncierto||lista.CurrentRow?.DataBoundItem is not UsuarioGestion u)return;
        id=u.Id;nombre.Text=u.Nombre;usuario.Text=u.Usuario;rol.Text=u.Rol=="Empleado"?"Mesero":u.Rol;activo.Checked=u.Activo;clave.Clear();confirmarClave.Clear();
    }
    private static string MensajeError(Exception ex)=>ex is SqlException sql?sql.Number switch
    {
        2601 or 2627=>"Ya existe ese usuario. Elige otro nombre de acceso.",
        229 or 230=>"La cuenta SQL no tiene permisos suficientes. Solicita revisión de la conexión.",
        -2=>"SQL Server tardó demasiado en responder. Comprueba la conexión.",
        53 or 2 or 40 or 4060 or 18456=>"No se pudo acceder a SQL Server. Comprueba servidor, base y permisos.",
        _=>"No se confirmó la operación con SQL Server. Solicita revisión sin repetir el envío."
    }:ex.Message;
    private async void Guardar_Click(object? sender,EventArgs e)
    {
        if(ocupado||resultadoIncierto)return;
        if(!ValidacionCredenciales.Campo(nombre,mensaje,"el nombre")||!ValidacionCredenciales.Campo(usuario,mensaje,"el usuario"))return;
        if((id is null||clave.Text.Length>0)&&!ValidacionCredenciales.Nueva(clave,mensaje,confirmarClave))return;
        var n=nombre.Text;var u=usuario.Text;var r=rol.Text;var a=activo.Checked;var c=clave.Text;var destino=id;
        Ocupado(true);
        try
        {
            await Task.Run(()=>repo.GuardarUsuario(destino,n,u,r,a,c));
            if(IsDisposed)return;
            lista.DataSource=(await Task.Run(repo.ListarUsuarios)).ToList();
            mensaje.Text=$"Usuario guardado: {u}. Una contraseña temporal exige cambio al entrar.";
            id=null;nombre.Clear();usuario.Clear();
        }
        catch(SqlException ex)
        {
            mensaje.Text=MensajeError(ex);
            if(ex.Number is not(2601 or 2627 or 229 or 230 or 18456 or 4060 or 53 or 2 or 40))
            {
                resultadoIncierto=true;
                mensaje.Text+=" El resultado del guardado puede ser incierto; no se repetirá desde esta ventana. Comprueba la cuenta y su auditoría antes de otra intervención. La lectura de campos no confirma un cambio de contraseña.";
                try{lista.DataSource=(await Task.Run(repo.ListarUsuarios)).ToList();}catch{mensaje.Text+=" La comprobación de la lista tampoco pudo completarse.";}
            }
        }
        catch(Exception ex){mensaje.Text=MensajeError(ex);}
        finally{c="";if(!IsDisposed){clave.Clear();confirmarClave.Clear();Ocupado(false);}}
    }
}
