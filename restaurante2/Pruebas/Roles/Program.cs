using System.Text.Json;
using Microsoft.Data.SqlClient;
using Restaurante.Backend.Servicios;
using Restaurante.Backend.Dominio;
using Restaurante.Frontend.Formularios;

internal static class Program
{
 static string cadena="";static int aprobadas;static string sufijo="";static RepositorioRestaurante admin=null!,gerente=null!,mesero=null!;
 const string Inicial="PruebaTemporal_2026!";const string Personal="PruebaPersonal_2026!";
 [STAThread] static int Main(string[] args)
 {
  try
  {
   if(args.Length==2&&args[0]=="--validar-acceso")
   {PruebasRoles.PruebasValidacionAcceso.Ejecutar(args[1],Ok);Console.WriteLine($"TOTAL {aprobadas} comprobaciones de acceso");return 0;}
   cadena=Environment.GetEnvironmentVariable("SABORES_ROLES_PRUEBAS")??throw new Exception("Defina SABORES_ROLES_PRUEBAS.");
   var cb=new SqlConnectionStringBuilder(cadena);if(!cb.InitialCatalog.StartsWith("Restaurante_Pruebas_",StringComparison.Ordinal))throw new Exception("Solo se admiten bases aisladas.");
   sufijo=Guid.NewGuid().ToString("N")[..8];
   var instalacion=new ServicioInstalacion(new ConfiguracionConexionTemporal(cadena));
   if(instalacion.NecesitaAdministrador())instalacion.CrearPrimerAdministrador(Environment.GetEnvironmentVariable("SABORES_TOKEN_PRUEBAS")??"","Administrador de pruebas","admin",Personal);
   var h=Contrasenas.Crear(Personal);
   Sql("UPDATE dbo.Usuarios SET ContrasenaHash=@h,Sal=@s,Algoritmo=N'PBKDF2-SHA256',Iteraciones=600000,CambioObligatorio=0,IntentosFallidos=0,BloqueadoHasta=NULL WHERE Usuario=N'admin';",("@h",h.Hash),("@s",h.Sal));
   admin=Repo("admin",Personal);admin.ValidarEsquema();Ok(true,"Esquema 5 compatible");
   var legadoNombre="legado"+sufijo;
   admin.GuardarUsuario(null,"Acceso legado de prueba",legadoNombre,"Mesero",true,Inicial);
   Sql("UPDATE dbo.Usuarios SET ContrasenaHash=CONVERT(varchar(64),HASHBYTES('SHA2_256','SalLegada:corta'),2),Sal=N'SalLegada',Algoritmo=N'SHA256',Iteraciones=0,CambioObligatorio=1 WHERE Usuario=@u;",("@u",legadoNombre));
   var accesoLegado=Repo(legadoNombre,"corta");Ok(accesoLegado.UsuarioActual!.CambioObligatorio,"Contraseña antigua corta permite llegar al cambio obligatorio");
   accesoLegado.CambiarMiContrasena("corta",Personal);Ok(!Repo(legadoNombre,Personal).UsuarioActual!.CambioObligatorio,"Contraseña antigua corta puede cambiarse por una válida");
   if(args.Length==2&&args[0]=="--solo-interfaz")
   {Render(args[1]);PruebasRoles.PruebasInterfazRoles.Ejecutar(cadena,admin,args[1],Ok);Console.WriteLine($"TOTAL {aprobadas} comprobaciones de interfaz");return 0;}
   Deniega(()=>new RepositorioRestaurante(new ConfiguracionConexionTemporal(cadena)).ListarMesas(),"Lectura sin sesión rechazada");
   Deniega(()=>new ServicioInstalacion(new ConfiguracionConexionTemporal(cadena)).CrearPrimerAdministrador("inventado","Intruso","intruso",Personal),"Base existente no admite primer administrador");
   admin.GuardarUsuario(null,"Gerente de prueba","g"+sufijo,"Gerente",true,Inicial);
   admin.GuardarUsuario(null,"Mesero de prueba","m"+sufijo,"Mesero",true,Inicial);
   gerente=Repo("g"+sufijo,Inicial);Ok(gerente.UsuarioActual!.CambioObligatorio,"Contraseña temporal exige cambio");
   Deniega(()=>gerente.ListarMesas(),"Cambio obligatorio bloquea operaciones");
   gerente.CambiarMiContrasena(Inicial,Personal);gerente=Repo("g"+sufijo,Personal);
   mesero=Repo("m"+sufijo,Inicial);mesero.CambiarMiContrasena(Inicial,Personal);mesero=Repo("m"+sufijo,Personal);
   Deniega(()=>gerente.GuardarUsuario(null,"X","x"+sufijo,"Administrador",true,Personal),"Gerente no crea administradores");
   Deniega(()=>mesero.ListarUsuarios(),"Mesero no consulta usuarios");
   Deniega(()=>mesero.ReportarVentasPeriodo(DateTime.Today,DateTime.Today,"Día"),"Mesero no consulta ventas");
   Deniega(()=>mesero.CrearMesa(99000,2),"Mesero no crea mesas");
   Ok(mesero.ObtenerResumen().VentasDelDia==0,"Resumen de mesero no revela ventas");
   var original=gerente.ObtenerRestaurante();gerente.GuardarRestaurante(original.Nombre,original.Ubicacion);Ok(true,"Gerente administra restaurante");
   var mesaNumero=700000+Random.Shared.Next(90000);gerente.CrearMesa(mesaNumero,4);
   var mesa=gerente.ListarMesas().Single(x=>x.Numero==mesaNumero);var producto=gerente.ListarMenu().First(x=>x.Precio>0);
   var pedido=mesero.CrearPedido(new SolicitudPedido{MesaId=mesa.Id,Detalles=[new(producto.Id,2)]});
   var cuenta=mesero.RevisarCuenta(pedido.Id);Ok(cuenta.ImporteOriginal==producto.Precio*2,"Nuevo pedido conserva original");
   var ajuste=new SolicitudAjuste(Guid.NewGuid(),pedido.Id,cuenta.Version,cuenta.Total,"Descuento",1,"Ajuste ficticio");
   Deniega(()=>mesero.AplicarAjuste(ajuste),"Descuento sin autorización rechazado");
   Deniega(()=>mesero.AutorizarOperacion(ajuste.OperacionId,"Ajuste",JsonSerializer.Serialize(ajuste),"m"+sufijo,Personal),"Mesero no autoriza descuento");
   mesero.AutorizarOperacion(ajuste.OperacionId,"Ajuste",JsonSerializer.Serialize(ajuste),"g"+sufijo,Personal);
   Deniega(()=>mesero.AplicarAjuste(ajuste with{Descuento=2}),"Autorización no admite cambiar importe");
   var total=mesero.AplicarAjuste(ajuste);Ok(total==cuenta.Total-1,"Descuento autorizado aplicado");
   Ok(mesero.AplicarAjuste(ajuste)==total,"Repetición devuelve resultado sin duplicar ajuste");
   Ok(Scalar("SELECT COUNT(*) FROM dbo.AjustesPedido WHERE PedidoId="+pedido.Id)==1,"Un único ajuste persistido");
   var resumen=mesero.RevisarCuenta(pedido.Id);
   var expirado=new SolicitudAjuste(Guid.NewGuid(),pedido.Id,resumen.Version,resumen.Total,"Descuento",2,"Expirado");
   mesero.AutorizarOperacion(expirado.OperacionId,"Ajuste",JsonSerializer.Serialize(expirado),"g"+sufijo,Personal);
   Sql("UPDATE dbo.AutorizacionesOperacion SET Caduca=DATEADD(minute,-1,SYSUTCDATETIME()) WHERE OperacionId=@id;",("@id",expirado.OperacionId));
   Deniega(()=>mesero.AplicarAjuste(expirado),"Autorización caducada rechazada");
   var cambio=new SolicitudAjuste(Guid.NewGuid(),pedido.Id,resumen.Version,resumen.Total,"Descuento",2,"Cambio de cuenta");
   mesero.AutorizarOperacion(cambio.OperacionId,"Ajuste",JsonSerializer.Serialize(cambio),"g"+sufijo,Personal);
   mesero.AgregarProductoAPedido(pedido.Id,producto.Id,1);
   Deniega(()=>mesero.AplicarAjuste(cambio),"Cambio del pedido invalida autorización");
   cuenta=mesero.RevisarCuenta(pedido.Id);Ok(cuenta.Descuento==0&&cuenta.Total==producto.Precio*3,"Agregar invalida descuento y restaura original");
   var cortesia=new SolicitudAjuste(Guid.NewGuid(),pedido.Id,cuenta.Version,cuenta.Total,"Cortesía",cuenta.ImporteOriginal,"Cortesía ficticia autorizada");
   mesero.AutorizarOperacion(cortesia.OperacionId,"Ajuste",JsonSerializer.Serialize(cortesia),"g"+sufijo,Personal);mesero.AplicarAjuste(cortesia);
   cuenta=mesero.RevisarCuenta(pedido.Id);Ok(cuenta.Total==0&&cuenta.ImporteOriginal>0,"Cortesía conserva consumo sin inventar dinero");
   var cobro=new SolicitudCobro(Guid.NewGuid(),pedido.Id,cuenta.Version,0,true,"Público general",null,null,null,"Cortesía",0,"",null,null,true,true,true);
   var r=mesero.RegistrarCobro(cobro);Ok(r.NumeroComprobante is not null,"Mesero cierra cortesía previamente autorizada");
   Ok(mesero.RegistrarCobro(cobro).NumeroComprobante==r.NumeroComprobante,"Cobro recuperado no se duplica");
   Ok(mesero.ConsultarComprobante(pedido.Id).Contains("Descuento"),"Comprobante conserva descuento");
   mesero.GestionarMesa(new(Guid.NewGuid(),mesa.Id,null,null,"Confirmar limpieza","Limpieza ficticia",false));Ok(mesero.ListarMesas().Single(x=>x.Id==mesa.Id).Estado=="Disponible","Mesero confirma limpieza");
   pedido=mesero.CrearPedido(new SolicitudPedido{MesaId=mesa.Id,Detalles=[new(producto.Id,1)]});cuenta=mesero.RevisarCuenta(pedido.Id);
   var cancelar=new SolicitudGestionMesa(Guid.NewGuid(),mesa.Id,pedido.Id,cuenta.Version,"Cancelar","Sin preparación ni consumo",true);
   Deniega(()=>mesero.GestionarMesa(cancelar),"Cancelación requiere gerente");
   mesero.AutorizarOperacion(cancelar.OperacionId,"GestionMesa",JsonSerializer.Serialize(cancelar),"g"+sufijo,Personal);mesero.GestionarMesa(cancelar);Ok(true,"Cancelación puntual mantiene sesión mesero");
   Ok(mesero.UsuarioActual!.Rol=="Mesero","Autorización no eleva la sesión");
   mesero.GestionarMesa(new(Guid.NewGuid(),mesa.Id,null,null,"Confirmar limpieza","Limpieza ficticia",false));
   pedido=mesero.CrearPedido(new SolicitudPedido{MesaId=mesa.Id,Detalles=[new(producto.Id,1)]});cuenta=mesero.RevisarCuenta(pedido.Id);
   var ajusteConcurrente=new SolicitudAjuste(Guid.NewGuid(),pedido.Id,cuenta.Version,cuenta.Total,"Descuento",1,"Ajuste concurrente ficticio");
   var pagoConcurrente=new SolicitudCobro(Guid.NewGuid(),pedido.Id,cuenta.Version,cuenta.Total,true,"Público general",null,null,null,"Efectivo",cuenta.Total,"",null,null,true,true);
   using(var barrera=new Barrier(2))
   {
    Task<bool> Intento(Action a)=>Task.Run(()=>{barrera.SignalAndWait(TimeSpan.FromSeconds(20));try{a();return true;}catch(InvalidOperationException){return false;}});
    var carrera=new[]{Intento(()=>gerente.AplicarAjuste(ajusteConcurrente)),Intento(()=>mesero.RegistrarCobro(pagoConcurrente))};Task.WaitAll(carrera);
    Ok(carrera.Count(x=>x.Result)==1,"Ajuste y cobro concurrentes no aplican versiones incompatibles");
   }
   if(mesero.ObtenerPedidoAbierto(mesa.Id) is not null)
   {cuenta=mesero.RevisarCuenta(pedido.Id);mesero.RegistrarCobro(pagoConcurrente with{OperacionId=Guid.NewGuid(),Version=cuenta.Version,TotalEsperado=cuenta.Total,Entregado=cuenta.Total});}
   Ok(Scalar("SELECT COUNT(*) FROM dbo.Facturas WHERE PedidoId="+pedido.Id)==1,"Carrera financiera deja un solo comprobante");
   mesero.GestionarMesa(new(Guid.NewGuid(),mesa.Id,null,null,"Confirmar limpieza","Limpieza ficticia",false));
   pedido=mesero.CrearPedido(new SolicitudPedido{MesaId=mesa.Id,Detalles=[new(producto.Id,1)]});cuenta=mesero.RevisarCuenta(pedido.Id);
   var revocada=new SolicitudAjuste(Guid.NewGuid(),pedido.Id,cuenta.Version,cuenta.Total,"Descuento",1,"Autorización revocada");
   mesero.AutorizarOperacion(revocada.OperacionId,"Ajuste",JsonSerializer.Serialize(revocada),"g"+sufijo,Personal);
   var jefeRevocado=admin.ListarUsuarios().Single(x=>x.Usuario=="g"+sufijo);
   admin.GuardarUsuario(jefeRevocado.Id,jefeRevocado.Nombre,jefeRevocado.Usuario,"Gerente",true,null);
   Deniega(()=>mesero.AplicarAjuste(revocada),"Cambio de versión del autorizador invalida aprobación pendiente");
   gerente=Repo("g"+sufijo,Personal);
   var incierto=new SolicitudCobro(Guid.NewGuid(),pedido.Id,cuenta.Version,cuenta.Total,true,"Público general",null,null,null,"Tarjeta",cuenta.Total,"Incierto","REF PRUEBA",null,true,false);
   mesero.RegistrarCobro(incierto);
   var conciliacion=new SolicitudConciliacion(Guid.NewGuid(),incierto.OperacionId,true,"Evidencia ficticia revisada");
   Deniega(()=>mesero.ConciliarTarjeta(incierto.OperacionId,true,conciliacion.Motivo,conciliacion.OperacionId),"Tarjeta incierta no se concilia sin autorización");
   mesero.AutorizarOperacion(conciliacion.OperacionId,"ConciliarTarjeta",JsonSerializer.Serialize(conciliacion),"g"+sufijo,Personal);
   var conciliado=mesero.ConciliarTarjeta(incierto.OperacionId,true,conciliacion.Motivo,conciliacion.OperacionId);Ok(conciliado.NumeroComprobante is not null,"Mesero concilia únicamente la tarjeta autorizada");
   Ok(mesero.ConciliarTarjeta(incierto.OperacionId,true,conciliacion.Motivo,conciliacion.OperacionId).NumeroComprobante==conciliado.NumeroComprobante,"Conciliación recuperada no duplica cierre");
   var usuario=admin.ListarUsuarios().Single(x=>x.Usuario=="m"+sufijo);admin.GuardarUsuario(usuario.Id,usuario.Nombre,usuario.Usuario,"Mesero",false,null);
   Deniega(()=>mesero.ListarMesas(),"Desactivación invalida sesión existente");
   var jefe=admin.ListarUsuarios().Single(x=>x.Usuario=="g"+sufijo);admin.GuardarUsuario(jefe.Id,jefe.Nombre,jefe.Usuario,"Mesero",true,null);
   Deniega(()=>gerente.CrearMesa(mesaNumero+1,2),"Cambio de rol invalida sesión gerente");
   var adm=admin.ListarUsuarios().Single(x=>x.Usuario=="admin");
   if(admin.ListarUsuarios().Count(x=>x.Rol=="Administrador"&&x.Activo)==1)Deniega(()=>admin.GuardarUsuario(adm.Id,adm.Nombre,adm.Usuario,"Mesero",true,null),"Último administrador protegido");
   admin.GuardarUsuario(null,"Bloqueo","b"+sufijo,"Mesero",true,Inicial);var bloqueo=new RepositorioRestaurante(new ConfiguracionConexionTemporal(cadena));
   for(var i=0;i<5;i++)Ok(bloqueo.IniciarSesion("b"+sufijo,"incorrecta") is null,"Intento incorrecto rechazado "+i);
   Ok(bloqueo.IniciarSesion("b"+sufijo,Inicial) is null,"Bloqueo temporal tras cinco intentos");
   PruebasRoles.PruebasConfiguracionRoles.Ejecutar(cadena,Ok);
   PruebasRoles.PruebasConcurrenciaRoles.Ejecutar(Ok);
   Render(args.Length>0?args[0]:"Evidencias");
   PruebasRoles.PruebasInterfazRoles.Ejecutar(cadena,admin,args.Length>0?args[0]:"Evidencias",Ok);
   Console.WriteLine($"TOTAL {aprobadas} comprobaciones correctas");return 0;
  }
  catch(Exception ex){Console.Error.WriteLine(ex);return 1;}
 }
 static RepositorioRestaurante Repo(string u,string p){var r=new RepositorioRestaurante(new ConfiguracionConexionTemporal(cadena));if(r.IniciarSesion(u,p) is null)throw new Exception("No autentica "+u);return r;}
 static void Sql(string sql,params (string,object)[] ps){using var c=new SqlConnection(cadena);c.Open();using var q=new SqlCommand(sql,c);foreach(var(n,v)in ps)q.Parameters.AddWithValue(n,v);q.ExecuteNonQuery();}
 static int Scalar(string sql){using var c=new SqlConnection(cadena);c.Open();using var q=new SqlCommand(sql,c);return Convert.ToInt32(q.ExecuteScalar());}
 static void Ok(bool v,string t){if(!v)throw new Exception(t);aprobadas++;Console.WriteLine("OK "+t);}
 static void Deniega(Action a,string t){try{a();}catch(Exception e)when(e is UnauthorizedAccessException or ArgumentException or InvalidOperationException){Ok(true,t);return;}throw new Exception("No rechazó: "+t);}
 static void Render(string dir)
 {
  Directory.CreateDirectory(dir);ApplicationConfiguration.Initialize();
  foreach(var f in new Form[]{new FormularioConexion(),new FormularioUsuarios(),new FormularioCambiarClave(),new FormularioPrimerAdministrador(),new FormularioAutorizar(),new FormularioAjuste(),new FormularioAcceso()})
  {using(f){f.ShowInTaskbar=false;f.StartPosition=FormStartPosition.Manual;f.Location=new Point(-20000,-20000);f.Show();Application.DoEvents();f.PerformLayout();using var b=new Bitmap(f.Width,f.Height);f.DrawToBitmap(b,new Rectangle(0,0,b.Width,b.Height));b.Save(Path.Combine(dir,f.GetType().Name+".png"));f.Hide();Ok(true,"Constructor y render "+f.GetType().Name);}}
 }
}
