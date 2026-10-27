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
        var conexion=new Button{Text="Configurar o recuperar conexión",AutoSize=true};conexion.SetBounds(35,340,420,42);
        conexion.Click+=(_,_)=>{using var f=new FormularioConexion(_configuracion);f.ShowDialog(this);_repositorio.CerrarSesion();};
        ayuda.Visible=false;Controls.Add(conexion);
    }
    private async void Entrar_Click(object? sender, EventArgs e)
    {
        var usuario = _usuario.Text;
        var contrasena = _contrasena.Text;
        _mensaje.Text = "";
        entrar.Enabled = false;
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
            _mensaje.Text = ex.Message;
        }
        finally { entrar.Enabled = true; }
    }
}
