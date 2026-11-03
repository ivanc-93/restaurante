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
    private readonly IDialogosUsuario _dialogos = new DialogosUsuario();
    private List<Cliente> _clientesCatalogo = new();
    private List<ElementoMenu> _productosCatalogo = new();
    private void AplicarFiltro()
    {
        if (dgvRegistros is null || ctBuscarCatalogo is null)
            return;
        var texto = ctBuscarCatalogo.Text.Trim();
        dgvRegistros.DataSource = cbModo.Text switch
        {
            "Clientes" => _clientesCatalogo.Where(c => c.Nombre.Contains(texto, StringComparison.CurrentCultureIgnoreCase) || (c.Telefono ?? "").Contains(texto, StringComparison.CurrentCultureIgnoreCase) || (c.Correo ?? "").Contains(texto, StringComparison.CurrentCultureIgnoreCase)).ToList(),
            "Productos" => _productosCatalogo.Where(p => p.Nombre.Contains(texto, StringComparison.CurrentCultureIgnoreCase) || p.Categoria.Contains(texto, StringComparison.CurrentCultureIgnoreCase)).ToList(),
            _ => null
        };
    }

    private bool ConfirmarSalida() => !_sucio || _dialogos.Mostrar(this, "Hay cambios sin guardar. ¿Deseas descartarlos?", "Cambios pendientes", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    public FormularioCatalogos() => InitializeComponent();
    public FormularioCatalogos(IRepositorioRestaurante repositorio, bool administrador, bool soloRestaurante = false, IDialogosUsuario? dialogos = null) : this()
    {
        _dialogos = dialogos ?? new DialogosUsuario();
        _repositorio = repositorio;
        _puedeAdministrar = repositorio.UsuarioActual?.Rol is "Administrador" or "Gerente";
        if (!_puedeAdministrar)
            cbModo.Items.Remove("Productos");
        Componentes.EstilizarTabla(dgvRegistros);
        if (administrador && _puedeAdministrar)
            cbModo.Items.Add("Restaurante");
        if (soloRestaurante)
        {
            cbModo.Items.Clear();
            cbModo.Items.Add("Restaurante");
        }

        cbModo.SelectedIndex = 0;
        Text = soloRestaurante ? "Información del restaurante" : "Catálogos · Clientes y productos";
        foreach (var campo in new TextBox[]
        {
            ctNombre,
            ctTelefono,
            ctCorreo,
            ctCategoria,
            ctUbicacion
        }

        )
            campo.TextChanged += (_, _) =>
            {
                if (_listo && Enabled)
                    _sucio = true;
            };
        nudPrecio.ValueChanged += (_, _) =>
        {
            if (_listo && Enabled)
                _sucio = true;
        };
        cbTipo.SelectedIndexChanged += (_, _) =>
        {
            if (_listo && Enabled)
                _sucio = true;
        };
        chkAlcoholica.CheckedChanged += (_, _) =>
        {
            if (_listo && Enabled)
                _sucio = true;
        };
        FormClosing += (_, e) =>
        {
            if (!Enabled || !ConfirmarSalida())
                e.Cancel = true;
        };
        Shown += async (_, _) =>
        {
            _listo = true;
            await CargarAsync();
        };
    }

    private async void Modo_Cambio(object? sender, EventArgs e)
    {
        if (!_listo || _cambiandoModo)
            return;
        if (!ConfirmarSalida())
        {
            _cambiandoModo = true;
            cbModo.SelectedIndex = _modoAnterior;
            _cambiandoModo = false;
            return;
        }

        _modoAnterior = cbModo.SelectedIndex;
        await CargarAsync();
    }

    private async Task CargarAsync()
    {
        var entidad = cbModo.Text;
        await EjecutarAsync(async () =>
        {
            if (entidad == "Clientes")
                _clientesCatalogo = await Task.Run(() => _repositorio.ListarClientes().ToList());
            if (entidad == "Productos")
                _productosCatalogo = await Task.Run(() => _repositorio.ListarMenu().ToList());
            lblTituloCatalogo.Text = entidad;
            ctBuscarCatalogo.Visible = entidad != "Restaurante";
            ctBuscarCatalogo.PlaceholderText = entidad == "Productos" ? "Buscar producto o categoría" : "Buscar cliente, teléfono o correo";
            AplicarFiltro();
            Limpiar();
            ctTelefono.Visible = lblTelefonoLabel.Visible = ctCorreo.Visible = lblCorreoLabel.Visible = entidad == "Clientes";
            ctCategoria.Visible = lblCategoriaLabel.Visible = cbTipo.Visible = lblTipoLabel.Visible = nudPrecio.Visible = lblPrecioLabel.Visible = chkAlcoholica.Visible = entidad == "Productos";
            ctUbicacion.Visible = lblUbicacionLabel.Visible = entidad == "Restaurante";
            btnNuevo.Visible = entidad != "Restaurante";
            btnEditar.Visible = _puedeAdministrar && entidad != "Restaurante";
            btnDesactivar.Enabled = _puedeAdministrar && entidad != "Restaurante";
            btnDesactivar.Visible = btnEditar.Visible;
            if (entidad == "Restaurante")
            {
                var datos = await Task.Run(_repositorio.ObtenerRestaurante);
                ctNombre.Text = datos.Nombre;
                ctUbicacion.Text = datos.Ubicacion;
            }

            _sucio = false;
        });
    }

    private async Task EjecutarAsync(Func<Task> accion)
    {
        Enabled = false;
        UseWaitCursor = true;
        try
        {
            await accion();
        }
        catch (Exception ex)
        {
            _dialogos.Mostrar(this, ex.Message, "Operación no completada", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Enabled = true;
            UseWaitCursor = false;
        }
    }

    private void Limpiar()
    {
        _id = null;
        ctNombre.Clear();
        ctTelefono.Clear();
        ctCorreo.Clear();
        ctCategoria.Clear();
        ctUbicacion.Clear();
        nudPrecio.Value = 0;
        cbTipo.SelectedIndex = 0;
        chkAlcoholica.Checked = false;
        lblEstadoEdicion.Text = cbModo.Text == "Restaurante" ? "Información del restaurante" : "Nuevo registro";
    }

    private void Nuevo_Click(object? sender, EventArgs e)
    {
        if (ConfirmarSalida())
        {
            Limpiar();
            _sucio = false;
        }
    }

    private void Seleccionar_Click(object? sender, EventArgs e)
    {
        if (!_puedeAdministrar || !ConfirmarSalida())
            return;
        switch (dgvRegistros.CurrentRow?.DataBoundItem)
        {
            case Cliente c:
                _id = c.Id;
                ctNombre.Text = c.Nombre;
                ctTelefono.Text = c.Telefono;
                ctCorreo.Text = c.Correo;
                break;
            case ElementoMenu p:
                _id = p.Id;
                ctNombre.Text = p.Nombre;
                ctCategoria.Text = p.Categoria;
                nudPrecio.Value = p.Precio;
                cbTipo.SelectedItem = p.Tipo;
                chkAlcoholica.Checked = p is Bebida { Alcoholica: true };
                break;
        }

        _sucio = false;
        lblEstadoEdicion.Text = _id.HasValue ? "Editar registro" : "Nuevo registro";
    }

    private async void Guardar_Click(object? sender, EventArgs e)
    {
        var entidad = cbModo.Text;
        var id = _id;
        var n = ctNombre.Text;
        var tel = ctTelefono.Text;
        var mail = ctCorreo.Text;
        var cat = ctCategoria.Text;
        var t = cbTipo.Text;
        var p = nudPrecio.Value;
        var a = chkAlcoholica.Checked;
        var u = ctUbicacion.Text;
        var guardado = false;
        await EjecutarAsync(async () =>
        {
            await Task.Run(() =>
            {
                if (entidad == "Clientes")
                    _repositorio.GuardarCliente(id, n, tel, mail);
                else if (entidad == "Productos")
                    _repositorio.GuardarProducto(id, n, cat, t, p, a);
                else
                    _repositorio.GuardarRestaurante(n, u);
            });
            guardado = true;
        });
        if (guardado)
        {
            _sucio = false;
            _dialogos.Mostrar(this, "Datos guardados.", "Catálogos");
            await CargarAsync();
        }
    }

    private async void Desactivar_Click(object? sender, EventArgs e)
    {
        if (!ConfirmarSalida())
            return;
        _sucio = false;
        Seleccionar_Click(sender, e);
        if (_id is null)
            return;
        if (_dialogos.Mostrar(this, "¿Desactivar el registro? Su historial se conservará.", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;
        var id = _id.Value;
        var entidad = cbModo.Text;
        await EjecutarAsync(() => Task.Run(() =>
        {
            if (entidad == "Clientes")
                _repositorio.DesactivarCliente(id);
            else
                _repositorio.DesactivarProducto(id);
        }));
        await CargarAsync();
    }
}
