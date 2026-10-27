using Restaurante.Backend.Dominio;

namespace Restaurante.Frontend.Formularios;

public sealed partial class FormularioPrincipal
{
    private async Task MostrarMesasPos()
    {
        _titulo.Text = "Servicio / Mesas";
        LimpiarContenido();
        var pagina = CrearPagina();
        pagina.Controls.Add(Encabezado("Mesas", "Selecciona una mesa para abrir o continuar su pedido."));
        var herramientas = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true };
        var buscar = new TextBox { Width = 170, PlaceholderText = "Buscar mesa", Margin = new Padding(0, 10, 12, 10) };
        var filtro = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 160, Margin = new Padding(0, 10, 12, 10) };
        filtro.Items.AddRange(["Todas", "Disponible", "Ocupada", "Reservada", "En limpieza"]); filtro.SelectedIndex = 0;
        var actualizar = Componentes.Boton("Actualizar", false);
        var administrar = Componentes.Boton("Administrar mesas", false);
        administrar.Click += async (_, _) => await EjecutarSeguro(MostrarMenuMesas);
        herramientas.Controls.AddRange([buscar, filtro, actualizar, administrar]);
        pagina.Controls.Add(herramientas);
        var tarjetas = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true };
        pagina.Controls.Add(tarjetas);
        var mesas = await Task.Run(_repositorio.ListarMesas);
        var pedidos = await Task.Run(_repositorio.ListarPedidos);
        void Dibujar()
        {
            tarjetas.SuspendLayout();
            while (tarjetas.Controls.Count > 0) tarjetas.Controls[0].Dispose();
            foreach (var mesa in mesas.Where(m => (filtro.SelectedIndex == 0 || m.Estado == filtro.Text)
                && m.Numero.ToString().Contains(buscar.Text.Trim())))
            {
                var pedido = pedidos.FirstOrDefault(p => p.NumeroMesa == mesa.Numero && p.Estado is "Abierto" or "En preparación");
                var boton = Componentes.Boton($"Mesa {mesa.Numero} · {mesa.Capacidad} personas\n{mesa.Estado}\n" +
                    (pedido is null ? "Sin pedido abierto" : $"Pedido #{pedido.Id} · {FormatoPresentacion.Moneda(pedido.Total)}") +
                    (mesa.Estado == "Disponible" ? "\nAbrir pedido" : pedido is null ? "" : "\nVer pedido"), false);
                boton.AutoSize = false; boton.Size = new Size((int)(240 * DeviceDpi / 96f), (int)(155 * DeviceDpi / 96f)); boton.Margin = new Padding(0, 0, 14, 14);
                boton.TextAlign = ContentAlignment.MiddleLeft;
                boton.BackColor = mesa.Estado == "Disponible" ? Color.FromArgb(234, 244, 235) :
                    mesa.Estado == "Ocupada" ? Color.FromArgb(255, 244, 219) : Color.FromArgb(235, 239, 244);
                boton.AccessibleName = $"Mesa {mesa.Numero}, {mesa.Estado}";
                Iconos.Aplicar(boton, "armchair");
                boton.Enabled = true;
                boton.Click += async (_, _) => await EjecutarSeguro(async () => { AbrirMesa(mesa); await MostrarMesasPos(); });
                tarjetas.Controls.Add(boton);
            }
            if (tarjetas.Controls.Count == 0) tarjetas.Controls.Add(Componentes.Etiqueta("No hay mesas para este filtro."));
            tarjetas.ResumeLayout(true);
        }
        buscar.TextChanged += (_, _) => Dibujar(); filtro.SelectedIndexChanged += (_, _) => Dibujar();
        actualizar.Click += async (_, _) => await EjecutarSeguro(MostrarMesasPos);
        if (InvokeRequired) Invoke(Dibujar); else Dibujar();
    }

    private void AbrirMesa(Mesa seleccionada)
    {
        // Re-read before editing: availability may have changed in another session.
        var actual = _repositorio.ListarMesas().FirstOrDefault(m => m.Id == seleccionada.Id)
            ?? throw new InvalidOperationException("La mesa ya no existe. Actualiza la vista.");
        using var detalle = new FormularioDetalleMesa(_repositorio, actual, _dialogos);
        detalle.ShowDialog(this);
    }

    private async Task MostrarPedidosPos()
    {
        _titulo.Text = "Servicio / Pedidos";
        LimpiarContenido();
        var pagina = CrearPagina();
        pagina.Controls.Add(Encabezado("Pedidos", "Continúa un pedido abierto o selecciona una mesa para uno nuevo."));
        var nuevo = Componentes.Boton("Nuevo pedido · elegir mesa");
        nuevo.AutoSize = true; Iconos.Aplicar(nuevo, "plus");
        nuevo.Click += async (_, _) => await EjecutarSeguro(MostrarMesasPos);
        pagina.Controls.Add(nuevo);
        var tabla = NuevaTabla(); tabla.Height = 380; tabla.Dock = DockStyle.Top;
        var datos = await Task.Run(_repositorio.ListarPedidos);
        tabla.DataSource = datos.OrderByDescending(p => p.Fecha).ToList();
        pagina.Controls.Add(tabla);
        var abrir = Componentes.Boton("Continuar pedido seleccionado", false); abrir.AutoSize = true;
        abrir.Click += async (_, _) => await EjecutarSeguro(async () =>
        {
            if (tabla.CurrentRow?.DataBoundItem is not PedidoResumen p || p.Estado is not ("Abierto" or "En preparación"))
                throw new InvalidOperationException("Selecciona un pedido abierto. Las facturas cerradas se consultan en Reportes.");
            var mesas = await Task.Run(_repositorio.ListarMesas);
            AbrirMesa(mesas.Single(m => m.Numero == p.NumeroMesa));
            await MostrarPedidosPos();
        });
        pagina.Controls.Add(abrir);
        var consultar=Componentes.Boton("Consultar comprobante seleccionado",false);
        consultar.Click+=(_,_)=> {
            if(tabla.CurrentRow?.DataBoundItem is PedidoResumen p && p.NumeroFactura is not null)
            {using var documento=new FormularioComprobante(_repositorio,p.Id);documento.ShowDialog(this);}
            else _dialogos.Mostrar(this,"Seleccione un pedido con comprobante emitido.","Consultar comprobante");
        };
        pagina.Controls.Add(consultar);
        if (datos.Count == 0) pagina.Controls.Add(Componentes.Etiqueta("Todavía no hay pedidos. Comienza seleccionando una mesa."));
        FormatoPresentacion.Aplicar(this);
    }

    private Task MostrarConfiguracion()
    {
        if (_usuario.Rol != "Administrador") throw new InvalidOperationException("Solo el administrador puede cambiar la configuración.");
        _titulo.Text = "Administración / Configuración";
        LimpiarContenido();
        var pagina = CrearPagina();
        pagina.Controls.Add(Encabezado("Configuración", "Información del restaurante y opciones avanzadas."));
        var restaurante = Componentes.Boton("Información del restaurante", false); restaurante.AutoSize = true;
        restaurante.Click += (_, _) => { using var formulario = new FormularioCatalogos(_repositorio, true, true); formulario.ShowDialog(this); };
        var sql = Componentes.Boton("Avanzado · Conexión SQL", false); sql.AutoSize = true; Iconos.Aplicar(sql, "database");
        sql.Click += async (_, _) => await EjecutarSeguro(MostrarConexion);
        pagina.Controls.Add(restaurante); pagina.Controls.Add(sql);
        return Task.CompletedTask;
    }
}

