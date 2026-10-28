using Restaurante.Backend.Dominio;
using Restaurante.Backend.Servicios;

namespace Restaurante.Frontend.Formularios;

public sealed partial class FormularioAcceso : Form
{
    private readonly IRepositorioRestaurante _repositorio = null!;
    private readonly IConfiguracionConexion _configuracion = null!;
    public UsuarioAutenticado? UsuarioActual { get; private set; }

    public FormularioAcceso() => InitializeComponent();
    public FormularioAcceso(IRepositorioRestaurante repositorio, IConfiguracionConexion configuracion) : this()
    {
        _repositorio = repositorio;
        _configuracion = configuracion;
        FormClosing+=(_,e)=>e.Cancel=ocupado;
    }
    private bool ocupado;
    private async void RecuperarClave_Click(object? sender,LinkLabelLinkClickedEventArgs e)
    {
        if(ocupado)return;ocupado=true;entrar.Enabled=conexion.Enabled=false;
        try
        {
            if(!await Task.Run(_repositorio.RecuperacionDisponible)){_mensaje.Text="La recuperación todavía no está habilitada en esta base. Solicita al responsable técnico la actualización respaldada; tus usuarios se conservan.";return;}
            using var f=new FormularioRecuperarClave(_repositorio);f.ShowDialog(this);
        }
        catch(Exception){_mensaje.Text="No se pudo comprobar la recuperación. Revisa la conexión SQL; no se cambió ninguna contraseña.";}
        finally{ocupado=false;entrar.Enabled=conexion.Enabled=true;_contrasena.Clear();}
    }
    private void Conexion_Click(object? sender,EventArgs e)
    {
        if(ocupado||_configuracion is null)return;
        using var f=new FormularioConexion(_configuracion);f.ShowDialog(this);_repositorio.CerrarSesion();
    }

    private async void Entrar_Click(object? sender, EventArgs e)
    {
        if(ocupado)return;
        _mensaje.ForeColor=Color.Firebrick;
        if(string.IsNullOrWhiteSpace(_usuario.Text)){_mensaje.Text="Escribe tu usuario de Sabores.";_usuario.Focus();return;}
        if(string.IsNullOrEmpty(_contrasena.Text)){_mensaje.Text="Escribe tu contraseña actual.";_contrasena.Focus();return;}
        var usuario = _usuario.Text;
        var contrasena = _contrasena.Text;
        ocupado=true;_mensaje.ForeColor=Color.Firebrick;_mensaje.Text="Comprobando acceso…";
        entrar.Enabled=conexion.Enabled=false;entrar.Text="Comprobando…";UseWaitCursor=true;
        UsuarioAutenticado? verificado=null;string? error=null;
        try{verificado=await Task.Run(()=>{_repositorio.ValidarEsquema();return _repositorio.IniciarSesion(usuario,contrasena);}).ConfigureAwait(false);}
        catch(Exception ex){error=ex is Microsoft.Data.SqlClient.SqlException?"No se pudo consultar SQL Server. Comprueba servidor, base y permisos desde Configurar o recuperar conexión.":ex.Message;}
        DisenoFormulario.EnInterfaz(this,()=>
        {
            try
            {
                UsuarioActual=verificado;
                if(error is not null){_mensaje.Text=error;return;}
                if(UsuarioActual is null){_mensaje.Text="Usuario o contraseña incorrectos, inactivo o temporalmente bloqueado.";return;}
                if(UsuarioActual.CambioObligatorio)
                {
                    using var cambio=new FormularioCambiarClave(_repositorio);cambio.ShowDialog(this);_repositorio.CerrarSesion();UsuarioActual=null;
                    _mensaje.Text="Vuelva a entrar con su contraseña personal actualizada.";return;
                }
                ocupado=false;DialogResult=DialogResult.OK;Close();
            }
            finally{ocupado=false;if(!IsDisposed){entrar.Enabled=conexion.Enabled=true;entrar.Text="Entrar al sistema";UseWaitCursor=false;_contrasena.Clear();mostrarClave.Checked=false;}}
        });
    }
}
