using System.Drawing.Drawing2D;
namespace Restaurante.Frontend.Presentacion;
// Original local vector artwork. No external images, downloads or runtime dependencies.
internal static class IlustracionesSabores
{
    internal static GraphicsPath Redondo(RectangleF r, float radio)
    {
        var p = new GraphicsPath(); var d = Math.Min(radio * 2, Math.Min(r.Width, r.Height));
        if (d <= 0) { p.AddRectangle(r); return p; }
        p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90); p.CloseFigure(); return p;
    }
    internal static void Caja(Graphics g, Color color, float x, float y, float w, float h, float radio = 10)
    { using var b = new SolidBrush(color); using var p = Redondo(new(x, y, w, h), radio); g.FillPath(b, p); }
    internal static void Ovalo(Graphics g, Color color, float x, float y, float w, float h)
    { using var b = new SolidBrush(color); g.FillEllipse(b, x, y, w, h); }
    internal static void Mesa(Graphics g, RectangleF espacio, Color acento, int capacidad)
    {
        var s = g.Save(); float escala = Math.Min(espacio.Width / 260, espacio.Height / 150); g.TranslateTransform(espacio.X + (espacio.Width - 260 * escala) / 2, espacio.Y + (espacio.Height - 150 * escala) / 2); g.ScaleTransform(escala, escala); g.SmoothingMode = SmoothingMode.AntiAlias;
        Ovalo(g, Color.FromArgb(25, 48, 78, 59), 46, 119, 174, 19);
        var asiento = Color.FromArgb(180, acento);
        Caja(g, asiento, 69, 8, 44, 32, 9); Caja(g, asiento, 147, 8, 44, 32, 9);
        Caja(g, asiento, 69, 112, 44, 32, 9); Caja(g, asiento, 147, 112, 44, 32, 9);
        if (capacidad > 4) { Caja(g, asiento, 22, 56, 32, 42, 9); Caja(g, asiento, 206, 56, 32, 42, 9); }
        Caja(g, Color.FromArgb(150, 116, 77), 48, 29, 166, 92, 17);
        Caja(g, Color.FromArgb(218, 190, 149), 46, 24, 168, 92, 17);
        using (var vetas = new Pen(Color.FromArgb(45, 139, 102, 62), 1.5f)) { g.DrawLine(vetas, 66, 45, 194, 45); g.DrawLine(vetas, 61, 77, 199, 77); g.DrawLine(vetas, 68, 98, 190, 98); }
        Caja(g, Color.FromArgb(239, 232, 212), 116, 25, 28, 89, 2);
        foreach (var x in new[] { 78, 182 }) { Ovalo(g, Color.FromArgb(120, 156, 131, 96), x - 19, 52, 39, 39); Ovalo(g, Color.White, x - 20, 49, 38, 38); using var p = new Pen(Color.FromArgb(225, 227, 215), 2); g.DrawEllipse(p, x - 14, 55, 26, 26); Ovalo(g, Color.FromArgb(200, 230, 230, 218), x + 15, 39, 11, 11); }
        Ovalo(g, Color.FromArgb(187, 139, 94), 122, 58, 17, 22); Ovalo(g, Color.FromArgb(67, 105, 72), 115, 47, 15, 23); Ovalo(g, Color.FromArgb(105, 137, 87), 129, 44, 13, 25); Ovalo(g, Color.FromArgb(53, 87, 63), 126, 50, 16, 19);
        g.Restore(s);
    }
    internal static void Producto(Graphics g, RectangleF espacio, string tipo, string nombre)
    {
        var s = g.Save(); var escala = Math.Min(espacio.Width / 230, espacio.Height / 132); g.TranslateTransform(espacio.X + (espacio.Width - 230 * escala) / 2, espacio.Y + (espacio.Height - 132 * escala) / 2); g.ScaleTransform(escala, escala); g.SmoothingMode = SmoothingMode.AntiAlias;
        var cafe = nombre.Contains("café", StringComparison.OrdinalIgnoreCase) || nombre.Contains("cafe", StringComparison.OrdinalIgnoreCase);
        var postre = nombre.Contains("pastel", StringComparison.OrdinalIgnoreCase) || nombre.Contains("postre", StringComparison.OrdinalIgnoreCase);
        Ovalo(g, Color.FromArgb(18, 48, 78, 59), 50, 109, 134, 15);
        if (tipo == "Bebida" && cafe)
        {
            Ovalo(g, Color.FromArgb(223, 207, 183), 53, 86, 124, 32); Ovalo(g, Color.FromArgb(250, 246, 234), 57, 82, 116, 30);
            using (var p = new Pen(Color.FromArgb(199, 158, 112), 10)) { g.DrawEllipse(p, 145, 39, 30, 35); }
            Caja(g, Color.FromArgb(206, 162, 114), 76, 34, 76, 63, 22); Ovalo(g, Color.FromArgb(246, 227, 198), 76, 25, 76, 27); Ovalo(g, Color.FromArgb(80, 51, 34), 83, 30, 62, 16);
            using var vapor = new Pen(Color.FromArgb(130, 154, 143, 119), 3); g.DrawBezier(vapor, 98, 22, 85, 10, 110, 12, 98, 1); g.DrawBezier(vapor, 125, 20, 112, 8, 136, 10, 123, 0);
        }
        else if (tipo == "Bebida")
        {
            Caja(g, Color.FromArgb(220, 230, 215), 78, 16, 77, 101, 15); Caja(g, Color.FromArgb(205, 223, 140), 83, 41, 67, 70, 11);
            using var pajita = new Pen(Componentes.Verde, 5); g.DrawLines(pajita, new[] { new Point(122, 90), new Point(131, 10), new Point(149, 3) });
            Caja(g, Color.FromArgb(180, 255, 255, 240), 91, 47, 20, 18, 4); Caja(g, Color.FromArgb(180, 255, 255, 240), 120, 69, 19, 18, 4);
            Ovalo(g, Color.FromArgb(111, 145, 65), 60, 28, 43, 43); Ovalo(g, Color.FromArgb(224, 232, 148), 64, 32, 35, 35);
            using var p = new Pen(Color.FromArgb(250, 249, 207), 2); g.DrawLine(p, 81, 33, 81, 66); g.DrawLine(p, 65, 49, 97, 49); g.DrawLine(p, 69, 37, 92, 61);
        }
        else if (postre)
        {
            Ovalo(g, Color.White, 48, 85, 140, 34); Caja(g, Color.FromArgb(110, 67, 46), 79, 44, 82, 52, 4); Caja(g, Color.FromArgb(241, 206, 159), 79, 67, 82, 10, 0); Caja(g, Color.FromArgb(75, 45, 32), 77, 36, 86, 16, 6);
            Ovalo(g, Componentes.Terracota, 111, 21, 21, 24); Ovalo(g, Componentes.Verde, 120, 17, 13, 8);
        }
        else
        {
            Ovalo(g, Color.FromArgb(218, 222, 203), 49, 14, 134, 105); Ovalo(g, Color.White, 46, 10, 134, 105); Ovalo(g, Color.FromArgb(242, 244, 234), 59, 21, 108, 82);
            Caja(g, Color.FromArgb(196, 126, 72), 88, 40, 59, 39, 13); using (var grill = new Pen(Color.FromArgb(145, 83, 45), 3)) for (int i = 0; i < 4; i++) g.DrawLine(grill, 98 + i * 12, 45, 93 + i * 12, 70);
            foreach (var p in new[] { new Point(71, 48), new Point(74, 67), new Point(90, 82) }) { Ovalo(g, Color.FromArgb(105, 142, 72), p.X, p.Y, 24, 18); Ovalo(g, Color.FromArgb(155, 177, 96), p.X + 5, p.Y - 4, 17, 19); }
            Ovalo(g, Componentes.Terracota, 126, 82, 15, 13); Ovalo(g, Color.FromArgb(216, 146, 92), 145, 73, 13, 14);
            using var cubiertos = new Pen(Componentes.Verde, 4); g.DrawLine(cubiertos, 29, 35, 29, 98); g.DrawLine(cubiertos, 21, 32, 21, 51); g.DrawLine(cubiertos, 37, 32, 37, 51); g.DrawArc(cubiertos, 21, 43, 16, 17, 0, 180); g.DrawLine(cubiertos, 198, 32, 198, 98); g.DrawLine(cubiertos, 195, 32, 190, 59);
        }
        g.Restore(s);
    }
}
