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
        var tlpPagina = CrearPagina();
        tlpPagina.Controls.Add(Encabezado("Tu restaurante, de un vistazo", "Hola, " + _usuario.Nombre + ". Selecciona una mesa o continúa una atención."));
        if (!_configuracion.EstaConfigurada)
        {
            tlpPagina.Controls.Add(Aviso("Configura la conexión del equipo para comenzar.", "Abrir conexión SQL", MostrarConexion));
            return;
        }

        var resumen = await Task.Run(_repositorio.ObtenerResumen);
        var restaurante = await Task.Run(_repositorio.ObtenerRestaurante);
        Text = "Sabores | " + restaurante.Nombre;
        var flpTarjetas = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            WrapContents = true,
            Margin = new Padding(0, 8, 0, 16),
            Name = "flpTarjetas"
        };
        if (_usuario.Rol is "Administrador" or "Gerente")
            flpTarjetas.Controls.Add(TarjetaEstadistica("COMPROBANTES HOY", FormatoPresentacion.Moneda(resumen.VentasDelDia), "Importe emitido; no caja", Componentes.Terracota));
        flpTarjetas.Controls.Add(TarjetaEstadistica("PEDIDOS ABIERTOS", resumen.PedidosAbiertos.ToString(), "Atenciones en curso", Componentes.Verde));
        flpTarjetas.Controls.Add(TarjetaEstadistica("MESAS DISPONIBLES", resumen.MesasDisponibles.ToString(), "Listas para atender", Componentes.Verde));
        flpTarjetas.Controls.Add(TarjetaEstadistica("PRODUCTOS ACTIVOS", resumen.ProductosActivos.ToString(), "En el menú actual", Componentes.Terracota));
        tlpPagina.Controls.Add(flpTarjetas);
        var flpAcciones = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            WrapContents = true,
            Margin = new Padding(0, 0, 0, 14),
            Name = "flpAcciones"
        };
        var btnVer = Componentes.Boton("Ver mesas");
        btnVer.Name = "btnVer";
        Iconos.Aplicar(btnVer, "armchair");
        btnVer.Click += async (_, _) => await EjecutarSeguro(MostrarMesasPos);
        var btnRecuperar = Componentes.Boton("Recuperar operaciones pendientes", false);
        btnRecuperar.Name = "btnRecuperar";
        Iconos.Aplicar(btnRecuperar, "refresh");
        btnRecuperar.Click += async (_, _) => await EjecutarSeguro(async () =>
        {
            using var formulario = new FormularioRecuperacion(_repositorio);
            formulario.ShowDialog(this);
            await MostrarInicio();
        });
        var btnIncidencias = Componentes.Boton("Incidencias de consumo", false);
        btnIncidencias.Name = "btnIncidencias";
        Iconos.Aplicar(btnIncidencias, "alert");
        btnIncidencias.Click += async (_, _) => await EjecutarSeguro(async () =>
        {
            using var f = new FormularioIncidencias(_repositorio);
            f.ShowDialog(this);
            await MostrarInicio();
        });
        foreach (var b in new[]
        {
            btnVer,
            btnRecuperar,
            btnIncidencias
        }

        )
            b.Margin = new Padding(0, 0, 12, 10);
        flpAcciones.Controls.AddRange([btnVer, btnRecuperar, btnIncidencias]);
        tlpPagina.Controls.Add(flpAcciones);
        var pendientes = DiarioOperaciones.Listar(_repositorio);
        if (pendientes.Count > 0)
            tlpPagina.Controls.Add(Componentes.Etiqueta($"Atención: {pendientes.Count} operación(es) pendiente(s). Recupera antes de repetir acciones.", 12, true));
        tlpPagina.Controls.Add(Componentes.Etiqueta("Pedidos abiertos recientes", 18, true));
        var abiertos = (await Task.Run(_repositorio.ListarPedidos)).Where(p => p.Estado is "Abierto" or "En preparación").Take(5).ToList();
        if (abiertos.Count == 0)
            tlpPagina.Controls.Add(Componentes.Etiqueta("No hay pedidos abiertos. Selecciona Ver mesas para comenzar."));
        foreach (var pedido in abiertos)
        {
            var btnContinuar = Componentes.Boton($"Mesa {pedido.NumeroMesa}    ·    Pedido #{pedido.Id}    ·    {pedido.Estado}    ·    {FormatoPresentacion.Moneda(pedido.Total)}", false);
            btnContinuar.Name = "btnContinuar";
            btnContinuar.Dock = DockStyle.Top;
            btnContinuar.Margin = new Padding(0, 0, 0, 8);
            Iconos.Aplicar(btnContinuar, "clipboard-list");
            btnContinuar.Click += async (_, _) => await EjecutarSeguro(async () =>
            {
                var mesas = await Task.Run(_repositorio.ListarMesas);
                await AbrirMesa(mesas.Single(m => m.Numero == pedido.NumeroMesa));
                await MostrarInicio();
            });
            tlpPagina.Controls.Add(btnContinuar);
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
