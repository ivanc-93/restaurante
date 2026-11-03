using Restaurante.Backend.Dominio;
namespace Restaurante.Frontend.Formularios;
internal sealed class TarjetaMesa : UserControl
{
    internal Mesa Mesa { get; }
    internal Button Accion { get; }
    internal TarjetaMesa(Mesa mesa,PedidoResumen? pedido)
    {
        Mesa=mesa;Name="mesa_"+mesa.Id;AccessibleName=$"Mesa {mesa.Numero}, {mesa.Estado}, capacidad {mesa.Capacidad}";
        AutoScaleMode=AutoScaleMode.Dpi;AutoScaleDimensions=new SizeF(96,96);
        BackColor=Componentes.Fondo;Font=Componentes.Fuente;Size=new Size(280,340);Margin=new Padding(0,0,18,18);DoubleBuffered=true;
        var estilo=EstadoVisualMesa.De(mesa.Estado);
        var cuerpo=new TableLayoutPanel{Dock=DockStyle.Fill,BackColor=Color.White,Padding=new Padding(16),ColumnCount=1,RowCount=6,Margin=Padding.Empty};
        cuerpo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        // Text rows use their measured font height, including Windows scaling.
        for(var fila=0;fila<6;fila++)cuerpo.RowStyles.Add(new RowStyle(fila==2?SizeType.Percent:SizeType.AutoSize,fila==2?100:0));
        var estado=new Label{Text="●  "+mesa.Estado,Dock=DockStyle.Left,AutoSize=true,Padding=new Padding(9,4,9,4),BackColor=estilo.Fondo,ForeColor=estilo.Tinta,Font=new Font("Segoe UI",10,FontStyle.Bold),Margin=Padding.Empty};
        var numero=new Label{Text="Mesa "+mesa.Numero,AutoSize=true,Dock=DockStyle.Top,Font=new Font("Segoe UI",19,FontStyle.Bold),ForeColor=Componentes.Tinta,Margin=new Padding(0,6,0,6)};
        var dibujo=new Panel{Dock=DockStyle.Fill,BackColor=Color.FromArgb(249,247,241),Margin=new Padding(0,10,0,10)};
        dibujo.Paint+=(_,e)=>IlustracionesSabores.Mesa(e.Graphics,dibujo.ClientRectangle,estilo.Tinta,mesa.Capacidad);
        dibujo.Resize+=(_,_)=>dibujo.Invalidate();
        var capacidad=new Label{Text=$"{mesa.Capacidad} personas · comedor",AutoSize=true,Dock=DockStyle.Top,ForeColor=Componentes.Secundario,Font=new Font("Segoe UI",10),TextAlign=ContentAlignment.MiddleLeft};
        var cuenta=new Label{Text=pedido is null?"Sin pedido abierto":$"Pedido #{pedido.Id}  ·  {FormatoPresentacion.Moneda(pedido.Total)}",AutoSize=true,Dock=DockStyle.Top,ForeColor=Componentes.Tinta,Font=new Font("Segoe UI",11,FontStyle.Bold),AutoEllipsis=true};
        Accion=Componentes.Boton(mesa.Estado=="Ocupada"&&pedido is null?"Gestionar estado":estilo.Accion);
        Accion.AutoSize=true;Accion.AutoSizeMode=AutoSizeMode.GrowAndShrink;Accion.Padding=new Padding(10);Accion.Name="accionMesa";Accion.Dock=DockStyle.Fill;Accion.Margin=new Padding(0,6,0,0);Accion.Font=new Font("Segoe UI",10,FontStyle.Bold);Accion.BackColor=estilo.Tinta;Accion.AccessibleName=$"{Accion.Text}, mesa {mesa.Numero}";
        cuerpo.Controls.Add(estado,0,0);cuerpo.Controls.Add(numero,0,1);cuerpo.Controls.Add(dibujo,0,2);cuerpo.Controls.Add(capacidad,0,3);cuerpo.Controls.Add(cuenta,0,4);cuerpo.Controls.Add(Accion,0,5);Controls.Add(cuerpo);
    }
}
