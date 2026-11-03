using Restaurante.Backend.Dominio;

namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioPrincipal
{
    private async Task MostrarMesasPos()
    {
        if (InvokeRequired)
        {
            await EnInterfazAsync(MostrarMesasPos);
            return;
        }

        lblTitulo.Text = "Servicio / Mesas";
        SeleccionarNavegacion("Mesas");
        LimpiarContenido();
        var tlpPagina = CrearPagina();
        tlpPagina.Controls.Add(Encabezado("Cada mesa, en su momento", "Elige una mesa. Su disponibilidad física no cancela los saldos pendientes."));
        var flpHerramientas = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            WrapContents = true,
            Margin = new Padding(0, 8, 0, 12),
            Name = "flpHerramientas"
        };
        var ctBuscar = new TextBox
        {
            Name = "ctBuscarMesa",
            Width = 230,
            PlaceholderText = "Buscar mesa",
            Margin = new Padding(0, 10, 12, 10),
            AccessibleName = "Buscar mesa por número"
        };
        var cbFiltro = new ComboBox
        {
            Name = "cbFiltroEstadoMesa",
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 190,
            Margin = new Padding(0, 10, 12, 10)
        };
        cbFiltro.Items.AddRange(["Todas", "Disponible", "Ocupada", "En limpieza", "Reservada"]);
        cbFiltro.SelectedIndex = 0;
        var btnActualizar = Componentes.Boton("Actualizar", false);
        btnActualizar.Name = "btnActualizar";
        Iconos.Aplicar(btnActualizar, "refresh");
        btnActualizar.Margin = new Padding(0, 0, 12, 6);
        var btnAdministrar = Componentes.Boton("Administrar mesas", false);
        btnAdministrar.Name = "btnAdministrar";
        Iconos.Aplicar(btnAdministrar, "settings");
        btnAdministrar.Visible = _usuario.Rol is "Administrador" or "Gerente";
        btnAdministrar.Click += async (_, _) => await EjecutarSeguro(MostrarMenuMesas);
        flpHerramientas.Controls.AddRange([ctBuscar, cbFiltro, btnActualizar, btnAdministrar]);
        tlpPagina.Controls.Add(flpHerramientas);
        var flpTarjetas = new FlowLayoutPanel
        {
            Name = "flpTarjetasMesas",
            Dock = DockStyle.Top,
            AutoSize = true,
            WrapContents = true,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        tlpPagina.Controls.Add(flpTarjetas);
        var mesas = await Task.Run(_repositorio.ListarMesas);
        var pedidos = await Task.Run(_repositorio.ListarPedidos);
        var reservasHabilitadas = await Task.Run(_repositorio.ReservasDisponibles);
        var operacionesPendientes = DiarioOperaciones.Listar(_repositorio).Count > 0;
        var reservas = reservasHabilitadas ? await Task.Run(_repositorio.ListarReservasActivas) : Array.Empty<ReservaResumen>();
        void Ajustar()
        {
            var escala = DeviceDpi / 96f;
            var disponible = Math.Max(1, flpTarjetas.ClientSize.Width);
            var columnas = Math.Clamp((int)(disponible / (275 * escala)), 1, 4);
            var ancho = Math.Max((int)(240 * escala), disponible / columnas - (int)(18 * escala));
            foreach (var tarjeta in flpTarjetas.Controls.OfType<TarjetaMesa>())
                tarjeta.Size = new Size(ancho, tarjeta.ObtenerAlturaPreferida(ancho));
        }

        void Dibujar()
        {
            flpTarjetas.SuspendLayout();
            while (flpTarjetas.Controls.Count > 0)
                flpTarjetas.Controls[0].Dispose();
            foreach (var mesa in mesas.Where(m => (cbFiltro.SelectedIndex == 0 || m.Estado == cbFiltro.Text) && m.Numero.ToString().Contains(ctBuscar.Text.Trim())))
            {
                var pedido = pedidos.FirstOrDefault(p => p.NumeroMesa == mesa.Numero && p.Estado is "Abierto" or "En preparación");
                var reserva = reservas.FirstOrDefault(r => r.MesaId == mesa.Id);
                var tarjeta = new TarjetaMesa(mesa, pedido, reserva, reservasHabilitadas, DiarioOperaciones.ParaPedido(_repositorio, mesa.Id, pedido?.Id).Count > 0);
                tarjeta.btnReservarMesa.Click += async (_, _) => await EjecutarSeguro(async () =>
                {
                    using var reservar = new FormularioReservarMesa(_repositorio, mesa);
                    reservar.ShowDialog(this);
                    await MostrarMesasPos();
                });
                tarjeta.btnAccionMesa.Click += async (_, _) => await EjecutarSeguro(async () =>
                {
                    var actual = (await Task.Run(_repositorio.ListarMesas)).Single(m => m.Id == mesa.Id);
                    var abierto = await Task.Run(() => _repositorio.ObtenerPedidoAbierto(actual.Id));
                    if (actual.Estado == "En limpieza" || actual.Estado == "Ocupada" && abierto is null)
                    {
                        using var gestion = new FormularioGestionMesa(_repositorio, actual, _dialogos);
                        gestion.ShowDialog(this);
                    }
                    else if (actual.Estado == "Reservada")
                    {
                        var vigente = reservasHabilitadas ? await Task.Run(() => _repositorio.ConsultarReservaMesa(actual.Id)) : null;
                        using var consulta = new FormularioReservaDetalle(_repositorio, actual, vigente, _dialogos);
                        consulta.ShowDialog(this);
                    }
                    else
                        await AbrirMesa(actual);
                    await MostrarMesasPos();
                });
                flpTarjetas.Controls.Add(tarjeta);
            }

            if (flpTarjetas.Controls.Count == 0)
                flpTarjetas.Controls.Add(Componentes.Etiqueta("No hay mesas para este filtro."));
            Ajustar();
            flpTarjetas.ResumeLayout(true);
        }

        ctBuscar.TextChanged += (_, _) => Dibujar();
        cbFiltro.SelectedIndexChanged += (_, _) => Dibujar();
        flpTarjetas.SizeChanged += (_, _) => Ajustar();
        btnActualizar.Click += async (_, _) => await EjecutarSeguro(MostrarMesasPos);
        if (InvokeRequired)
            Invoke(Dibujar);
        else
            Dibujar();
    }

    private async Task AbrirMesa(Mesa seleccionada)
    {
        // Re-read before editing: availability may have changed in another session.
        var actual = (await Task.Run(_repositorio.ListarMesas)).FirstOrDefault(m => m.Id == seleccionada.Id) ?? throw new InvalidOperationException("La mesa ya no existe. Actualiza la vista.");
        using var detalle = new FormularioDetalleMesa(_repositorio, actual, _dialogos);
        detalle.ShowDialog(this);
    }

    private async Task MostrarPedidosPos()
    {
        lblTitulo.Text = "Servicio / Pedidos";
        LimpiarContenido();
        var tlpPagina = CrearPagina();
        tlpPagina.Controls.Add(Encabezado("Pedidos", "Continúa un pedido abierto o selecciona una mesa para uno nuevo."));
        var btnNuevo = Componentes.Boton("Nuevo pedido · elegir mesa");
        btnNuevo.Name = "btnNuevo";
        btnNuevo.AutoSize = true;
        Iconos.Aplicar(btnNuevo, "plus");
        btnNuevo.Click += async (_, _) => await EjecutarSeguro(MostrarMesasPos);
        tlpPagina.Controls.Add(btnNuevo);
        var dgvRegistros = NuevaTabla();
        dgvRegistros.Height = 380;
        dgvRegistros.Dock = DockStyle.Top;
        var datos = await Task.Run(_repositorio.ListarPedidos);
        dgvRegistros.DataSource = datos.OrderByDescending(p => p.Fecha).ToList();
        tlpPagina.Controls.Add(dgvRegistros);
        var btnAbrir = Componentes.Boton("Continuar pedido seleccionado", false);
        btnAbrir.Name = "btnAbrir";
        btnAbrir.AutoSize = true;
        btnAbrir.Click += async (_, _) => await EjecutarSeguro(async () =>
        {
            if (dgvRegistros.CurrentRow?.DataBoundItem is not PedidoResumen p || p.Estado is not ("Abierto" or "En preparación"))
                throw new InvalidOperationException("Selecciona un pedido abierto. Las facturas cerradas se consultan en Reportes.");
            var mesas = await Task.Run(_repositorio.ListarMesas);
            await AbrirMesa(mesas.Single(m => m.Numero == p.NumeroMesa));
            await MostrarPedidosPos();
        });
        tlpPagina.Controls.Add(btnAbrir);
        var btnConsultar = Componentes.Boton("Consultar comprobante seleccionado", false);
        btnConsultar.Name = "btnConsultar";
        btnConsultar.Click += (_, _) =>
        {
            if (dgvRegistros.CurrentRow?.DataBoundItem is PedidoResumen p && p.NumeroFactura is not null)
            {
                using var documento = new FormularioComprobante(_repositorio, p.Id);
                documento.ShowDialog(this);
            }
            else
                _dialogos.Mostrar(this, "Seleccione un pedido con comprobante emitido.", "Consultar comprobante");
        };
        tlpPagina.Controls.Add(btnConsultar);
        if (datos.Count == 0)
            tlpPagina.Controls.Add(Componentes.Etiqueta("Todavía no hay pedidos. Comienza seleccionando una mesa."));
        FormatoPresentacion.Aplicar(this);
    }

    private Task MostrarConfiguracion()
    {
        if (_usuario.Rol is not ("Administrador" or "Gerente"))
            throw new InvalidOperationException("Solo gerente o administrador.");
        lblTitulo.Text = "Administración / Configuración";
        LimpiarContenido();
        var tlpPagina = CrearPagina();
        tlpPagina.Controls.Add(Encabezado("Configuración", "Información del restaurante y opciones avanzadas."));
        var btnRestaurante = Componentes.Boton("Información del restaurante", false);
        btnRestaurante.Name = "btnRestaurante";
        btnRestaurante.AutoSize = true;
        btnRestaurante.Click += (_, _) =>
        {
            using var formulario = new FormularioCatalogos(_repositorio, true, true);
            formulario.ShowDialog(this);
        };
        var btnSql = Componentes.Boton("Avanzado · Conexión SQL", false);
        btnSql.Name = "btnSql";
        btnSql.AutoSize = true;
        Iconos.Aplicar(btnSql, "database");
        btnSql.Click += async (_, _) => await EjecutarSeguro(MostrarConexion);
        tlpPagina.Controls.Add(btnRestaurante);
        if (_usuario.Rol == "Administrador")
            tlpPagina.Controls.Add(btnSql);
        return Task.CompletedTask;
    }
}
