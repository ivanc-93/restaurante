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
    }
    private bool ocupado;
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
        try
        {
            await Task.Run(_repositorio.ValidarEsquema);
            UsuarioActual=await Task.Run(()=>_repositorio.IniciarSesion(usuario,contrasena));
            if(UsuarioActual is null){_mensaje.Text="Usuario o contraseña incorrectos, inactivo o temporalmente bloqueado.";return;}
            if(UsuarioActual.CambioObligatorio)
            {
                using var cambio=new FormularioCambiarClave(_repositorio);
                cambio.ShowDialog(this);_repositorio.CerrarSesion();UsuarioActual=null;
                _mensaje.Text="Vuelva a entrar con su contraseña personal actualizada.";return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            _mensaje.Text = ex is Microsoft.Data.SqlClient.SqlException ? "No se pudo consultar SQL Server. Comprueba servidor, base y permisos desde Configurar o recuperar conexión." : ex.Message;
        }
        finally { ocupado=false;if(!IsDisposed){entrar.Enabled=conexion.Enabled=true;entrar.Text="Entrar al sistema";UseWaitCursor=false;_contrasena.Clear();mostrarClave.Checked=false;} }
    }
}
