using Restaurante.Backend.Dominio;
using Restaurante.Backend.Servicios;
namespace Restaurante.Frontend.Formularios;
public sealed class FormularioReactivar : Form
{
    public FormularioReactivar(IRepositorioRestaurante repo,bool productos)
    {
        Text=productos?"Productos inactivos":"Clientes inactivos";Size=new Size(760,560);MinimumSize=new Size(520,380);
        var panel=new TableLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(20),ColumnCount=1,RowCount=4};
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));panel.RowStyles.Add(new RowStyle(SizeType.Percent,100));panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var aviso=Componentes.Etiqueta("Reactiva el registro original para conservar su historial.");
        var buscar=new TextBox{Dock=DockStyle.Fill,PlaceholderText="Buscar inactivos por nombre"};
        var tabla=new DataGridView{Dock=DockStyle.Fill};Componentes.EstilizarTabla(tabla);
        var boton=Componentes.Boton("Reactivar seleccionado");var lista=new List<RegistroInactivo>();bool ocupado=false;
        void Filtrar(){tabla.DataSource=lista.Where(x=>x.Nombre.Contains(buscar.Text,StringComparison.CurrentCultureIgnoreCase)).ToList();if(tabla.Columns.Contains("Id"))tabla.Columns["Id"].Visible=false;boton.Enabled=!ocupado&&tabla.Rows.Count>0;}
        async Task Cargar(){ocupado=true;boton.Enabled=false;try{lista=(await Task.Run(()=>repo.ListarInactivos(productos))).ToList();aviso.Text=lista.Count==0?"No hay registros inactivos.":"Reactiva el registro original para conservar su historial.";}catch(Exception ex){aviso.Text=ex.Message;}finally{ocupado=false;Filtrar();}}
        buscar.TextChanged+=(_,_)=>Filtrar();Shown+=async(_,_)=>await Cargar();FormClosing+=(_,e)=>e.Cancel=ocupado;
        boton.Click+=async(_,_)=>{if(ocupado||tabla.CurrentRow?.DataBoundItem is not RegistroInactivo item)return;ocupado=true;boton.Enabled=false;try{await Task.Run(()=>repo.ReactivarRegistro(productos,item.Id));await Cargar();}catch(Exception ex){aviso.Text=ex.Message;}finally{ocupado=false;Filtrar();}};
        panel.Controls.Add(aviso,0,0);panel.Controls.Add(buscar,0,1);panel.Controls.Add(tabla,0,2);panel.Controls.Add(boton,0,3);Controls.Add(panel);FormatoPresentacion.Aplicar(this);
    }
}
