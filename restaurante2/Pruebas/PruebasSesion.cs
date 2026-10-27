using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Restaurante.Backend.Dominio;
using Restaurante.Backend.Servicios;
using Restaurante.Frontend.Formularios;
using Restaurante.Frontend.Servicios;

internal static class PruebasSesion
{
    public static void Ejecutar(string cadena,Action<string,Action> caso,string salida)
    {
        caso("Modo aislado rechaza bases reales y no guarda configuración",()=>
        {
            try {new ConfiguracionConexionPruebas("Server=.;Database=RestauranteDB;Integrated Security=True;");throw new Exception("Se aceptó una base real.");}
            catch(ArgumentException){}
            var cfg=new ConfiguracionConexionPruebas(cadena);
            try{cfg.ProbarYGuardar("",1,"","","",false);throw new Exception("Se permitió guardar configuración.");}catch(InvalidOperationException){}
        });
        var repo=new RepositorioRestaurante(new ConfiguracionConexionPruebas(cadena));
        repo.IniciarSesion("admin","Admin123!");
        repo.CrearMesa(9100,4);
        repo.GuardarProducto(null,"Sesión plato","Sesión","Plato",8,false);
        repo.GuardarProducto(null,"Sesión bebida","Sesión","Bebida",3,false);
        using(var conexion=new SqlConnection(cadena))
        {
            conexion.Open();
            using var comando=new SqlCommand("""
                INSERT dbo.Usuarios(Nombre,Usuario,ContrasenaHash,Sal,Rol)
                VALUES(N'Segundo usuario',N'segundo',CONVERT(CHAR(64),HASHBYTES('SHA2_256',CONVERT(VARCHAR(200),'SalPrueba:ClavePrueba123!')),2),N'SalPrueba',N'Empleado');
                """,conexion);
            comando.ExecuteNonQuery();
        }
        var archivo=Path.Combine(salida,"sesion-proceso-1.json");
        if(File.Exists(archivo))File.Delete(archivo);
        var segundo=Path.Combine(salida,"sesion-proceso-2.json");if(File.Exists(segundo))File.Delete(segundo);
        caso("Interfaz: login, pedido de dos productos, factura, logout y segundo usuario",()=>
        {
            Lanzar(cadena,"crear",archivo,"0");
            using var resultado=JsonDocument.Parse(File.ReadAllText(archivo));
            if(resultado.RootElement.GetProperty("Estado").GetInt32()!=8)throw new Exception("El recorrido de sesiones no terminó.");
            if(resultado.RootElement.GetProperty("Total").GetDecimal()!=14m)throw new Exception("Total de la interfaz incorrecto.");
        });
        caso("Persistencia: nuevo proceso recupera la factura después de cerrar el anterior",()=>
        {
            using var previo=JsonDocument.Parse(File.ReadAllText(archivo));
            var pedido=previo.RootElement.GetProperty("PedidoId").GetInt32();
            var nuevo=Path.Combine(salida,"sesion-proceso-2.json");
            Lanzar(cadena,"verificar",nuevo,pedido.ToString());
            using var resultado=JsonDocument.Parse(File.ReadAllText(nuevo));
            if(resultado.RootElement.GetProperty("Pid").GetInt32()==previo.RootElement.GetProperty("Pid").GetInt32())throw new Exception("Se reutilizó el proceso.");
            if(resultado.RootElement.GetProperty("Total").GetDecimal()!=14m)throw new Exception("La factura no se recuperó.");
        });
    }
    private static void Lanzar(string cadena,string modo,string archivo,string pedido)
    {
        var inicio=new ProcessStartInfo(Environment.ProcessPath!) {UseShellExecute=false,CreateNoWindow=true,WindowStyle=ProcessWindowStyle.Hidden,RedirectStandardOutput=true,RedirectStandardError=true};
        if(Path.GetFileNameWithoutExtension(Environment.ProcessPath!).Equals("dotnet",StringComparison.OrdinalIgnoreCase))inicio.ArgumentList.Add(Assembly.GetExecutingAssembly().Location);
        foreach(var arg in new[]{"--sesion-ui",modo,archivo,pedido})inicio.ArgumentList.Add(arg);
        inicio.Environment["RESTAURANTE_TEST_CONNECTION"]=cadena;
        using var proceso=Process.Start(inicio)!;
        var texto=proceso.StandardOutput.ReadToEndAsync(); var error=proceso.StandardError.ReadToEndAsync();
        if(!proceso.WaitForExit(90000)){proceso.Kill();throw new TimeoutException("La prueba de sesión excedió 90 segundos.");}
        if(proceso.ExitCode!=0)throw new Exception(texto.GetAwaiter().GetResult()+error.GetAwaiter().GetResult());
    }
    public static int Proceso(string modo,string archivo,int pedidoId)
    {
        File.WriteAllText(archivo+".pasos.txt","");
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2); Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
        var cfg=new ConfiguracionConexionPruebas(Environment.GetEnvironmentVariable("RESTAURANTE_TEST_CONNECTION")!);
        var repo=new RepositorioRestaurante(cfg); var dialogos=new DialogosPrueba();
        Exception? fallo=null; int estado=0; bool ocupado=false; int cantidadLogin=0; int dpi=0;
        var reloj=Stopwatch.StartNew(); FormularioDetalleMesa? detalle=null;int ultimoEstado=-1;
        using var timer=new System.Windows.Forms.Timer {Interval=100};
        timer.Tick+=async (_,_)=>
        {
            if(reloj.Elapsed.TotalSeconds>65){Console.Error.WriteLine("Se agotó el tiempo del recorrido aislado.");Environment.Exit(1);return;}
            if(ocupado)return; ocupado=true;
            if(estado!=ultimoEstado){File.AppendAllText(archivo+".pasos.txt",$"{reloj.Elapsed.TotalSeconds:F1}s · Estado {estado}\n");ultimoEstado=estado;}
            try
            {
                var acceso=Application.OpenForms.OfType<FormularioAcceso>().FirstOrDefault();
                var principal=Application.OpenForms.OfType<FormularioPrincipal>().FirstOrDefault();
                foreach(Form ventana in Application.OpenForms){ventana.Opacity=0;ventana.ShowInTaskbar=false;}
                if(acceso is not null && (estado==0||estado==6))
                {
                    Campo<TextBox>(acceso,"_usuario").Text=estado==0?"admin":"segundo";
                    Campo<TextBox>(acceso,"_contrasena").Text=estado==0?"Admin123!":"ClavePrueba123!";
                    cantidadLogin++; estado=estado==0?1:7;
                    ((Button)acceso.AcceptButton!).PerformClick();
                }
                else if(principal is not null && Campo<FlowLayoutPanel>(principal,"menu").Enabled)
                {
                    dpi=principal.DeviceDpi;
                    if(modo=="verificar"&&estado==1)
                    {
                        var factura=repo.ListarPedidos().Single(p=>p.Id==pedidoId);
                        if(factura.Estado!="Facturado"||factura.Total!=14m||factura.NumeroFactura is null)throw new Exception("Persistencia incompleta.");
                        estado=8;principal.Close();
                    }
                    else if(estado==1)
                    {
                        dpi=principal.DeviceDpi;
                        await Invocar(principal,"MostrarMesasPos");
                        var mesa=repo.ListarMesas().Single(m=>m.Numero==9100);
                        detalle=new FormularioDetalleMesa(repo,mesa,dialogos){Opacity=0,ShowInTaskbar=false};
                        detalle.Show(principal);estado=2;
                    }
                    else if(estado==10 && (detalle is null || detalle.IsDisposed))
                    {
                        var factura=repo.ListarPedidos().Single(p=>p.Id==pedidoId);
                        if(factura.Estado!="Facturado"||factura.Total!=14m)throw new Exception("No se facturó desde la interfaz.");
                        estado=6;Campo<Button>(principal,"salir").PerformClick();
                    }
                    else if(estado==7)
                    {
                        if(Campo<UsuarioAutenticado>(principal,"_usuario").Nombre!="Segundo usuario")throw new Exception("No cambió el usuario.");
                        estado=8;principal.Close();
                    }
                }
                var cobro=Application.OpenForms.OfType<FormularioCobro>().FirstOrDefault();
                if(cobro is not null && estado==5)
                {
                    File.AppendAllText(archivo+".pasos.txt","Configurar cobro\n");
                    Campo<ComboBox>(cobro,"metodo").SelectedItem="Efectivo";
                    File.AppendAllText(archivo+".pasos.txt","Método seleccionado\n");
                    Campo<NumericUpDown>(cobro,"entregado").Value=14;
                    Campo<CheckBox>(cobro,"datosConfirmados").Checked=true;
                    Campo<CheckBox>(cobro,"pagoConfirmado").Checked=true;
                    var previo=cobro.Size;
                    foreach(var tamano in new[]{new System.Drawing.Size(1280,720),new System.Drawing.Size(1366,768)})
                    {
                        File.AppendAllText(archivo+".pasos.txt","Redimensionar "+tamano+"\n");
                        cobro.Size=tamano;cobro.PerformLayout();Application.DoEvents();
                        File.AppendAllText(archivo+".pasos.txt","Capturar "+tamano+"\n");
                        var confirmar=Campo<Button>(cobro,"confirmar");
                        using var captura=new System.Drawing.Bitmap(cobro.Width,cobro.Height);
                        cobro.DrawToBitmap(captura,new System.Drawing.Rectangle(0,0,captura.Width,captura.Height));
                        captura.Save(Path.Combine(Path.GetDirectoryName(archivo)!,"cobro-"+tamano.Width+".png"));
                        if(!cobro.ClientRectangle.Contains(cobro.RectangleToClient(confirmar.RectangleToScreen(confirmar.ClientRectangle))))
                            throw new Exception($"Confirmar cobro queda fuera del área visible: ventana {cobro.ClientRectangle}, acción {cobro.RectangleToClient(confirmar.RectangleToScreen(confirmar.ClientRectangle))}.");
                    }
                    cobro.Size=previo;
                    Campo<Button>(cobro,"confirmar").PerformClick();estado=10;
                }
                var documento=Application.OpenForms.OfType<FormularioComprobante>().FirstOrDefault();
                if(documento is not null && Campo<TextBox>(documento,"contenido").Text.Contains("no válido como documento tributario"))
                {
                    using var captura=new System.Drawing.Bitmap(documento.Width,documento.Height);
                    documento.DrawToBitmap(captura,new System.Drawing.Rectangle(0,0,captura.Width,captura.Height));
                    captura.Save(Path.Combine(Path.GetDirectoryName(archivo)!,"comprobante-cobro.png"));
                    documento.Close();
                }
                if(detalle is not null&&!detalle.IsDisposed)
                {
                    if(estado==2&&Campo<Button>(detalle,"_agregar").Enabled)
                    {
                        if(repo.ObtenerPedidoAbierto(repo.ListarMesas().Single(m=>m.Numero==9100).Id) is not null)
                            throw new Exception("Se creó un pedido vacío antes de agregar un producto.");
                        var productos=Campo<ComboBox>(detalle,"_productos");
                        productos.SelectedItem=productos.Items.Cast<ElementoMenu>().Single(p=>p.Nombre=="Sesión plato");
                        Campo<Button>(detalle,"_agregar").PerformClick();estado=9;
                    }
                    else if(estado==9&&Campo<Button>(detalle,"_agregar").Enabled)
                    {
                        pedidoId=repo.ObtenerPedidoAbierto(repo.ListarMesas().Single(m=>m.Numero==9100).Id)!.Id;
                        var buscar=Campo<TextBox>(detalle,"_buscar"); buscar.Text="Sesión";
                        if(Campo<ComboBox>(detalle,"_productos").Items.Count!=2)throw new Exception("La búsqueda no filtra productos.");
                        buscar.Clear();
                        var previo=detalle.Size;
                        foreach(var tamano in new[]{new System.Drawing.Size(1280,720),new System.Drawing.Size(1366,768)})
                        {
                            detalle.Size=tamano; detalle.PerformLayout();
                            var accion=Campo<Button>(detalle,"_facturar");
                            if(!detalle.ClientRectangle.Contains(detalle.RectangleToClient(accion.RectangleToScreen(accion.ClientRectangle))))
                                throw new Exception("La facturación queda fuera del área visible en "+tamano);
                            using var imagen=new System.Drawing.Bitmap(detalle.Width,detalle.Height);
                            detalle.DrawToBitmap(imagen,new System.Drawing.Rectangle(0,0,imagen.Width,imagen.Height));
                            imagen.Save(Path.Combine(Path.GetDirectoryName(archivo)!,"pedido-"+tamano.Width+".png"));
                        }
                        detalle.Size=previo;
                        var facturar=Campo<Button>(detalle,"_facturar");
                        if(!detalle.RectangleToScreen(detalle.ClientRectangle).Contains(facturar.RectangleToScreen(facturar.ClientRectangle)))
                            throw new Exception("Facturar no es accesible.");
                        using(var captura=new System.Drawing.Bitmap(detalle.Width,detalle.Height))
                        {
                            detalle.DrawToBitmap(captura,new System.Drawing.Rectangle(0,0,captura.Width,captura.Height));
                            captura.Save(Path.Combine(Path.GetDirectoryName(archivo)!,"pedido-pos.png"));
                        }
                        estado=3;
                    }
                    else if(estado==3&&Campo<Button>(detalle,"_agregar").Enabled&&Campo<ComboBox>(detalle,"_productos").Items.Count>0)
                    {
                        var productos=Campo<ComboBox>(detalle,"_productos");
                        productos.SelectedItem=productos.Items.Cast<ElementoMenu>().Single(p=>p.Nombre=="Sesión bebida");
                        Campo<NumericUpDown>(detalle,"_cantidad").Value=2;
                        Campo<Button>(detalle,"_agregar").PerformClick();estado=4;
                    }
                    else if(estado==4&&Campo<Button>(detalle,"_agregar").Enabled)
                    {
                        if(repo.ListarDetallesPedido(pedidoId).Count!=2)throw new Exception("No se agregaron ambos productos.");
                        Campo<Button>(detalle,"_facturar").PerformClick();estado=5;
                    }
                }
            }
            catch(Exception ex){fallo=ex;timer.Stop();Console.Error.WriteLine(ex);Environment.Exit(1);}
            finally{ocupado=false;}
        };
        timer.Start();
        CicloSesion.Ejecutar(repo,cfg,dialogos);
        timer.Stop();
        if(fallo is not null){Console.Error.WriteLine(fallo);return 1;}
        if(estado!=8||cantidadLogin!=(modo=="crear"?2:1)){Console.Error.WriteLine("Sesión incompleta.");return 1;}
        var guardado=repo.ListarPedidos().Single(p=>p.Id==pedidoId);
        File.WriteAllText(archivo,JsonSerializer.Serialize(new{Estado=estado,PedidoId=pedidoId,Total=guardado.Total,Factura=guardado.NumeroFactura,Pid=Environment.ProcessId,Logins=cantidadLogin,Dpi=dpi,Monitores=Screen.AllScreens.Length}));
        return 0;
    }
    private static Task Invocar(Form form,string metodo)=>(Task)form.GetType().GetMethod(metodo,BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(form,null)!;
    private static T Campo<T>(Form form,string nombre)=>(T)form.GetType().GetField(nombre,BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(form)!;
    private static IEnumerable<Control> Controles(Control raiz)
    {
        foreach(Control control in raiz.Controls){yield return control;foreach(var hijo in Controles(control))yield return hijo;}
    }
}
