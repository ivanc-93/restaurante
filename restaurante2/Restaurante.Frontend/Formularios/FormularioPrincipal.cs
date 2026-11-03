using Restaurante.Frontend.Servicios;
using Restaurante.Backend.Dominio;

namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioPrincipal : Form
{
    private readonly IDialogosUsuario _dialogos = new DialogosUsuario();
    private readonly IRepositorioRestaurante _repositorio = null!;
    private readonly IConfiguracionConexion _configuracion = null!;
    private readonly UsuarioAutenticado _usuario = null!;
    public bool SesionCerrada { get; private set; }

    public FormularioPrincipal() => InitializeComponent();
    public FormularioPrincipal(IRepositorioRestaurante repositorio, IConfiguracionConexion configuracion, UsuarioAutenticado usuario, IDialogosUsuario? dialogos = null) : this()
    {
        _repositorio = repositorio;
        _configuracion = configuracion;
        _usuario = usuario;
        _dialogos = dialogos ?? new DialogosUsuario();
        lblUsuarioLabel.Text = $"{usuario.Nombre} ({usuario.Rol})";
        AgregarNavegacion(flpNavegacion, "RESUMEN", "Inicio", MostrarInicio);
        AgregarNavegacion(flpNavegacion, "SERVICIO", "Mesas", MostrarMesasPos);
        AgregarNavegacion(flpNavegacion, "SERVICIO", "Pedidos", MostrarPedidosPos);
        AgregarNavegacion(flpNavegacion, "DATOS", "Catálogos", MostrarCatalogos);
        if (usuario.Rol is "Administrador" or "Gerente")
            AgregarNavegacion(flpNavegacion, "ANÁLISIS", "Reportes", MostrarReportes);
        if (usuario.Rol == "Administrador")
            AgregarNavegacion(flpNavegacion, "ADMINISTRACIÓN", "Usuarios", () =>
            {
                using var f = new FormularioUsuarios(_repositorio);
                f.ShowDialog(this);
                return Task.CompletedTask;
            });
        AgregarNavegacion(flpNavegacion, "CUENTA", "Cambiar contraseña", () =>
        {
            using var f = new FormularioCambiarClave(_repositorio);
            if (f.ShowDialog(this) == DialogResult.OK)
            {
                SesionCerrada = true;
                Close();
            }

            return Task.CompletedTask;
        });
        if (usuario.Rol is "Administrador" or "Gerente")
            AgregarNavegacion(flpNavegacion, "ADMINISTRACIÓN", "Configuración", MostrarConfiguracion);
        flpNavegacion.Enabled = false;
        pnlContenido.Enabled = false;
        btnCerrarSesion.Enabled = false;
        Shown += async (_, _) =>
        {
            await EjecutarSeguro(MostrarInicio);
            if (IsDisposed)
                return;
            SeleccionarNavegacion("Inicio");
        };
        FormClosing += (_, e) =>
        {
            if (UseWaitCursor)
                e.Cancel = true;
        };
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
        var btnBoton = Componentes.Boton(texto, false);
        btnBoton.Name = "btnBoton";
        btnBoton.Name = "nav_" + texto;
        btnBoton.AutoSize = false;
        btnBoton.BackColor = Componentes.Verde;
        btnBoton.ForeColor = Color.White;
        btnBoton.FlatAppearance.BorderSize = 0;
        btnBoton.FlatAppearance.MouseOverBackColor = Color.FromArgb(68, 103, 76);
        btnBoton.FlatAppearance.MouseDownBackColor = Componentes.VerdeOscuro;
        btnBoton.Width = Math.Max(190, menu.ClientSize.Width - 24);
        btnBoton.Tag = grupo;
        btnBoton.Height = 48;
        btnBoton.TextAlign = ContentAlignment.MiddleLeft;
        btnBoton.Margin = new Padding(0, 2, 0, 2);
        var icono = texto switch
        {
            "Inicio" => "layout-dashboard",
            "Mesas" => "armchair",
            "Pedidos" => "clipboard-list",
            "Catálogos" => "utensils-crossed",
            "Reportes" => "chart-column",
            _ => "settings"
        };
        Iconos.Aplicar(btnBoton, icono);
        btnBoton.Click += async (_, _) =>
        {
            await EjecutarSeguro(accion);
            SeleccionarNavegacion(texto);
        };
        btnBoton.Enabled = _configuracion.EstaConfigurada || accion == MostrarConfiguracion || accion == MostrarInicio;
        menu.Controls.Add(btnBoton);
    }

    private void SeleccionarNavegacion(string texto)
    {
        foreach (var item in flpNavegacion.Controls.OfType<Button>())
        {
            item.BackColor = item.Text == texto ? Color.FromArgb(75, 105, 79) : Componentes.Verde;
            item.ForeColor = Color.White;
        }
    }

    private async Task EjecutarSeguro(Func<Task> accion)
    {
        flpNavegacion.Enabled = false;
        pnlContenido.Enabled = false;
        btnCerrarSesion.Enabled = false;
        UseWaitCursor = true;
        try
        {
            await accion();
        }
        catch (Exception ex)
        {
            MostrarError(ex);
        }
        finally
        {
            if (!IsDisposed)
            {
                FormatoPresentacion.Aplicar(this);
                flpNavegacion.Enabled = true;
                pnlContenido.Enabled = true;
                btnCerrarSesion.Enabled = true;
                UseWaitCursor = false;
            }
        }
    }

    private Task EnInterfazAsync(Func<Task> accion)
    {
        if (!InvokeRequired)
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
            return accion();
        }

        var fin = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        BeginInvoke((Action)(async () =>
        {
            try
            {
                await accion();
                fin.SetResult();
            }
            catch (Exception ex)
            {
                fin.SetException(ex);
            }
        }));
        return fin.Task;
    }

    private async Task MostrarInicio()
    {
        if (InvokeRequired)
        {
            await EnInterfazAsync(MostrarInicio);
            return;
        }

        lblTitulo.Text = "Panel de control / Inicio";
        SeleccionarNavegacion("Inicio");
        LimpiarContenido();
        var pagina = CrearPagina();
        pagina.Controls.Add(Encabezado("Listos para atender", $"Hola, {_usuario.Nombre}. Consulta reservas, mesas y pedidos pendientes."));
        if (!_configuracion.EstaConfigurada)
        {
            pagina.Controls.Add(Aviso("Configura la conexión para comenzar.", "Abrir conexión SQL", MostrarConexion));
            return;
        }

        var mesas = await Task.Run(_repositorio.ListarMesas);
        var restaurante = await Task.Run(_repositorio.ObtenerRestaurante);
        Text = "Sabores | " + restaurante.Nombre;
        pagina.Controls.Add(Componentes.Etiqueta($"MESAS   {mesas.Count(m => m.Estado == "Disponible")} disponibles   ·   {mesas.Count(m => m.Estado == "Ocupada")} ocupadas   ·   {mesas.Count(m => m.Estado == "Reservada")} reservadas   ·   {mesas.Count(m => m.Estado == "En limpieza")} en limpieza", 12, true));
        var acciones = new FlowLayoutPanel
        {
            Name = "flpAccionesInicio",
            Dock = DockStyle.Top,
            AutoSize = true,
            WrapContents = true
        };
        void Accion(string nombre, string texto, Func<Task> ejecutar)
        {
            var b = Componentes.Boton(texto, false);
            b.Name = nombre;
            b.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            b.Margin = new Padding(0, 6, 8, 6);
            b.Click += async (_, _) => await EjecutarSeguro(ejecutar);
            acciones.Controls.Add(b);
        }

        Accion("btnVer", "Ver mesas", MostrarMesasPos);
        var pendientes = DiarioOperaciones.Listar(_repositorio);
        Accion("btnRecuperar", $"Operaciones pendientes ({pendientes.Count})", async () =>
        {
            using var f = new FormularioRecuperacion(_repositorio);
            f.ShowDialog(this);
            await MostrarInicio();
        });
        Accion("btnIncidencias", "Incidencias de consumo", async () =>
        {
            using var f = new FormularioIncidencias(_repositorio);
            f.ShowDialog(this);
            await MostrarInicio();
        });
        pagina.Controls.Add(acciones);
        if (pendientes.Count > 0)
            pagina.Controls.Add(Componentes.Etiqueta("Recupera las operaciones pendientes antes de repetir una acción.", 12, true));
        pagina.Controls.Add(Componentes.Etiqueta("Reservas activas", 16, true));
        if (await Task.Run(_repositorio.ReservasDisponibles))
        {
            var reservas = await Task.Run(_repositorio.ListarReservasActivas);
            if (reservas.Count == 0)
                pagina.Controls.Add(Componentes.Etiqueta("Sin reservas activas por el momento."));
            foreach (var reserva in reservas.Take(5))
            {
                var b = Componentes.Boton($"Mesa {reserva.NumeroMesa} · {reserva.Nombre} · {reserva.Fecha:dd/MM HH:mm}", false);
                b.Name = "btnVerReserva_" + reserva.Id;
                b.Dock = DockStyle.Top;
                b.AutoSize = false;
                b.AutoEllipsis = true;
                b.Height = (int)(50 * DeviceDpi / 96f);
                b.Font = new Font("Segoe UI", 11);
                b.TextAlign = ContentAlignment.MiddleLeft;
                b.Click += async (_, _) => await EjecutarSeguro(async () =>
                {
                    var mesa = (await Task.Run(_repositorio.ListarMesas)).Single(m => m.Id == reserva.MesaId);
                    var actual = await Task.Run(() => _repositorio.ConsultarReservaMesa(mesa.Id));
                    using var f = new FormularioReservaDetalle(_repositorio, mesa, actual);
                    f.ShowDialog(this);
                    await MostrarInicio();
                });
                pagina.Controls.Add(b);
            }

            if (reservas.Count > 5)
            {
                var todas = Componentes.Boton($"Ver todas las reservas ({reservas.Count})", false);
                todas.Name = "btnTodasReservas";
                todas.Click += async (_, _) => await EjecutarSeguro(async () =>
                {
                    await MostrarMesasPos();
                    var filtro = Controls.Find("cbFiltroEstadoMesa", true).OfType<ComboBox>().FirstOrDefault();
                    if (filtro is not null)
                        filtro.SelectedItem = "Reservada";
                });
                pagina.Controls.Add(todas);
            }
        }

        pagina.Controls.Add(Componentes.Etiqueta("Pedidos abiertos recientes", 16, true));
        var pedidos = (await Task.Run(_repositorio.ListarPedidos)).Where(p => p.Estado is "Abierto" or "En preparación").ToList();
        if (pedidos.Count == 0)
            pagina.Controls.Add(Componentes.Etiqueta("No hay pedidos abiertos. Abre una mesa para comenzar."));
        foreach (var p in pedidos.Take(5))
        {
            var b = Componentes.Boton($"Mesa {p.NumeroMesa} · Pedido #{p.Id} · {p.Estado}", false);
            b.Name = "btnContinuar";
            b.Font = new Font("Segoe UI", 11);
            b.Dock = DockStyle.Top;
            b.Click += async (_, _) => await EjecutarSeguro(async () =>
            {
                var actual = (await Task.Run(_repositorio.ListarMesas)).Single(m => m.Numero == p.NumeroMesa);
                await AbrirMesa(actual);
                await MostrarInicio();
            });
            pagina.Controls.Add(b);
        }

        if (_usuario.Rol is "Gerente" or "Administrador")
        {
            var resumen = await Task.Run(_repositorio.ObtenerResumen);
            pagina.Controls.Add(Componentes.Etiqueta($"Comprobantes de hoy: {FormatoPresentacion.Moneda(resumen.VentasDelDia)} (importe emitido; no equivale a caja)", 11));
            if (await Task.Run(_repositorio.CobranzaParcialDisponible))
            {
                var b = Componentes.Boton("Configurar promoción de clientes frecuentes", false);
                b.Name = "btnPromociones";
                b.Click += async (_, _) => await EjecutarSeguro(async () =>
                {
                    using var f = new FormularioPromocion(_repositorio);
                    f.ShowDialog(this);
                    await MostrarInicio();
                });
                pagina.Controls.Add(b);
            }
        }

        FormatoPresentacion.Aplicar(this);
    }

    private TableLayoutPanel CrearPagina()
    {
        var tlpPagina = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            GrowStyle = TableLayoutPanelGrowStyle.AddRows,
            Name = "tlpPagina"
        };
        tlpPagina.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        pnlContenido.Controls.Add(tlpPagina);
        return tlpPagina;
    }

    private static Control Encabezado(string titulo, string descripcion)
    {
        var tlpPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1,
            Padding = new Padding(0, 0, 0, 15),
            Name = "tlpPanel"
        };
        tlpPanel.Controls.Add(new Label { Text = titulo, AutoSize = true, Font = new Font("Segoe UI", 23, FontStyle.Bold), ForeColor = Componentes.Tinta });
        tlpPanel.Controls.Add(new Label { Text = descripcion, AutoSize = true, ForeColor = Componentes.Secundario, Margin = new Padding(0, 10, 0, 5) });
        tlpPanel.SizeChanged += (_, _) =>
        {
            var ancho = Math.Max(150, tlpPanel.ClientSize.Width - 20);
            foreach (var l in tlpPanel.Controls.OfType<Label>())
                l.MaximumSize = new Size(ancho, 0);
        };
        return tlpPanel;
    }

    private static Control TarjetaEstadistica(string titulo, string valor, string detalle, Color acento)
    {
        var tlpPanel = new TableLayoutPanel
        {
            BackColor = Color.White,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(16),
            Margin = new Padding(0, 0, 12, 12),
            MinimumSize = new Size(205, 128),
            Name = "tlpPanel"
        };
        tlpPanel.Controls.Add(new Label { Text = titulo, AutoSize = true, ForeColor = Componentes.Secundario, Font = new Font("Segoe UI", 11, FontStyle.Bold) });
        tlpPanel.Controls.Add(new Label { Text = valor, AutoSize = true, ForeColor = acento, Font = new Font("Segoe UI", 23, FontStyle.Bold), Margin = new Padding(0, 6, 0, 4) });
        tlpPanel.Controls.Add(new Label { Text = detalle, AutoSize = true, ForeColor = Componentes.Secundario });
        return tlpPanel;
    }

    private async Task MostrarMenuMesas()
    {
        lblTitulo.Text = "Servicio  /  Menú y mesas";
        LimpiarContenido();
        var tlpPagina = CrearPagina();
        tlpPagina.Controls.Add(Encabezado("Menú y mesas", "Consulta los productos activos y el estado de las mesas."));
        var tabPestanas = new TabControl
        {
            Dock = DockStyle.Top,
            Height = 510,
            Font = Componentes.Fuente,
            Name = "tabPestanas"
        };
        var tpTabMenu = new TabPage("  Menú  ")
        {
            BackColor = Color.White,
            Padding = new Padding(12),
            Name = "tpTabMenu"
        };
        var tpTabMesas = new TabPage("  Mesas  ")
        {
            BackColor = Color.White,
            Padding = new Padding(12),
            Name = "tpTabMesas"
        };
        var dgvMenu = NuevaTabla();
        var dgvMesas = NuevaTabla();
        var flpHerramientasMesas = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 52,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 2, 0, 4),
            Name = "flpHerramientasMesas"
        };
        var btnCrear = BotonMesa("＋ Nueva mesa", 125);
        var btnEditar = BotonMesa("Editar", 90);
        var btnVisualizar = BotonMesa("Visualizar", 100);
        var btnEliminar = BotonMesa("Eliminar", 90, false);
        btnCrear.Enabled = btnEditar.Enabled = btnEliminar.Enabled = _repositorio.UsuarioActual?.Rol is "Administrador" or "Gerente";
        flpHerramientasMesas.Controls.AddRange([btnCrear, btnEditar, btnVisualizar, btnEliminar]);
        tpTabMesas.Controls.Add(dgvMesas);
        tpTabMesas.Controls.Add(flpHerramientasMesas);
        tpTabMenu.Controls.Add(dgvMenu);
        tabPestanas.TabPages.Add(tpTabMenu);
        tabPestanas.TabPages.Add(tpTabMesas);
        tlpPagina.Controls.Add(tabPestanas);
        dgvMenu.DataSource = (await Task.Run(_repositorio.ListarMenu)).ToList();
        async Task<List<Mesa>> CargarMesas()
        {
            var lista = (await Task.Run(_repositorio.ListarMesas)).ToList();
            dgvMesas.DataSource = null;
            dgvMesas.DataSource = lista;
            return lista;
        }

        Mesa? MesaSeleccionada() => dgvMesas.CurrentRow?.DataBoundItem as Mesa;
        btnCrear.Click += async (_, _) => await EjecutarSeguro(async () =>
        {
            var siguiente = (await CargarMesas()).Select(m => m.Numero).DefaultIfEmpty(0).Max() + 1;
            using var formulario = new FormularioMesa(siguienteNumero: siguiente);
            if (formulario.ShowDialog(this) != DialogResult.OK)
                return;
            var numero = formulario.NumeroMesa;
            var capacidad = formulario.Capacidad;
            await Task.Run(() => _repositorio.CrearMesa(numero, capacidad));
            await CargarMesas();
        });
        btnEditar.Click += async (_, _) => await EjecutarSeguro(async () =>
        {
            var seleccionada = MesaSeleccionada();
            if (seleccionada is null)
            {
                _dialogos.Mostrar(this, "Selecciona una mesa para editar.", "Mesas", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var formulario = new FormularioMesa(seleccionada);
            if (formulario.ShowDialog(this) != DialogResult.OK)
                return;
            var numero = formulario.NumeroMesa;
            var capacidad = formulario.Capacidad;
            var estado = formulario.EstadoMesa;
            await Task.Run(() => _repositorio.ActualizarMesa(seleccionada.Id, numero, capacidad, estado));
            await CargarMesas();
        });
        btnVisualizar.Click += async (_, _) => await EjecutarSeguro(async () =>
        {
            var seleccionada = MesaSeleccionada();
            if (seleccionada is null)
            {
                _dialogos.Mostrar(this, "Selecciona una mesa para visualizar.", "Mesas", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var formulario = new FormularioDetalleMesa(_repositorio, seleccionada, _dialogos);
            formulario.ShowDialog(this);
            await CargarMesas();
        });
        btnEliminar.Click += async (_, _) => await EjecutarSeguro(async () =>
        {
            var seleccionada = MesaSeleccionada();
            if (seleccionada is null)
            {
                _dialogos.Mostrar(this, "Selecciona una mesa para eliminar.", "Mesas", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirmacion = _dialogos.Mostrar(this, $"¿Deseas eliminar la mesa {seleccionada.Numero}?", "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirmacion != DialogResult.Yes)
                return;
            await Task.Run(() => _repositorio.EliminarMesa(seleccionada.Id));
            await CargarMesas();
        });
        await CargarMesas();
    }

    private static Button BotonMesa(string texto, int ancho, bool primario = true)
    {
        var btnBoton = Componentes.Boton(texto, primario);
        btnBoton.Name = "btnBoton";
        btnBoton.Width = ancho;
        btnBoton.AutoSize = true;
        btnBoton.Height = 36;
        btnBoton.Margin = new Padding(0, 2, 8, 2);
        return btnBoton;
    }

    private Task MostrarReportes()
    {
        using var formulario = new FormularioReportes(_repositorio);
        formulario.ShowDialog(this);
        return Task.CompletedTask;
    }

    private void LimpiarContenido()
    {
        while (pnlContenido.Controls.Count > 0)
            pnlContenido.Controls[0].Dispose();
    }

    private Task MostrarConexion()
    {
        if (_usuario.Rol != "Administrador")
            throw new UnauthorizedAccessException("Solo administrador.");
        using var f = new FormularioConexion(_configuracion);
        if (f.ShowDialog(this) == DialogResult.OK)
        {
            _repositorio.CerrarSesion();
            SesionCerrada = true;
            Close();
        }

        return Task.CompletedTask;
    }

    private static Control Campo(string etiqueta, Control campo)
    {
        var tlpGrupo = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = new Padding(0, 0, 12, 0),
            Name = "tlpGrupo"
        };
        tlpGrupo.RowStyles.Add(new RowStyle(SizeType.Absolute, 27));
        tlpGrupo.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var lblTitulo = Componentes.Etiqueta(etiqueta, 9, true);
        lblTitulo.Name = "lblTitulo";
        lblTitulo.Margin = new Padding(0, 2, 0, 0);
        campo.Dock = DockStyle.Fill;
        tlpGrupo.Controls.Add(lblTitulo, 0, 0);
        tlpGrupo.Controls.Add(campo, 0, 1);
        return tlpGrupo;
    }

    private static ComboBox Combo() => new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        DisplayMember = "Nombre",
        Font = Componentes.Fuente,
        Dock = DockStyle.Top,
        Height = 34,
        Margin = new Padding(0, 3, 0, 9)
    };
    private static DataGridView NuevaTabla()
    {
        var dgvRegistros = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = true,
            Name = "dgvRegistros"
        };
        Componentes.EstilizarTabla(dgvRegistros);
        return dgvRegistros;
    }

    private static Control ComponenteSeccion(string titulo, string subtitulo)
    {
        var pnlPanel = Componentes.Tarjeta();
        pnlPanel.Name = "pnlPanel";
        pnlPanel.Dock = DockStyle.Fill;
        pnlPanel.Controls.Add(new Label { Text = titulo + "\r\n" + subtitulo, Dock = DockStyle.Top, Height = 48, ForeColor = Componentes.Tinta, Font = Componentes.Fuente });
        return pnlPanel;
    }

    private static Control PanelConTitulo(string titulo, DataGridView tabla, Button? accion = null)
    {
        var pnlPanel = Componentes.Tarjeta();
        pnlPanel.Name = "pnlPanel";
        pnlPanel.Dock = DockStyle.Fill;
        pnlPanel.Controls.Add(tabla);
        if (accion is null)
        {
            pnlPanel.Controls.Add(new Label { Text = titulo, Dock = DockStyle.Top, Height = 35, Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Componentes.Tinta });
        }
        else
        {
            var pnlEncabezado = new Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                Name = "pnlEncabezado"
            };
            pnlEncabezado.Controls.Add(new Label { Text = titulo, Dock = DockStyle.Left, AutoSize = true, Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Componentes.Tinta, TextAlign = ContentAlignment.MiddleLeft });
            accion.Dock = DockStyle.Right;
            accion.Height = 36;
            pnlEncabezado.Controls.Add(accion);
            pnlPanel.Controls.Add(pnlEncabezado);
        }

        return pnlPanel;
    }

    private Control Aviso(string texto, string botonTexto, Func<Task> accion)
    {
        var pnlPanel = Componentes.Tarjeta();
        pnlPanel.Name = "pnlPanel";
        pnlPanel.Dock = DockStyle.Top;
        pnlPanel.Height = 155;
        pnlPanel.Controls.Add(new Label { Text = texto, AutoSize = true, Font = new Font("Segoe UI", 12), Location = new Point(20, 20) });
        var btnBoton = Componentes.Boton(botonTexto);
        btnBoton.Name = "btnBoton";
        btnBoton.Location = new Point(20, 75);
        btnBoton.Width = 210;
        var icono = texto switch
        {
            "Inicio" => "layout-dashboard",
            "Mesas" => "armchair",
            "Pedidos" => "clipboard-list",
            "Catálogos" => "utensils-crossed",
            "Reportes" => "chart-column",
            _ => "settings"
        };
        Iconos.Aplicar(btnBoton, icono);
        btnBoton.Click += async (_, _) =>
        {
            await EjecutarSeguro(accion);
            foreach (var item in flpNavegacion.Controls.OfType<Button>())
            {
                item.BackColor = item == btnBoton ? Componentes.Verde : Color.White;
                item.ForeColor = item == btnBoton ? Color.White : Componentes.Tinta;
            }
        };
        pnlPanel.Controls.Add(btnBoton);
        return pnlPanel;
    }

    private void MostrarError(Exception excepcion) => _dialogos.Mostrar(this, excepcion.Message, "No se pudo completar la operación", MessageBoxButtons.OK, MessageBoxIcon.Error);
}
