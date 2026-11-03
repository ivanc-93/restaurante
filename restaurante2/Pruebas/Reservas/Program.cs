using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Restaurante.Backend.Aplicacion;
using Restaurante.Backend.Configuration;
using Restaurante.Backend.Data;
using Restaurante.Backend.Data.Repositorios;
using Restaurante.Backend.Dominio;
using Restaurante.Frontend.Formularios;
using Restaurante.Frontend.Controles;
using Restaurante.Frontend.Infraestructura.Recuperacion;
using Restaurante.Frontend.Servicios;

internal static class Program
{
    private const string Clave="Personal ficticia reservas 2026";
    private const string Temporal="Temporal ficticia reservas 2026";
    private static string cadena="",clon="",salida="";
    private static int aprobadas,numero=41000,usuarios,dpiObservado;
    private static RepositorioRestaurante admin=null!;
    private static readonly DialogosPrueba dialogos=new();

    [STAThread] private static int Main(string[] args)
    {
        try
        {
            cadena=Environment.GetEnvironmentVariable("SABORES_RESERVAS_PRUEBAS")??throw new Exception("Falta conexión aislada.");
            var cb=new SqlConnectionStringBuilder(cadena);
            if(!cb.InitialCatalog.StartsWith("Restaurante_Pruebas_Reservas_",StringComparison.Ordinal)||!cb.IntegratedSecurity||cb.AttachDBFilename.Length>0)throw new Exception("Solo fixture aislada de reservas.");
            if(args.Length==3&&args[0]=="--consultar")return Repo().ConsultarReservaMesa(int.Parse(args[1]))?.Id==int.Parse(args[2])?0:1;
            salida=args.Single();Directory.CreateDirectory(salida);
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);Application.EnableVisualStyles();
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException+=(_,e)=>{Console.Error.WriteLine(e.Exception);Environment.Exit(1);};
            var cfg=new ConfiguracionConexionTemporal(cadena);
            new ServicioInstalacion(cfg).CrearPrimerAdministrador(Environment.GetEnvironmentVariable("SABORES_TOKEN_PRUEBAS")??"","Administración ficticia","admin",Clave);
            admin=Repo();
            Ok(!admin.ReservasDisponibles(),"Esquema 007 mantiene reservas deshabilitadas sin migrar automáticamente");
            var legado=NuevaMesa();Sql("UPDATE dbo.Mesas SET Estado=N'Reservada' WHERE Id=@m;",("@m",legado.Id));
            var huella=Huella();
            var respaldo=RespaldoVerificadoSql.Verificar(cadena);
            var sufijo=Path.GetFileNameWithoutExtension(respaldo)["Sabores_Respaldo_".Length..];clon="Sabores_Validacion_"+sufijo;
            var cadenaClon=new SqlConnectionStringBuilder(cadena){InitialCatalog=clon}.ConnectionString;
            var script=File.ReadAllText(Path.Combine(Environment.GetEnvironmentVariable("SABORES_SCRIPTS_PRUEBAS")!,"Migracion_008.sql"));
            SqlEn(cadenaClon,script);SqlEn(cadenaClon,script);SqlEn(cadenaClon,$"DBCC CHECKDB([{clon}]) WITH NO_INFOMSGS;");
            Ok(true,"Respaldo, restauración aislada y ensayo repetido de migración 008 verificados");
            File.WriteAllText(Path.Combine(salida,"respaldo-ensayo.txt"),$"Base de prueba: {cb.InitialCatalog}\nRespaldo: {respaldo}\nClon: {clon}\n");
            Sql(script);Sql(script);
            Ok(Huella()==huella,"Migración repetida preserva todas las filas previas salvo versión técnica");
            admin.ValidarEsquema();Ok(admin.ReservasDisponibles(),"Esquema 008 completo habilita reservas");
            Ok(admin.ConsultarReservaMesa(legado.Id) is null&&Estado(legado.Id)=="Reservada","Mesa reservada anterior conserva estado sin inventar titular");
            Sql("DROP INDEX UX_Reserva_MesaActiva ON dbo.ReservasMesa;");
            Deniega(()=>admin.ValidarEsquema(),"008 incompleta rechaza validación en lugar de operar sin garantía única");
            Sql("CREATE UNIQUE INDEX UX_Reserva_MesaActiva ON dbo.ReservasMesa(MesaId) WHERE Estado=N'Activa';");
            var mesero=Alta("Mesero");var gerente=Alta("Gerente");var empleado=Alta("Empleado");
            var previo=Consumo();
            foreach(var actor in new[]{admin,mesero,gerente,empleado})
            {
                var mesa=NuevaMesa();var s=new SolicitudReserva(Guid.NewGuid(),mesa.Id,"María O'Connor · reserva ficticia");
                var r=actor.ReservarMesa(s);
                Ok(r.Nombre==s.Nombre&&Estado(mesa.Id)=="Reservada","Reserva válida por "+actor.UsuarioActual!.Rol);
                Ok(actor.ReservarMesa(s).Id==r.Id,"Reintento no duplica reserva: "+actor.UsuarioActual!.Rol);
                Deniega(()=>actor.ReservarMesa(s with{Nombre="Otro nombre"}),"GUID vinculado a nombre original");
            }
            Ok(Consumo()==previo,"Reservar no crea cliente, pedido, pago ni factura");
            var vacia=NuevaMesa();
            foreach(var nombre in new[]{"","   ",new string('x',121),"Nombre\nNo válido"})Deniega(()=>admin.ReservarMesa(new(Guid.NewGuid(),vacia.Id,nombre)),"Nombre inválido rechazado sin modificar mesa");
            var maximo=new string('á',120);var mesaMax=NuevaMesa();var reservaMax=admin.ReservarMesa(new(Guid.NewGuid(),mesaMax.Id,maximo));
            Ok(reservaMax.Nombre==maximo&&Math.Abs((reservaMax.Fecha-DateTime.UtcNow.AddHours(-6)).TotalSeconds)<90,"Nombre de 120 caracteres y fecha Guatemala conservados");
            Deniega(()=>new RepositorioRestaurante(cfg).ReservarMesa(new(Guid.NewGuid(),vacia.Id,"Sin sesión")),"Sesión ausente rechazada");
            var temporal=Alta("Mesero",false);Deniega(()=>temporal.ReservarMesa(new(Guid.NewGuid(),vacia.Id,"Cambio pendiente")),"Cambio obligatorio bloquea reserva");
            var desactivado=Alta("Mesero");var usuario=admin.ListarUsuarios().Single(u=>u.Id==desactivado.UsuarioActual!.Id);
            admin.GuardarUsuario(usuario.Id,usuario.Nombre,usuario.Usuario,"Mesero",false,null);
            Deniega(()=>desactivado.ReservarMesa(new(Guid.NewGuid(),vacia.Id,"Desactivado")),"Desactivación invalida sesión existente");
            var rolCambiado=Alta("Gerente");usuario=admin.ListarUsuarios().Single(u=>u.Id==rolCambiado.UsuarioActual!.Id);admin.GuardarUsuario(usuario.Id,usuario.Nombre,usuario.Usuario,"Mesero",true,null);
            Deniega(()=>rolCambiado.ReservarMesa(new(Guid.NewGuid(),vacia.Id,"Rol cambiado")),"Cambio de rol invalida sesión anterior");
            var producto=admin.ListarMenu().First(p=>p.Precio>0);
            var ocupada=NuevaMesa();var pedido=admin.CrearPedido(new(){MesaId=ocupada.Id,Detalles=[new(producto.Id,1)]});
            Deniega(()=>admin.ReservarMesa(new(Guid.NewGuid(),ocupada.Id,"Ocupada")),"No reserva una mesa ocupada");
            Sql("UPDATE dbo.Mesas SET Estado=N'Disponible' WHERE Id=@m;",("@m",ocupada.Id));
            Deniega(()=>admin.ReservarMesa(new(Guid.NewGuid(),ocupada.Id,"Inconsistente")),"Disponibilidad inconsistente con pedido abierto rechazada");
            Sql("UPDATE dbo.Mesas SET Estado=N'Ocupada' WHERE Id=@m;",("@m",ocupada.Id));
            var cuenta=admin.RevisarCuenta(pedido.Id);admin.RegistrarCobro(new(Guid.NewGuid(),pedido.Id,cuenta.Version,cuenta.Total,true,"Público general",null,null,null,"Efectivo",cuenta.Total,"",null,"Pago ficticio",true,true));
            Deniega(()=>admin.ReservarMesa(new(Guid.NewGuid(),ocupada.Id,"Limpieza")),"No reserva una mesa en limpieza");
            Deniega(()=>admin.ReservarMesa(new(Guid.NewGuid(),mesaMax.Id,"Ya reservada")),"No reemplaza reserva activa");
            Deniega(()=>admin.ActualizarMesa(mesaMax.Id,mesaMax.Numero,4,"Disponible"),"Catálogo no libera reserva sin historial");
            Deniega(()=>admin.ActualizarMesa(mesaMax.Id,mesaMax.Numero+1,4,"Reservada"),"Catálogo no renumera una reserva activa");
            Deniega(()=>admin.EliminarMesa(mesaMax.Id),"No elimina mesa reservada");
            var crearManual=NuevaMesa();Deniega(()=>admin.ActualizarMesa(crearManual.Id,crearManual.Numero,4,"Reservada"),"Catálogo no crea reserva sin titular");
            Concurrencia(producto.Id);
            var paraCancelar=NuevaMesa();var rCancel=mesero.ReservarMesa(new(Guid.NewGuid(),paraCancelar.Id,"Cancelar con gerente"));
            var cancel=new SolicitudCancelacionReserva(Guid.NewGuid(),rCancel.Id,paraCancelar.Id,rCancel.Version,"Persona no asistirá");
            Deniega(()=>mesero.CancelarReserva(cancel),"Mesero no cancela sin aprobación puntual");
            var gestor=admin.ListarUsuarios().Single(u=>u.Id==gerente.UsuarioActual!.Id);
            var permiso=mesero.IntentarAutorizarOperacion(cancel.OperacionId,"CancelarReserva",JsonSerializer.Serialize(cancel),gestor.Usuario,Clave);
            Ok(permiso.Autorizada,"Gerente autoriza exclusivamente esa cancelación");
            mesero.CancelarReserva(cancel);mesero.CancelarReserva(cancel);
            Ok(Estado(paraCancelar.Id)=="Disponible"&&Convert.ToInt32(Sql("SELECT COUNT(*) FROM dbo.ReservasMesa WHERE Id=@r AND Estado=N'Cancelada' AND UsuarioCierre=@u AND MotivoCierre=@motivo;",("@r",rCancel.Id),("@u",mesero.UsuarioActual!.Id),("@motivo",cancel.Motivo)))==1,"Cancelación recuperable conserva motivo, actor e historial");
            Ok(Convert.ToInt32(Sql("SELECT COUNT(*) FROM dbo.AuditoriaOperaciones WHERE OperacionId=@op AND AutorizadorId=@j;",("@op",cancel.OperacionId),("@j",gerente.UsuarioActual!.Id)))==1,"Historial identifica al autorizador");
            var cambio=NuevaMesa();var rc=mesero.ReservarMesa(new(Guid.NewGuid(),cambio.Id,"Solicitud inmutable"));var sc=new SolicitudCancelacionReserva(Guid.NewGuid(),rc.Id,cambio.Id,rc.Version,"Motivo original");
            mesero.AutorizarOperacion(sc.OperacionId,"CancelarReserva",JsonSerializer.Serialize(sc),gestor.Usuario,Clave);
            Deniega(()=>mesero.CancelarReserva(sc with{Motivo="Motivo diferente"}),"Autorización no sirve para otros datos");
            admin.GuardarUsuario(gestor.Id,gestor.Nombre,gestor.Usuario,"Gerente",false,null);
            Deniega(()=>mesero.CancelarReserva(sc),"Desactivar autorizador revoca aprobación pendiente");
            var atender=NuevaMesa();var ra=admin.ReservarMesa(new(Guid.NewGuid(),atender.Id,"Titular no es receptor fiscal"));
            var sa=new SolicitudAtencionReserva(Guid.NewGuid(),ra.Id,atender.Id,ra.Version,null,[new(producto.Id,2)]);
            var pa=mesero.AtenderReserva(sa);Ok(mesero.AtenderReserva(sa).Id==pa.Id,"Atención repetida recupera el mismo pedido");
            Ok(Estado(atender.Id)=="Ocupada"&&admin.ConsultarReservaMesa(atender.Id) is null,"Primer producto atiende reserva y ocupa mesa atómicamente");
            Ok(Sql("SELECT ClienteHistorico FROM dbo.Pedidos WHERE Id=@p;",("@p",pa.Id)) as string=="Público general","Titular no se convierte en receptor de comprobante");
            Ok(Convert.ToInt32(Sql("SELECT COUNT(*) FROM dbo.ReservasMesa WHERE Id=@r AND Estado=N'Atendida' AND PedidoId=@p;",("@r",ra.Id),("@p",pa.Id)))==1,"Historial vincula reserva y pedido");
            Deniega(()=>admin.CancelarReserva(new(Guid.NewGuid(),ra.Id,atender.Id,ra.Version,"Después de consumo")),"No cancela reserva atendida ni oculta consumo");
            var rev=admin.RevisarCuenta(pa.Id);admin.RegistrarCobro(new(Guid.NewGuid(),pa.Id,rev.Version,rev.Total,true,"Público general",null,null,null,"Efectivo",rev.Total+5,"",null,"Pago de reserva atendida",true,true));
            admin.GestionarMesa(new(Guid.NewGuid(),atender.Id,null,null,"Confirmar limpieza","Limpieza ficticia",false));Ok(Estado(atender.Id)=="Disponible","Cobro y limpieza siguen funcionando después de atender reserva");
            RollbackYRecuperacion();
            var psi=new ProcessStartInfo(Environment.ProcessPath!){UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};psi.ArgumentList.Add("--consultar");psi.ArgumentList.Add(mesaMax.Id.ToString());psi.ArgumentList.Add(reservaMax.Id.ToString());
            using(var proceso=Process.Start(psi)!){if(!proceso.WaitForExit(30000))throw new Exception("Proceso de persistencia no terminó.");Ok(proceso.ExitCode==0,"Otro proceso recupera la reserva desde SQL");}
            Interfaz(legado,maximo);
            Sql("DBCC CHECKDB WITH NO_INFOMSGS;");Ok(true,"Integridad SQL final verificada");
            Console.WriteLine($"TOTAL {aprobadas} comprobaciones de reservas; DPI observado {dpiObservado}, sin modificar escalado de Windows.");return 0;
        }
        catch(Exception ex){Console.Error.WriteLine(ex);return 1;}
    }

    private static void Concurrencia(int producto)
    {
        var mesa=NuevaMesa();
        var carreras=Enumerable.Range(0,2).Select(i=>Task.Run(()=>{try{Repo().ReservarMesa(new(Guid.NewGuid(),mesa.Id,"Carrera "+i));return true;}catch(InvalidOperationException){return false;}})).ToArray();Task.WaitAll(carreras);
        Ok(carreras.Count(t=>t.Result)==1&&Estado(mesa.Id)=="Reservada","Dos reservas concurrentes producen una sola ganadora");
        mesa=NuevaMesa();var id=mesa.Id;
        var reservar=Task.Run(()=>{try{Repo().ReservarMesa(new(Guid.NewGuid(),id,"Reserva contra apertura"));return true;}catch(InvalidOperationException){return false;}});
        var abrir=Task.Run(()=>{try{Repo().CrearPedido(new(){MesaId=id,Detalles=[new(producto,1)]});return true;}catch(InvalidOperationException){return false;}});Task.WaitAll(reservar,abrir);
        Ok(reservar.Result!=abrir.Result,"Reserva y apertura simultáneas son excluyentes");
        mesa=NuevaMesa();var r=admin.ReservarMesa(new(Guid.NewGuid(),mesa.Id,"Atender contra cancelar"));id=mesa.Id;
        var atender=Task.Run(()=>{try{Repo().AtenderReserva(new(Guid.NewGuid(),r.Id,id,r.Version,null,[new(producto,1)]));return true;}catch(InvalidOperationException){return false;}});
        var cancelar=Task.Run(()=>{try{Repo().CancelarReserva(new(Guid.NewGuid(),r.Id,id,r.Version,"Cancelación concurrente"));return true;}catch(InvalidOperationException){return false;}});Task.WaitAll(atender,cancelar);
        Ok(atender.Result!=cancelar.Result,"Atender y cancelar simultáneamente conservan un solo desenlace");
    }

    private static void RollbackYRecuperacion()
    {
        var mesa=NuevaMesa();var id=Guid.NewGuid();
        Sql("CREATE OR ALTER TRIGGER dbo.FalloReservaPrueba ON dbo.ReservasMesa AFTER INSERT AS THROW 51700,N'Fallo controlado de fixture',1;");
        try{admin.ReservarMesa(new(id,mesa.Id,"Rollback ficticio"));throw new Exception("No falló fixture");}catch(SqlException e)when(e.Number==51700){}
        finally{Sql("DROP TRIGGER dbo.FalloReservaPrueba;");}
        Ok(Estado(mesa.Id)=="Disponible"&&Convert.ToInt32(Sql("SELECT COUNT(*) FROM dbo.OperacionesRestaurante WHERE Id=@op;",("@op",id)))==0,"Fallo SQL revierte reserva, estado y operación");
        var solicitud=new SolicitudReserva(Guid.NewGuid(),mesa.Id,"Respuesta perdida");
        var perdida=DiarioOperaciones.Ejecutar<ReservaResumen>(admin,"Reservar",solicitud.OperacionId,solicitud,()=>{admin.ReservarMesa(solicitud);throw new IOException("Respuesta perdida ficticia");});Esperar(()=>perdida.IsCompleted);try{perdida.GetAwaiter().GetResult();}catch(IOException){}
        var pendiente=DiarioOperaciones.Listar(admin).Single(p=>p.Id==solicitud.OperacionId);
        var recuperar=DiarioOperaciones.Reanudar(admin,pendiente);Esperar(()=>recuperar.IsCompleted);recuperar.GetAwaiter().GetResult();
        Ok(Convert.ToInt32(Sql("SELECT COUNT(*) FROM dbo.ReservasMesa WHERE OperacionId=@op;",("@op",solicitud.OperacionId)))==1&&!DiarioOperaciones.TienePendiente(admin,solicitud.OperacionId),"Respuesta perdida se recupera con el mismo GUID sin duplicar reserva");
    }

    private static void Interfaz(Mesa legado,string nombreLargo)
    {
        var mesa=NuevaMesa();
        using(var principal=new FormularioPrincipal(admin,new ConfiguracionConexionTemporal(cadena),admin.UsuarioActual!,dialogos))
        {
            Mostrar(principal);dpiObservado=principal.DeviceDpi;Esperar(()=>Campo<FlowLayoutPanel>(principal,"flpNavegacion").Enabled);Ejecutar(principal,"MostrarMesasPos");
            Ok(!Controles(principal).Any(c=>c.Name=="flpLeyenda"),"Leyenda general retirada; estados permanecen en tarjetas");
            var buscar=Controles(principal).OfType<TextBox>().Single(c=>c.Name=="ctBuscarMesa");buscar.Text=mesa.Numero.ToString();Application.DoEvents();
            var tarjeta=Controles(principal).OfType<TarjetaMesa>().Single();Ok(tarjeta.btnReservarMesa.Visible&&tarjeta.btnReservarMesa.Enabled,"Mesa disponible muestra acción secundaria de reserva");
            Modal<FormularioReservarMesa>(()=>tarjeta.btnReservarMesa.PerformClick(),f=>
            {
                Pulsar(f,"btnReservarMesa");Ok(Campo<Label>(f,"lblMensaje").Text.Contains("nombre"),"Formulario valida titular antes de SQL");
                Campo<TextBox>(f,"ctNombreReserva").Text=nombreLargo;Campo<CheckBox>(f,"chkConfirmar").Checked=true;
                Ok(f.ClientRectangle.Contains(f.RectangleToClient(Campo<Button>(f,"btnReservarMesa").RectangleToScreen(Campo<Button>(f,"btnReservarMesa").ClientRectangle))),"Confirmación visible sin desplazarse al pie");Capturar(f,"formulario-reservar");Pulsar(f,"btnReservarMesa");Esperar(()=>f.DialogResult==DialogResult.OK);
            });Esperar(()=>Campo<FlowLayoutPanel>(principal,"flpNavegacion").Enabled);
            var reserva=admin.ConsultarReservaMesa(mesa.Id)!;buscar=Controles(principal).OfType<TextBox>().Single(c=>c.Name=="ctBuscarMesa");buscar.Text=mesa.Numero.ToString();Application.DoEvents();tarjeta=Controles(principal).OfType<TarjetaMesa>().Single();
            Ok(!tarjeta.btnReservarMesa.Visible&&tarjeta.btnAccionMesa.Text=="Ver reserva","Mesa reservada no vuelve a ofrecer reservar");
            var filtro=Controles(principal).OfType<ComboBox>().Single(c=>c.Name=="cbFiltroEstadoMesa");filtro.SelectedItem="Reservada";Application.DoEvents();
            Ok(Controles(principal).OfType<TarjetaMesa>().Count()==1,"Búsqueda y filtro incluyen reserva nueva");Capturar(principal,"mesas-reserva-1366");principal.Size=new Size(1920,1080);Application.DoEvents();Capturar(principal,"mesas-reserva-1920");
            Modal<FormularioReservaDetalle>(()=>tarjeta.btnAccionMesa.PerformClick(),f=>
            {
                Ok(Campo<TextBox>(f,"ctTitular").Text==nombreLargo&&Campo<TextBox>(f,"ctTitular").ReadOnly,"Consulta conserva el nombre completo sin editarlo");Capturar(f,"consulta-reserva");
                Modal<FormularioDetalleMesa>(()=>Pulsar(f,"btnAtenderReserva"),d=>{Esperar(()=>Campo<Button>(d,"btnAgregarProducto").Enabled);Ok(admin.ConsultarReservaMesa(mesa.Id) is not null,"Abrir atención no crea consumo");Pulsar(d,"btnAgregarProducto");Esperar(()=>Campo<PedidoResumen?>(d,"_pedido") is not null&&!Campo<bool>(d,"_ocupado"));d.Close();});Esperar(()=>f.DialogResult==DialogResult.OK);
            });Esperar(()=>Campo<FlowLayoutPanel>(principal,"flpNavegacion").Enabled);
            Ok(Estado(mesa.Id)=="Ocupada","Primer producto desde la interfaz atiende la reserva");principal.Close();
        }
        using(var anterior=new FormularioReservaDetalle(admin,legado,null,dialogos)){Mostrar(anterior);Ok(!Campo<Button>(anterior,"btnAtenderReserva").Enabled&&!Campo<Button>(anterior,"btnCancelarReserva").Enabled,"Reserva anterior es consultable sin liberar ni inventar titular");Capturar(anterior,"reserva-anterior");anterior.Close();}
        var cancelar=NuevaMesa();var rc=admin.ReservarMesa(new(Guid.NewGuid(),cancelar.Id,"Cancelación desde interfaz"));
        using(var f=new FormularioReservaDetalle(admin,cancelar,rc,dialogos)){Mostrar(f);Pulsar(f,"btnCancelarReserva");Ok(Campo<Label>(f,"lblMensaje").Text.Contains("motivo"),"Cancelación exige motivo");Campo<TextBox>(f,"ctMotivo").Text="Cliente llamó para cancelar";Pulsar(f,"btnCancelarReserva");Esperar(()=>f.DialogResult==DialogResult.OK);Ok(Estado(cancelar.Id)=="Disponible","Cancelación desde controles conserva flujo operativo");}
    }

    private static RepositorioRestaurante Repo(string usuario="admin",string clave=Clave){var r=new RepositorioRestaurante(new ConfiguracionConexionTemporal(cadena));if(r.IniciarSesion(usuario,clave) is null)throw new Exception("No autenticó fixture");return r;}
    private static RepositorioRestaurante Alta(string rol,bool cambiar=true){var u="usuario"+(++usuarios);admin.GuardarUsuario(null,"Persona ficticia",u,rol,true,Temporal);var r=Repo(u,Temporal);if(cambiar){r.CambiarMiContrasena(Temporal,Clave);r=Repo(u);}return r;}
    private static Mesa NuevaMesa(){admin.CrearMesa(++numero,4);return admin.ListarMesas().Single(m=>m.Numero==numero);}
    private static string Estado(int id)=>(string)Sql("SELECT Estado FROM dbo.Mesas WHERE Id=@id;",("@id",id))!;
    private static string Consumo()=>string.Join("/",new[]{"Clientes","Pedidos","PagosManuales","Facturas"}.Select(t=>Sql($"SELECT COUNT(*) FROM dbo.{t};")));
    private static string Huella(){using var c=new SqlConnection(cadena);c.Open();using var q=new SqlCommand("SELECT name FROM sys.tables WHERE schema_id=SCHEMA_ID(N'dbo') AND name NOT IN(N'VersionEsquema',N'ReservasMesa') ORDER BY name;",c);var tablas=new List<string>();using(var r=q.ExecuteReader())while(r.Read())tablas.Add(r.GetString(0));return string.Join(";",tablas.Select(t=>{var quoted="["+t.Replace("]","]]")+"]";using var x=new SqlCommand($"SELECT CONCAT(COUNT_BIG(*),N':',COALESCE(CHECKSUM_AGG(BINARY_CHECKSUM(*)),0)) FROM dbo.{quoted};",c);return t+":"+x.ExecuteScalar();}));}
    private static object? Sql(string sql,params(string,object?)[] datos)=>SqlEn(cadena,sql,datos);
    private static object? SqlEn(string destino,string sql,params(string,object?)[] datos){var cb=new SqlConnectionStringBuilder(destino);if(!cb.InitialCatalog.StartsWith("Restaurante_Pruebas_Reservas_",StringComparison.Ordinal)&&cb.InitialCatalog!=clon)throw new Exception("Destino no permitido");using var c=new SqlConnection(destino);c.Open();using var q=new SqlCommand(sql,c){CommandTimeout=120};foreach(var(n,v)in datos)q.Parameters.AddWithValue(n,v??DBNull.Value);return q.ExecuteScalar();}
    private static void Ok(bool condicion,string nombre){if(!condicion)throw new Exception(nombre);Console.WriteLine("OK "+nombre);aprobadas++;}
    private static void Deniega(Action accion,string nombre){try{accion();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or UnauthorizedAccessException){Ok(true,nombre);return;}throw new Exception("No rechazó: "+nombre);}
    private static T Campo<T>(Form f,string nombre)=>(T)f.GetType().GetField(nombre,BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(f)!;
    private static IEnumerable<Control> Controles(Control c){foreach(Control h in c.Controls){yield return h;foreach(var x in Controles(h))yield return x;}}
    private static void Mostrar(Form f){f.StartPosition=FormStartPosition.Manual;f.Location=new Point(0,0);f.Size=new Size(1366,768);f.Show();SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());Application.DoEvents();}
    private static void Pulsar(Form f,string campo){SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());Campo<Button>(f,campo).PerformClick();}
    private static void Ejecutar(Form f,string metodo){SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());var t=(Task)f.GetType().GetMethod(metodo,BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(f,null)!;Esperar(()=>t.IsCompleted);t.GetAwaiter().GetResult();}
    private static void Esperar(Func<bool> condicion){var reloj=Stopwatch.StartNew();while(!condicion()){if(reloj.Elapsed.TotalSeconds>45)throw new TimeoutException("Operación de prueba no terminó.");Application.DoEvents();Thread.Sleep(10);}Application.DoEvents();}
    private static void Capturar(Form f,string nombre){Application.DoEvents();using var b=new Bitmap(f.Width,f.Height);f.DrawToBitmap(b,new Rectangle(Point.Empty,b.Size));b.Save(Path.Combine(salida,nombre+".png"));}
    private static void Modal<T>(Action abrir,Action<T> completar)where T:Form{bool atendido=false;using var timer=new System.Windows.Forms.Timer{Interval=80};timer.Tick+=(_,_)=>{var f=Application.OpenForms.OfType<T>().FirstOrDefault();if(f is null||!f.Visible||atendido)return;atendido=true;try{completar(f);}catch(Exception e){Console.Error.WriteLine(e);Environment.Exit(1);}};timer.Start();SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());abrir();Esperar(()=>atendido&&!Application.OpenForms.OfType<T>().Any());SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());}
    private sealed class DialogosPrueba:IDialogosUsuario{public DialogResult Mostrar(IWin32Window p,string m,string t,MessageBoxButtons b=MessageBoxButtons.OK,MessageBoxIcon i=MessageBoxIcon.None)=>b==MessageBoxButtons.YesNo?DialogResult.Yes:DialogResult.OK;}
}
