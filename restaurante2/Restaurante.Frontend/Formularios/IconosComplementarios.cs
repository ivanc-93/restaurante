using System.Drawing.Drawing2D;

namespace Restaurante.Frontend.Formularios;

// Simple original vector strokes complement the existing Lucide family; text stays separate.
internal static class IconosComplementarios
{
    internal static Bitmap? Dibujar(string nombre,int tamano)
    {
        if(nombre is not("brush" or "user-minus" or "calendar" or "alert" or "refresh" or "save"))return null;
        var imagen=new Bitmap(tamano,tamano);
        using var g=Graphics.FromImage(imagen);g.SmoothingMode=SmoothingMode.AntiAlias;g.ScaleTransform(tamano/24f,tamano/24f);
        using var p=new Pen(Componentes.Verde,1.8f){StartCap=LineCap.Round,EndCap=LineCap.Round,LineJoin=LineJoin.Round};
        switch(nombre)
        {
            case "brush":g.DrawLine(p,16,3,10,13);g.DrawPolygon(p,(Point[])[new(8,11),new(15,15),new(12,21),new(3,17)]);g.DrawLine(p,7,14,5,18);break;
            case "user-minus":g.DrawEllipse(p,6,3,7,7);g.DrawArc(p,3,13,13,12,180,180);g.DrawLine(p,16,12,22,12);break;
            case "calendar":g.DrawRectangle(p,3,5,18,16);g.DrawLine(p,3,10,21,10);g.DrawLine(p,7,3,7,7);g.DrawLine(p,17,3,17,7);break;
            case "alert":g.DrawPolygon(p,(Point[])[new(12,3),new(22,21),new(2,21)]);g.DrawLine(p,12,9,12,14);g.DrawEllipse(p,11.5f,17,1,1);break;
            case "refresh":g.DrawArc(p,4,4,16,16,45,285);g.DrawLines(p,(Point[])[new(20,3),new(20,9),new(14,9)]);break;
            case "save":g.DrawPolygon(p,(Point[])[new(3,3),new(17,3),new(21,7),new(21,21),new(3,21)]);g.DrawRectangle(p,7,3,8,6);g.DrawRectangle(p,7,14,10,7);break;
        }
        return imagen;
    }
}
