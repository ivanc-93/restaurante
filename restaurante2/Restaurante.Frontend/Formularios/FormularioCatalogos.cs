using Restaurante.Backend.Dominio;
using Restaurante.Backend.Servicios;
using Restaurante.Frontend.Servicios;
namespace Restaurante.Frontend.Formularios;

public sealed partial class FormularioCatalogos : Form
{
    private readonly IRepositorioRestaurante _repositorio = null!;
    private bool _listo;
    private int? _id;
    private bool _sucio;
    private int _modoAnterior;
    private bool _cambiandoModo;
    private bool _puedeAdministrar;
    private readonly IDialogosUsuario _dialogos=new DialogosUsuario();
    private List<Cliente> _clientesCatalogo=new();
    private List<ElementoMenu> _productosCatalogo=new();
    private void AplicarFiltro()
    {
        if(tabla is null||buscarCatalogo is null)return;
        var texto=buscarCatalogo.Text.Trim();
        tabla.DataSource=modo.Text switch
        {
            "Clientes"=>_clientesCatalogo.Where(c=>c.Nombre.Contains(texto,StringComparison.CurrentCultureIgnoreCase)||(c.Telefono??"").Contains(texto,StringComparison.CurrentCultureIgnoreCase)||(c.Correo??"").Contains(texto,StringComparison.CurrentCultureIgnoreCase)).ToList(),
            "Productos"=>_productosCatalogo.Where(p=>p.Nombre.Contains(texto,StringComparison.CurrentCultureIgnoreCase)||p.Categoria.Contains(texto,StringComparison.CurrentCultureIgnoreCase)).ToList(),
            _=>null
        };
    }
    private bool ConfirmarSalida() => !_sucio || _dialogos.Mostrar(this,
        "Hay cambios sin guardar. ¿Deseas descartarlos?", "Cambios pendientes", MessageBoxButtons.YesNo,
        MessageBoxIcon.Question) == DialogResult.Yes;
    public FormularioCatalogos() => InitializeComponent();
    public FormularioCatalogos(IRepositorioRestaurante repositorio, bool administrador, bool soloRestaurante = false,IDialogosUsuario? dialogos=null) : this()
    {
        _dialogos=dialogos??new DialogosUsuario();
        _repositorio = repositorio;
        _puedeAdministrar=repositorio.UsuarioActual?.Rol is "Administrador" or "Gerente";
        if(!_puedeAdministrar)modo.Items.Remove("Productos");
        Componentes.EstilizarTabla(tabla);
        if (administrador&&_puedeAdministrar) modo.Items.Add("Restaurante");
        if (soloRestaurante) { modo.Items.Clear(); modo.Items.Add("Restaurante"); }
        modo.SelectedIndex = 0;
        Text = soloRestaurante ? "Información del restaurante" : "Catálogos · Clientes y productos";
        foreach (var campo in new TextBox[] {nombre,telefono,correo,categoria,ubicacion})
            campo.TextChanged += (_,_) => { if (_listo && Enabled) _sucio=true; };
        precio.ValueChanged += (_,_) => { if (_listo && Enabled) _sucio=true; };
        tipo.SelectedIndexChanged += (_,_) => { if (_listo && Enabled) _sucio=true; };
        alcoholica.CheckedChanged += (_,_) => { if (_listo && Enabled) _sucio=true; };
        FormClosing += (_,e) => { if (!Enabled || !ConfirmarSalida()) e.Cancel=true; };
        Shown += async (_, _) => { _listo = true; await CargarAsync(); };
    }
    private async void Modo_Cambio(object? sender, EventArgs e)
    {
        if (!_listo || _cambiandoModo) return;
        if (!ConfirmarSalida()) { _cambiandoModo=true; modo.SelectedIndex=_modoAnterior; _cambiandoModo=false; return; }
        _modoAnterior=modo.SelectedIndex;
        await CargarAsync();
    }
    private async Task CargarAsync()
    {
        var entidad = modo.Text;
        await EjecutarAsync(async () =>
        {
            if(entidad=="Clientes")_clientesCatalogo=await Task.Run(()=>_repositorio.ListarClientes().ToList());
            if(entidad=="Productos")_productosCatalogo=await Task.Run(()=>_repositorio.ListarMenu().ToList());
            tituloCatalogo.Text=entidad;
            buscarCatalogo.Visible=entidad!="Restaurante";
            buscarCatalogo.PlaceholderText=entidad=="Productos"?"Buscar producto o categoría":"Buscar cliente, teléfono o correo";
            AplicarFiltro();
            Limpiar();
            telefono.Visible = telefonoLabel.Visible = correo.Visible = correoLabel.Visible = entidad == "Clientes";
            categoria.Visible = categoriaLabel.Visible = tipo.Visible = tipoLabel.Visible = precio.Visible = precioLabel.Visible = alcoholica.Visible = entidad == "Productos";
            ubicacion.Visible = ubicacionLabel.Visible = entidad == "Restaurante";
            nuevo.Visible = entidad != "Restaurante";
            editar.Visible = _puedeAdministrar && entidad != "Restaurante";
            desactivar.Enabled = _puedeAdministrar && entidad != "Restaurante";
            desactivar.Visible=editar.Visible;
            if (entidad == "Restaurante")
            {
                var datos = await Task.Run(_repositorio.ObtenerRestaurante);
                nombre.Text = datos.Nombre;
                ubicacion.Text = datos.Ubicacion;
            }
            _sucio=false;
        });
    }
    private async Task EjecutarAsync(Func<Task> accion)
    {
        Enabled = false;
        UseWaitCursor = true;
        try { await accion(); }
        catch (Exception ex) { _dialogos.Mostrar(this, ex.Message, "Operación no completada", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        finally { Enabled = true; UseWaitCursor = false; }
    }
    private void Limpiar()
    {
        _id = null;
        nombre.Clear(); telefono.Clear(); correo.Clear(); categoria.Clear(); ubicacion.Clear();
        precio.Value = 0; tipo.SelectedIndex = 0; alcoholica.Checked = false;
        estadoEdicion.Text=modo.Text=="Restaurante"?"Información del restaurante":"Nuevo registro";
    }
    private void Nuevo_Click(object? sender, EventArgs e) { if (ConfirmarSalida()) { Limpiar(); _sucio=false; } }
    private void Seleccionar_Click(object? sender, EventArgs e)
    {
        if (!_puedeAdministrar || !ConfirmarSalida()) return;
        switch (tabla.CurrentRow?.DataBoundItem)
        {
            case Cliente c:
                _id=c.Id; nombre.Text=c.Nombre; telefono.Text=c.Telefono; correo.Text=c.Correo;
                break;
            case ElementoMenu p:
                _id=p.Id; nombre.Text=p.Nombre; categoria.Text=p.Categoria; precio.Value=p.Precio;
                tipo.SelectedItem=p.Tipo; alcoholica.Checked=p is Bebida { Alcoholica: true };
                break;
        }
        _sucio=false;
        estadoEdicion.Text=_id.HasValue?"Editar registro":"Nuevo registro";
    }
    private async void Guardar_Click(object? sender, EventArgs e)
    {
        var entidad=modo.Text; var id=_id; var n=nombre.Text; var tel=telefono.Text; var mail=correo.Text;
        var cat=categoria.Text; var t=tipo.Text; var p=precio.Value; var a=alcoholica.Checked; var u=ubicacion.Text;
        var guardado=false;
        await EjecutarAsync(async () =>
        {
            await Task.Run(() =>
            {
                if(entidad=="Clientes") _repositorio.GuardarCliente(id,n,tel,mail);
                else if(entidad=="Productos") _repositorio.GuardarProducto(id,n,cat,t,p,a);
                else _repositorio.GuardarRestaurante(n,u);
            });
            guardado=true;
        });
        if(guardado) { _sucio=false; _dialogos.Mostrar(this,"Datos guardados.","Catálogos"); await CargarAsync(); }
    }
    private async void Desactivar_Click(object? sender, EventArgs e)
    {
        if (!ConfirmarSalida()) return;
        _sucio=false; Seleccionar_Click(sender,e);
        if(_id is null) return;
        if(_dialogos.Mostrar(this,"¿Desactivar el registro? Su historial se conservará.","Confirmar",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes) return;
        var id=_id.Value; var entidad=modo.Text;
        await EjecutarAsync(() => Task.Run(() =>
        {
            if(entidad=="Clientes") _repositorio.DesactivarCliente(id);
            else _repositorio.DesactivarProducto(id);
        }));
        await CargarAsync();
    }
}
