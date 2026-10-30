using System.Reflection;
using Restaurante.Backend.Servicios;
using Restaurante.Frontend.Formularios;
using Restaurante.Frontend.Servicios;

internal static class Program
{
    private static int aprobadas;
    [STAThread]
    private static int Main(string[] args)
    {
        Application.EnableVisualStyles();
        if(args.Length==2&&args[0]=="--primera-instalacion")
        {
            try{return PruebasPrimeraInstalacion.Ejecutar(args[1]);}
            catch(Exception e){Console.Error.WriteLine(e.GetType().Name+": "+e.Message);return 1;}
        }
        if(args.Length==2&&args[0]=="--primera-interfaz")
        {
            try{return PruebasInterfazInicial.Ejecutar(args[1]);}
            catch(Exception e){Console.Error.WriteLine(e.GetType().Name+": "+e.Message);return 1;}
        }
        if (args.SequenceEqual(new[] { "--dependencia-ausente" }))
        {
            try
            {
                using var c = new ConfiguracionConexionTemporal(@"Server=(localdb)\MSSQLLocalDB;Database=RestauranteDB;Integrated Security=True;Encrypt=True;TrustServerCertificate=True").CrearConexion();
                c.Open();
                Console.Error.WriteLine("La dependencia sigue disponible: no se reprodujo el fallo.");
                return 1;
            }
            catch (Exception e)
            {
                var mensaje = ErroresConexion.Describir(e);
                Console.WriteLine("FALLO NATIVO REAL: " + mensaje);
                return mensaje.Contains("componente local") ? 0 : 1;
            }
        }
        var ruta = Path.Combine(Path.GetTempPath(), "SaboresConexion_" + Guid.NewGuid().ToString("N"), "conexion.bin");
        try
        {
            var cfg = new ConfiguracionConexion(ruta);
            cfg.ProbarYGuardar("localhost", 0, "RestauranteDB", "", "", true);
            var original = File.ReadAllBytes(ruta);
            Probar(cfg, "localhost", "RestauranteDB", "Conexión correcta");
            Probar(cfg, "localhost", "Sabores_NoExiste_" + Guid.NewGuid().ToString("N"), "Puede no existir");
            Probar(cfg, "tcp:127.0.0.1", "RestauranteDB", "", 1);
            Probar(cfg, "localhost", "master", "esquema incompatible");
            foreach (var host in new[] { "localhost", @"equipo\INSTANCIA", @"(localdb)\MSSQLLocalDB", "tcp:equipo,1500" })
                Ok(new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(ConfiguracionConexion.CrearCadena(host,0,"RestauranteDB","","",true)).DataSource == host, "Servidor automático sin imponer puerto: " + host);
            Ok(new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(ConfiguracionConexion.CrearCadena("equipo",1500,"RestauranteDB","","",true)).DataSource == "equipo,1500", "Puerto explícito conservado");
            Ok(original.SequenceEqual(File.ReadAllBytes(ruta)), "Los fallos y las pruebas conservan la configuración cifrada");
            Ok(ErroresConexion.Describir(new TypeInitializationException("SQL", new DllNotFoundException("secreto"))).Contains("componente local"), "SIMULADO: dependencia nativa no disponible, incluso envuelta");
            Ok(ErroresConexion.Describir(new BadImageFormatException()).Contains("arquitectura"), "SIMULADO: arquitectura incompatible");
            Ok(ErroresConexion.Describir(new TimeoutException()).Contains("tiempo de espera"), "SIMULADO: tiempo agotado");
            Ok(ErroresConexion.Describir(new UnauthorizedAccessException()).Contains("autorizar"), "SIMULADO: permiso insuficiente");
            var secreto = "SecretoNoRegistrar_" + Guid.NewGuid().ToString("N");
            var referencia = ErroresConexion.Registrar(new DllNotFoundException("Password=" + secreto + ";Token=" + secreto));
            var archivo = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SaboresRestaurante", "Diagnosticos", "conexion-" + DateTime.UtcNow.ToString("yyyyMMdd") + ".jsonl");
            var contenido = File.ReadAllText(archivo);
            Ok(!contenido.Contains(secreto) && referencia.Contains("Diagnóstico:"), "Diagnóstico conserva referencia y omite contenido sensible de la excepción");
            Console.WriteLine($"TOTAL {aprobadas} comprobaciones correctas");
            return 0;
        }
        catch (Exception e) { Console.Error.WriteLine(e); return 1; }
        finally
        {
            if (File.Exists(ruta)) File.Delete(ruta);
            if (Directory.Exists(Path.GetDirectoryName(ruta))) Directory.Delete(Path.GetDirectoryName(ruta)!);
        }
    }

    private static void Probar(ConfiguracionConexion cfg, string servidor, string baseDatos, string esperado, int puerto = 0)
    {
        using var f = new FormularioConexion(cfg);
        f.Show();
        Campo<TextBox>(f, "servidor").Text = servidor;
        Campo<TextBox>(f, "baseDatos").Text = baseDatos;
        Campo<NumericUpDown>(f, "puerto").Value = puerto;
        Campo<Button>(f, "probar").PerformClick();
        var limite = DateTime.UtcNow.AddSeconds(35);
        var vueltas = 0;
        while (Campo<bool>(f, "ocupado") && DateTime.UtcNow < limite)
        {
            Application.DoEvents(); Thread.Sleep(10); vueltas++;
        }
        var mensaje = Campo<Label>(f, "mensaje").Text;
        Console.WriteLine("RESULTADO REAL: " + mensaje);
        Ok(!Campo<bool>(f, "ocupado"), "La operación termina");
        Ok(Campo<Button>(f, "probar").Enabled && Campo<Button>(f, "guardar").Enabled, "Botones restaurados");
        Ok(vueltas > 0, "La interfaz procesa eventos durante la conexión");
        Ok(esperado.Length > 0 ? mensaje.Contains(esperado) : mensaje.StartsWith("No se pudo conectar con el servidor") || mensaje.StartsWith("Se agotó el tiempo de espera"), "Mensaje apropiado para " + servidor + " / " + baseDatos);
        f.Close();
    }

    private static T Campo<T>(object objeto, string nombre) => (T)objeto.GetType().GetField(nombre, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(objeto)!;
    private static void Ok(bool condicion, string nombre)
    {
        if (!condicion) throw new InvalidOperationException(nombre);
        aprobadas++; Console.WriteLine("OK " + nombre);
    }
}
