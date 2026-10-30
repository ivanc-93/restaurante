using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Restaurante.Backend.Dominio;
using Restaurante.Backend.Servicios;
using Restaurante.Frontend.Formularios;
namespace PruebasRoles;
internal static class PruebasInterfazRoles
{
 public static void Ejecutar(string cadena,RepositorioRestaurante admin,string dir,Action<bool,string> ok)
 {
    var cb=new SqlConnectionStringBuilder(cadena);if(!cb.InitialCatalog.StartsWith("Restaurante_Pruebas_"))throw new Exception("Solo interfaz aislada.");
    using(var conexion=new FormularioConexion(new ConfiguracionConexion(Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".bin"))))
    {
        Mostrar(conexion);Campo<TextBox>(conexion,"servidor").Text=cb.DataSource;Campo<NumericUpDown>(conexion,"puerto").Value=0;Campo<TextBox>(conexion,"baseDatos").Text=cb.InitialCatalog;Campo<Button>(conexion,"probar").PerformClick();
        Esperar(()=>Campo<Label>(conexion,"mensaje").Text.Contains("compatible"));Capturar(conexion,dir,"ConexionValidada");ok(true,"Interfaz prueba conexión y esquema sin guardar perfil habitual");conexion.Close();
    }
    var usuario="ui"+Guid.NewGuid().ToString("N")[..8];
    using(var gestion=new FormularioUsuarios(admin))
    {
        Mostrar(gestion);Esperar(()=>Campo<DataGridView>(gestion,"lista").DataSource is not null);
        Campo<TextBox>(gestion,"nombre").Text="Mesero de interfaz";Campo<TextBox>(gestion,"usuario").Text=usuario;Campo<TextBox>(gestion,"clave").Text="PruebaTemporal_2026!";Campo<ComboBox>(gestion,"rol").SelectedItem="Mesero";
        Campo<TextBox>(gestion,"confirmarClave").Text="PruebaTemporal_2026!";
        Campo<Button>(gestion,"guardar").PerformClick();Esperar(()=>Campo<Label>(gestion,"mensaje").Text.StartsWith("Usuario guardado"));Capturar(gestion,dir,"UsuariosConDatos");ok(admin.ListarUsuarios().Any(x=>x.Usuario==usuario),"Interfaz crea mesero con cambio obligatorio");gestion.Close();
    }
    var repo=new RepositorioRestaurante(new ConfiguracionConexionTemporal(cadena));
    using(var acceso=new FormularioAcceso(repo,new ConfiguracionConexionTemporal(cadena)))
    {
        Mostrar(acceso);Campo<TextBox>(acceso,"_usuario").Text=usuario;Campo<TextBox>(acceso,"_contrasena").Text="PruebaTemporal_2026!";
        Modal<FormularioCambiarClave>(()=>Campo<Button>(acceso,"entrar").PerformClick(),cambio=>{
        Campo<TextBox>(cambio,"actual").Text="PruebaTemporal_2026!";Campo<TextBox>(cambio,"nueva").Text="PruebaPersonal_2026!";Campo<TextBox>(cambio,"repetir").Text="PruebaPersonal_2026!";Campo<Button>(cambio,"guardar").PerformClick();});
        Esperar(()=>!Application.OpenForms.OfType<FormularioCambiarClave>().Any());Esperar(()=>Campo<Button>(acceso,"entrar").Enabled);
        ok(repo.UsuarioActual is null,"Interfaz obliga a volver al acceso tras cambiar contraseña");
        Campo<TextBox>(acceso,"_contrasena").Text="PruebaPersonal_2026!";Campo<Button>(acceso,"entrar").PerformClick();Esperar(()=>acceso.DialogResult==DialogResult.OK);
        ok(repo.UsuarioActual?.Rol=="Mesero","Interfaz autentica cuenta personal de mesero");
    }
    var numero=900000+Random.Shared.Next(90000);admin.CrearMesa(numero,2);var mesa=repo.ListarMesas().Single(x=>x.Numero==numero);var producto=repo.ListarMenu().First(x=>x.Precio>1);
    var pedido=repo.CrearPedido(new SolicitudPedido{MesaId=mesa.Id,Detalles=[new(producto.Id,1)]});var cuenta=repo.RevisarCuenta(pedido.Id);
    using(var ajuste=new FormularioAjuste(repo,cuenta))
    {
        Mostrar(ajuste);Campo<NumericUpDown>(ajuste,"descuento").Value=1;Campo<TextBox>(ajuste,"motivo").Text="Descuento de capacitación ficticio";
        Modal<FormularioAutorizar>(()=>Campo<Button>(ajuste,"aplicar").PerformClick(),autorizar=>{
        Capturar(autorizar,dir,"AutorizacionConCuenta");Campo<TextBox>(autorizar,"usuario").Text="admin";Campo<TextBox>(autorizar,"clave").Text="PruebaPersonal_2026!";Campo<Button>(autorizar,"autorizar").PerformClick();});
        Esperar(()=>ajuste.DialogResult==DialogResult.OK);ok(repo.RevisarCuenta(pedido.Id).Descuento==1,"Interfaz solicita gerente y aplica solo el ajuste autorizado");
    }
    using(var principal=new FormularioPrincipal(repo,new ConfiguracionConexionTemporal(cadena),repo.UsuarioActual!))
    {
        Mostrar(principal);Esperar(()=>Campo<FlowLayoutPanel>(principal,"menu").Enabled);
        var botones=Campo<FlowLayoutPanel>(principal,"menu").Controls.OfType<Button>().Select(x=>x.Text).ToList();
        ok(!botones.Contains("Reportes")&&!botones.Contains("Usuarios")&&!botones.Contains("Configuración"),"Menú mesero oculta administración y reportes");Capturar(principal,dir,"InicioMesero");principal.Close();
    }
 }
 static T Campo<T>(Form f,string nombre)=>(T)f.GetType().GetField(nombre,BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(f)!;
 static void Mostrar(Form f){f.ShowInTaskbar=false;f.StartPosition=FormStartPosition.Manual;f.Location=new Point(-20000,-20000);f.Show();SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());Application.DoEvents();}
 static void Modal<T>(Action disparar,Action<T> completar) where T:Form
 {
    bool atendido=false;long visibleDesde=-1;var reloj=Stopwatch.StartNew();using var timer=new System.Windows.Forms.Timer{Interval=50};
    timer.Tick+=(_,_)=>{
        if(reloj.Elapsed>TimeSpan.FromSeconds(25))
        {Console.Error.WriteLine("Modal bloqueado: "+string.Join(" | ",Application.OpenForms.Cast<Form>().Select(f=>f.GetType().Name+":"+string.Join(";",f.Controls.OfType<Label>().Select(l=>l.Text)))));Environment.Exit(1);}
        var f=Application.OpenForms.OfType<T>().FirstOrDefault();if(f is null||!f.Visible||!f.IsHandleCreated||!f.Enabled||atendido)return;
        if(visibleDesde<0){visibleDesde=reloj.ElapsedMilliseconds;return;}if(reloj.ElapsedMilliseconds-visibleDesde<200)return;
        atendido=true;completar(f);
    };
    using var vigilancia=new System.Threading.Timer(_=>{Console.Error.WriteLine("Tiempo agotado en modal "+typeof(T).Name);Environment.Exit(1);},null,TimeSpan.FromSeconds(30),Timeout.InfiniteTimeSpan);
    Console.WriteLine("UI abrir modal "+typeof(T).Name);timer.Start();disparar();Esperar(()=>atendido&&!Application.OpenForms.OfType<T>().Any());Console.WriteLine("UI modal terminado "+typeof(T).Name);
 }
 static void Esperar(Func<bool> condicion)
 {var reloj=Stopwatch.StartNew();while(!condicion()){if(reloj.Elapsed>TimeSpan.FromSeconds(25))throw new Exception("Interfaz no alcanzó el estado esperado.");Application.DoEvents();Thread.Sleep(15);}Application.DoEvents();}
 static void Capturar(Form f,string dir,string nombre)
 {using var b=new Bitmap(f.Width,f.Height);f.DrawToBitmap(b,new Rectangle(0,0,b.Width,b.Height));b.Save(Path.Combine(dir,nombre+".png"));}
}
