using Restaurante.Frontend.Servicios;
using Restaurante.Backend.Dominio;

namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioDetalleMesa : Form
{
    private readonly IDialogosUsuario _dialogos = new DialogosUsuario();
    private readonly IRepositorioRestaurante _repositorio = null!;
    private readonly Mesa _mesa = null!;
    private PedidoResumen? _pedido;
    private List<ElementoMenu> _menu = new();
    private bool _ocupado;
    private bool _cargando;
    private bool _recuperacionPendiente;
    public FormularioDetalleMesa() => InitializeComponent();
    public FormularioDetalleMesa(IRepositorioRestaurante repositorio, Mesa mesa, IDialogosUsuario? dialogos = null) : this()
    {
        _repositorio = repositorio;
        _mesa = mesa;
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
        btnAgregarProducto.Enabled = btnRevisarCobro.Enabled = false;
        flpCatalogo.Enabled = false;
        cbClientes.Enabled = false;
        _cargando = true;
        try
        {
            var datos = await Task.Run(() =>
            {
                var actual = _repositorio.ListarMesas().Single(m => m.Id == _mesa.Id);
                var pedido = _repositorio.ObtenerPedidoAbierto(_mesa.Id);
                var productos = _repositorio.ListarMenu().ToList();
                var clientes = _repositorio.ListarClientes().ToList();
                var revision = pedido is null ? null : _repositorio.RevisarCuenta(pedido.Id);
                var incierta = pedido is null ? null : _repositorio.ObtenerPagoIncierto(pedido.Id);
                var detalles = pedido is null ? new List<DetallePedidoResumen>() : _repositorio.ListarDetallesPedido(pedido.Id).ToList();
                return (actual, pedido, productos, clientes, detalles, revision, incierta);
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
            cbClientes.Enabled = _pedido is null && datos.actual.Estado == "Disponible";
            cbCategoria.Items.Clear();
            cbCategoria.Items.Add("Todas las categorías");
            cbCategoria.Items.AddRange(_menu.Select(p => p.Categoria).Distinct().Order().Cast<object>().ToArray());
            cbCategoria.SelectedItem = cbCategoria.Items.Contains(categoria) ? categoria : cbCategoria.Items[0];
            dgvDetalles.DataSource = datos.detalles;
            lblResumen.Text = $"Mesa {_mesa.Numero} · " + (_pedido is null ? datos.actual.Estado : $"Pedido #{_pedido.Id} · {_pedido.Estado} · {_pedido.Cliente}");
            lblTotal.Text = _pedido is null ? "Sin pedido registrado" : $"Total: {FormatoPresentacion.Moneda(_pedido.Total)}";
            lblEstado.Text = _pedido is null ? "El primer producto abre y guarda el pedido." : "Productos guardados. Cada incorporación se registra al agregar.";
            _recuperacionPendiente = DiarioOperaciones.Listar(_repositorio).Count > 0;
            btnAgregarProducto.Enabled = _menu.Count > 0 && (_pedido is not null || datos.actual.Estado == "Disponible") && datos.incierta is null;
            flpCatalogo.Enabled = btnAgregarProducto.Enabled;
            if (_recuperacionPendiente)
                btnAgregarProducto.Enabled = flpCatalogo.Enabled = false;
            btnRevisarCobro.Enabled = _pedido is not null && datos.detalles.Count > 0 && datos.incierta is null && !_recuperacionPendiente;
            btnGestionar.Enabled = !_recuperacionPendiente;
            if (_recuperacionPendiente)
                lblEstado.Text = "Hay operaciones pendientes. Cierre la mesa y use Recuperar operaciones desde Inicio.";
            if (datos.incierta is not null)
                lblEstado.Text = "Tarjeta incierta: no repita el cobro. Utilice Gestionar cierre para conciliar.";
            _cargando = false;
            DibujarCatalogo();
            if (productoId is not null)
                cbProductos.SelectedItem = cbProductos.Items.Cast<ElementoMenu>().FirstOrDefault(p => p.Id == productoId) ?? cbProductos.SelectedItem;
            FormatoPresentacion.Aplicar(this);
        }
        catch (Exception ex)
        {
            _recuperacionPendiente = true;
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
            var btnTarjeta = Componentes.Boton($"{producto.Nombre}\n{producto.Categoria} · {producto.Tipo}\n{FormatoPresentacion.Moneda(producto.Precio)}\nSeleccionar", false);
            btnTarjeta.Name = "btnTarjeta";
            btnTarjeta.Size = new Size((int)(180 * DeviceDpi / 96f), (int)(135 * DeviceDpi / 96f));
            btnTarjeta.AutoSize = false;
            btnTarjeta.Margin = new Padding(8);
            btnTarjeta.TextAlign = ContentAlignment.MiddleLeft;
            Iconos.Aplicar(btnTarjeta, "utensils-crossed");
            btnTarjeta.Click += (_, _) =>
            {
                cbProductos.SelectedItem = producto;
                lblEstado.Text = $"Seleccionado: {producto.Nombre}. Elige cantidad y pulsa Agregar.";
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
            if (_pedido is null)
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
        if (_ocupado || _cargando)
            return;
        using var gestion = new FormularioGestionMesa(_repositorio, _mesa, _dialogos);
        gestion.ShowDialog(this);
        await RecargarSeguroAsync();
    }

    private async void Corregir_Click(object? sender, EventArgs e)
    {
        if (_ocupado || _cargando || _recuperacionPendiente || _pedido is null)
            return;
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
