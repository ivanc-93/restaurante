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
        FormClosing += (_, e) => e.Cancel = ocupado;
    }

    private bool ocupado;
    private async void RecuperarClave_Click(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        if (ocupado)
            return;
        ocupado = true;
        btnEntrarSistema.Enabled = btnConfigurarConexion.Enabled = false;
        try
        {
            if (!await Task.Run(_repositorio.RecuperacionDisponible))
            {
                lblMensaje.Text = "La recuperación todavía no está habilitada en esta base. Solicita al responsable técnico la actualización respaldada; tus usuarios se conservan.";
                return;
            }

            using var f = new FormularioRecuperarClave(_repositorio);
            f.ShowDialog(this);
        }
        catch (Exception)
        {
            lblMensaje.Text = "No se pudo comprobar la recuperación. Revisa la conexión SQL; no se cambió ninguna contraseña.";
        }
        finally
        {
            ocupado = false;
            btnEntrarSistema.Enabled = btnConfigurarConexion.Enabled = true;
            ctContrasena.Clear();
        }
    }

    private void Conexion_Click(object? sender, EventArgs e)
    {
        if (ocupado || _configuracion is null)
            return;
        using var f = new FormularioConexion(_configuracion);
        f.ShowDialog(this);
        _repositorio.CerrarSesion();
    }

    private async void Entrar_Click(object? sender, EventArgs e)
    {
        if (ocupado)
            return;
        lblMensaje.ForeColor = Color.Firebrick;
        if (string.IsNullOrWhiteSpace(ctUsuario.Text))
        {
            lblMensaje.Text = "Escribe tu usuario de Sabores.";
            ctUsuario.Focus();
            return;
        }

        if (string.IsNullOrEmpty(ctContrasena.Text))
        {
            lblMensaje.Text = "Escribe tu contraseña actual.";
            ctContrasena.Focus();
            return;
        }

        var usuario = ctUsuario.Text;
        var contrasena = ctContrasena.Text;
        ocupado = true;
        lblMensaje.ForeColor = Color.Firebrick;
        lblMensaje.Text = "Comprobando acceso…";
        btnEntrarSistema.Enabled = btnConfigurarConexion.Enabled = false;
        btnEntrarSistema.Text = "Comprobando…";
        UseWaitCursor = true;
        UsuarioAutenticado? verificado = null;
        string? error = null;
        try
        {
            verificado = await Task.Run(() =>
            {
                _repositorio.ValidarEsquema();
                return _repositorio.IniciarSesion(usuario, contrasena);
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            error = ex is Microsoft.Data.SqlClient.SqlException ? "No se pudo consultar SQL Server. Comprueba servidor, base y permisos desde Configurar o recuperar conexión." : ex.Message;
        }

        DisenoFormulario.EnInterfaz(this, () =>
        {
            try
            {
                UsuarioActual = verificado;
                if (error is not null)
                {
                    lblMensaje.Text = error;
                    return;
                }

                if (UsuarioActual is null)
                {
                    lblMensaje.Text = "Usuario o contraseña incorrectos, inactivo o temporalmente bloqueado.";
                    return;
                }

                if (UsuarioActual.CambioObligatorio)
                {
                    using var cambio = new FormularioCambiarClave(_repositorio);
                    cambio.ShowDialog(this);
                    _repositorio.CerrarSesion();
                    UsuarioActual = null;
                    lblMensaje.Text = "Vuelva a entrar con su contraseña personal actualizada.";
                    return;
                }

                ocupado = false;
                DialogResult = DialogResult.OK;
                Close();
            }
            finally
            {
                ocupado = false;
                if (!IsDisposed)
                {
                    btnEntrarSistema.Enabled = btnConfigurarConexion.Enabled = true;
                    btnEntrarSistema.Text = "Entrar al sistema";
                    UseWaitCursor = false;
                    ctContrasena.Clear();
                    chkMostrarContrasena.Checked = false;
                }
            }
        });
    }
}
