using Restaurante.Backend.Servicios;
namespace Restaurante.Frontend.Formularios;

public sealed partial class FormularioReportes : Form
{
    private readonly IRepositorioRestaurante _repositorio=null!;
    private bool _listo;
    private bool _cargando;
    public FormularioReportes() => InitializeComponent();
    public FormularioReportes(IRepositorioRestaurante repositorio) : this()
    {
        _repositorio=repositorio;
        Componentes.EstilizarTabla(ventas); Componentes.EstilizarTabla(platos); Componentes.EstilizarTabla(pedidos);
        inicio.Value=DateTime.Today.AddDays(-30); fin.Value=DateTime.Today;
        agrupacion.SelectedIndex=0;
        Pestana_Cambio(this,EventArgs.Empty);
                Shown+=async (_,_)=>
        {
            filtros.Enabled=false;
            try
            {
                var opciones=await Task.Run(()=> (Mesas:_repositorio.ListarMesas(),Clientes:_repositorio.ListarClientesHistoricos()));
                mesa.DataSource=new List<OpcionFiltro>{new(null,"Todas las mesas")}.Concat(opciones.Mesas.Select(m=>new OpcionFiltro(m.Id,$"Mesa {m.Numero}"))).ToList();
                cliente.DataSource=new List<OpcionFiltro>{new(null,"Todos los clientes"),new(0,"Público general")}.Concat(opciones.Clientes.Select(c=>new OpcionFiltro(c.Id,c.Nombre))).ToList();
                _listo=true;
                await CargarAsync();
            }
            catch(Exception ex) { MessageBox.Show(this,ex.Message,"Filtros de reportes",MessageBoxButtons.OK,MessageBoxIcon.Error); }
            finally {if(!IsDisposed) filtros.Enabled=true;}
        };
    }
    private void Pestana_Cambio(object? sender, EventArgs e)
    {
        agrupacion.Visible=pestanas.SelectedIndex==0;
        mesa.Visible=mesaLabel.Visible=cliente.Visible=clienteLabel.Visible=pestanas.SelectedIndex==2;
        total.Visible=pestanas.SelectedIndex==0;
    }
    private async void Consultar_Click(object? sender,EventArgs e) => await CargarAsync();
    private async Task CargarAsync()
    {
        if(_repositorio is null || !_listo || _cargando) return;
        _cargando=true;
        var desde=inicio.Value; var hasta=fin.Value; var grupo=agrupacion.Text;
        int? mesaId=(mesa.SelectedItem as OpcionFiltro)?.Id;
        int? clienteId=(cliente.SelectedItem as OpcionFiltro)?.Id;
        filtros.Enabled=false; UseWaitCursor=true;
        try
        {
            ReglasRestaurante.Fechas(desde,hasta);
            var datos=await Task.Run(()=> (
                Ventas:_repositorio.ReportarVentasPeriodo(desde,hasta,grupo),
                Platos:_repositorio.ReportarPlatosMasVendidos(desde,hasta),
                Pedidos:_repositorio.ReportarPedidos(desde,hasta,mesaId,clienteId)));
            ventas.DataSource=datos.Ventas.ToList(); platos.DataSource=datos.Platos.ToList(); pedidos.DataSource=datos.Pedidos.ToList();
            total.Text=$"Ventas facturadas: {FormatoPresentacion.Moneda(datos.Ventas.Sum(x=>x.Total))} · {datos.Ventas.Sum(x=>x.Facturas)} facturas. Semana: lunes a domingo.";
            FormatoPresentacion.Aplicar(this);
        }
        catch(Exception ex) { MessageBox.Show(this,ex.Message,"Reportes",MessageBoxButtons.OK,MessageBoxIcon.Error); }
        finally { _cargando=false; if(!IsDisposed) { filtros.Enabled=true; UseWaitCursor=false; } }
    }
}
