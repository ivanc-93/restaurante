using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Restaurante.Backend.Dominio;
using Restaurante.Backend.Servicios;
using Restaurante.Frontend.Formularios;
namespace PruebasRoles;
internal static class PruebasRechazosAutorizacion
{
 const string Temporal="TemporalSoloPruebas2026!",Personal="PersonalSoloPruebas2026!";
 public static void Ejecutar(string cadena,RepositorioRestaurante admin,string dir,Action<bool,string> ok)
 {
    if(!new SqlConnectionStringBuilder(cadena).InitialCatalog.StartsWith("Restaurante_Pruebas_"))throw new Exception("Solo bases aisladas.");
    Directory.CreateDirectory(dir);ApplicationConfiguration.Initialize();var sufijo=Guid.NewGuid().ToString("N")[..8];
    RepositorioRestaurante Repo(string u,string p){var r=new RepositorioRestaurante(new ConfiguracionConexionTemporal(cadena));if(r.IniciarSesion(u,p) is null)throw new Exception("No autentica fixture");return r;}
    string Crear(string rol,bool cambiar=true)
    {var u=rol+sufijo+Guid.NewGuid().ToString("N")[..4];admin.GuardarUsuario(null,"Persona ficticia "+rol,u,rol,true,Temporal);if(cambiar){var r=Repo(u,Temporal);r.CambiarMiContrasena(Temporal,Personal);}return u;}
    void Sql(string sql,params(string,object)[] ps){using var c=new SqlConnection(cadena);c.Open();using var q=new SqlCommand(sql,c);foreach(var(n,v) in ps)q.Parameters.AddWithValue(n,v);q.ExecuteNonQuery();}
    int Conteo(string tabla,Guid op){using var c=new SqlConnection(cadena);c.Open();using var q=new SqlCommand($"SELECT COUNT(*) FROM dbo.{tabla} WHERE "+(tabla=="OperacionesRestaurante"?"Id":"OperacionId")+"=@id;",c);q.Parameters.AddWithValue("@id",op);return(int)q.ExecuteScalar()!;}
    var meseroNombre=Crear("Mesero");var gerente=Crear("Gerente");var administrador=Crear("Administrador");var temporal=Crear("Gerente",false);var inactivo=Crear("Gerente");
    var uInactivo=admin.ListarUsuarios().Single(u=>u.Usuario==inactivo);admin.GuardarUsuario(uInactivo.Id,uInactivo.Nombre,uInactivo.Usuario,uInactivo.Rol,false,null);
    var mesero=Repo(meseroNombre,Personal);var numero=1000000+Random.Shared.Next(90000);admin.CrearMesa(numero,4);var mesa=admin.ListarMesas().Single(m=>m.Numero==numero);var producto=admin.ListarMenu().First(p=>p.Precio>0);
    var pedido=mesero.CrearPedido(new SolicitudPedido{MesaId=mesa.Id,Detalles=[new(producto.Id,1)]});var cuenta=mesero.RevisarCuenta(pedido.Id);
    var solicitud=new SolicitudGestionMesa(Guid.NewGuid(),mesa.Id,pedido.Id,cuenta.Version,"Cancelar","Sin preparación ni consumo en prueba aislada",true);
    ResultadoAutorizacion Intentar(string u,string p)=>mesero.IntentarAutorizarOperacion(solicitud.OperacionId,"GestionMesa",JsonSerializer.Serialize(solicitud),u,p);
    int excepciones=0;EventHandler<System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs> escuchar=(_,e)=>{if(e.Exception is UnauthorizedAccessException)excepciones++;};AppDomain.CurrentDomain.FirstChanceException+=escuchar;
    try
    {
        ok(Intentar("","").Estado==EstadoAutorizacion.DatosIncompletos,"Servidor rechaza campos vacíos como resultado tipado");
        ok(Intentar(gerente,"incorrecta").Estado==EstadoAutorizacion.CredencialesNoVerificadas,"Contraseña incorrecta no lanza excepción normal");
        ok(Intentar(inactivo,Personal).Estado==EstadoAutorizacion.CredencialesNoVerificadas,"Cuenta inactiva no revela existencia");
        ok(Intentar(temporal,Temporal).Estado==EstadoAutorizacion.CambioObligatorio,"Cuenta autenticada recibe instrucción de cambio obligatorio");
        ok(Intentar(meseroNombre,Personal).Estado==EstadoAutorizacion.SinPermiso,"Mesero autenticado recibe mensaje de falta de rol");
        ok(Conteo("OperacionesRestaurante",solicitud.OperacionId)==0&&Conteo("AutorizacionesOperacion",solicitud.OperacionId)==0,"Rechazos no aplican gestión ni dejan aprobación");
        ok(mesero.RevisarCuenta(pedido.Id).Total==cuenta.Total,"Importe y pedido permanecen abiertos tras rechazos");
        ok(Intentar(gerente,Personal).Autorizada,"Gerente válido autoriza");
        ok(Intentar(gerente,"incorrecta").Estado==EstadoAutorizacion.CredencialesNoVerificadas&&Conteo("AutorizacionesOperacion",solicitud.OperacionId)==0,"Reintento incorrecto revoca aprobación anterior no consumida");
        var bloqueado=Crear("Gerente");for(var i=0;i<5;i++)Intentar(bloqueado,"incorrecta");
        ok(Intentar(bloqueado,Personal).Estado==EstadoAutorizacion.CredencialesNoVerificadas,"Bloqueo temporal conserva mensaje no enumerativo");
        ok(Intentar(administrador,Personal).Autorizada,"Administrador válido autoriza sin elevar sesión solicitante");
        ok(mesero.UsuarioActual!.Rol=="Mesero","Sesión del mesero permanece sin cambios");
        ok(excepciones==0,"Rechazos esperados no generan UnauthorizedAccessException en API de interfaz");
    }
    finally{AppDomain.CurrentDomain.FirstChanceException-=escuchar;}
    using(var f=new FormularioAutorizar(mesero,solicitud.OperacionId,"GestionMesa",solicitud,"Mesa ficticia · cancelar sin consumo"))
    {Mostrar(f);Campo<Button>(f,"cancelar").PerformClick();Esperar(()=>f.IsDisposed||f.DialogResult==DialogResult.Cancel);ok(Conteo("AutorizacionesOperacion",solicitud.OperacionId)==0,"Cancelar revoca aprobación previa y no ejecuta gestión");}
    Intentar(gerente,Personal);Sql("UPDATE dbo.AutorizacionesOperacion SET Caduca=DATEADD(minute,-1,SYSUTCDATETIME()) WHERE OperacionId=@id;",("@id",solicitud.OperacionId));
    Rechaza(()=>mesero.GestionarMesa(solicitud),ok,"Aprobación vencida no permite ejecutar");
    mesero.AgregarProductoAPedido(pedido.Id,producto.Id,1);ok(Intentar(gerente,Personal).Estado==EstadoAutorizacion.SolicitudModificada,"Cuenta modificada devuelve instrucción de actualizar");
    solicitud=solicitud with{Version=mesero.RevisarCuenta(pedido.Id).Version};Intentar(gerente,Personal);
    var jefe=admin.ListarUsuarios().Single(u=>u.Usuario==gerente);admin.GuardarUsuario(jefe.Id,jefe.Nombre,jefe.Usuario,jefe.Rol,true,null);
    Rechaza(()=>mesero.GestionarMesa(solicitud),ok,"Cambio de cuenta autorizadora invalida aprobación");
    using(var f=new FormularioAutorizar(mesero,solicitud.OperacionId,"GestionMesa",solicitud,$"Mesa {mesa.Numero} · Pedido {pedido.Id}\nCancelar sin preparación ni consumo\nImporte {MonedaGuatemala.Formatear(mesero.RevisarCuenta(pedido.Id).Total)}"))
    {
        Mostrar(f);Campo<Button>(f,"autorizar").PerformClick();ok(Campo<Label>(f,"mensaje").Text.Contains("usuario"),"Interfaz valida usuario vacío sin enviar");
        Campo<TextBox>(f,"usuario").Text=gerente;Campo<Button>(f,"autorizar").PerformClick();ok(Campo<Label>(f,"mensaje").Text.Contains("contraseña vigente"),"Interfaz valida contraseña vacía sin nueva política de longitud");
        Campo<TextBox>(f,"clave").Text="incorrecta";Campo<CheckBox>(f,"mostrarClave").Checked=true;ok(!Campo<TextBox>(f,"clave").UseSystemPasswordChar,"Mostrar contraseña funciona en autorización");
        Campo<Button>(f,"autorizar").PerformClick();Esperar(()=>f.Resultado is not null&&Campo<Button>(f,"autorizar").Enabled);
        ok(f.Resultado!.Estado==EstadoAutorizacion.CredencialesNoVerificadas&&Campo<TextBox>(f,"usuario").Text==gerente&&Campo<TextBox>(f,"clave").Text==""&& !Campo<CheckBox>(f,"mostrarClave").Checked,"Rechazo mantiene usuario limpia y vuelve a ocultar contraseña");
        Capturar(f,dir,"AutorizacionRechazada");
        foreach(var tamano in new[]{new Size(1280,720),new Size(1366,768)})
        {f.Size=tamano;f.PerformLayout();Application.DoEvents();foreach(var n in new[]{"autorizar","cancelar","mensaje"}){var c=Campo<Control>(f,n);ok(f.ClientRectangle.Contains(f.RectangleToClient(c.RectangleToScreen(c.ClientRectangle))),"Control visible "+n+" en "+tamano.Width);}Capturar(f,dir,"Autorizacion-"+tamano.Width);}
        ok(!Campo<TextBox>(f,"clave").InvokeRequired,"Campo de contraseña conserva el hilo de interfaz después del rechazo");
        Campo<TextBox>(f,"clave").Text=Personal;Application.DoEvents();
        ok(Campo<TextBox>(f,"clave").Text.Length==Personal.Length,"Contraseña de reintento permanece en el campo antes de enviar");
        Campo<Button>(f,"autorizar").PerformClick();
        try{Esperar(()=>f.Resultado is not null&&Campo<Button>(f,"autorizar").Enabled||f.DialogResult==DialogResult.OK);}
        catch{Capturar(f,dir,"ReintentoDiagnostico");Console.Error.WriteLine("Estado de reintento: "+f.Resultado?.Estado+"; "+Campo<Label>(f,"mensaje").Text+"; botón activo "+Campo<Button>(f,"autorizar").Enabled);throw;}
        if(f.DialogResult!=DialogResult.OK)throw new Exception("Reintento: "+f.Resultado?.Estado+"; "+Campo<Label>(f,"mensaje").Text);
        ok(f.Resultado!.Autorizada,"Mismo diálogo permite reintento correcto sin reiniciar");
    }
    mesero.GestionarMesa(solicitud);var repetido=mesero.GestionarMesa(solicitud);ok(repetido=="Cancelado"&&Conteo("OperacionesRestaurante",solicitud.OperacionId)==1,"Recuperación del resultado no duplica gestión");
    ok(Intentar(gerente,Personal).Estado==EstadoAutorizacion.OperacionAplicada,"Solicitud ya aplicada exige recuperación y no otra aprobación");
    var cfgSinSql=new ConfiguracionConexionTemporal("Server=127.0.0.1,1;Database=Restaurante_Pruebas_SinServidor;Integrated Security=True;Connect Timeout=1;Encrypt=False;");
    using(var f=new FormularioAutorizar(new RepositorioRestaurante(cfgSinSql),Guid.NewGuid(),"GestionMesa",solicitud,"Prueba de conexión no disponible"))
    {Mostrar(f);Campo<TextBox>(f,"usuario").Text="cuenta ficticia";Campo<TextBox>(f,"clave").Text="valor ficticio";Campo<Button>(f,"autorizar").PerformClick();Esperar(()=>Campo<Button>(f,"autorizar").Enabled&&Campo<Label>(f,"mensaje").Text.Contains("base de datos"));ok(f.DialogResult!=DialogResult.OK&&f.Resultado is null,"Error SQL no se presenta como contraseña incorrecta ni autoriza");Capturar(f,dir,"AutorizacionSinSql");Campo<Button>(f,"cancelar").PerformClick();Esperar(()=>f.IsDisposed||f.DialogResult==DialogResult.Cancel);}
 }
 static void Rechaza(Action a,Action<bool,string> ok,string mensaje){try{a();throw new Exception("Se permitió: "+mensaje);}catch(UnauthorizedAccessException){ok(true,mensaje);}}
 static T Campo<T>(Form f,string n)=>(T)f.GetType().GetField(n,BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(f)!;
 static void Mostrar(Form f){f.StartPosition=FormStartPosition.Manual;f.Location=new Point(-20000,-20000);f.ShowInTaskbar=false;f.Show();Application.DoEvents();}
 static void Esperar(Func<bool> p){var t=Stopwatch.StartNew();while(!p()){if(t.Elapsed>TimeSpan.FromSeconds(25))throw new Exception("Interfaz no alcanzó el estado esperado");Application.DoEvents();Thread.Sleep(15);}Application.DoEvents();}
 static void Capturar(Form f,string dir,string n){using var b=new Bitmap(f.Width,f.Height);f.DrawToBitmap(b,new Rectangle(0,0,b.Width,b.Height));b.Save(Path.Combine(dir,n+".png"));}
}
