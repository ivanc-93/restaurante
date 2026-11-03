using Microsoft.Data.SqlClient;
using Restaurante.Backend.Dominio;
using Restaurante.Backend.Servicios;
using Restaurante.Frontend.Formularios;
using Restaurante.Frontend.Servicios;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

internal static class Programa
{
    static readonly string Base="Restaurante_Pruebas_Frontend_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+"_"+Guid.NewGuid().ToString("N")[..6];
    static readonly string Salida=Path.Combine(AppContext.BaseDirectory,"Resultados");
    static readonly string Server="lpc:localhost\\MSSQLSERVER01";
    const string Clave="PruebaAdmin_2026!";
    static string Cadena(string servidor) => new SqlConnectionStringBuilder { DataSource=servidor,InitialCatalog=Base,IntegratedSecurity=true,Encrypt=true,TrustServerCertificate=true,ConnectTimeout=8 }.ConnectionString;
    static int buenas,malas,mesa=8000;
    static StreamWriter log=null!;
    static RepositorioRestaurante repo=null!;
    static int producto;
    [STAThread] static void Main()
    {
        Directory.CreateDirectory(Salida);log=new(Path.Combine(Salida,"resultado.txt")){AutoFlush=true};
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
        try { Preparar();Probar();Visuales(); }
        catch(Exception ex){malas++;log.WriteLine("ERROR FATAL: "+ex);}
        log.WriteLine($"RESULTADO: {buenas} correctas; {malas} fallidas. Base aislada: {Base}");log.Dispose();
    }
    static void Sql(string texto) { if(!Base.StartsWith("Restaurante_Pruebas_"))throw new Exception("Destino no permitido");using var c=new SqlConnection(Cadena(Server));c.Open();using var q=new SqlCommand(texto,c){CommandTimeout=90};q.ExecuteNonQuery(); }
    static object Scalar(string texto){using var c=new SqlConnection(Cadena(Server));c.Open();using var q=new SqlCommand(texto,c);return q.ExecuteScalar()!;}
    static void Preparar()
    {
        log.WriteLine("Base nueva: "+Base+". No se consulta ni modifica RestauranteDB.");
        var builder=new SqlConnectionStringBuilder(Cadena(Server)){InitialCatalog="master"};using var c=new SqlConnection(builder.ConnectionString);c.Open();
        foreach(var archivo in new[]{"CrearBaseDatos.sql","Migracion_004.sql","Migracion_005.sql","Migracion_006.sql","Migracion_007.sql"}) {
            var texto=File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Sql",archivo)).Replace("RestauranteDB",Base);
            foreach(var batch in Regex.Split(texto,@"^\s*GO\s*$",RegexOptions.Multiline|RegexOptions.IgnoreCase))if(!string.IsNullOrWhiteSpace(batch)){using var q=new SqlCommand(batch,c){CommandTimeout=90};q.ExecuteNonQuery();}
        }
        Sql("UPDATE dbo.Usuarios SET ContrasenaHash=CONVERT(varchar(64),HASHBYTES('SHA2_256','SalRegresion:PruebaAdmin_2026!'),2),Sal=N'SalRegresion',Algoritmo=N'SHA256',CambioObligatorio=0 WHERE Usuario=N'admin';");
        repo=Repo();repo.GuardarProducto(null,"Producto prueba","Pruebas","Plato",12.35m,false);producto=repo.ListarMenu().Single(p=>p.Nombre=="Producto prueba").Id;
    }
    static RepositorioRestaurante Repo(string? servidor=null,string usuario="admin",string clave=Clave){var r=new RepositorioRestaurante(new ConfiguracionConexionTemporal(Cadena(servidor??Server)));r.ValidarEsquema();if(r.IniciarSesion(usuario,clave) is null)throw new Exception("Sesión de prueba rechazada");return r;}
    static int Pedido(int cantidad=1,int? menu=null){repo.CrearMesa(++mesa,4);var m=repo.ListarMesas().Single(m=>m.Numero==mesa);return repo.CrearPedido(new(){MesaId=m.Id,Detalles=[new(menu??producto,cantidad)]}).Id;}
    static void Caso(string nombre,Action accion){try{accion();buenas++;log.WriteLine("OK: "+nombre);}catch(Exception e){malas++;log.WriteLine("ERROR: "+nombre+"\n"+e);}}
    static void Eq<T>(T a,T b){if(!EqualityComparer<T>.Default.Equals(a,b))throw new Exception($"Esperado {a}; obtenido {b}");}
    static void Rechaza(Action a){try{a();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or UnauthorizedAccessException){return;}throw new Exception("Se aceptó una operación inválida");}
    static SolicitudCobro Cobro(CuentaRevision c,decimal? efectivo=null)=>new(Guid.NewGuid(),c.PedidoId,c.Version,c.Total,true,"Público general",null,null,null,"Efectivo",efectivo??c.Total,"Confirmado",null,null,true,true);
    static void Ajuste(int id,decimal d,string tipo="Descuento"){var c=repo.RevisarCuenta(id);repo.AplicarAjuste(new(Guid.NewGuid(),id,c.Version,c.Total,tipo,d,"Prueba de ajuste"));}
    static void Retirar(int id,int linea,int cantidad){var c=repo.RevisarCuenta(id);repo.CorregirLinea(new(Guid.NewGuid(),id,linea,cantidad,c.Version,c.Total,"Corrección de prueba",true));}
    static void Probar()
    {
        Caso("Centavos exactos y cantidad inválida",()=>{Eq(37.05m,new Plato(1,"X","Y",12.35m).CalcularImporte(3));Rechaza(()=>new Plato(1,"X","Y",1).CalcularImporte(51));});
        Caso("Límite de pedido antes de SQL",()=>{Eq(9999999999.99m,ReglasRestaurante.TotalAdmitido(9999999999.98m,.01m));Rechaza(()=>ReglasRestaurante.TotalAdmitido(9999999999.99m,.01m));});
        Caso("Fechas bisiestas y semana entre años",()=>{Eq(new DateTime(2024,3,1),ReglasRestaurante.Fechas(new(2024,2,29),new(2024,2,29)).HastaExclusivo);Eq(new DateTime(2025,12,29),ReglasRestaurante.InicioPeriodo(new(2026,1,1),"Semana"));});
        Caso("Descuento no se pierde ni cambia el pedido",()=>{var p=Pedido(3);Ajuste(p,5);Rechaza(()=>repo.AgregarProductoAPedido(p,producto,1));var c=repo.RevisarCuenta(p);Eq(32.05m,c.Total);Eq(5m,c.Descuento);Rechaza(()=>Retirar(p,repo.ListarLineasCorregibles(p)[0].Id,1));Eq(32.05m,repo.RevisarCuenta(p).Total);Ajuste(p,0);repo.AgregarProductoAPedido(p,producto,1);Eq(49.40m,repo.RevisarCuenta(p).Total);});
        Caso("Cortesía no desaparece al agregar",()=>{var p=Pedido();Ajuste(p,12.35m,"Cortesía");Rechaza(()=>repo.AgregarProductoAPedido(p,producto,1));Eq(0m,repo.RevisarCuenta(p).Total);});
        Caso("No prepara pedido vacío; cancelación sigue posible",()=>{var p=Pedido();Retirar(p,repo.ListarLineasCorregibles(p)[0].Id,1);Rechaza(()=>repo.MarcarPreparacion(p,Guid.NewGuid()));var c=repo.RevisarCuenta(p);Eq("Abierto",c.Estado);repo.GestionarMesa(new(Guid.NewGuid(),c.MesaId,p,c.Version,"Cancelar","Pedido vacío sin consumo",true));});
        Caso("Preparación por tandas protege lo preparado y permite corregir lo nuevo",()=>{var p=Pedido();var primera=repo.ListarLineasCorregibles(p)[0].Id;repo.MarcarPreparacion(p,Guid.NewGuid());Rechaza(()=>Retirar(p,primera,1));repo.AgregarProductoAPedido(p,producto,2);var nueva=repo.ListarLineasCorregibles(p).Last();Eq(false,nueva.Preparada);Retirar(p,nueva.Id,1);Eq(24.70m,repo.RevisarCuenta(p).Total);repo.MarcarPreparacion(p,Guid.NewGuid());Eq(true,repo.ListarLineasCorregibles(p).Last().Preparada);Rechaza(()=>Retirar(p,nueva.Id,1));Rechaza(()=>repo.MarcarPreparacion(p,Guid.NewGuid()));});
        Caso("Pedido preparado antiguo conserva protección",()=>{var p=Pedido();Sql($"UPDATE dbo.Pedidos SET Estado=N'En preparación' WHERE Id={p};");var vieja=repo.ListarLineasCorregibles(p)[0];Eq(true,vieja.Preparada);repo.AgregarProductoAPedido(p,producto,1);var nueva=repo.ListarLineasCorregibles(p).Last();Eq(false,nueva.Preparada);Rechaza(()=>Retirar(p,vieja.Id,1));Retirar(p,nueva.Id,1);});
        Caso("Importe acumulado excesivo no deja detalles parciales",()=>{repo.GuardarProducto(null,"Máximo","Pruebas","Plato",99999999.99m,false);var m=repo.ListarMenu().Single(x=>x.Nombre=="Máximo").Id;var p=Pedido(50,m);repo.AgregarProductoAPedido(p,m,50);Rechaza(()=>repo.AgregarProductoAPedido(p,m,50));Eq(2,repo.ListarLineasCorregibles(p).Count);Eq(9999999999m,repo.RevisarCuenta(p).Total);});
        Caso("Efectivo insuficiente y cobro idempotente con cambio exacto",()=>{var p=Pedido(3);var c=repo.RevisarCuenta(p);Rechaza(()=>repo.RegistrarCobro(Cobro(c,37)));var s=Cobro(c,50);var a=repo.RegistrarCobro(s);var b=repo.RegistrarCobro(s);Eq(12.95m,a.Cambio);Eq(a.NumeroComprobante,b.NumeroComprobante);Eq(1,(int)Scalar($"SELECT COUNT(*) FROM dbo.Facturas WHERE PedidoId={p}"));Eq("En limpieza",repo.ListarMesas().Single(x=>x.Id==c.MesaId).Estado);});
        Caso("Cuenta modificada rechaza cobro desactualizado",()=>{var p=Pedido();var c=repo.RevisarCuenta(p);repo.AgregarProductoAPedido(p,producto,1);Rechaza(()=>repo.RegistrarCobro(Cobro(c)));Eq(0,(int)Scalar($"SELECT COUNT(*) FROM dbo.Facturas WHERE PedidoId={p}"));});
        Caso("Tarjeta incierta no emite comprobante ni permite cobrar otra vez",()=>{var p=Pedido();var c=repo.RevisarCuenta(p);var s=Cobro(c) with{Metodo="Tarjeta",ResultadoTarjeta="Incierto",Referencia="PRUEBA-INC"};Eq("Incierto",repo.RegistrarCobro(s).Estado);Eq(0,(int)Scalar($"SELECT COUNT(*) FROM dbo.Facturas WHERE PedidoId={p}"));Rechaza(()=>repo.RegistrarCobro(Cobro(c)));Eq("Confirmado",repo.ConciliarTarjeta(s.OperacionId,true,"Confirmación de prueba",Guid.NewGuid()).Estado);});
        Caso("Reactivar producto y cliente conserva ID y bloquea duplicados",()=>{repo.GuardarProducto(null,"Inactivo","Pruebas","Plato",5,false);var p=repo.ListarMenu().Single(x=>x.Nombre=="Inactivo").Id;repo.DesactivarProducto(p);Eq(p,repo.ListarInactivos(true).Single(x=>x.Nombre=="Inactivo").Id);repo.ReactivarRegistro(true,p);Eq(p,repo.ListarMenu().Single(x=>x.Nombre=="Inactivo").Id);repo.DesactivarProducto(p);repo.GuardarProducto(null,"Inactivo","Pruebas","Plato",6,false);Rechaza(()=>repo.ReactivarRegistro(true,p));repo.GuardarCliente(null,"Cliente prueba",null,"prueba@example.test");var cl=repo.ListarClientes().Single(x=>x.Nombre=="Cliente prueba").Id;repo.DesactivarCliente(cl);repo.ReactivarRegistro(false,cl);Eq(cl,repo.ListarClientes().Single(x=>x.Nombre=="Cliente prueba").Id);});
        Caso("Mesero no reactiva ni cambia ajustes sin autorización",()=>{repo.GuardarUsuario(null,"Mesero prueba","mesero_prueba","Mesero",true,"MeseroPrueba_2026!");Sql("UPDATE dbo.Usuarios SET CambioObligatorio=0 WHERE Usuario=N'mesero_prueba';");var r=Repo(usuario:"mesero_prueba",clave:"MeseroPrueba_2026!");Rechaza(()=>r.ReactivarRegistro(true,producto));Rechaza(()=>r.ListarInactivos(true));var p=Pedido();var c=repo.RevisarCuenta(p);Rechaza(()=>r.AplicarAjuste(new(Guid.NewGuid(),p,c.Version,c.Total,"Descuento",1,"Sin autorización")));});
        Caso("Misma identidad SQL por conexión normal y memoria compartida",()=>{Eq(repo.IdentificadorBase,Repo("localhost\\MSSQLSERVER01").IdentificadorBase);});
        Caso("Recuperación de respuesta perdida no duplica producto al cambiar alias",()=>{var p=Pedido();var s=new SolicitudIncorporacion(Guid.NewGuid(),p,producto,1);try{DiarioOperaciones.Ejecutar<decimal>(repo,"Agregar",s.OperacionId,s,()=>{repo.AgregarProductoAPedido(p,producto,1,s.OperacionId);throw new IOException("Respuesta perdida simulada");}).GetAwaiter().GetResult();}catch(IOException){}var alias=Repo("localhost\\MSSQLSERVER01");var pendiente=DiarioOperaciones.Listar(alias).Single(x=>x.Id==s.OperacionId);DiarioOperaciones.Reanudar(alias,pendiente).GetAwaiter().GetResult();Eq(24.70m,repo.RevisarCuenta(p).Total);Eq(false,DiarioOperaciones.TienePendiente(repo,s.OperacionId));});
        Caso("Registro de alias anterior migra sin perder solicitud",()=>{
            var p=Pedido();var s=new SolicitudIncorporacion(Guid.NewGuid(),p,producto,1);
            var carpeta=Path.Combine(Path.GetTempPath(),"SaboresPruebas","Operaciones");Directory.CreateDirectory(carpeta);
            var archivo=Path.Combine(carpeta,repo.IdentificadorBaseAnterior+"_"+repo.UsuarioActual!.Id+"_"+s.OperacionId.ToString("N")+".bin");
            var registro=new OperacionPendienteLocal(s.OperacionId,repo.IdentificadorBaseAnterior,repo.UsuarioActual.Id,"Agregar",JsonSerializer.Serialize(s));
            File.WriteAllBytes(archivo,ProtectedData.Protect(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(registro)),null,DataProtectionScope.CurrentUser));
            var pendiente=DiarioOperaciones.Listar(repo).Single(x=>x.Id==s.OperacionId);Eq(false,File.Exists(archivo));
            DiarioOperaciones.Reanudar(repo,pendiente).GetAwaiter().GetResult();Eq(24.70m,repo.RevisarCuenta(p).Total);
        });
        Caso("Destino antiguo desconocido queda visible y no ejecutable",()=>{
            var carpeta=Path.Combine(Path.GetTempPath(),"SaboresPruebas","Operaciones");Directory.CreateDirectory(carpeta);
            var archivo=Path.Combine(carpeta,new string('A',64)+"_"+repo.UsuarioActual!.Id+"_"+Guid.NewGuid().ToString("N")+".bin");
            try{File.WriteAllBytes(archivo,new byte[]{1,2,3});var p=DiarioOperaciones.Listar(repo).Single(x=>x.Datos==Path.GetFileName(archivo));Eq("Destino por verificar",p.Tipo);Rechaza(()=>DiarioOperaciones.DescartarRechazada(repo,p));Eq(true,File.Exists(archivo));Eq(true,PresentacionPendiente.Resumen(p).Contains("otra conexión"));}finally{File.Delete(archivo);}
        });
        Caso("Dos cobros concurrentes solo generan una factura",()=>{var p=Pedido();var c=repo.RevisarCuenta(p);int exitos=0;Parallel.Invoke(()=>Intentar(),()=>Intentar());void Intentar(){try{Repo().RegistrarCobro(Cobro(c));Interlocked.Increment(ref exitos);}catch(InvalidOperationException){}}Eq(1,exitos);Eq(1,(int)Scalar($"SELECT COUNT(*) FROM dbo.Facturas WHERE PedidoId={p}"));});
        Caso("Reportes netos concilian con comprobantes",()=>{var desde=DateTime.Today.AddDays(-2);var hasta=DateTime.Today.AddDays(2);var total=repo.ReportarVentas(desde,hasta).Sum(x=>x.Total);Eq((decimal)Scalar("SELECT COALESCE(SUM(Total),0) FROM dbo.Facturas"),total);Eq(total,repo.ReportarVentasPeriodo(desde,hasta,"Mes").Sum(x=>x.Total));});
    }
    static void Visuales()
    {
        // UI uses only the isolated database and fictitious administrator. No user login is automated.
        foreach(var x in repo.ListarMenu().Where(x=>x.Nombre=="Máximo"))repo.DesactivarProducto(x.Id);
        var p=Pedido(2);repo.AgregarProductoAPedido(p,producto,1);var c=repo.RevisarCuenta(p);var m=repo.ListarMesas().Single(x=>x.Id==c.MesaId);
        var makers=new (string,Func<Form>)[]{("Inicio",()=>new FormularioPrincipal(repo,new ConfiguracionConexionTemporal(Cadena(Server)),repo.UsuarioActual!)),("Pedido",()=>new FormularioDetalleMesa(repo,m)),("Cobro",()=>new FormularioCobro(repo,c)),("Catalogos",()=>new FormularioCatalogos(repo,true)),("Reportes",()=>new FormularioReportes(repo)),("Inactivos",()=>new FormularioReactivar(repo,true))};
        foreach(var (nombre,crear) in makers)Caso("Pantalla "+nombre,()=>{using var f=crear();f.ShowInTaskbar=false;f.Show();var sw=System.Diagnostics.Stopwatch.StartNew();while(sw.ElapsedMilliseconds<2500){Application.DoEvents();Thread.Sleep(15);}var area=Screen.FromControl(f).WorkingArea;if(f.Width>area.Width||f.Height>area.Height)throw new Exception("Ventana supera área útil");if(f is FormularioCobro){
            T Campo<T>(string name)=>(T)f.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(f)!;
            Campo<CheckBox>("datosConfirmados").Checked=true;Campo<CheckBox>("pagoConfirmado").Checked=true;
            Eq(false,Campo<Button>("confirmar").Enabled);Campo<NumericUpDown>("entregado").Value=50;Eq(true,Campo<Button>("confirmar").Enabled);
            Campo<ComboBox>("metodo").SelectedItem="Tarjeta";Campo<ComboBox>("resultadoTarjeta").SelectedItem="Confirmado";Eq(false,Campo<Button>("confirmar").Enabled);Campo<TextBox>("referencia").Text="PRUEBA-VISUAL";Eq(true,Campo<Button>("confirmar").Enabled);Campo<ComboBox>("metodo").SelectedItem="Efectivo";Campo<NumericUpDown>("entregado").Value=50;
        }
        if(f is FormularioReportes){foreach(var campo in new[]{"agrupacion","total"}){var control=(Control)f.GetType().GetField(campo,BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(f)!;Eq(true,control.Visible);}}
        f.Size=new Size(Math.Min(1366,area.Width),Math.Min(728,area.Height));f.PerformLayout();Application.DoEvents();
        using var bmp=new Bitmap(f.Width,f.Height);f.DrawToBitmap(bmp,new Rectangle(Point.Empty,bmp.Size));bmp.Save(Path.Combine(Salida,nombre+".png"));f.Close();});
    }
}
