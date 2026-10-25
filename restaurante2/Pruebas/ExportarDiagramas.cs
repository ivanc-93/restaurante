using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;

internal static class ExportarDiagramas
{
    public static void Guardar(string directorio)
    {
        Clases(directorio);
        BaseDatos(directorio);
    }
    private static void Clases(string directorio)
    {
        var fuente=File.ReadAllText(Path.Combine(directorio,"DiagramaClases.mmd"));
        var nombres=Regex.Matches(fuente,@"(?m)^    class (\w+)").Select(m=>m.Groups[1].Value).ToArray();
        var grupos=new Dictionary<string,int>();
        foreach(var nombre in nombres)
            grupos[nombre]=nombre.StartsWith("Formulario")||nombre is "Program" or "Componentes" or "OpcionFiltro" or "CicloSesion" or "DialogosUsuario" or "IDialogosUsuario"?3:
                nombre.StartsWith("IRepositorio")||nombre.Contains("Conexion")||nombre is "RepositorioRestaurante" or "ReglasRestaurante"?2:0;
        var posiciones=new Dictionary<string,Rectangle>();
        int dominio=0,servicios=0,ui=0;
        foreach(var nombre in nombres)
        {
            var grupo=grupos[nombre]; int columna,fila;
            if(grupo==0){columna=dominio%2;fila=dominio/2;dominio++;}
            else if(grupo==2){columna=2;fila=servicios++;}
            else {columna=3;fila=ui++;}
            posiciones[nombre]=new Rectangle(35+columna*355,145+fila*125,300,78);
        }
        var altura=posiciones.Values.Max(r=>r.Bottom)+90;
        using var imagen=new Bitmap(1460,altura); using var g=Graphics.FromImage(imagen);
        Preparar(g,"Clases: dominio, servicios e interfaz");
        using var grupoFont=new Font("Segoe UI",15,FontStyle.Bold);
        g.DrawString("DOMINIO",grupoFont,Brushes.DarkSlateGray,35,95);
        g.DrawString("SERVICIOS",grupoFont,Brushes.DarkSlateGray,745,95);
        g.DrawString("WINDOWS FORMS",grupoFont,Brushes.DarkSlateGray,1100,95);
        using var linea=new Pen(Color.FromArgb(150,170,180),1.5f);
        linea.CustomEndCap=new AdjustableArrowCap(4,5);
        foreach(Match relacion in Regex.Matches(fuente,@"(?m)^    (\w+)\s+(<\|--|<\|\.\.|-->|\.\.>|\*--)\s+(\w+)"))
        {
            var origen=relacion.Groups[1].Value; var destino=relacion.Groups[3].Value;
            if(!posiciones.TryGetValue(origen,out var a)||!posiciones.TryGetValue(destino,out var b)) continue;
            if(relacion.Groups[2].Value.StartsWith("<")) (a,b)=(b,a);
            g.DrawLine(linea,a.Right,a.Top+a.Height/2,b.Left,b.Top+b.Height/2);
        }
        foreach(var nombre in nombres)
        {
            var rect=posiciones[nombre];
            var tipo=nombre=="ElementoMenu"?"abstracta":nombre.StartsWith("IRepositorio")||nombre=="IConfiguracionConexion"?"interfaz":grupos[nombre]==3?"presentación":grupos[nombre]==2?"servicio":"entidad / resultado";
            Caja(g,rect,nombre,tipo);
        }
        using var nota=new Font("Segoe UI",11);
        g.DrawString("Vista general. Los contratos y detalles se encuentran en DiagramaClases.mmd y en los archivos .cs.",nota,Brushes.DimGray,35,altura-45);
        imagen.Save(Path.Combine(directorio,"DiagramaClases.png"));
    }
    private static void BaseDatos(string directorio)
    {
        var fuente=File.ReadAllText(Path.Combine(directorio,"DiagramaBaseDatos.mmd"));
        using var imagen=new Bitmap(1550,1350); using var g=Graphics.FromImage(imagen);
        Preparar(g,"Base de datos: relaciones e integridad");
        var posiciones=new Dictionary<string,Rectangle>{
            ["USUARIOS"]=new(35,130,430,300),["CLIENTES"]=new(35,480,430,240),["MESAS"]=new(35,790,430,210),
            ["PEDIDOS"]=new(550,400,430,300),["MENU"]=new(1060,130,430,300),["DETALLESPEDIDO"]=new(1060,480,430,310),
            ["FACTURAS"]=new(550,830,430,300),["RESTAURANTE"]=new(1060,930,430,190)};
        using var linea=new Pen(Color.SlateGray,2);
        linea.CustomEndCap=new AdjustableArrowCap(5,6);
        foreach(var par in new[]{("CLIENTES","PEDIDOS"),("MESAS","PEDIDOS"),("PEDIDOS","DETALLESPEDIDO"),("MENU","DETALLESPEDIDO"),("PEDIDOS","FACTURAS")})
        {
            var a=posiciones[par.Item1]; var b=posiciones[par.Item2];
            g.DrawLine(linea,a.Left+a.Width/2,a.Bottom,b.Left+b.Width/2,b.Top);
        }
        using var titulo=new Font("Segoe UI",17,FontStyle.Bold); using var texto=new Font("Consolas",12);
        foreach(Match tabla in Regex.Matches(fuente,@"(?ms)^    (\w+) \{(.*?)^    \}"))
        {
            var nombre=tabla.Groups[1].Value;if(!posiciones.TryGetValue(nombre,out var r))continue;
            g.FillRectangle(Brushes.White,r); g.DrawRectangle(Pens.SlateGray,r);
            g.FillRectangle(Brushes.DarkSlateGray,r.X,r.Y,r.Width,42);
            g.DrawString(nombre,titulo,Brushes.White,r.X+12,r.Y+5);
            var y=r.Y+55;
            foreach(var campo in tabla.Groups[2].Value.Split('\n').Select(s=>s.Trim()).Where(s=>s.Length>0))
            {g.DrawString(campo,texto,Brushes.Black,r.X+12,y);y+=25;}
        }
        using var nota=new Font("Segoe UI",13);
        g.DrawString("PK: primaria · FK: foránea · UK: única. Restaurante contiene una fila (Id=1).",nota,Brushes.DimGray,35,1230);
        g.DrawString("Un pedido puede tener una factura. Los detalles conservan nombre, tipo y precio capturados al agregar el producto.",nota,Brushes.DimGray,35,1265);
        imagen.Save(Path.Combine(directorio,"DiagramaBaseDatos.png"));
    }
    private static void Preparar(Graphics g,string titulo)
    {
        g.Clear(Color.FromArgb(245,248,246));g.SmoothingMode=SmoothingMode.AntiAlias;
        using var fuente=new Font("Segoe UI",25,FontStyle.Bold);g.DrawString(titulo,fuente,Brushes.DarkSlateGray,35,25);
    }
    private static void Caja(Graphics g,Rectangle r,string nombre,string tipo)
    {
        g.FillRectangle(Brushes.White,r);g.DrawRectangle(Pens.DarkSlateGray,r);
        using var titulo=new Font("Segoe UI",12,FontStyle.Bold);using var detalle=new Font("Segoe UI",11);
        g.DrawString(nombre,titulo,Brushes.DarkSlateGray,r.X+8,r.Y+12);
        g.DrawString(tipo,detalle,Brushes.DimGray,r.X+8,r.Y+43);
    }
}
