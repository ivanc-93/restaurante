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
            grupos[nombre]=nombre.StartsWith("Formulario")||nombre is "Program" or "Componentes" or "OpcionFiltro" or "CicloSesion" or "DialogosUsuario" or "IDialogosUsuario" or "DiarioOperaciones" or "OperacionPendienteLocal"?3:
                nombre.StartsWith("IRepositorio")||nombre.Contains("Conexion")||nombre is "RepositorioRestaurante" or "ReglasRestaurante" or "MonedaGuatemala" or "RelojGuatemala"?2:0;
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
        var tablas=Regex.Matches(fuente,@"(?ms)^    (\w+) \{(.*?)^    \}").Cast<Match>().ToArray();
        var posiciones=new Dictionary<string,Rectangle>();
        int y=120;
        for(int inicio=0;inicio<tablas.Length;inicio+=3)
        {
            int alto=0;
            for(int i=inicio;i<Math.Min(inicio+3,tablas.Length);i++)
            {
                int campos=tablas[i].Groups[2].Value.Split('\n').Count(s=>!string.IsNullOrWhiteSpace(s));
                var rect=new Rectangle(35+(i-inicio)*440,y,400,75+campos*25);
                posiciones[tablas[i].Groups[1].Value]=rect;alto=Math.Max(alto,rect.Height);
            }
            y+=alto+90;
        }
        using var imagen=new Bitmap(1360,y+100);using var g=Graphics.FromImage(imagen);
        Preparar(g,"SQL Server · esquema 4 · cobros académicos");
        using var linea=new Pen(Color.FromArgb(150,170,180),1.5f);linea.CustomEndCap=new AdjustableArrowCap(4,5);
        foreach(Match relacion in Regex.Matches(fuente,@"(?m)^    (\w+)\s+[^\r\n]*?--[^\r\n]*?\s+(\w+)\s+:"))
        {
            if(posiciones.TryGetValue(relacion.Groups[1].Value,out var origen)&&posiciones.TryGetValue(relacion.Groups[2].Value,out var destino))
                g.DrawLine(linea,origen.Right,origen.Top+origen.Height/2,destino.Left,destino.Top+destino.Height/2);
        }
        using var titulo=new Font("Segoe UI",15,FontStyle.Bold);using var texto=new Font("Consolas",11);
        foreach(var tabla in tablas)
        {
            var r=posiciones[tabla.Groups[1].Value];g.FillRectangle(Brushes.White,r);g.DrawRectangle(Pens.SlateGray,r);
            g.FillRectangle(Brushes.DarkSlateGray,r.X,r.Y,r.Width,42);g.DrawString(tabla.Groups[1].Value,titulo,Brushes.White,r.X+10,r.Y+6);
            int fila=r.Y+55;
            foreach(var campo in tabla.Groups[2].Value.Split('\n').Select(x=>x.Trim()).Where(x=>x.Length>0))
            {g.DrawString(campo,texto,Brushes.Black,r.X+10,fila);fila+=25;}
        }
        using var nota=new Font("Segoe UI",12);
        g.DrawString("Esquema principal abreviado. PK: primaria · FK: foránea · UK: única. Fuente: DiagramaBaseDatos.mmd",nota,Brushes.DimGray,35,y+15);
        g.DrawString("Pagos manuales y comprobantes internos; sin integración SAT/FEL ni bancaria.",nota,Brushes.DimGray,35,y+45);
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
