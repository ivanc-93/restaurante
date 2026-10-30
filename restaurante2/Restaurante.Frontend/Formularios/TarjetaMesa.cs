using Restaurante.Backend.Dominio;

namespace Restaurante.Frontend.Formularios;

internal sealed class TarjetaMesa : UserControl
{
    internal Mesa Mesa { get; }
    internal Button Accion { get; }
    internal TarjetaMesa(Mesa mesa, PedidoResumen? pedido)
    {
        Mesa=mesa; Name="mesa_"+mesa.Id; AccessibleName=$"Mesa {mesa.Numero}, {mesa.Estado}, capacidad {mesa.Capacidad}";
        AutoScaleMode=AutoScaleMode.Dpi;AutoScaleDimensions=new SizeF(96,96);
        BackColor=Color.White;Font=Componentes.Fuente;Size=new Size(270,270);Margin=new Padding(0,0,16,16);
        BorderStyle=BorderStyle.FixedSingle;
        var estilo=EstadoVisualMesa.De(mesa.Estado);
        var cuerpo=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=5,Padding=new Padding(14)};
        cuerpo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        cuerpo.RowStyles.Add(new RowStyle(SizeType.Absolute,36));
        cuerpo.RowStyles.Add(new RowStyle(SizeType.Absolute,64));
        cuerpo.RowStyles.Add(new RowStyle(SizeType.Absolute,32));
        cuerpo.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        cuerpo.RowStyles.Add(new RowStyle(SizeType.Absolute,52));
        var estado=new Label{Text="  "+mesa.Estado,Dock=DockStyle.Fill,BackColor=estilo.Fondo,ForeColor=estilo.Tinta,TextAlign=ContentAlignment.MiddleLeft,Font=new Font("Segoe UI",12,FontStyle.Bold),Image=Iconos.Obtener(estilo.Icono),ImageAlign=ContentAlignment.MiddleRight,Padding=new Padding(4)};
        var numero=new Label{Text="Mesa "+mesa.Numero,Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft,Font=new Font("Segoe UI",25,FontStyle.Bold),ForeColor=Componentes.Tinta};
        var capacidad=new Label{Text=$"Capacidad: {mesa.Capacidad} personas",Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft};
        var cuenta=new Label{Text=pedido is null?"Sin pedido abierto":$"Pedido #{pedido.Id} · {FormatoPresentacion.Moneda(pedido.Total)}",Dock=DockStyle.Fill,ForeColor=Componentes.Secundario,AutoEllipsis=true};
        Accion=Componentes.Boton(mesa.Estado=="Ocupada"&&pedido is null?"Gestionar estado":estilo.Accion);
        Accion.Name="accionMesa";Accion.Dock=DockStyle.Fill;Accion.Margin=new Padding(0,4,0,0);Accion.BackColor=estilo.Tinta;Accion.AccessibleName=$"{Accion.Text}, mesa {mesa.Numero}";
        Iconos.Aplicar(Accion,estilo.Icono);
        cuerpo.Controls.Add(estado,0,0);cuerpo.Controls.Add(numero,0,1);cuerpo.Controls.Add(capacidad,0,2);cuerpo.Controls.Add(cuenta,0,3);cuerpo.Controls.Add(Accion,0,4);
        Controls.Add(cuerpo);
    }
}
