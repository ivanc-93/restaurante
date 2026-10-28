using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.Data.SqlClient;
using Restaurante.Backend.Servicios;
using Restaurante.Backend.Dominio;
using Restaurante.Frontend.Formularios;
using Sabores.Mantenimiento;
namespace PruebasRoles;
internal static class PruebasRecuperacion
{
    private const string Inicial="PruebaPersonal_2026!",Nueva="RecuperadaIndividual2026!",Temporal="TemporalIndividual2026!";
    public static void Ejecutar(string cadena,RepositorioRestaurante admin,string dir,Action<bool,string> ok)
    {
        if(!new SqlConnectionStringBuilder(cadena).InitialCatalog.StartsWith("Restaurante_Pruebas_"))throw new Exception("Solo base aislada.");
        Directory.CreateDirectory(dir);ApplicationConfiguration.Initialize();
        object? Sql(string texto,params (string,object)[] datos){using var c=new SqlConnection(cadena);c.Open();using var q=new SqlCommand(texto,c){CommandTimeout=120};foreach(var (n,v) in datos)q.Parameters.AddWithValue(n,v);return q.ExecuteScalar();}
        RepositorioRestaurante Repo(string? u=null,string? p=null){var r=new RepositorioRestaurante(new ConfiguracionConexionTemporal(cadena));if(u is not null&&r.IniciarSesion(u,p!) is null)throw new Exception("No autenticó fixture");return r;}
        void Deniega(Action a,string texto){try{a();}catch(Exception e)when(e is UnauthorizedAccessException or InvalidOperationException){ok(true,texto);return;}throw new Exception(texto);}
        string Recibo()=>Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        string Codigo(RepositorioRestaurante r,string p){var k=r.GenerarCodigoRecuperacion(p);r.ConfirmarCodigoRecuperacion(k);return k;}
        var publico=Repo();Deniega(()=>publico.RecuperarConCodigo(Guid.NewGuid(),Recibo(),"admin","x",Nueva),"Esquema anterior informa migración sin reiniciar usuarios");
        var respaldo=ActualizacionRecuperacion.Ejecutar(cadena,Path.GetFullPath(Path.Combine(dir,"../../BaseDatos/Migracion_006.sql")));ok(File.Exists(respaldo),"Respaldo con checksum restaurado y CHECKDB antes de migrar");
        var migracion=File.ReadAllText(Path.GetFullPath(Path.Combine(dir,"../../BaseDatos/Migracion_006.sql")));
        Sql(migracion);Sql(migracion);admin.ValidarEsquema();ok(Convert.ToInt32(Sql("SELECT MAX(Version) FROM dbo.VersionEsquema"))==6,"Migración 006 idempotente y esquema compatible");
        var numero=800000+Random.Shared.Next(90000);admin.CrearMesa(numero,4);var mesa=admin.ListarMesas().Single(x=>x.Numero==numero);var producto=admin.ListarMenu().First(x=>x.Precio>0);
        var pedido=admin.CrearPedido(new SolicitudPedido{MesaId=mesa.Id,Detalles=[new(producto.Id,2)]});var cuenta=admin.RevisarCuenta(pedido.Id);
        admin.RegistrarCobro(new SolicitudCobro(Guid.NewGuid(),pedido.Id,cuenta.Version,cuenta.Total,true,"Consumidor final",null,null,null,"Efectivo",cuenta.Total,"",null,"Prueba aislada",true,true));
        var ventasAntes=Convert.ToDecimal(Sql("SELECT SUM(Total) FROM dbo.Facturas"));ok(ventasAntes>0,"Esquema 6 mantiene pedido y cobro manual GTQ");
        var instalacion=(Guid)Sql("SELECT InstalacionId FROM dbo.RecuperacionInstalacion")!;
        ok(admin.NecesitaCodigoRecuperacion(),"Primer Administrador requiere guardar código");
        var k=admin.GenerarCodigoRecuperacion(Inicial);
        ok(!publico.RecuperarConCodigo(Guid.NewGuid(),Recibo(),"admin",k,Nueva).Correcto,"Código sin confirmación de custodia no recupera");
        admin.ConfirmarCodigoRecuperacion(k);ok(!admin.NecesitaCodigoRecuperacion(),"Confirmación habilita código guardado");
        ok(k.Length==64&&!Convert.ToHexString((byte[])Sql("SELECT Verificador FROM dbo.CodigosRecuperacion")!).Equals(k,StringComparison.Ordinal),"SQL conserva verificador y no código en claro");
        Sql("UPDATE dbo.RecuperacionInstalacion SET InstalacionId=NEWID()");ok(!publico.RecuperarConCodigo(Guid.NewGuid(),Recibo(),"admin",k,Nueva).Correcto,"Código de otra identidad de instalación rechazado");Sql("UPDATE dbo.RecuperacionInstalacion SET InstalacionId=@i",("@i",instalacion));
        var op=Guid.NewGuid();var recibo=Recibo();ok(publico.RecuperarConCodigo(op,recibo,"admin",k,Nueva).Correcto,"Único Administrador recupera sin sesión previa");
        ok(publico.UsuarioActual is null,"Recuperación no inicia ni eleva sesión");
        Deniega(()=>admin.ListarUsuarios(),"Recuperación invalida sesión administrativa anterior");
        ok(Repo().IniciarSesion("admin",Inicial) is null&&Repo().IniciarSesion("admin",Nueva) is not null,"Clave anterior rechazada y nueva aceptada");
        ok(publico.RecuperarConCodigo(op,recibo,"admin",k,Nueva).Correcto,"Repetición original recupera resultado sin otro cambio");
        ok(publico.ConsultarRecuperacion(op,recibo).Correcto&&!publico.ConsultarRecuperacion(op,Recibo()).Correcto,"Consulta necesita comprobante secreto y no solo identificador");
        ok(!publico.RecuperarConCodigo(Guid.NewGuid(),Recibo(),"admin",k,Temporal).Correcto,"Código consumido no permite otra recuperación");
        admin=Repo("admin",Nueva);ok(admin.NecesitaCodigoRecuperacion(),"Después de recuperación exige reemplazo");
        k=Codigo(admin,Nueva);var viejo=k;k=Codigo(admin,Nueva);
        ok(!publico.RecuperarConCodigo(Guid.NewGuid(),Recibo(),"admin",viejo,Temporal).Correcto,"Rotación revoca código anterior");
        void Usuario(string u,string rol,bool activo=true){admin.GuardarUsuario(null,u,u,rol,activo,Inicial);if(activo)Repo(u,Inicial).CambiarMiContrasena(Inicial,Temporal);}
        Usuario("otroadmin","Administrador");Usuario("mesero","Mesero");Usuario("gerente","Gerente");Usuario("inactivo","Administrador",false);admin.GuardarUsuario(null,"Pendiente","pendiente","Administrador",true,Inicial);
        var mesero=Repo("mesero",Temporal);Deniega(()=>mesero.GenerarCodigoRecuperacion(Temporal),"Mesero no genera código administrativo");
        var error=publico.RecuperarConAdministrador(Guid.NewGuid(),Recibo(),"mesero","otroadmin","incorrecta",Nueva);
        ok(!error.Correcto,"Contraseña administrativa incorrecta rechazada");
        ok(!publico.RecuperarConAdministrador(Guid.NewGuid(),Recibo(),"mesero","inactivo",Inicial,Nueva).Correcto,"Administrador inactivo no autoriza");
        ok(publico.RecuperarConAdministrador(Guid.NewGuid(),Recibo(),"mesero","pendiente",Inicial,Nueva).Mensaje.Contains("Cambia"),"Cambio obligatorio requiere completar acceso");
        ok(!publico.RecuperarConAdministrador(Guid.NewGuid(),Recibo(),"mesero","gerente",Temporal,Nueva).Correcto,"Gerente no restablece cuentas");
        ok(!publico.RecuperarConAdministrador(Guid.NewGuid(),Recibo(),"admin","admin",Nueva,Temporal).Correcto,"Recuperación asistida exige otro Administrador");
        ok(!publico.RecuperarConAdministrador(Guid.NewGuid(),Recibo(),"inactivo","otroadmin",Temporal,Nueva).Correcto,"No reactiva cuenta inactiva");
        ok(!publico.RecuperarConAdministrador(Guid.NewGuid(),Recibo(),"mesero","otroadmin",Temporal,"corta").Correcto,"Política de contraseña nueva conserva mínimo");
        ok(!publico.RecuperarConAdministrador(Guid.NewGuid(),Recibo(),"mesero","otroadmin",Temporal,new string('a',129)).Correcto,"Rechaza contraseña nueva de más de 128 caracteres");
        Sql("INSERT dbo.AutorizacionesOperacion(OperacionId,Tipo,SolicitudHash,SolicitanteId,AutorizadorId,VersionAutorizador,Caduca) SELECT NEWID(),N'Ajuste',REPLICATE('A',64),s.Id,a.Id,a.VersionSeguridad,DATEADD(minute,2,SYSUTCDATETIME()) FROM dbo.Usuarios s CROSS JOIN dbo.Usuarios a WHERE s.Usuario=N'mesero' AND a.Usuario=N'otroadmin';");
        var consumo=Convert.ToInt32(Sql("SELECT COUNT(*) FROM dbo.Pedidos"));
        foreach(var objetivo in new[]{"mesero","gerente","admin"})ok(publico.RecuperarConAdministrador(Guid.NewGuid(),Recibo(),objetivo,"otroadmin",Temporal,objetivo=="admin"?Inicial:Nueva).Correcto,"Otro Administrador recupera "+objetivo);
        Deniega(()=>mesero.ListarMesas(),"Sesión de mesero recuperado queda invalidada");
        ok(Convert.ToInt32(Sql("SELECT COUNT(*) FROM dbo.AutorizacionesOperacion WHERE Consumida=0 AND SolicitanteId=(SELECT Id FROM dbo.Usuarios WHERE Usuario=N'mesero')"))==0,"Recuperación revoca aprobaciones pendientes del solicitante");
        ok(Convert.ToInt32(Sql("SELECT COUNT(*) FROM dbo.Pedidos"))==consumo&&(string)Sql("SELECT Rol FROM dbo.Usuarios WHERE Usuario=N'mesero'")! =="Mesero"&&!(bool)Sql("SELECT Activo FROM dbo.Usuarios WHERE Usuario=N'inactivo'")!,"Conserva consumo roles y estado activo");
        admin=Repo("admin",Inicial);k=Codigo(admin,Inicial);
        for(var i=0;i<5;i++)publico.RecuperarConCodigo(Guid.NewGuid(),Recibo(),"admin","incorrecto",Nueva);
        ok(!publico.RecuperarConCodigo(Guid.NewGuid(),Recibo(),"admin",k,Nueva).Correcto&&Repo().IniciarSesion("admin",Inicial) is not null,"Pausa de recuperación independiente del acceso normal");
        Sql("UPDATE dbo.CodigosRecuperacion SET PausaHasta=DATEADD(minute,-1,SYSUTCDATETIME()) WHERE UsuarioId=(SELECT Id FROM dbo.Usuarios WHERE Usuario=N'admin')");
        var tareas=new[]{Task.Run(()=>Repo().RecuperarConCodigo(Guid.NewGuid(),Recibo(),"admin",k,Nueva)),Task.Run(()=>Repo().RecuperarConCodigo(Guid.NewGuid(),Recibo(),"admin",k,Temporal))};Task.WaitAll(tareas);
        ok(tareas.Count(x=>x.Result.Correcto)==1,"Dos recuperaciones concurrentes consumen código una sola vez");
        var claveAdmin=tareas[0].Result.Correcto?Nueva:Temporal;admin=Repo("admin",claveAdmin);k=Codigo(admin,claveAdmin);
        var carpeta=Path.Combine(Path.GetTempPath(),"SaboresRecuperacionPruebas_"+Guid.NewGuid().ToString("N"));
        var antesCancel=(string)Sql("SELECT ContrasenaHash FROM dbo.Usuarios WHERE Usuario=N'admin'")!;
        using(var f=new FormularioRecuperarClave(publico,carpeta)){Mostrar(f);Campo<ComboBox>(f,"metodo").SelectedIndex=1;Campo<TextBox>(f,"usuario").Text="admin";Campo<TextBox>(f,"codigo").Text=k;Campo<Button>(f,"cancelar").PerformClick();}
        ok((string)Sql("SELECT ContrasenaHash FROM dbo.Usuarios WHERE Usuario=N'admin'")! ==antesCancel,"Cancelar recuperación no cambia credenciales ni consume código");
        using(var f=new FormularioRecuperarClave(publico,carpeta))
        {
            Mostrar(f);Campo<Button>(f,"recuperar").PerformClick();ok(Campo<Label>(f,"mensaje").Text.Contains("usuario"),"Interfaz valida campos vacíos antes de SQL");
            Campo<ComboBox>(f,"metodo").SelectedIndex=1;Campo<TextBox>(f,"usuario").Text="admin";Campo<TextBox>(f,"codigo").Text="incorrecto";Campo<TextBox>(f,"nueva").Text=Inicial;Campo<TextBox>(f,"confirmacion").Text="diferente";Campo<Button>(f,"recuperar").PerformClick();ok(Campo<Label>(f,"mensaje").Text.Contains("coincid"),"Interfaz exige confirmación coincidente");
            Campo<TextBox>(f,"confirmacion").Text=Inicial;Campo<Button>(f,"recuperar").PerformClick();Esperar(()=>Campo<Button>(f,"recuperar").Enabled);ok(Campo<TextBox>(f,"codigo").Text.Length==0&&Campo<TextBox>(f,"usuario").Text=="admin","Rechazo limpia secretos y conserva usuario");
            Capturar(f,dir,"RecuperacionRechazada");
            foreach(var tamano in new[]{new Size(1280,720),new Size(1366,768)}){f.Size=tamano;f.PerformLayout();Application.DoEvents();foreach(var campo in new[]{"recuperar","consultar","cancelar","mensaje"}){var control=Campo<Control>(f,campo);ok(f.ClientRectangle.Contains(f.RectangleToClient(control.RectangleToScreen(control.ClientRectangle))),"Recuperación mantiene visible "+campo+" a "+tamano.Width);}Capturar(f,dir,"Recuperacion-"+tamano.Width);}
            Campo<TextBox>(f,"codigo").Text=k;Campo<TextBox>(f,"nueva").Text=Inicial;Campo<TextBox>(f,"confirmacion").Text=Inicial;Campo<Button>(f,"recuperar").PerformClick();Esperar(()=>Campo<Label>(f,"mensaje").Text.Contains("actualizada"));ok(!Campo<Button>(f,"recuperar").Enabled,"Interfaz permite reintento correcto y evita repetir resultado");Capturar(f,dir,"RecuperacionCompletada");f.Close();
        }
        using(var f=new FormularioRecuperarClave(publico,carpeta)){Mostrar(f);Campo<ComboBox>(f,"metodo").SelectedIndex=2;ok(!Campo<Button>(f,"recuperar").Enabled,"Sin código muestra asistencia sin puerta pública de mantenimiento");Capturar(f,dir,"RecuperacionSinCodigo");f.Close();}
        admin=Repo("admin",Inicial);
        using(var f=new FormularioCodigoRecuperacion(admin)){Mostrar(f);Campo<TextBox>(f,"clave").Text=Inicial;Campo<Button>(f,"generar").PerformClick();Esperar(()=>Campo<TextBox>(f,"codigo").Text.Length>0);ok(Campo<TextBox>(f,"codigo").UseSystemPasswordChar&&!Campo<Button>(f,"continuar").Enabled,"Código oculto y confirmación de custodia obligatoria");Campo<CheckBox>(f,"guardado").Checked=true;Campo<Button>(f,"continuar").PerformClick();Esperar(()=>f.DialogResult==DialogResult.OK);ok(!admin.NecesitaCodigoRecuperacion(),"Interfaz confirma custodia del código");}
        using(var f=new FormularioAcceso(publico,new ConfiguracionConexionTemporal(cadena))){Mostrar(f);ok(Controles(f).OfType<LinkLabel>().Any(x=>x.Text.Contains("Olvidaste")),"Acceso contiene enlace de recuperación");Capturar(f,dir,"AccesoConRecuperacion");f.Close();}
        var cb=new SqlConnectionStringBuilder(cadena){DataSource="127.0.0.1,1",ConnectTimeout=1};var fuera=new RepositorioRestaurante(new ConfiguracionConexionTemporal(cb.ConnectionString));
        using(var f=new FormularioRecuperarClave(fuera,carpeta)){Mostrar(f);Campo<ComboBox>(f,"metodo").SelectedIndex=1;Campo<TextBox>(f,"usuario").Text="ficticio";Campo<TextBox>(f,"codigo").Text="ficticio";Campo<TextBox>(f,"nueva").Text=Nueva;Campo<TextBox>(f,"confirmacion").Text=Nueva;Campo<Button>(f,"recuperar").PerformClick();Esperar(()=>Campo<Button>(f,"consultar").Enabled);ok(!Campo<Button>(f,"recuperar").Enabled,"Error SQL obliga comprobar resultado antes de repetir");Capturar(f,dir,"RecuperacionSinSql");f.Close();}
        using(var f=new FormularioRecuperarClave(fuera,carpeta)){Mostrar(f);ok(Campo<Button>(f,"consultar").Enabled&&!Campo<Button>(f,"recuperar").Enabled,"Comprobante DPAPI conserva consulta tras cerrar ventana");f.Close();}
        var opMant=Guid.NewGuid();var bak=RestablecimientoAdministrativo.Ejecutar(cadena,"admin",Nueva,"Técnico de prueba","Validación aislada autorizada",opMant);
        var recuperado=Repo("admin",Nueva);ok(recuperado.UsuarioActual!.CambioObligatorio&&File.Exists(bak),"Mantenimiento restaura respaldo y exige cambio obligatorio");Deniega(()=>recuperado.ListarUsuarios(),"Credencial temporal no abre operación diaria");recuperado.CambiarMiContrasena(Nueva,Temporal);ok(Repo("admin",Temporal).NecesitaCodigoRecuperacion(),"Después de mantenimiento debe registrar nuevo código");
        ok(Convert.ToInt32(Sql("SELECT COUNT(*) FROM dbo.OperacionesRecuperacion WHERE Id=@op",("@op",opMant)))==1,"Intervención excepcional conserva identificador auditable");
        ok(Convert.ToDecimal(Sql("SELECT SUM(Total) FROM dbo.Facturas"))==ventasAntes&&Convert.ToInt32(Sql("SELECT COUNT(*) FROM dbo.Pedidos"))==consumo,"Recuperaciones conservan comprobante y ventas existentes");
        using(var original=new FormularioRecuperacion(publico)){Mostrar(original);ok(Controles(original).OfType<Control>().Any(c=>c.Text.Contains("Descartar")),"Recuperación de operaciones pendientes permanece separada");original.Close();}
    }
    private static T Campo<T>(Form f,string nombre)where T:Control=>(T)f.GetType().GetField(nombre,BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(f)!;
    private static IEnumerable<Control> Controles(Control c){foreach(Control hijo in c.Controls){yield return hijo;foreach(var nieto in Controles(hijo))yield return nieto;}}
    private static void Mostrar(Form f){f.ShowInTaskbar=false;f.StartPosition=FormStartPosition.Manual;f.Location=new Point(-20000,-20000);f.Show();SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());Application.DoEvents();}
    private static void Esperar(Func<bool> condicion){var r=Stopwatch.StartNew();while(!condicion()){if(r.ElapsedMilliseconds>30000)throw new Exception("Estado de recuperación no alcanzado.");Application.DoEvents();Thread.Sleep(15);}Application.DoEvents();}
    private static void Capturar(Form f,string dir,string nombre){using var b=new Bitmap(f.Width,f.Height);f.DrawToBitmap(b,new Rectangle(0,0,b.Width,b.Height));b.Save(Path.Combine(dir,nombre+".png"));}
}
