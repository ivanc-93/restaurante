using Restaurante.Backend.Dominio;
using Restaurante.Backend.Servicios;
namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioUsuarios:Form
{
    private readonly IRepositorioRestaurante repo=null!;private int? id;private bool ocupado;
    public FormularioUsuarios()=>InitializeComponent();
    public FormularioUsuarios(IRepositorioRestaurante repo):this(){this.repo=repo;Shown+=async(_,_)=>await Recargar();FormClosing+=(_,e)=>e.Cancel=ocupado;}
    private async Task Recargar(){try{lista.DataSource=(await Task.Run(repo.ListarUsuarios)).ToList();}catch(Exception ex){mensaje.Text=ex.Message;}}
    private void Limpiar(){id=null;nombre.Clear();usuario.Clear();clave.Clear();confirmarClave.Clear();rol.SelectedIndex=0;activo.Checked=true;}
    private async void Auditoria_Click(object? sender,EventArgs e)
    {
        try{lista.DataSource=(await Task.Run(repo.ConsultarAuditoriaSeguridad)).ToList();mensaje.Text="Últimas 200 acciones de seguridad, hora de Guatemala. Use Nuevo usuario para volver al mantenimiento.";}
        catch(Exception ex){mensaje.Text=ex.Message;}
    }
    private void Editar_Click(object? sender,EventArgs e)
    {if(lista.CurrentRow?.DataBoundItem is not UsuarioGestion u)return;id=u.Id;nombre.Text=u.Nombre;usuario.Text=u.Usuario;rol.Text=u.Rol=="Empleado"?"Mesero":u.Rol;activo.Checked=u.Activo;clave.Clear();}
    private async void Guardar_Click(object? sender,EventArgs e)
    {
        if(ocupado)return;
        if(!ValidacionCredenciales.Campo(nombre,mensaje,"el nombre")||!ValidacionCredenciales.Campo(usuario,mensaje,"el usuario"))return;
        if((id is null||clave.Text.Length>0)&&!ValidacionCredenciales.Nueva(clave,mensaje,confirmarClave))return;
        var n=nombre.Text;var u=usuario.Text;var r=rol.Text;var a=activo.Checked;var c=clave.Text;var destino=id;ocupado=true;guardar.Enabled=false;
        try{await Task.Run(()=>repo.GuardarUsuario(destino,n,u,r,a,c));mensaje.Text="Usuario guardado. Una contraseña temporal exige cambio al entrar.";Limpiar();await Recargar();}
        catch(Exception ex){mensaje.Text=ex is Microsoft.Data.SqlClient.SqlException?"No se pudo guardar. Compruebe que el usuario no esté repetido.":ex.Message;}
        finally{clave.Clear();confirmarClave.Clear();ocupado=false;guardar.Enabled=true;}
    }
}
