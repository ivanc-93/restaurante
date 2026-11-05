using Restaurante.Frontend.Servicios;
using Restaurante.Backend.Dominio;

namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioDetalleMesa : Form
{
    private readonly IDialogosUsuario _dialogos = new DialogosUsuario();
    private readonly IRepositorioRestaurante _repositorio = null!;
    private readonly Mesa _mesa = null!;
    private PedidoResumen? _pedido;
    private ReservaResumen? _reserva;
    private List<ElementoMenu> _menu = new();
    private bool _ocupado;
    private bool _cargando;
    private bool _recuperacionPendiente;
    public FormularioDetalleMesa() => InitializeComponent();
    public FormularioDetalleMesa(IRepositorioRestaurante repositorio, Mesa mesa, IDialogosUsuario? dialogos = null, ReservaResumen? reserva = null) : this()
    {
        _repositorio = repositorio;
        _mesa = mesa;
        _reserva = reserva;
        _dialogos = dialogos ?? new DialogosUsuario();
        Text = $"Mesa {mesa.Numero} | Pedido";
        Shown += async (_, _) => await RecargarSeguroAsync();
        FormClosing += (_, e) =>
        {
            if (_ocupado || _cargando)
            {
                e.Cancel = true;
                lblEstado.Text = "Espera a que termine la operación.";
            }
        };
    }

    private async Task RecargarSeguroAsync()
    {
        if (InvokeRequired)
        {
            await Restaurante.Frontend.Infraestructura.ContextoInterfaz.Invocar(this, RecargarSeguroAsync);
            return;
        }

        Restaurante.Frontend.Infraestructura.ContextoInterfaz.AsegurarActual();
        btnAgregarProducto.Enabled = btnRevisarCobro.Enabled = false;
        flpCatalogo.Enabled = false;
        cbClientes.Enabled = false;
        _cargando = true;
        try
        {
            var datos = await Task.Run(() =>
            {
                var actual = _repositorio.ListarMesas().Single(m => m.Id == _mesa.Id);
                if (_reserva is not null)
                {
                    var vigente = _repositorio.ConsultarReservaMesa(_mesa.Id);
                    if (actual.Estado != "Reservada" || vigente?.Id != _reserva.Id || vigente.Version != _reserva.Version)
                        throw new InvalidOperationException("La reserva cambió o ya fue atendida/cancelada. Regresa a Mesas y actualiza.");
                }

                var pedido = _repositorio.ObtenerPedidoAbierto(_mesa.Id);
                var productos = _repositorio.ListarMenu().ToList();
                var clientes = _repositorio.ListarClientes().ToList();
                var revision = pedido is null ? null : _repositorio.RevisarCuenta(pedido.Id);
                var incierta = pedido is null ? null : _repositorio.ObtenerPagoIncierto(pedido.Id);
                var detalles = pedido is null ? new List<DetallePedidoResumen>() : _repositorio.ListarDetallesPedido(pedido.Id).ToList();
                var cobranza = pedido is not null && _repositorio.CobranzaParcialDisponible() ? _repositorio.ConsultarCobranza(pedido.Id) : null;
                var servicio = _repositorio.CuentasOperativasDisponibles() ? _repositorio.ConsultarServicioMesa(_mesa.Id) : null;
                return (actual, pedido, productos, clientes, detalles, revision, incierta, cobranza, servicio);
            });
            var clienteId = (cbClientes.SelectedItem as Cliente)?.Id ?? 0;
            var productoId = (cbProductos.SelectedItem as ElementoMenu)?.Id;
            var categoria = cbCategoria.Text;
            _pedido = datos.pedido;
            _menu = datos.productos;
            cbClientes.DataSource = datos.revision is null ? new List<Cliente>
            {
                new(0, "Público general", null, null)
            }.Concat(datos.clientes).ToList() : new List<Cliente>
            {
                new(datos.revision.ClienteId ?? 0, datos.revision.Cliente, null, null)
            };
            if (_pedido is null)
                cbClientes.SelectedItem = cbClientes.Items.Cast<Cliente>().FirstOrDefault(c => c.Id == clienteId) ?? cbClientes.Items[0];
            cbClientes.Enabled = _pedido is null && (datos.actual.Estado == "Disponible" || datos.actual.Estado == "Reservada" && _reserva is not null);
            cbCategoria.Items.Clear();
            cbCategoria.Items.Add("Todas las categorías");
            cbCategoria.Items.AddRange(_menu.Select(p => p.Categoria).Distinct().Order().Cast<object>().ToArray());
            cbCategoria.SelectedItem = cbCategoria.Items.Contains(categoria) ? categoria : cbCategoria.Items[0];
            dgvDetalles.DataSource = datos.detalles;
            lblResumen.Text = $"Mesa {_mesa.Numero} · " + (_pedido is null ? datos.actual.Estado : $"Pedido #{_pedido.Id} · {_pedido.Estado} · {_pedido.Cliente}");
            lblTotal.Text = _pedido is null ? "Sin pedido registrado" : $"Total: {FormatoPresentacion.Moneda(_pedido.Total)}";
            if (datos.cobranza is not null)
                lblTotal.Text = $"Consumo {FormatoPresentacion.Moneda(datos.cobranza.Pedido.ImporteOriginal)}\nAjustes {FormatoPresentacion.Moneda(datos.cobranza.Pedido.Descuento)} · pagado {FormatoPresentacion.Moneda(datos.cobranza.Pagado)}\nSaldo {FormatoPresentacion.Moneda(datos.cobranza.Saldo)}";
            lblEstado.Text = _pedido is null ? "El primer producto abre y guarda el pedido." : "Comanda guardada. Agregar registra el consumo.";
            _recuperacionPendiente = DiarioOperaciones.ParaPedido(_repositorio, _mesa.Id, _pedido?.Id).Count > 0;
            btnAgregarProducto.Enabled = _menu.Count > 0 && (_pedido is not null || (datos.actual.Estado == "Disponible" || datos.actual.Estado == "Reservada" && _reserva is not null)) && datos.incierta is null;
            if (datos.cobranza?.Congelada == true)
                btnAgregarProducto.Enabled = false;
            foreach (var b in tlpDerecha.Controls.Find("btnCorregir", true).Concat(tlpDerecha.Controls.Find("btnPreparar", true)))
                b.Enabled = _pedido is not null && datos.cobranza?.Congelada != true;
            flpCatalogo.Enabled = btnAgregarProducto.Enabled;
            if (_recuperacionPendiente)
                btnAgregarProducto.Enabled = flpCatalogo.Enabled = false;
            btnRevisarCobro.Enabled = _pedido is not null && datos.detalles.Count > 0 && datos.incierta is null && !_recuperacionPendiente;
            btnGestionar.Enabled = !_recuperacionPendiente;
            if (_recuperacionPendiente)
                lblEstado.Text = "Hay una operación pendiente de confirmar. Revísala aquí. Cerrar esta ventana no cancela el pedido ni libera la mesa.";
            if (datos.incierta is not null)
                lblEstado.Text = "Tarjeta incierta: no repita el cobro. Utilice Gestionar cierre para conciliar.";
            _cargando = false;
            DibujarCatalogo();
            if (productoId is not null)
                cbProductos.SelectedItem = cbProductos.Items.Cast<ElementoMenu>().FirstOrDefault(p => p.Id == productoId) ?? cbProductos.SelectedItem;
            AplicarServicio(datos.servicio, datos.revision, datos.cobranza);
            FormatoPresentacion.Aplicar(this);
            EstiloResumenCuentas();
            dgvDetalles.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgvDetalles.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            if (dgvDetalles.Columns.Contains("Cantidad"))
            {
                dgvDetalles.Columns["Cantidad"].HeaderText = "Cant.";
                dgvDetalles.Columns["Cantidad"].FillWeight = 45;
            }

            if (dgvDetalles.Columns.Contains("Importe"))
            {
                dgvDetalles.Columns["Importe"].HeaderText = "Total";
                dgvDetalles.Columns["Importe"].FillWeight = 100;
                dgvDetalles.Columns["Importe"].DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            }

            if (dgvDetalles.Columns.Contains("Producto"))
                dgvDetalles.Columns["Producto"].FillWeight = 150;
        }
        catch (Exception ex)
        {
            _recuperacionPendiente = true;
            btnGestionar.Enabled = false;
            lblEstado.Text = "No se pudo confirmar el estado. Cierra y vuelve a abrir la mesa para actualizar; no repitas la operación.";
            _dialogos.Mostrar(this, ex.Message, "No se pudo cargar la mesa", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _cargando = false;
        }
    }

    private void Filtro_Cambio(object? sender, EventArgs e)
    {
        if (!_cargando)
            DibujarCatalogo();
    }

    private void DibujarCatalogo()
    {
        var seleccion = (cbProductos.SelectedItem as ElementoMenu)?.Id;
        var lista = _menu.Where(p => p.Nombre.Contains(ctBuscar.Text.Trim(), StringComparison.CurrentCultureIgnoreCase) && (cbCategoria.SelectedIndex <= 0 || p.Categoria == cbCategoria.Text) && (cbTipo.SelectedIndex <= 0 || p.Tipo == cbTipo.Text)).ToList();
        cbProductos.DataSource = lista;
        if (seleccion is not null)
            cbProductos.SelectedItem = lista.FirstOrDefault(p => p.Id == seleccion) ?? cbProductos.SelectedItem;
        flpCatalogo.SuspendLayout();
        while (flpCatalogo.Controls.Count > 0)
            flpCatalogo.Controls[0].Dispose();
        foreach (var producto in lista)
        {
            var btnTarjeta = new Restaurante.Frontend.Controles.TarjetaProducto(producto);
            var escala = DeviceDpi / 96f;
            var columnas = Math.Max(1, flpCatalogo.ClientSize.Width / (int)(210 * escala));
            btnTarjeta.Size = new Size(Math.Max((int)(180 * escala), flpCatalogo.ClientSize.Width / columnas - (int)(18 * escala)), (int)(145 * escala));
            btnTarjeta.Margin = new Padding(6);
            btnTarjeta.Marcar(producto.Id == (cbProductos.SelectedItem as ElementoMenu)?.Id);
            btnTarjeta.Seleccionar += (_, _) =>
            {
                cbProductos.SelectedItem = producto;
                foreach (var tarjeta in flpCatalogo.Controls.OfType<Restaurante.Frontend.Controles.TarjetaProducto>())
                    tarjeta.Marcar(tarjeta == btnTarjeta);
                lblEstado.Text = $"{producto.Nombre} · {FormatoPresentacion.Moneda(producto.Precio)}. Indica cantidad y agrega al pedido.";
                nudCantidad.Focus();
            };
            flpCatalogo.Controls.Add(btnTarjeta);
        }

        if (lista.Count == 0)
            flpCatalogo.Controls.Add(Componentes.Etiqueta("No hay productos para esta búsqueda."));
        flpCatalogo.ResumeLayout(true);
    }

    private async void Agregar_Click(object? sender, EventArgs e)
    {
        if (_ocupado || _recuperacionPendiente)
            return;
        if (cbProductos.SelectedItem is not ElementoMenu producto)
        {
            lblEstado.Text = "Selecciona un producto.";
            return;
        }

        var cantidad = (int)nudCantidad.Value;
        var cliente = (cbClientes.SelectedItem as Cliente)?.Id;
        _ocupado = true;
        btnAgregarProducto.Enabled = btnRevisarCobro.Enabled = flpCatalogo.Enabled = cbClientes.Enabled = false;
        try
        {
            if (_servicio?.Operativa == true)
            {
                var seleccion = CuentaSeleccionada();
                if (seleccion is null || seleccion.Id < 0)
                    throw new ArgumentException("Elige una cuenta concreta antes de agregar el producto.");
                var s = new SolicitudProductoCuenta(Guid.NewGuid(), _mesa.Id, _servicio.PedidoId, _servicio.VersionPedido, seleccion.Id, seleccion.Version, producto.Id, cantidad, cliente > 0 ? cliente : null, _reserva?.Id, _reserva?.Version);
                var r = await DiarioOperaciones.Ejecutar(_repositorio, "ProductoCuenta", s.OperacionId, s, () => _repositorio.AgregarProductoCuenta(s));
                _cuentaSeleccionada = r.CuentaId;
                _reserva = null;
            }
            else if (_pedido is null && _reserva is not null)
            {
                var solicitud = new SolicitudAtencionReserva(Guid.NewGuid(), _reserva.Id, _mesa.Id, _reserva.Version, cliente > 0 ? cliente : null, [new DetallePedidoSolicitud(producto.Id, cantidad)]);
                await DiarioOperaciones.Ejecutar(_repositorio, "AtenderReserva", solicitud.OperacionId, solicitud, () => _repositorio.AtenderReserva(solicitud));
                _reserva = null;
            }
            else if (_pedido is null)
            {
                var solicitud = new SolicitudPedido
                {
                    MesaId = _mesa.Id,
                    ClienteId = cliente > 0 ? cliente : null,
                    Detalles = [new DetallePedidoSolicitud(producto.Id, cantidad)]
                };
                await DiarioOperaciones.Ejecutar(_repositorio, "Abrir", solicitud.OperacionId, solicitud, () => _repositorio.CrearPedido(solicitud));
            }
            else
            {
                var solicitud = new SolicitudIncorporacion(Guid.NewGuid(), _pedido.Id, producto.Id, cantidad);
                await DiarioOperaciones.Ejecutar(_repositorio, "Agregar", solicitud.OperacionId, solicitud, () => _repositorio.AgregarProductoAPedido(solicitud.PedidoId, solicitud.MenuId, solicitud.Cantidad, solicitud.OperacionId));
            }

            nudCantidad.Value = 1;
        }
        catch (Exception ex)
        {
            _dialogos.Mostrar(this, ex.Message + "\nSe consultará el estado guardado antes de continuar.", "No se pudo completar la operación", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            await RecargarSeguroAsync();
            _ocupado = false;
        }
    }

    private async void Facturar_Click(object? sender, EventArgs e)
    {
        if (_ocupado || _recuperacionPendiente || _pedido is null)
            return;
        _ocupado = true;
        btnRevisarCobro.Enabled = false;
        try
        {
            var revision = await Task.Run(() => _repositorio.RevisarCuenta(_pedido.Id));
            if (await Task.Run(_repositorio.CobranzaParcialDisponible))
            {
                if (_servicio?.Operativa == true && CuentaSeleccionada()is { Id: 0 } general)
                {
                    var preparar = new SolicitudCuentaMesa(Guid.NewGuid(), _mesa.Id, revision.PedidoId, revision.Version, 0, null, "Renombrar", general.Nombre, true);
                    _cuentaSeleccionada = await DiarioOperaciones.Ejecutar(_repositorio, "CuentaMesa", preparar.OperacionId, preparar, () => _repositorio.GestionarCuentaMesa(preparar));
                    _servicio = await Task.Run(() => _repositorio.ConsultarServicioMesa(_mesa.Id));
                }

                using var nueva = new FormularioCobranza(_repositorio, revision.PedidoId, _servicio?.Operativa == true && _cuentaSeleccionada > 0 ? _cuentaSeleccionada : null);
                nueva.ShowDialog(this);
                if (nueva.Finalizado)
                {
                    _ocupado = false;
                    DialogResult = DialogResult.OK;
                    Close();
                }

                return;
            }

            using var cobro = new FormularioCobro(_repositorio, revision, _dialogos);
            cobro.ShowDialog(this);
            if (cobro.Resultado?.Estado == "Confirmado")
            {
                using var comprobante = new FormularioComprobante(_repositorio, revision.PedidoId);
                comprobante.ShowDialog(this);
                _ocupado = false;
                DialogResult = DialogResult.OK;
                Close();
                return;
            }
        }
        catch (Exception ex)
        {
            _dialogos.Mostrar(this, ex.Message, "No se pudo revisar el cobro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            if (!IsDisposed)
                await RecargarSeguroAsync();
            _ocupado = false;
        }
    }

    private async void Gestionar_Click(object? sender, EventArgs e)
    {
        if (_ocupado || _cargando || _recuperacionPendiente)
            return;
        using var gestion = new FormularioGestionMesa(_repositorio, _mesa, _dialogos);
        gestion.ShowDialog(this);
        await RecargarSeguroAsync();
    }

    private async void Corregir_Click(object? sender, EventArgs e)
    {
        if (_ocupado || _cargando || _recuperacionPendiente || _pedido is null)
            return;
        if (_servicio?.Operativa == true)
        {
            await EditarProductosCuenta(true);
            return;
        }

        _ocupado = true;
        try
        {
            var cuenta = await Task.Run(() => _repositorio.RevisarCuenta(_pedido.Id));
            using var f = new FormularioCorreccionLinea(_repositorio, cuenta);
            f.ShowDialog(this);
        }
        catch (Exception ex)
        {
            _dialogos.Mostrar(this, ex.Message, "Corrección de comanda");
        }
        finally
        {
            _ocupado = false;
            await RecargarSeguroAsync();
        }
    }

    private async void Preparar_Click(object? sender, EventArgs e)
    {
        if (_ocupado || _cargando || _recuperacionPendiente || _pedido is null)
            return;
        if (MessageBox.Show(this, "Declara que el pedido entró en preparación. Después no se permite retirar unidades como error de captura; se conservará el consumo y deberá registrarse incidencia. ¿Continuar?", "Declarar preparación", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;
        _ocupado = true;
        try
        {
            var id = Guid.NewGuid();
            var pedido = _pedido.Id;
            await DiarioOperaciones.Ejecutar(_repositorio, "Preparacion", id, pedido, () => _repositorio.MarcarPreparacion(pedido, id));
        }
        catch (Exception ex)
        {
            _dialogos.Mostrar(this, ex.Message, "Preparación");
        }
        finally
        {
            _ocupado = false;
            await RecargarSeguroAsync();
        }
    }
}
