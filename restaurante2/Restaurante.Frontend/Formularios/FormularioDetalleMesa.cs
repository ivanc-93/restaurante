using Restaurante.Frontend.Servicios;
using Restaurante.Backend.Dominio;
using Restaurante.Backend.Servicios;
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
        _repositorio = repositorio; _mesa = mesa; _dialogos = dialogos ?? new DialogosUsuario();
        Text = $"Mesa {mesa.Numero} | Pedido";
        Shown += async (_, _) => await RecargarSeguroAsync();
        FormClosing += (_, e) => { if (_ocupado || _cargando) { e.Cancel=true; _estado.Text="Espera a que termine la operación."; } };
    }
    private async Task RecargarSeguroAsync()
    {
        _agregar.Enabled = _facturar.Enabled = false; _catalogo.Enabled=false; _clientes.Enabled=false; _cargando=true;
        try
        {
            var datos = await Task.Run(() => {
                var actual = _repositorio.ListarMesas().Single(m=>m.Id==_mesa.Id);
                var pedido = _repositorio.ObtenerPedidoAbierto(_mesa.Id);
                var productos = _repositorio.ListarMenu().ToList();
                var clientes = _repositorio.ListarClientes().ToList();
                var detalles = pedido is null ? new List<DetallePedidoResumen>() : _repositorio.ListarDetallesPedido(pedido.Id).ToList();
                return (actual, pedido, productos, clientes, detalles);
            });
            var clienteId = (_clientes.SelectedItem as Cliente)?.Id ?? 0;
            var productoId = (_productos.SelectedItem as ElementoMenu)?.Id;
            var categoria = _categoria.Text;
            _pedido = datos.pedido; _menu=datos.productos;
            _clientes.DataSource = new List<Cliente>{new(0,"Público general",null,null)}.Concat(datos.clientes).ToList();
            if (_pedido is null) _clientes.SelectedItem = _clientes.Items.Cast<Cliente>().FirstOrDefault(c=>c.Id==clienteId) ?? _clientes.Items[0];
            _clientes.Enabled = _pedido is null && datos.actual.Estado=="Disponible";
            _categoria.Items.Clear(); _categoria.Items.Add("Todas las categorías");
            _categoria.Items.AddRange(_menu.Select(p=>p.Categoria).Distinct().Order().Cast<object>().ToArray());
            _categoria.SelectedItem=_categoria.Items.Contains(categoria)?categoria:_categoria.Items[0];
            _detalles.DataSource=datos.detalles;
            _resumen.Text=$"Mesa {_mesa.Numero} · " + (_pedido is null ? datos.actual.Estado : $"Pedido #{_pedido.Id} · {_pedido.Estado} · {_pedido.Cliente}");
            _total.Text=_pedido is null ? "Sin pedido registrado" : $"Total: {FormatoPresentacion.Moneda(_pedido.Total)}";
            _estado.Text=_pedido is null ? "El primer producto abre y guarda el pedido." : "Productos guardados. Cada incorporación se registra al agregar.";
            _recuperacionPendiente=false;
            _agregar.Enabled=_menu.Count>0 && (_pedido is not null || datos.actual.Estado=="Disponible");
            _catalogo.Enabled=_agregar.Enabled;
            _facturar.Enabled=_pedido is not null && datos.detalles.Count>0;
            _cargando=false; DibujarCatalogo();
            if (productoId is not null) _productos.SelectedItem=_productos.Items.Cast<ElementoMenu>().FirstOrDefault(p=>p.Id==productoId) ?? _productos.SelectedItem;
            FormatoPresentacion.Aplicar(this);
        }
        catch (Exception ex)
        {
            _recuperacionPendiente=true; _estado.Text="No se pudo confirmar el estado. Cierra y vuelve a abrir la mesa para actualizar; no repitas la operación.";
            _dialogos.Mostrar(this,ex.Message,"No se pudo cargar la mesa",MessageBoxButtons.OK,MessageBoxIcon.Error);
        }
        finally { _cargando=false; }
    }
    private void Filtro_Cambio(object? sender, EventArgs e) { if (!_cargando) DibujarCatalogo(); }
    private void DibujarCatalogo()
    {
        var seleccion = (_productos.SelectedItem as ElementoMenu)?.Id;
        var lista=_menu.Where(p=>p.Nombre.Contains(_buscar.Text.Trim(),StringComparison.CurrentCultureIgnoreCase)
            && (_categoria.SelectedIndex<=0 || p.Categoria==_categoria.Text)
            && (_tipo.SelectedIndex<=0 || p.Tipo==_tipo.Text)).ToList();
        _productos.DataSource=lista;
        if (seleccion is not null) _productos.SelectedItem=lista.FirstOrDefault(p=>p.Id==seleccion) ?? _productos.SelectedItem;
        _catalogo.SuspendLayout(); while (_catalogo.Controls.Count>0) _catalogo.Controls[0].Dispose();
        foreach(var producto in lista)
        {
            var tarjeta=Componentes.Boton($"{producto.Nombre}\n{producto.Categoria} · {producto.Tipo}\n{FormatoPresentacion.Moneda(producto.Precio)}\nSeleccionar",false);
            tarjeta.Size=new Size((int)(180 * DeviceDpi / 96f),(int)(135 * DeviceDpi / 96f)); tarjeta.AutoSize=false; tarjeta.Margin=new Padding(8); tarjeta.TextAlign=ContentAlignment.MiddleLeft;
            Iconos.Aplicar(tarjeta,"utensils-crossed");
            tarjeta.Click+=(_,_)=> { _productos.SelectedItem=producto; _estado.Text=$"Seleccionado: {producto.Nombre}. Elige cantidad y pulsa Agregar."; _cantidad.Focus(); };
            _catalogo.Controls.Add(tarjeta);
        }
        if(lista.Count==0) _catalogo.Controls.Add(Componentes.Etiqueta("No hay productos para esta búsqueda."));
        _catalogo.ResumeLayout(true);
    }
    private async void Agregar_Click(object? sender, EventArgs e)
    {
        if(_ocupado || _recuperacionPendiente)return;
        if(_productos.SelectedItem is not ElementoMenu producto){_estado.Text="Selecciona un producto.";return;}
        var cantidad=(int)_cantidad.Value; var cliente=(_clientes.SelectedItem as Cliente)?.Id;
        _ocupado=true; _agregar.Enabled=_facturar.Enabled=_catalogo.Enabled=_clientes.Enabled=false;
        try
        {
            if(_pedido is null)
                await Task.Run(()=>_repositorio.CrearPedido(new SolicitudPedido {MesaId=_mesa.Id,ClienteId=cliente>0?cliente:null,
                    Detalles=[new DetallePedidoSolicitud(producto.Id,cantidad)]}));
            else
            {
                var id=_pedido.Id;
                await Task.Run(()=>_repositorio.AgregarProductoAPedido(id,producto.Id,cantidad));
            }
            _cantidad.Value=1;
        }
        catch(Exception ex)
        {
            _dialogos.Mostrar(this,ex.Message+"\nSe consultará el estado guardado antes de continuar.","No se pudo completar la operación",MessageBoxButtons.OK,MessageBoxIcon.Error);
        }
        finally { await RecargarSeguroAsync(); _ocupado=false; }
    }
    private async void Facturar_Click(object? sender, EventArgs e)
    {
        if(_ocupado || _recuperacionPendiente || _pedido is null)return;
        if(_dialogos.Mostrar(this,$"¿Facturar el pedido #{_pedido.Id} por {FormatoPresentacion.Moneda(_pedido.Total)} y liberar la mesa?",
            "Confirmar factura",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)return;
        _ocupado=true; _agregar.Enabled=_facturar.Enabled=_catalogo.Enabled=false;
        try
        {
            var id=_pedido.Id; var factura=await Task.Run(()=>_repositorio.FacturarPedido(id));
            _dialogos.Mostrar(this,$"Factura {factura} emitida. La mesa {_mesa.Numero} quedó disponible.","Pedido facturado",MessageBoxButtons.OK,MessageBoxIcon.Information);
            _ocupado=false; DialogResult=DialogResult.OK; Close();
        }
        catch(Exception ex)
        {
            _dialogos.Mostrar(this,ex.Message,"No se pudo facturar",MessageBoxButtons.OK,MessageBoxIcon.Error);
            await RecargarSeguroAsync();
        }
        finally { _ocupado=false; }
    }
}
