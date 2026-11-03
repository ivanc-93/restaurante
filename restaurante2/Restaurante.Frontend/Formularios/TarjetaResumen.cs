namespace Restaurante.Frontend.Formularios;
internal sealed class TarjetaResumen : Control
{
    private readonly string titulo,valor,detalle;private readonly Color acento;
    internal TarjetaResumen(string titulo,string valor,string detalle,Color acento)
    {
        this.titulo=titulo;this.valor=valor;this.detalle=detalle;this.acento=acento;
        Size=new Size(235,150);Margin=new Padding(0,0,14,14);AccessibleName=$"{titulo}: {valor}. {detalle}";
        SetStyle(ControlStyles.UserPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.AllPaintingInWmPaint|ControlStyles.ResizeRedraw,true);
    }
    protected override void OnParentChanged(EventArgs e)
    {
        base.OnParentChanged(e); AjustarEscala();
    }
    protected override void OnDpiChangedAfterParent(EventArgs e)
    {
        base.OnDpiChangedAfterParent(e); AjustarEscala();
    }
    private void AjustarEscala()
    {
        float s=DeviceDpi/96f;Size=new Size((int)(235*s),(int)(150*s));Margin=new Padding(0,0,(int)(14*s),(int)(14*s));
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        var g=e.Graphics;g.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;float s=DeviceDpi/96f;g.Clear(Parent?.BackColor??Componentes.Fondo);
        IlustracionesSabores.Caja(g,Color.White,0,0,Width-1,Height-1,12*s);
        IlustracionesSabores.Caja(g,acento,0,17*s,4*s,34*s,2*s);
        using var ft=new Font("Segoe UI",9,FontStyle.Bold);using var fv=new Font("Segoe UI",25,FontStyle.Bold);using var fd=new Font("Segoe UI",10);
        TextRenderer.DrawText(g,titulo,ft,new Rectangle((int)(18*s),(int)(20*s),Width-(int)(32*s),(int)(24*s)),Componentes.Secundario,TextFormatFlags.EndEllipsis);
        TextRenderer.DrawText(g,valor,fv,new Rectangle((int)(17*s),(int)(49*s),Width-(int)(30*s),(int)(52*s)),acento,TextFormatFlags.EndEllipsis);
        TextRenderer.DrawText(g,detalle,fd,new Rectangle((int)(18*s),(int)(112*s),Width-(int)(30*s),(int)(25*s)),Componentes.Secundario,TextFormatFlags.EndEllipsis);
    }
}
