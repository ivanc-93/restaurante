using Restaurante.Frontend.Servicios;
using Restaurante.Backend.Dominio;
using Restaurante.Backend.Servicios;

namespace Restaurante.Frontend.Formularios;

public sealed partial class FormularioPrincipal : Form
{
    private readonly IDialogosUsuario _dialogos = new DialogosUsuario();
    private readonly IRepositorioRestaurante _repositorio = null!;
    private readonly IConfiguracionConexion _configuracion = null!;
    private readonly UsuarioAutenticado _usuario = null!;

    public bool SesionCerrada { get; private set; }
    public FormularioPrincipal() => InitializeComponent();
    public FormularioPrincipal(IRepositorioRestaurante repositorio, IConfiguracionConexion configuracion,
        UsuarioAutenticado usuario, IDialogosUsuario? dialogos = null) : this()
    {
        _repositorio = repositorio;
        _configuracion = configuracion;
        _usuario = usuario;
        _dialogos = dialogos ?? new DialogosUsuario();
        usuarioLabel.Text = $"{usuario.Nombre} ({usuario.Rol})";
        AgregarNavegacion(menu, "RESUMEN", "Inicio", MostrarInicio);
        AgregarNavegacion(menu, "SERVICIO", "Mesas", MostrarMesasPos);
        AgregarNavegacion(menu, "SERVICIO", "Pedidos", MostrarPedidosPos);
        AgregarNavegacion(menu, "DATOS", "Catálogos", MostrarCatalogos);
        AgregarNavegacion(menu, "ANÁLISIS", "Reportes", MostrarReportes);
        if (usuario.Rol == "Administrador")
            AgregarNavegacion(menu, "ADMINISTRACIÓN", "Configuración", MostrarConfiguracion);
        menu.Enabled = false;
        _contenido.Enabled = false;
        salir.Enabled = false;
        Shown += async (_, _) => {
            await EjecutarSeguro(MostrarInicio);
            if (IsDisposed) return;
            var inicio = menu.Controls.OfType<Button>().First();
            inicio.BackColor = Componentes.Verde; inicio.ForeColor = Color.White;
            Iconos.Aplicar(inicio, "layout-dashboard");
        };
        FormClosing += (_, e) => { if (UseWaitCursor) e.Cancel = true; };
    }

    private void Salir_Click(object? sender, EventArgs e)
    {
        SesionCerrada = true;
        Close();
    }

    private Task MostrarCatalogos()
    {
        using var formulario = new FormularioCatalogos(_repositorio, false);
        formulario.ShowDialog(this);
        return Task.CompletedTask;
    }
    private void AgregarNavegacion(FlowLayoutPanel menu, string grupo, string texto, Func<Task> accion)
    {
        if (menu.Controls.Count == 0 || !Equals(menu.Controls[^1].Tag, grupo))
        {
            var etiqueta = Componentes.Etiqueta(grupo, 8, true);
            etiqueta.ForeColor = Componentes.Secundario;
            etiqueta.Margin = new Padding(8, 14, 0, 3);
            etiqueta.Tag = grupo;
            menu.Controls.Add(etiqueta);
        }
        var boton = Componentes.Boton(texto, false);
        boton.Width = Math.Max(190, menu.ClientSize.Width - 24);
        boton.Tag = grupo;
        boton.Height = 48;
        boton.TextAlign = ContentAlignment.MiddleLeft;
        boton.Margin = new Padding(0, 2, 0, 2);
        var icono = texto switch { "Inicio" => "layout-dashboard", "Mesas" => "armchair", "Pedidos" => "clipboard-list", "Catálogos" => "utensils-crossed", "Reportes" => "chart-column", _ => "settings" };
        Iconos.Aplicar(boton, icono);
        boton.Click += async (_, _) => {
            await EjecutarSeguro(accion);
            foreach (var item in menu.Controls.OfType<Button>()) {
                item.BackColor = item == boton ? Componentes.Verde : Color.White;
                item.ForeColor = item == boton ? Color.White : Componentes.Tinta;
                Iconos.Aplicar(item, item.Text switch { "Inicio" => "layout-dashboard", "Mesas" => "armchair", "Pedidos" => "clipboard-list", "Catálogos" => "utensils-crossed", "Reportes" => "chart-column", _ => "settings" });
            }
        };
        boton.Enabled = _configuracion.EstaConfigurada || accion == MostrarConfiguracion || accion == MostrarInicio;
        menu.Controls.Add(boton);
    }

