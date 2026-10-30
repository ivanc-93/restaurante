using Restaurante.Backend.Dominio;

namespace Restaurante.Frontend.Formularios;

public sealed partial class FormularioPrincipal
{
    private async Task MostrarMesasPos()
    {
        if(InvokeRequired){await EnInterfazAsync(MostrarMesasPos);return;}
        _titulo.Text="Servicio / Mesas";SeleccionarNavegacion("Mesas");LimpiarContenido();
        var pagina=CrearPagina();
        pagina.Controls.Add(Encabezado("Cada mesa, en su momento", "Elige una mesa. Su disponibilidad física no cancela los saldos pendientes."));
        var herramientas=new FlowLayoutPanel{Dock=DockStyle.Top,AutoSize=true,WrapContents=true,Margin=new Padding(0,8,0,12)};
        var buscar=new TextBox{Name="buscarMesa",Width=230,PlaceholderText="Buscar mesa",Margin=new Padding(0,10,12,10),AccessibleName="Buscar mesa por número"};
        var filtro=new ComboBox{Name="filtroEstadoMesa",DropDownStyle=ComboBoxStyle.DropDownList,Width=190,Margin=new Padding(0,10,12,10)};
        filtro.Items.AddRange(["Todas","Disponible","Ocupada","En limpieza","Reservada"]);filtro.SelectedIndex=0;
        var actualizar=Componentes.Boton("Actualizar",false);Iconos.Aplicar(actualizar,"refresh");actualizar.Margin=new Padding(0,0,12,6);
        var administrar=Componentes.Boton("Administrar mesas",false);Iconos.Aplicar(administrar,"settings");administrar.Visible=_usuario.Rol is "Administrador" or "Gerente";
        administrar.Click+=async(_,_)=>await EjecutarSeguro(MostrarMenuMesas);
        herramientas.Controls.AddRange([buscar,filtro,actualizar,administrar]);pagina.Controls.Add(herramientas);
        var leyenda=new FlowLayoutPanel{Dock=DockStyle.Top,AutoSize=true,WrapContents=true,Margin=new Padding(0,0,0,12)};
        foreach(var estado in new[]{"Disponible","En limpieza","Ocupada","Reservada"})
        {
            var estilo=EstadoVisualMesa.De(estado);leyenda.Controls.Add(new Label{Text="  "+estado+"  ",AutoSize=true,BackColor=estilo.Fondo,ForeColor=estilo.Tinta,Margin=new Padding(0,0,12,4),Padding=new Padding(4)});
        }
        pagina.Controls.Add(leyenda);
        var tarjetas=new FlowLayoutPanel{Name="tarjetasMesas",Dock=DockStyle.Top,AutoSize=true,WrapContents=true,Margin=Padding.Empty,Padding=Padding.Empty};pagina.Controls.Add(tarjetas);
        var mesas=await Task.Run(_repositorio.ListarMesas);var pedidos=await Task.Run(_repositorio.ListarPedidos);
        void Ajustar()
        {
            var escala=DeviceDpi/96f;var disponible=Math.Max(1,tarjetas.ClientSize.Width);
            var columnas=Math.Clamp((int)(disponible/(275*escala)),1,4);
            var ancho=Math.Max((int)(240*escala),disponible/columnas-(int)(18*escala));
            foreach(var tarjeta in tarjetas.Controls.OfType<TarjetaMesa>())tarjeta.Size=new Size(ancho,(int)(270*escala));
        }
        void Dibujar()
        {
            tarjetas.SuspendLayout();while(tarjetas.Controls.Count>0)tarjetas.Controls[0].Dispose();
            foreach(var mesa in mesas.Where(m=>(filtro.SelectedIndex==0||m.Estado==filtro.Text)&&m.Numero.ToString().Contains(buscar.Text.Trim())))
            {
                var pedido=pedidos.FirstOrDefault(p=>p.NumeroMesa==mesa.Numero&&p.Estado is "Abierto" or "En preparación");
                var tarjeta=new TarjetaMesa(mesa,pedido);
                tarjeta.Accion.Click+=async(_,_)=>await EjecutarSeguro(async()=>
                {
                    var actual=(await Task.Run(_repositorio.ListarMesas)).Single(m=>m.Id==mesa.Id);
                    var abierto=await Task.Run(()=>_repositorio.ObtenerPedidoAbierto(actual.Id));
                    if(actual.Estado=="En limpieza"||actual.Estado=="Ocupada"&&abierto is null)
                    {using var gestion=new FormularioGestionMesa(_repositorio,actual,_dialogos);gestion.ShowDialog(this);}
                    else if(actual.Estado=="Reservada"&&_usuario.Rol is "Administrador" or "Gerente")
                    {await MostrarMenuMesas();return;}
                    else await AbrirMesa(actual);
                    await MostrarMesasPos();
                });
                tarjetas.Controls.Add(tarjeta);
            }
            if(tarjetas.Controls.Count==0)tarjetas.Controls.Add(Componentes.Etiqueta("No hay mesas para este filtro."));
            Ajustar();tarjetas.ResumeLayout(true);
        }
        buscar.TextChanged+=(_,_)=>Dibujar();filtro.SelectedIndexChanged+=(_,_)=>Dibujar();tarjetas.SizeChanged+=(_,_)=>Ajustar();
        actualizar.Click+=async(_,_)=>await EjecutarSeguro(MostrarMesasPos);
        if(InvokeRequired)Invoke(Dibujar);else Dibujar();
    }
    private async Task AbrirMesa(Mesa seleccionada)
    {
        // Re-read before editing: availability may have changed in another session.
        var actual = (await Task.Run(_repositorio.ListarMesas)).FirstOrDefault(m => m.Id == seleccionada.Id)
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
            await AbrirMesa(mesas.Single(m => m.Numero == p.NumeroMesa));
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
        if (_usuario.Rol is not("Administrador" or "Gerente")) throw new InvalidOperationException("Solo gerente o administrador.");
        _titulo.Text = "Administración / Configuración";
        LimpiarContenido();
        var pagina = CrearPagina();
        pagina.Controls.Add(Encabezado("Configuración", "Información del restaurante y opciones avanzadas."));
        var restaurante = Componentes.Boton("Información del restaurante", false); restaurante.AutoSize = true;
        restaurante.Click += (_, _) => { using var formulario = new FormularioCatalogos(_repositorio, true, true); formulario.ShowDialog(this); };
        var sql = Componentes.Boton("Avanzado · Conexión SQL", false); sql.AutoSize = true; Iconos.Aplicar(sql, "database");
        sql.Click += async (_, _) => await EjecutarSeguro(MostrarConexion);
        pagina.Controls.Add(restaurante); if(_usuario.Rol=="Administrador")pagina.Controls.Add(sql);
        return Task.CompletedTask;
    }
}
