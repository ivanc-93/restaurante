using Restaurante.Backend.Dominio;
using Restaurante.Backend.Servicios;

namespace Restaurante.Frontend.Formularios;

public sealed class FormularioPrincipal : Form
{
    private readonly IRepositorioRestaurante _repositorio;
    private readonly IConfiguracionConexion _configuracion;
    private readonly UsuarioAutenticado _usuario;
    private readonly Panel _contenido = new() { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(30) };
    private readonly Label _titulo = new() { AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Componentes.Tinta };

    public FormularioPrincipal(IRepositorioRestaurante repositorio, IConfiguracionConexion configuracion,
        UsuarioAutenticado usuario)
    {
        _repositorio = repositorio;
        _configuracion = configuracion;
        _usuario = usuario;
        Text = "Sabores | Gestión de restaurante";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1080, 720);
        Size = new Size(1280, 820);
        BackColor = Componentes.Fondo;
        CrearInterfaz();
        Shown += (_, _) => EjecutarSeguro(MostrarInicio);
    }

    private void CrearInterfaz()
    {
        var lateral = new Panel { Dock = DockStyle.Left, Width = 225, BackColor = Color.White, Padding = new Padding(16, 22, 16, 12) };
        Controls.Add(lateral);
        lateral.Controls.Add(new Label { Text = "S  sabores", AutoSize = true, Font = new Font("Georgia", 21, FontStyle.Bold), ForeColor = Componentes.Verde, Location = new Point(16, 20) });
        lateral.Controls.Add(new Label { Text = "GESTIÓN RESTAURANTE", AutoSize = true, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Componentes.Secundario, Location = new Point(50, 50) });
        var menu = new FlowLayoutPanel { Location = new Point(12, 100), Width = 198, Height = 480, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, BackColor = Color.White };
        lateral.Controls.Add(menu);
        AgregarNavegacion(menu, "RESUMEN", "◫   Resumen", MostrarInicio);
        AgregarNavegacion(menu, "SERVICIO", "▤   Pedidos y facturas", MostrarPedidos);
        AgregarNavegacion(menu, "SERVICIO", "♨   Menú y mesas", MostrarMenuMesas);
        AgregarNavegacion(menu, "ANÁLISIS", "▥   Reportes", MostrarReportes);
        if (_usuario.Rol == "Administrador")
            AgregarNavegacion(menu, "ADMINISTRACIÓN", "⚙   Conexión SQL", MostrarConexion);

        var pie = new Panel { Dock = DockStyle.Bottom, Height = 112, BackColor = Color.White };
        lateral.Controls.Add(pie);
        pie.Controls.Add(new Label { Text = _usuario.Nombre, AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(8, 14) });
        pie.Controls.Add(new Label { Text = _usuario.Rol, AutoSize = true, ForeColor = Componentes.Secundario, Location = new Point(8, 37) });
        var salir = Componentes.Boton("Cerrar sesión", false);
        salir.Location = new Point(5, 64);
        salir.Width = 182;
        salir.Click += (_, _) => Close();
        pie.Controls.Add(salir);

        var area = new Panel { Dock = DockStyle.Fill };
        Controls.Add(area);
        area.BringToFront();
        var barra = new Panel { Dock = DockStyle.Top, Height = 54, BackColor = Color.White, Padding = new Padding(28, 17, 28, 0) };
        barra.Controls.Add(_titulo);
        barra.Controls.Add(new Label
        {
            Text = DateTime.Today.ToString("dddd, dd MMMM yyyy", new System.Globalization.CultureInfo("es-MX")),
            AutoSize = true, ForeColor = Componentes.Secundario, Dock = DockStyle.Right
        });
        area.Controls.Add(_contenido);
        area.Controls.Add(barra);
    }

    private void AgregarNavegacion(FlowLayoutPanel menu, string grupo, string texto, Action accion)
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
        boton.Width = 190;
        boton.Height = 40;
        boton.TextAlign = ContentAlignment.MiddleLeft;
        boton.Margin = new Padding(0, 2, 0, 2);
        boton.Click += (_, _) => EjecutarSeguro(accion);
        menu.Controls.Add(boton);
    }

    private void EjecutarSeguro(Action accion)
    {
        try { accion(); }
        catch (Exception ex) { MostrarError(ex); }
    }

    private void MostrarInicio()
    {
        _titulo.Text = "Panel de control  /  Resumen";
        _contenido.Controls.Clear();
        _contenido.Controls.Add(Encabezado("Buenos días, " + _usuario.Nombre, "Esto es lo que sucede hoy en tu restaurante."));
        if (!_configuracion.EstaConfigurada)
        {
            _contenido.Controls.Add(Aviso("Configura una conexión a SQL Server para comenzar.",
                "Abrir conexión SQL", MostrarConexion));
            return;
        }
        var resumen = _repositorio.ObtenerResumen();
        var tarjetas = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 136, WrapContents = true };
        tarjetas.Controls.Add(TarjetaEstadistica("VENTAS DE HOY", resumen.VentasDelDia.ToString("C"), "Facturación del día", Color.FromArgb(216, 131, 80)));
        tarjetas.Controls.Add(TarjetaEstadistica("PEDIDOS ABIERTOS", resumen.PedidosAbiertos.ToString(), "En atención", Color.FromArgb(89, 129, 149)));
        tarjetas.Controls.Add(TarjetaEstadistica("MESAS DISPONIBLES", resumen.MesasDisponibles.ToString(), "Listas para clientes", Componentes.Verde));
        tarjetas.Controls.Add(TarjetaEstadistica("PRODUCTOS ACTIVOS", resumen.ProductosActivos.ToString(), "En el menú", Color.FromArgb(122, 111, 161)));
        _contenido.Controls.Add(tarjetas);
        var bienvenida = new Panel { Dock = DockStyle.Top, Height = 180, BackColor = Color.FromArgb(238, 241, 232), Padding = new Padding(24), Margin = new Padding(0, 12, 0, 0) };
        bienvenida.Controls.Add(new Label { Text = "OPERACIÓN DIARIA\r\n\r\nUna gran experiencia empieza aquí.\r\n\r\nRegistra pedidos, consulta tus ventas y mantén las mesas en movimiento.", AutoSize = true, Font = new Font("Segoe UI", 12), ForeColor = Componentes.Tinta });
        var nuevo = Componentes.Boton("＋  Nuevo pedido");
        nuevo.Location = new Point(24, 127);
        nuevo.Width = 175;
        nuevo.Click += (_, _) => EjecutarSeguro(MostrarPedidos);
        bienvenida.Controls.Add(nuevo);
        _contenido.Controls.Add(bienvenida);
    }

    private static Control Encabezado(string titulo, string descripcion)
    {
        var panel = new Panel { Dock = DockStyle.Top, Height = 94, Margin = new Padding(0, 0, 0, 10) };
        panel.Controls.Add(new Label { Text = "TU RESTAURANTE, EN CONTROL", AutoSize = true, ForeColor = Componentes.Secundario, Font = new Font("Segoe UI", 8, FontStyle.Bold), Location = new Point(0, 0) });
        panel.Controls.Add(new Label { Text = titulo, AutoSize = true, Font = new Font("Georgia", 23, FontStyle.Bold), ForeColor = Componentes.Tinta, Location = new Point(0, 20) });
        panel.Controls.Add(new Label { Text = descripcion, AutoSize = true, ForeColor = Componentes.Secundario, Location = new Point(0, 60) });
        return panel;
    }

    private static Control TarjetaEstadistica(string titulo, string valor, string detalle, Color acento)
    {
        var panel = Componentes.Tarjeta();
        panel.Size = new Size(220, 116);
        panel.Controls.Add(new Label { Text = titulo, AutoSize = true, ForeColor = Componentes.Secundario, Font = new Font("Segoe UI", 8, FontStyle.Bold), Location = new Point(15, 12) });
        panel.Controls.Add(new Label { Text = valor, AutoSize = true, ForeColor = acento, Font = new Font("Georgia", 23, FontStyle.Bold), Location = new Point(15, 36) });
        panel.Controls.Add(new Label { Text = detalle, AutoSize = true, ForeColor = Componentes.Secundario, Font = new Font("Segoe UI", 8), Location = new Point(15, 78) });
        return panel;
    }

    private void MostrarPedidos()
    {
        _titulo.Text = "Servicio  /  Pedidos y facturas";
        _contenido.Controls.Clear();
        _contenido.Controls.Add(Encabezado("Pedidos y facturas", "Registra pedidos, emite facturas y consulta los últimos movimientos."));
        var columnas = new TableLayoutPanel { Dock = DockStyle.Top, Height = 360, ColumnCount = 2, RowCount = 1 };
        columnas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 43));
        columnas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 57));
        var formulario = ComponentePedido(out var mesa, out var cliente, out var producto, out var cantidad, out var registrar);
        columnas.Controls.Add(formulario, 0, 0);
        columnas.Controls.Add(ComponenteMenu(out var menuTabla), 1, 0);
        _contenido.Controls.Add(columnas);
        var tabla = NuevaTabla();
        var facturarSeleccionado = BotonMesa("Facturar pedido seleccionado", 220);
        _contenido.Controls.Add(PanelConTitulo("Pedidos recientes", tabla, facturarSeleccionado));

        void Recargar()
        {
            mesa.DisplayMember = "";
            mesa.DataSource = _repositorio.ListarMesas().Where(x => x.Estado == "Disponible").ToList();
            cliente.DataSource = _repositorio.ListarClientes().ToList();
            var productos = _repositorio.ListarMenu().ToList();
            producto.DataSource = productos;
            menuTabla.DataSource = productos;
            tabla.DataSource = _repositorio.ListarPedidos().ToList();
        }

        registrar.Click += (_, _) =>
        {
            try
            {
                if (mesa.SelectedItem is not Mesa mesaElegida || producto.SelectedItem is not ElementoMenu productoElegido)
                    throw new InvalidOperationException("Selecciona una mesa disponible y un producto.");
                var clienteElegido = cliente.SelectedItem as Cliente;
                var pedido = _repositorio.CrearPedido(new SolicitudPedido
                {
                    MesaId = mesaElegida.Id,
                    ClienteId = clienteElegido?.Id,
                    Detalles = [new DetallePedidoSolicitud(productoElegido.Id, (int)cantidad.Value)]
                });
                MessageBox.Show(this, $"Pedido #{pedido.Id} abierto.\r\nTotal actual: {pedido.Total:C}\r\nLa factura se emitirá cuando los comensales decidan retirarse.",
                    "Pedido registrado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Recargar();
            }
            catch (Exception ex) { MostrarError(ex); }
        };

        facturarSeleccionado.Click += (_, _) => EjecutarSeguro(() =>
        {
            if (tabla.CurrentRow?.DataBoundItem is not PedidoResumen pedido)
                throw new InvalidOperationException("Selecciona un pedido para facturar.");
            if (pedido.Estado is not ("Abierto" or "En preparación"))
                throw new InvalidOperationException("El pedido seleccionado ya está facturado o cerrado.");
            if (MessageBox.Show(this,
                $"¿Los comensales se retiran y deseas facturar el pedido #{pedido.Id} por {pedido.Total:C}?",
                "Confirmar factura", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            var numeroFactura = _repositorio.FacturarPedido(pedido.Id);
            MessageBox.Show(this,
                $"Factura {numeroFactura} emitida. La mesa {pedido.NumeroMesa} quedó disponible.",
                "Pedido facturado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Recargar();
        });
        Recargar();
    }

    private static Control ComponentePedido(out ComboBox mesa, out ComboBox cliente,
        out ComboBox producto, out NumericUpDown cantidad, out Button registrar)
    {
        var panel = ComponenteSeccion("Nuevo pedido", "Selecciona la mesa y los productos.");
        mesa = Combo();
        cliente = Combo();
        producto = Combo();
        cantidad = new NumericUpDown { Minimum = 1, Maximum = 50, Value = 1, Dock = DockStyle.Top, Height = 34 };
        registrar = Componentes.Boton("Abrir pedido (sin facturar)");
        var layout = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 1, AutoSize = true };
        layout.Controls.Add(Componentes.Etiqueta("Mesa disponible"));
        layout.Controls.Add(mesa);
        layout.Controls.Add(Componentes.Etiqueta("Cliente (opcional)"));
        layout.Controls.Add(cliente);
        layout.Controls.Add(Componentes.Etiqueta("Producto"));
        layout.Controls.Add(producto);
        layout.Controls.Add(Componentes.Etiqueta("Cantidad"));
        layout.Controls.Add(cantidad);
        layout.Controls.Add(registrar);
        panel.Controls.Add(layout);
        return panel;
    }

    private static Control ComponenteMenu(out DataGridView tabla)
    {
        tabla = NuevaTabla();
        return PanelConTitulo("Menú disponible", tabla);
    }

    private void MostrarMenuMesas()
    {
        _titulo.Text = "Servicio  /  Menú y mesas";
        _contenido.Controls.Clear();
        _contenido.Controls.Add(Encabezado("Menú y mesas", "Consulta los productos activos y el estado de las mesas."));
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
        _contenido.Controls.Add(pestanas);
        menu.DataSource = _repositorio.ListarMenu().ToList();

        List<Mesa> CargarMesas()
        {
            var lista = _repositorio.ListarMesas().ToList();
            mesas.DataSource = null;
            mesas.DataSource = lista;
            return lista;
        }

        Mesa? MesaSeleccionada() => mesas.CurrentRow?.DataBoundItem as Mesa;

        crear.Click += (_, _) => EjecutarSeguro(() =>
        {
            var siguiente = CargarMesas().Select(m => m.Numero).DefaultIfEmpty(0).Max() + 1;
            using var formulario = new FormularioMesa(siguienteNumero: siguiente);
            if (formulario.ShowDialog(this) != DialogResult.OK) return;
            _repositorio.CrearMesa(formulario.NumeroMesa, formulario.Capacidad);
            CargarMesas();
        });

        editar.Click += (_, _) => EjecutarSeguro(() =>
        {
            var seleccionada = MesaSeleccionada();
            if (seleccionada is null)
            {
                MessageBox.Show(this, "Selecciona una mesa para editar.", "Mesas",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using var formulario = new FormularioMesa(seleccionada);
            if (formulario.ShowDialog(this) != DialogResult.OK) return;
            _repositorio.ActualizarMesa(seleccionada.Id, formulario.NumeroMesa,
                formulario.Capacidad, formulario.EstadoMesa);
            CargarMesas();
        });

        visualizar.Click += (_, _) =>
        {
            var seleccionada = MesaSeleccionada();
            if (seleccionada is null)
            {
                MessageBox.Show(this, "Selecciona una mesa para visualizar.", "Mesas",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using var formulario = new FormularioDetalleMesa(_repositorio, seleccionada);
            formulario.ShowDialog(this);
            CargarMesas();
        };

        eliminar.Click += (_, _) => EjecutarSeguro(() =>
        {
            var seleccionada = MesaSeleccionada();
            if (seleccionada is null)
            {
                MessageBox.Show(this, "Selecciona una mesa para eliminar.", "Mesas",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var confirmacion = MessageBox.Show(this,
                $"¿Deseas eliminar la mesa {seleccionada.Numero}?",
                "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirmacion != DialogResult.Yes) return;
            _repositorio.EliminarMesa(seleccionada.Id);
            CargarMesas();
        });

        CargarMesas();
    }

    private static Button BotonMesa(string texto, int ancho, bool primario = true)
    {
        var boton = Componentes.Boton(texto, primario);
        boton.Width = ancho;
        boton.Height = 36;
        boton.Margin = new Padding(0, 2, 8, 2);
        return boton;
    }

    private void MostrarReportes()
    {
        _titulo.Text = "Análisis  /  Reportes";
        _contenido.Controls.Clear();
        _contenido.Controls.Add(Encabezado("Reportes", "Ventas diarias y productos más vendidos."));
        var filtros = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 54, FlowDirection = FlowDirection.LeftToRight };
        var inicio = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(-30), Width = 135 };
        var fin = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 135 };
        var actualizar = Componentes.Boton("Consultar ventas");
        actualizar.Width = 150;
        filtros.Controls.Add(Componentes.Etiqueta("Desde"));
        filtros.Controls.Add(inicio);
        filtros.Controls.Add(Componentes.Etiqueta("Hasta"));
        filtros.Controls.Add(fin);
        filtros.Controls.Add(actualizar);
        _contenido.Controls.Add(filtros);
        var tablas = new TableLayoutPanel { Dock = DockStyle.Top, Height = 480, ColumnCount = 2 };
        tablas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        tablas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        var ventas = NuevaTabla();
        var productos = NuevaTabla();
        tablas.Controls.Add(PanelConTitulo("Ventas por día", ventas), 0, 0);
        tablas.Controls.Add(PanelConTitulo("Platos más vendidos", productos), 1, 0);
        _contenido.Controls.Add(tablas);
        void CargarVentas() => ventas.DataSource = _repositorio.ReportarVentas(inicio.Value, fin.Value).ToList();
        actualizar.Click += (_, _) => EjecutarSeguro(CargarVentas);
        productos.DataSource = _repositorio.ReportarPlatosMasVendidos().ToList();
        CargarVentas();
    }

    private void MostrarConexion()
    {
        if (_usuario.Rol != "Administrador")
        {
            MessageBox.Show(this, "Solo el administrador puede cambiar la conexión.", "Acceso denegado",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        _titulo.Text = "Administración  /  Conexión SQL";
        _contenido.Controls.Clear();
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
        guardar.Click += (_, _) =>
        {
            guardar.Enabled = false;
            estado.Text = "Conectando y verificando credenciales…";
            try
            {
                _configuracion.ProbarYGuardar(servidor.Text, (int)puerto.Value, baseDatos.Text,
                    usuario.Text, contrasena.Text, certificado.Checked);
                estado.Text = "●  Conexión probada y guardada de forma protegida.";
                estado.ForeColor = Componentes.Verde;
                MessageBox.Show(this, "La conexión SQL Server es válida y se guardó protegida.",
                    "Conexión correcta", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                Font = new Font("Georgia", 13, FontStyle.Bold),
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
                Font = new Font("Georgia", 13, FontStyle.Bold),
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

    private static Control Aviso(string texto, string botonTexto, Action accion)
    {
        var panel = Componentes.Tarjeta();
        panel.Dock = DockStyle.Top;
        panel.Height = 155;
        panel.Controls.Add(new Label { Text = texto, AutoSize = true, Font = new Font("Segoe UI", 12), Location = new Point(20, 20) });
        var boton = Componentes.Boton(botonTexto);
        boton.Location = new Point(20, 75);
        boton.Width = 210;
        boton.Click += (_, _) => accion();
        panel.Controls.Add(boton);
        return panel;
    }

    private void MostrarError(Exception excepcion) =>
        MessageBox.Show(this, excepcion.Message, "No se pudo completar la operación",
            MessageBoxButtons.OK, MessageBoxIcon.Error);
}
