using Restaurante.Backend.Dominio;
using System.Drawing.Drawing2D;
namespace Restaurante.Frontend.Formularios;
internal sealed class TarjetaProducto : Button
{
    private readonly ElementoMenu producto;
    private bool encima,seleccionada;
    internal int ProductoId => producto.Id;
    internal bool Seleccionada {get=>seleccionada;set{seleccionada=value;Invalidate();}}
    internal TarjetaProducto(ElementoMenu producto)
    {
        this.producto=producto;Text=producto.Nombre;AccessibleName=$"{producto.Nombre}, {producto.Tipo}, {FormatoPresentacion.Moneda(producto.Precio)}. Seleccionar producto";
        AutoSize=false;Size=new Size(220,238);Margin=new Padding(8);Cursor=Cursors.Hand;
        SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);
        BackColor=Color.White;ForeColor=Componentes.Tinta;
    }
    protected override void OnMouseEnter(EventArgs e){encima=true;Invalidate();base.OnMouseEnter(e);}
    protected override void OnMouseLeave(EventArgs e){encima=false;Invalidate();base.OnMouseLeave(e);}
    protected override void OnGotFocus(EventArgs e){Invalidate();base.OnGotFocus(e);}
    protected override void OnLostFocus(EventArgs e){Invalidate();base.OnLostFocus(e);}
    protected override void OnPaint(PaintEventArgs e)
    {
        var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;float s=DeviceDpi/96f;
        g.Clear(Parent?.BackColor??Componentes.Fondo);
        var rect=new RectangleF(1,1,Width-3,Height-3);using var path=IlustracionesSabores.Redondo(rect,12*s);
        using(var b=new SolidBrush(Color.White))g.FillPath(b,path);
        using(var p=new Pen(seleccionada||Focused?Componentes.Verde:encima?Color.FromArgb(152,174,141):Componentes.Borde,(seleccionada?2:1)*s))g.DrawPath(p,path);
        var alto=(int)(Height*.50f);IlustracionesSabores.Caja(g,producto.Tipo=="Bebida"?Color.FromArgb(235,240,223):Color.FromArgb(247,235,214),10*s,10*s,Width-20*s,alto-10*s,8*s);
        IlustracionesSabores.Producto(g,new RectangleF(12*s,14*s,Width-24*s,alto-14*s),producto.Tipo,producto.Nombre);
        using var nombre=new Font("Segoe UI",11,FontStyle.Bold);using var detalle=new Font("Segoe UI",9);using var precio=new Font("Segoe UI",12,FontStyle.Bold);
        TextRenderer.DrawText(g,producto.Nombre,nombre,new Rectangle((int)(14*s),alto+8,(int)(Width-28*s),(int)(43*s)),Componentes.Tinta,TextFormatFlags.WordBreak|TextFormatFlags.EndEllipsis);
        TextRenderer.DrawText(g,producto.Categoria,detalle,new Rectangle((int)(14*s),Height-(int)(66*s),Width-(int)(28*s),(int)(20*s)),Componentes.Secundario,TextFormatFlags.EndEllipsis);
        TextRenderer.DrawText(g,FormatoPresentacion.Moneda(producto.Precio),precio,new Rectangle((int)(14*s),Height-(int)(42*s),Width-(int)(55*s),(int)(27*s)),Componentes.Verde,TextFormatFlags.EndEllipsis);
        IlustracionesSabores.Caja(g,seleccionada?Componentes.Verde:Componentes.Seleccion,Width-40*s,Height-40*s,27*s,27*s,8*s);
        TextRenderer.DrawText(g,seleccionada?"✓":"+",precio,new Rectangle(Width-(int)(40*s),Height-(int)(40*s),(int)(27*s),(int)(27*s)),seleccionada?Color.White:Componentes.Verde,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);
        if(!Enabled)using(var disabled=new SolidBrush(Color.FromArgb(100,255,255,255)))g.FillPath(disabled,path);
    }
}
