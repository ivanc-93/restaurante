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
    private async void Entrar_Click(object? sender, EventArgs e)
    {
        var usuario = _usuario.Text;
        var contrasena = _contrasena.Text;
        _mensaje.Text = "";
        entrar.Enabled = false;
        try
        {
            if (!_configuracion.EstaConfigurada &&
                _usuario.Text.Trim().Equals("admin", StringComparison.OrdinalIgnoreCase) &&
                _contrasena.Text == "Admin123!")
            {
                UsuarioActual = new UsuarioAutenticado("Administrador", "Administrador");
            }
            else
            {
                UsuarioActual = await Task.Run(() => _repositorio.IniciarSesion(usuario, contrasena));
                if (UsuarioActual is null)
                {
                    _mensaje.Text = _configuracion.EstaConfigurada
                        ? "Usuario o contraseña incorrectos."
                        : "Configure primero SQL Server o use el acceso inicial de administrador.";
                    return;
                }
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
