using Restaurante.Backend.Dominio;
using Restaurante.Backend.Servicios;

namespace Restaurante.Frontend.Formularios;

public sealed class FormularioAcceso : Form
{
    private readonly IRepositorioRestaurante _repositorio;
    private readonly IConfiguracionConexion _configuracion;
    private readonly TextBox _usuario = Componentes.Campo();
    private readonly TextBox _contrasena = Componentes.Campo(true);
    private readonly Label _mensaje = new() { AutoSize = true, ForeColor = Color.Firebrick };
    public UsuarioAutenticado? UsuarioActual { get; private set; }

    public FormularioAcceso(IRepositorioRestaurante repositorio, IConfiguracionConexion configuracion)
    {
        _repositorio = repositorio;
        _configuracion = configuracion;
        Text = "Sabores | Iniciar sesión";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(850, 540);
        Size = new Size(940, 590);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = Componentes.Fondo;
        CrearInterfaz();
    }

    private void CrearInterfaz()
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52));
        Controls.Add(panel);

        var ladoMarca = new Panel { Dock = DockStyle.Fill, BackColor = Componentes.Verde, Padding = new Padding(48) };
        panel.Controls.Add(ladoMarca, 0, 0);
        var marca = new Label { Text = "S   sabores", ForeColor = Color.White, Font = new Font("Georgia", 22, FontStyle.Bold), AutoSize = true, Location = new Point(42, 32) };
        ladoMarca.Controls.Add(marca);
        var descripcion = new Label
        {
            Text = "GESTIÓN CON SABOR\r\n\r\nTodo tu restaurante,\r\nen una mesa.\r\n\r\nPedidos, mesas y ventas: simples, claros, en un mismo lugar.",
            ForeColor = Color.White,
            Font = new Font("Georgia", 18),
            AutoSize = true,
            Location = new Point(52, 176)
        };
        ladoMarca.Controls.Add(descripcion);
        ladoMarca.Controls.Add(new Label { Text = "SISTEMA DE GESTIÓN · 2026", ForeColor = Color.FromArgb(208, 220, 205), AutoSize = true, Location = new Point(52, 490) });

        var centro = new Panel { Dock = DockStyle.Fill, Padding = new Padding(60, 78, 60, 50), BackColor = Color.White };
        panel.Controls.Add(centro, 1, 0);
        var formulario = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 1, AutoSize = true };
        centro.Controls.Add(formulario);
        formulario.Controls.Add(new Label { Text = "BIENVENIDO DE VUELTA", AutoSize = true, ForeColor = Componentes.Secundario, Font = new Font("Segoe UI", 9, FontStyle.Bold), Margin = new Padding(0, 4, 0, 13) });
        formulario.Controls.Add(new Label { Text = "Inicia sesión", AutoSize = true, ForeColor = Componentes.Tinta, Font = new Font("Georgia", 25, FontStyle.Bold), Margin = new Padding(0, 0, 0, 4) });
        formulario.Controls.Add(new Label { Text = "Ingresa tus credenciales para continuar.", AutoSize = true, ForeColor = Componentes.Secundario, Margin = new Padding(0, 0, 0, 25) });
        formulario.Controls.Add(Componentes.Etiqueta("Usuario"));
        _usuario.Text = "admin";
        formulario.Controls.Add(_usuario);
        formulario.Controls.Add(Componentes.Etiqueta("Contraseña"));
        _contrasena.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; IniciarSesion(); } };
        formulario.Controls.Add(_contrasena);
        _mensaje.Margin = new Padding(0, 0, 0, 12);
        formulario.Controls.Add(_mensaje);
        var entrar = Componentes.Boton("Entrar al sistema     →");
        entrar.Dock = DockStyle.Top;
        entrar.Click += (_, _) => IniciarSesion();
        formulario.Controls.Add(entrar);
        formulario.Controls.Add(new Label
        {
            Text = "Acceso inicial: admin / Admin123!\r\nConfigura SQL Server después de iniciar sesión.",
            AutoSize = true,
            ForeColor = Componentes.Secundario,
            BackColor = Componentes.Fondo,
            Padding = new Padding(12),
            Margin = new Padding(0, 18, 0, 0)
        });
        AcceptButton = entrar;
    }

    private void IniciarSesion()
    {
        _mensaje.Text = "";
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
                UsuarioActual = _repositorio.IniciarSesion(_usuario.Text, _contrasena.Text);
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
    }
}