    private async Task EjecutarSeguro(Func<Task> accion)
    {
        menu.Enabled = false;
        _contenido.Enabled = false;
        salir.Enabled = false;
        UseWaitCursor = true;
        try { await accion(); }
        catch (Exception ex) { MostrarError(ex); }
        finally
        {
            if (!IsDisposed) { FormatoPresentacion.Aplicar(this); menu.Enabled = true; _contenido.Enabled = true; salir.Enabled = true; UseWaitCursor = false; }
        }
    }

    private async Task MostrarInicio()
    {
        _titulo.Text = "Panel de control  /  Resumen";
        LimpiarContenido();
        var pagina = CrearPagina();
        pagina.Controls.Add(Encabezado("Hola, " + _usuario.Nombre, "Esto es lo que sucede hoy en tu restaurante."));
        if (!_configuracion.EstaConfigurada)
        {
            pagina.Controls.Add(Aviso("Configura una conexión a SQL Server para comenzar.",
                "Abrir conexión SQL", MostrarConexion));
            return;
        }
        var resumen = await Task.Run(_repositorio.ObtenerResumen);
        var restaurante = await Task.Run(_repositorio.ObtenerRestaurante);
        Text = restaurante.Nombre + " | " + restaurante.Ubicacion;
        var tarjetas = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true };
        tarjetas.Controls.Add(TarjetaEstadistica("VENTAS DE HOY", FormatoPresentacion.Moneda(resumen.VentasDelDia), "Facturación del día", Color.FromArgb(216, 131, 80)));
        tarjetas.Controls.Add(TarjetaEstadistica("PEDIDOS ABIERTOS", resumen.PedidosAbiertos.ToString(), "En atención", Color.FromArgb(89, 129, 149)));
        tarjetas.Controls.Add(TarjetaEstadistica("MESAS DISPONIBLES", resumen.MesasDisponibles.ToString(), "Listas para clientes", Componentes.Verde));
        tarjetas.Controls.Add(TarjetaEstadistica("PRODUCTOS ACTIVOS", resumen.ProductosActivos.ToString(), "En el menú", Color.FromArgb(122, 111, 161)));
        pagina.Controls.Add(tarjetas);
        var bienvenida = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(20) };
        bienvenida.Controls.Add(new Label { Text="Registra pedidos, consulta ventas y administra tus mesas.", AutoSize=true, Font=Componentes.Fuente });
        var nuevo = Componentes.Boton("Ver mesas");
        nuevo.AutoSize = true;
        nuevo.Click += async (_, _) => await EjecutarSeguro(MostrarMesasPos);
        bienvenida.Controls.Add(nuevo); pagina.Controls.Add(bienvenida);
        var abiertos = (await Task.Run(_repositorio.ListarPedidos)).Where(p => p.Estado is "Abierto" or "En preparación").Take(5).ToList();
        pagina.Controls.Add(Componentes.Etiqueta("Pedidos abiertos", 14, true));
        if (abiertos.Count == 0) pagina.Controls.Add(Componentes.Etiqueta("No hay pedidos abiertos. Selecciona una mesa para comenzar."));
        foreach (var pedido in abiertos)
        {
            var continuar = Componentes.Boton($"Mesa {pedido.NumeroMesa} · Pedido #{pedido.Id} · {FormatoPresentacion.Moneda(pedido.Total)}", false);
            continuar.AutoSize = true;
            continuar.Click += async (_, _) => await EjecutarSeguro(async () => {
                var mesas = await Task.Run(_repositorio.ListarMesas);
                AbrirMesa(mesas.Single(m => m.Numero == pedido.NumeroMesa)); await MostrarInicio();
            });
            pagina.Controls.Add(continuar);
        }
        FormatoPresentacion.Aplicar(this);
    }

    private TableLayoutPanel CrearPagina()
    {
        var pagina = new TableLayoutPanel { Dock=DockStyle.Top, AutoSize=true, AutoSizeMode=AutoSizeMode.GrowAndShrink, ColumnCount=1, GrowStyle=TableLayoutPanelGrowStyle.AddRows };
        pagina.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        _contenido.Controls.Add(pagina);
        return pagina;
    }

    private static Control Encabezado(string titulo, string descripcion)
    {
        var panel = new TableLayoutPanel { Dock=DockStyle.Top, AutoSize=true, ColumnCount=1, Padding=new Padding(0,0,0,15) };
        panel.Controls.Add(new Label { Text=titulo, AutoSize=true, Font=new Font("Segoe UI",23,FontStyle.Bold), ForeColor=Componentes.Tinta });
        panel.Controls.Add(new Label { Text=descripcion, AutoSize=true, ForeColor=Componentes.Secundario, Margin=new Padding(0,10,0,5) });
        return panel;
    }
    private static Control TarjetaEstadistica(string titulo, string valor, string detalle, Color acento)
    {
        var panel = new TableLayoutPanel { BackColor = Color.White, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1, RowCount = 3, Padding = new Padding(16), Margin = new Padding(0, 0, 12, 12), MinimumSize = new Size(205, 128) };
        panel.Controls.Add(new Label { Text = titulo, AutoSize = true, ForeColor = Componentes.Secundario, Font = new Font("Segoe UI", 9, FontStyle.Bold) });
        panel.Controls.Add(new Label { Text = valor, AutoSize = true, ForeColor = acento, Font = new Font("Segoe UI", 23, FontStyle.Bold), Margin = new Padding(0, 6, 0, 4) });
        panel.Controls.Add(new Label { Text = detalle, AutoSize = true, ForeColor = Componentes.Secundario });
        return panel;
    }

    private async Task MostrarMenuMesas()
    {
        _titulo.Text = "Servicio  /  Menú y mesas";
        LimpiarContenido();
        var pagina = CrearPagina();
        pagina.Controls.Add(Encabezado("Menú y mesas", "Consulta los productos activos y el estado de las mesas."));
        var pestanas = new TabControl { Dock = DockStyle.Top, Height = 510, Font = Componentes.Fuente };
        var tabMenu = new TabPage("  Menú  ") { BackColor = Color.White, Padding = new Padding(12) };
        var tabMesas = new TabPage("  Mesas  ") { BackColor = Color.White, Padding = new Padding(12) };
        var menu = NuevaTabla();
        var mesas = NuevaTabla();
        var herramientasMesas = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 52,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 2, 0, 4)
        };
        var crear = BotonMesa("＋ Nueva mesa", 125);
        var editar = BotonMesa("Editar", 90);
        var visualizar = BotonMesa("Visualizar", 100);
        var eliminar = BotonMesa("Eliminar", 90, false);
        herramientasMesas.Controls.AddRange([crear, editar, visualizar, eliminar]);
        tabMesas.Controls.Add(mesas);
        tabMesas.Controls.Add(herramientasMesas);
        tabMenu.Controls.Add(menu);
        pestanas.TabPages.Add(tabMenu);
        pestanas.TabPages.Add(tabMesas);
        pagina.Controls.Add(pestanas);
        menu.DataSource = (await Task.Run(_repositorio.ListarMenu)).ToList();

        async Task<List<Mesa>> CargarMesas()
        {
            var lista = (await Task.Run(_repositorio.ListarMesas)).ToList();
            mesas.DataSource = null;
            mesas.DataSource = lista;
            return lista;
        }

        Mesa? MesaSeleccionada() => mesas.CurrentRow?.DataBoundItem as Mesa;

        crear.Click += async (_, _) => await EjecutarSeguro(async () =>
        {
            var siguiente = (await CargarMesas()).Select(m => m.Numero).DefaultIfEmpty(0).Max() + 1;
            using var formulario = new FormularioMesa(siguienteNumero: siguiente);
            if (formulario.ShowDialog(this) != DialogResult.OK) return;
            var numero = formulario.NumeroMesa; var capacidad = formulario.Capacidad;
            await Task.Run(() => _repositorio.CrearMesa(numero, capacidad));
            await CargarMesas();
        });

        editar.Click += async (_, _) => await EjecutarSeguro(async () =>
        {
            var seleccionada = MesaSeleccionada();
            if (seleccionada is null)
            {
                _dialogos.Mostrar(this, "Selecciona una mesa para editar.", "Mesas",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using var formulario = new FormularioMesa(seleccionada);
            if (formulario.ShowDialog(this) != DialogResult.OK) return;
            var numero = formulario.NumeroMesa; var capacidad = formulario.Capacidad; var estado = formulario.EstadoMesa;
            await Task.Run(() => _repositorio.ActualizarMesa(seleccionada.Id, numero, capacidad, estado));
            await CargarMesas();
        });

        visualizar.Click += async (_, _) => await EjecutarSeguro(async () =>
        {
            var seleccionada = MesaSeleccionada();
            if (seleccionada is null)
            {
                _dialogos.Mostrar(this, "Selecciona una mesa para visualizar.", "Mesas",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using var formulario = new FormularioDetalleMesa(_repositorio, seleccionada, _dialogos);
            formulario.ShowDialog(this);
            await CargarMesas();
        });

        eliminar.Click += async (_, _) => await EjecutarSeguro(async () =>
        {
            var seleccionada = MesaSeleccionada();
            if (seleccionada is null)
            {
                _dialogos.Mostrar(this, "Selecciona una mesa para eliminar.", "Mesas",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var confirmacion = _dialogos.Mostrar(this,
                $"¿Deseas eliminar la mesa {seleccionada.Numero}?",
                "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirmacion != DialogResult.Yes) return;
            await Task.Run(() => _repositorio.EliminarMesa(seleccionada.Id));
            await CargarMesas();
        });

        await CargarMesas();
    }

    private static Button BotonMesa(string texto, int ancho, bool primario = true)
    {
        var boton = Componentes.Boton(texto, primario);
        boton.Width = ancho;
        boton.AutoSize = true;
        boton.Height = 36;
        boton.Margin = new Padding(0, 2, 8, 2);
        return boton;
    }

    private Task MostrarReportes()
    {
        using var formulario = new FormularioReportes(_repositorio);
        formulario.ShowDialog(this);
        return Task.CompletedTask;
    }

    private void LimpiarContenido()
    {
        while (_contenido.Controls.Count > 0) _contenido.Controls[0].Dispose();
    }
    private Task MostrarConexion()
    {
        if (_usuario.Rol != "Administrador")
        {
            _dialogos.Mostrar(this, "Solo el administrador puede cambiar la conexión.", "Acceso denegado",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return Task.CompletedTask;
        }
        _titulo.Text = "Administración  /  Conexión SQL";
        LimpiarContenido();
        var encabezado = Encabezado("Conexión a SQL Server",
            "Configura el servidor, la base de datos y las credenciales.");
        var panel = Componentes.Tarjeta();
        panel.Dock = DockStyle.Top;
        panel.Height = 500;
        panel.Padding = new Padding(20);
        panel.Margin = new Padding(0, 0, 0, 16);
        var servidor = Componentes.Campo();
        servidor.PlaceholderText = "192.168.1.100 o localhost";
        var puerto = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 65535,
            Value = 1433,
            Dock = DockStyle.Top,
            Height = 34,
            Font = Componentes.Fuente,
            Margin = new Padding(0, 4, 0, 8)
        };
        var baseDatos = Componentes.Campo();
        baseDatos.Text = "RestauranteDB";
        var usuario = Componentes.Campo();
        usuario.Text = "restaurante_app";
        usuario.PlaceholderText = "restaurante_app";
        var contrasena = Componentes.Campo(true);
        contrasena.PlaceholderText = "Contraseña del usuario SQL";
        var certificado = new CheckBox
        {
            Text = "Confiar en el certificado SQL",
            Checked = true,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(4, 18, 0, 0)
        };
        var estado = new Label
        {
            Text = _configuracion.EstaConfigurada ? "●  Conexión configurada" : "●  Falta configurar la base de datos",
            AutoSize = true,
            ForeColor = _configuracion.EstaConfigurada ? Componentes.Verde : Color.DarkGoldenrod,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(3, 8, 0, 0)
        };
        var campos = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 6 };
        campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
        campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        campos.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        campos.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        campos.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
        campos.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        campos.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        campos.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        campos.Controls.Add(Campo("IP o nombre del servidor SQL", servidor), 0, 0);
        campos.Controls.Add(Campo("Puerto SQL Server", puerto), 1, 0);
        campos.Controls.Add(Campo("Nombre de la base de datos", baseDatos), 0, 1);
        campos.Controls.Add(Campo("Usuario SQL", usuario), 1, 1);
        campos.Controls.Add(Campo("Contraseña SQL", contrasena), 0, 2);
        campos.Controls.Add(certificado, 1, 2);
        campos.Controls.Add(estado, 0, 3);
        campos.SetColumnSpan(estado, 2);
        var guardar = Componentes.Boton("Probar y guardar conexión");
        guardar.Anchor = AnchorStyles.Left | AnchorStyles.Top;
        guardar.Width = 230;
        guardar.Click += async (_, _) =>
        {
            guardar.Enabled = false;
            estado.Text = "Conectando y verificando credenciales…";
            try
            {
                var host = servidor.Text; var port = (int)puerto.Value; var db = baseDatos.Text;
                var user = usuario.Text; var password = contrasena.Text; var trust = certificado.Checked;
                await Task.Run(() => _configuracion.ProbarYGuardar(host, port, db, user, password, trust));
                estado.Text = "●  Conexión probada y guardada de forma protegida.";
                estado.ForeColor = Componentes.Verde;
                _dialogos.Mostrar(this, "La conexión SQL Server es válida y se guardó protegida.",
                    "Conexión correcta", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SesionCerrada = true;
                Close();
            }
            catch (Exception ex)
            {
                estado.Text = "No se pudo conectar.";
                estado.ForeColor = Color.Firebrick;
                MostrarError(ex);
            }
            finally { guardar.Enabled = true; }
        };
        campos.Controls.Add(guardar, 0, 4);
        var ayuda = new Label
        {
            Text = string.Empty,
            Dock = DockStyle.Fill, ForeColor = Componentes.Secundario,
            BackColor = Componentes.Fondo, Padding = new Padding(10), Margin = new Padding(0, 4, 0, 0)
        };
        ayuda.Text = "Ejemplo: IP 192.168.1.100; puerto 1433; base de datos RestauranteDB; usuario restaurante_app; contrasena TuClaveSegura. Cadena: Server=192.168.1.100,1433;Database=RestauranteDB;User Id=restaurante_app;Password=TuClaveSegura;Encrypt=True;TrustServerCertificate=True.";
        campos.Controls.Add(ayuda, 0, 5);
        campos.SetColumnSpan(ayuda, 2);
        panel.Controls.Add(campos);
        _contenido.Controls.Add(panel);
        _contenido.Controls.Add(encabezado);
        return Task.CompletedTask;
    }

    private static Control Campo(string etiqueta, Control campo)
    {
        var grupo = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = new Padding(0, 0, 12, 0)
        };
        grupo.RowStyles.Add(new RowStyle(SizeType.Absolute, 27));
        grupo.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var titulo = Componentes.Etiqueta(etiqueta, 9, true);
        titulo.Margin = new Padding(0, 2, 0, 0);
        campo.Dock = DockStyle.Fill;
        grupo.Controls.Add(titulo, 0, 0);
        grupo.Controls.Add(campo, 0, 1);
        return grupo;
    }

    private static ComboBox Combo() => new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "Nombre",
        Font = Componentes.Fuente, Dock = DockStyle.Top, Height = 34, Margin = new Padding(0, 3, 0, 9)
    };

    private static DataGridView NuevaTabla()
    {
        var tabla = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = true };
        Componentes.EstilizarTabla(tabla);
        return tabla;
    }

    private static Control ComponenteSeccion(string titulo, string subtitulo)
    {
        var panel = Componentes.Tarjeta();
        panel.Dock = DockStyle.Fill;
        panel.Controls.Add(new Label { Text = titulo + "\r\n" + subtitulo, Dock = DockStyle.Top, Height = 48, ForeColor = Componentes.Tinta, Font = Componentes.Fuente });
        return panel;
    }

    private static Control PanelConTitulo(string titulo, DataGridView tabla, Button? accion = null)
    {
        var panel = Componentes.Tarjeta();
        panel.Dock = DockStyle.Fill;
        panel.Controls.Add(tabla);
        if (accion is null)
        {
            panel.Controls.Add(new Label
            {
                Text = titulo,
                Dock = DockStyle.Top,
                Height = 35,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Componentes.Tinta
            });
        }
        else
        {
            var encabezado = new Panel { Dock = DockStyle.Top, Height = 48 };
            encabezado.Controls.Add(new Label
            {
                Text = titulo,
                Dock = DockStyle.Left,
                AutoSize = true,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Componentes.Tinta,
                TextAlign = ContentAlignment.MiddleLeft
            });
            accion.Dock = DockStyle.Right;
            accion.Height = 36;
            encabezado.Controls.Add(accion);
            panel.Controls.Add(encabezado);
        }
        return panel;
    }

    private Control Aviso(string texto, string botonTexto, Func<Task> accion)
    {
        var panel = Componentes.Tarjeta();
        panel.Dock = DockStyle.Top;
        panel.Height = 155;
        panel.Controls.Add(new Label { Text = texto, AutoSize = true, Font = new Font("Segoe UI", 12), Location = new Point(20, 20) });
        var boton = Componentes.Boton(botonTexto);
        boton.Location = new Point(20, 75);
        boton.Width = 210;
        var icono = texto switch { "Inicio" => "layout-dashboard", "Mesas" => "armchair", "Pedidos" => "clipboard-list", "Catálogos" => "utensils-crossed", "Reportes" => "chart-column", _ => "settings" };
        Iconos.Aplicar(boton, icono);
        boton.Click += async (_, _) => {
            await EjecutarSeguro(accion);
            foreach (var item in menu.Controls.OfType<Button>()) {
                item.BackColor = item == boton ? Componentes.Verde : Color.White;
                item.ForeColor = item == boton ? Color.White : Componentes.Tinta;
            }
        };
        panel.Controls.Add(boton);
        return panel;
    }

    private void MostrarError(Exception excepcion) =>
        _dialogos.Mostrar(this, excepcion.Message, "No se pudo completar la operación",
            MessageBoxButtons.OK, MessageBoxIcon.Error);
}
