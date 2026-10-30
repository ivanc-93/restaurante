using System.Diagnostics;
using System.Reflection;
using Microsoft.Data.SqlClient;
using Restaurante.Backend.Dominio;
using Restaurante.Backend.Servicios;
using Restaurante.Frontend.Formularios;
using Restaurante.Frontend.Servicios;

internal static class Program
{
    private static int aprobadas;
    private static string salida="";
    private static readonly DialogosPrueba dialogos=new();
    [STAThread]
    private static int Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException+=(_,e)=>{Console.Error.WriteLine(e.Exception);Environment.Exit(1);};
        try
        {
            salida=args.Single();Directory.CreateDirectory(salida);
            var cadena=Environment.GetEnvironmentVariable("SABORES_VISUAL_CONEXION")??throw new Exception("Falta conexión de pruebas");
            if(!new SqlConnectionStringBuilder(cadena).InitialCatalog.StartsWith("Restaurante_Pruebas_Visual_"))throw new Exception("Solo base visual aislada");
            var cfg=new ConfiguracionConexionTemporal(cadena);
            var instalacion=new ServicioInstalacion(cfg);
            if(instalacion.NecesitaAdministrador())instalacion.CrearPrimerAdministrador(Environment.GetEnvironmentVariable("SABORES_VISUAL_TOKEN")??"","Administradora de prueba","admin_visual","Cuenta ficticia visual 2026");
            var repo=new RepositorioRestaurante(cfg);repo.IniciarSesion("admin_visual","Cuenta ficticia visual 2026");
            repo.GuardarCliente(null,"Cliente ejemplo 01","5550-0101","cliente01@example.com");
            repo.GuardarCliente(null,"Cliente ejemplo 02","5550-0102","cliente02@example.com");
            var mesas=repo.ListarMesas();var producto=repo.ListarMenu().First(p=>p.Precio>0);
            var disponible=mesas[0];var ocupada=mesas[1];var limpieza=mesas[3];var reservada=mesas[6];
            var pedido=repo.CrearPedido(new SolicitudPedido{MesaId=ocupada.Id,Detalles=[new(producto.Id,2)]});
            var pLimpio=repo.CrearPedido(new SolicitudPedido{MesaId=limpieza.Id,Detalles=[new(producto.Id,1)]});var rev=repo.RevisarCuenta(pLimpio.Id);
            repo.RegistrarCobro(new SolicitudCobro(Guid.NewGuid(),pLimpio.Id,rev.Version,rev.Total,true,"Público general",null,null,null,"Efectivo",rev.Total,"",null,"Fixture visual",true,true));
            repo.ActualizarMesa(reservada.Id,reservada.Numero,reservada.Capacidad,"Reservada");
            using(var principal=new FormularioPrincipal(repo,cfg,repo.UsuarioActual!,dialogos))
            {
                Mostrar(principal,new Size(1366,768));Esperar(()=>Campo<FlowLayoutPanel>(principal,"menu").Enabled);
                Capturar(principal,"inicio-1366");
                principal.Size=new Size(1920,1080);Application.DoEvents();Capturar(principal,"inicio-1920");principal.Size=new Size(1366,768);
                Ok(Controles(principal).OfType<Button>().Any(b=>b.Text=="Usuarios"),"Conserva acceso administrativo de usuarios");
                Ok(Controles(principal).OfType<Button>().Any(b=>b.Text=="Cambiar contraseña"),"Conserva cambio de contraseña");
                Ejecutar(principal,"MostrarMesasPos");Capturar(principal,"mesas-1366");
                Ok(Controles(principal).OfType<TarjetaMesa>().Count()==8,"Ocho tarjetas reales con datos SQL");
                var buscar=Controles(principal).OfType<TextBox>().Single(c=>c.Name=="buscarMesa");buscar.Text="1";Application.DoEvents();
                Ok(Controles(principal).OfType<TarjetaMesa>().All(t=>t.Mesa.Numero.ToString().Contains("1")),"Búsqueda de mesa funcional");buscar.Clear();
                var filtro=Controles(principal).OfType<ComboBox>().Single(c=>c.Name=="filtroEstadoMesa");filtro.SelectedItem="En limpieza";Application.DoEvents();
                Ok(Controles(principal).OfType<TarjetaMesa>().Count()==1,"Filtro de limpieza funcional");filtro.SelectedIndex=0;
                Modal<FormularioDetalleMesa>(()=>Tarjeta(principal,disponible.Id).Accion.PerformClick(),f=>
                {
                    Esperar(()=>Campo<ComboBox>(f,"_productos").Items.Count>0&&Campo<Button>(f,"_agregar").Enabled);
                    Campo<Button>(f,"_agregar").PerformClick();Esperar(()=>Campo<PedidoResumen?>(f,"_pedido") is not null&&!Campo<bool>(f,"_ocupado"));f.Close();
                });Esperar(()=>Campo<FlowLayoutPanel>(principal,"menu").Enabled);
                Ok(repo.ObtenerPedidoAbierto(disponible.Id) is not null,"Tarjeta abre y registra pedido mediante controles");
                Modal<FormularioDetalleMesa>(()=>Tarjeta(principal,ocupada.Id).Accion.PerformClick(),f=>{Esperar(()=>Campo<PedidoResumen?>(f,"_pedido") is not null&&!Campo<bool>(f,"_cargando"));Ok(Campo<PedidoResumen>(f,"_pedido").Id==pedido.Id,"Continuar abre el pedido existente");f.Close();});Esperar(()=>Campo<FlowLayoutPanel>(principal,"menu").Enabled);
                Modal<FormularioGestionMesa>(()=>Tarjeta(principal,limpieza.Id).Accion.PerformClick(),f=>{Esperar(()=>Campo<Button>(f,"aplicar").Enabled);Campo<Button>(f,"aplicar").PerformClick();});Esperar(()=>Campo<FlowLayoutPanel>(principal,"menu").Enabled);
                Ok(repo.ListarMesas().Single(m=>m.Id==limpieza.Id).Estado=="Disponible","Limpieza usa la operación existente y libera mesa");
                principal.Size=new Size(1920,1080);Application.DoEvents();Capturar(principal,"mesas-1920");
                principal.Close();
            }
            using(var catalogo=new FormularioCatalogos(repo,true,false,dialogos))
            {
                Mostrar(catalogo,new Size(1366,768));Esperar(()=>Campo<DataGridView>(catalogo,"tabla").DataSource is not null&&catalogo.Enabled);
                Campo<TextBox>(catalogo,"buscarCatalogo").Text="Cliente ejemplo 01";Application.DoEvents();
                Ok(Campo<DataGridView>(catalogo,"tabla").Rows.Count==1,"Búsqueda de clientes funcional");
                Campo<Button>(catalogo,"editar").PerformClick();Campo<TextBox>(catalogo,"telefono").Text="5550-0999";Campo<Button>(catalogo,"guardar").PerformClick();Esperar(()=>catalogo.Enabled&&!Campo<bool>(catalogo,"_sucio")&&Campo<DataGridView>(catalogo,"tabla").Rows.Cast<DataGridViewRow>().Any(r=>r.DataBoundItem is Cliente{Telefono:"5550-0999"}));
                Ok(repo.ListarClientes().Single(c=>c.Nombre=="Cliente ejemplo 01").Telefono=="5550-0999","Guardar catálogo modifica únicamente cliente de prueba");
                Campo<Button>(catalogo,"editar").PerformClick();Capturar(catalogo,"catalogos-1366");catalogo.Size=new Size(1920,1080);Application.DoEvents();Capturar(catalogo,"catalogos-1920");
                var clienteDesactivar=repo.ListarClientes().Single(c=>c.Nombre=="Cliente ejemplo 01").Id;
                Campo<Button>(catalogo,"desactivar").PerformClick();Esperar(()=>catalogo.Enabled&&Campo<DataGridView>(catalogo,"tabla").Rows.Count==0);
                Ok(!repo.ListarClientes().Any(c=>c.Id==clienteDesactivar)&&repo.ListarClientesHistoricos().Any(c=>c.Id==clienteDesactivar),"Desactivar conserva historial del cliente de prueba");
                catalogo.Close();
            }
            var cuenta=repo.RevisarCuenta(pedido.Id);
            using(var cobro=new FormularioCobro(repo,cuenta,dialogos))
            {
                Mostrar(cobro,new Size(1366,768));Esperar(()=>Campo<ComboBox>(cobro,"cliente").DataSource is not null);
                Campo<ComboBox>(cobro,"metodo").SelectedItem="Tarjeta";Campo<ComboBox>(cobro,"resultadoTarjeta").SelectedItem="Incierto";Campo<CheckBox>(cobro,"datosConfirmados").Checked=true;
                Ok(Campo<Button>(cobro,"confirmar").Enabled,"Tarjeta incierta puede registrarse sin inventar confirmación de pago");
                Campo<CheckBox>(cobro,"datosConfirmados").Checked=false;
                Campo<ComboBox>(cobro,"metodo").SelectedItem="Efectivo";Campo<NumericUpDown>(cobro,"entregado").Value=cuenta.Total+10;
                Ok(!Campo<Button>(cobro,"confirmar").Enabled,"Cobro exige confirmaciones");
                Campo<CheckBox>(cobro,"datosConfirmados").Checked=true;Campo<CheckBox>(cobro,"pagoConfirmado").Checked=true;
                Capturar(cobro,"cobro-1366");cobro.Size=new Size(1920,1080);Application.DoEvents();Capturar(cobro,"cobro-1920");
                Campo<Button>(cobro,"confirmar").PerformClick();Esperar(()=>cobro.Resultado is not null);
                Ok(cobro.Resultado!.Estado=="Confirmado"&&cobro.Resultado.Cambio==10,"Cobro real de prueba confirma y calcula cambio");
            }
            Ok(repo.ListarMesas().Single(m=>m.Id==ocupada.Id).Estado=="En limpieza","Cobro conserva paso a limpieza");
            repo.GuardarUsuario(null,"Mesero de prueba","mesero_visual","Mesero",true,"Temporal ficticia visual 2026");
            var mesero=new RepositorioRestaurante(cfg);mesero.IniciarSesion("mesero_visual","Temporal ficticia visual 2026");mesero.CambiarMiContrasena("Temporal ficticia visual 2026","Personal ficticia visual 2026");mesero.IniciarSesion("mesero_visual","Personal ficticia visual 2026");
            using(var f=new FormularioPrincipal(mesero,cfg,mesero.UsuarioActual!,dialogos))
            {Mostrar(f,new Size(1366,768));Esperar(()=>Campo<FlowLayoutPanel>(f,"menu").Enabled);Ejecutar(f,"MostrarMesasPos");Ok(!Controles(f).OfType<Button>().Any(b=>b.Text=="Administrar mesas"&&b.Visible),"Mesero no ve administrar mesas");f.Close();}
            using(var f=new FormularioCatalogos(mesero,false,false,dialogos))
            {Mostrar(f,new Size(1366,768));Esperar(()=>f.Enabled&&Campo<DataGridView>(f,"tabla").DataSource is not null);Ok(!Campo<Button>(f,"editar").Visible&&!Campo<Button>(f,"desactivar").Visible,"Catálogo conserva restricciones de Mesero");f.Close();}
            Console.WriteLine($"TOTAL {aprobadas} comprobaciones visuales y funcionales correctas; DPI nativo {SystemDpi()}.");return 0;
        }
        catch(Exception e){Console.Error.WriteLine(e);return 1;}
    }
    private static int SystemDpi(){using var f=new Form();return f.DeviceDpi;}
    private static TarjetaMesa Tarjeta(Form f,int id)=>Controles(f).OfType<TarjetaMesa>().Single(t=>t.Mesa.Id==id);
    private static T Campo<T>(object f,string nombre)=>(T)f.GetType().GetField(nombre,BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(f)!;
    private static IEnumerable<Control> Controles(Control c){foreach(Control h in c.Controls){yield return h;foreach(var n in Controles(h))yield return n;}}
    private static void Mostrar(Form f,Size size){f.StartPosition=FormStartPosition.Manual;f.Location=new Point(0,0);f.Size=size;f.Show();SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());Application.DoEvents();}
    private static void Ejecutar(Form f,string metodo){SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());var t=(Task)f.GetType().GetMethod(metodo,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(f,null)!;Esperar(()=>t.IsCompleted);t.GetAwaiter().GetResult();}
    private static void Esperar(Func<bool> condicion){var r=Stopwatch.StartNew();while(!condicion()){if(r.Elapsed.TotalSeconds>40)throw new TimeoutException("La interfaz no terminó la operación");Application.DoEvents();Thread.Sleep(10);}Application.DoEvents();}
    private static void Capturar(Form f,string nombre){Application.DoEvents();using var b=new Bitmap(f.Width,f.Height);f.DrawToBitmap(b,new Rectangle(Point.Empty,b.Size));b.Save(Path.Combine(salida,nombre+".png"));}
    private static void Ok(bool valor,string texto){if(!valor)throw new Exception(texto);aprobadas++;Console.WriteLine("OK "+texto);}
    private static void Modal<T>(Action abrir,Action<T> completar)where T:Form
    {
        SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
        Console.WriteLine("ABRIR "+typeof(T).Name);
        using var vigilancia=new System.Threading.Timer(_=>{Console.Error.WriteLine("Tiempo agotado en diálogo de prueba "+typeof(T).Name);Environment.Exit(1);},null,TimeSpan.FromSeconds(50),Timeout.InfiniteTimeSpan);
        bool atendido=false;using var timer=new System.Windows.Forms.Timer{Interval=100};
        timer.Tick+=(_,_)=>{var f=Application.OpenForms.OfType<T>().FirstOrDefault();if(f is null||!f.Visible||atendido)return;atendido=true;Console.WriteLine("ATENDER "+typeof(T).Name);try{completar(f);}catch(Exception e){Console.Error.WriteLine(e);Environment.Exit(1);}};
        timer.Start();abrir();Esperar(()=>atendido&&!Application.OpenForms.OfType<T>().Any());
        SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
    }
    private sealed class DialogosPrueba:IDialogosUsuario
    {
        public DialogResult Mostrar(IWin32Window propietario,string mensaje,string titulo,MessageBoxButtons botones=MessageBoxButtons.OK,MessageBoxIcon icono=MessageBoxIcon.None){Console.WriteLine("DIALOGO: "+titulo+" · "+mensaje);return botones==MessageBoxButtons.YesNo?DialogResult.Yes:DialogResult.OK;}
    }
}
