using System.Reflection;

namespace Restaurante.Frontend.Presentacion;
internal static class Iconos
{
    // Shared immutable images, retained for the application lifetime.
    private static readonly Dictionary<string, Image> Cache = new();
    private static readonly HashSet<string> FaltantesInformados = new();
    public static Image? Obtener(string nombre, int dpi = 96, bool blanco = false)
    {
        var tamano = dpi > 144 ? 48 : dpi > 96 ? 32 : 24;
        var clave = $"{nombre}.{tamano}.{blanco}";
        if (Cache.TryGetValue(clave, out var imagen))
            return imagen;
        var ensamblado = Assembly.GetExecutingAssembly();
        var ruta = ensamblado.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith($".{nombre}.png") && (n.Contains($".{tamano}.") || n.Contains($"._{tamano}.")));
        using var recurso = ruta is null ? null : ensamblado.GetManifestResourceStream(ruta);
        using var original = recurso is null ? CrearAlternativa(nombre, tamano) : Image.FromStream(recurso);
        var resultado = new Bitmap(original);
        if (blanco)
        {
            using var grafico = Graphics.FromImage(resultado);
            using var atributos = new System.Drawing.Imaging.ImageAttributes();
            atributos.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix(new float[][] { [0, 0, 0, 0, 0], [0, 0, 0, 0, 0], [0, 0, 0, 0, 0], [0, 0, 0, 1, 0], [1, 1, 1, 0, 1] }));
            grafico.Clear(Color.Transparent);
            grafico.DrawImage(original, new Rectangle(0, 0, resultado.Width, resultado.Height), 0, 0, original.Width, original.Height, GraphicsUnit.Pixel, atributos);
        }

        return Cache[clave] = resultado;
    }

    private static Image CrearAlternativa(string nombre, int tamano)
    {
        var complemento = IconosComplementarios.Dibujar(nombre, tamano);
        if (complemento is not null)
            return complemento;
        // No interrumpir al operador por una clave desconocida; informar solo una vez.
        if (FaltantesInformados.Add(nombre))
            System.Diagnostics.Trace.TraceWarning("Icono no encontrado: {0}. Se utiliza alerta visual.", nombre);
        return IconosComplementarios.Dibujar("alert", tamano)!;
    }

    public static void Aplicar(Button boton, string nombre)
    {
        boton.Image = Obtener(nombre, boton.DeviceDpi, boton.ForeColor == Color.White);
        boton.ImageAlign = ContentAlignment.MiddleLeft;
        boton.TextImageRelation = TextImageRelation.ImageBeforeText;
        boton.Padding = new Padding(8, 0, 8, 0);
    }
}
