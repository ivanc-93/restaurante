using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Restaurante.Backend.Dominio;
using Restaurante.Backend.Servicios;
using Restaurante.Frontend.Formularios;

internal static class Verificar
{
    private static int correctas, fallidas;
    private static string salida=null!;
    [STAThread]
    private static int Main(string[] args)
    {
        if(args.Length==4 && args[0]=="--sesion-ui") return PruebasSesion.Proceso(args[1],args[2],int.Parse(args[3]));
        ApplicationConfiguration.Initialize();
        salida=args[0]; Directory.CreateDirectory(salida);
        if(args.Length>1) ExportarDiagramas.Guardar(args[1]);
        Caso("Cálculo decimal y objetos derivados",()=>
        {
            ElementoMenu plato=new Plato(1,"Plato","Principal",12.35m);
            ElementoMenu bebida=new Bebida(2,"Agua","Bebidas",0m,false);
            Igual(37.05m,plato.CalcularImporte(3)); Igual(0m,bebida.CalcularImporte(2));
            Igual("Plato",plato.Tipo); Igual("Bebida",bebida.Tipo);
            Rechaza(()=>plato.CalcularImporte(0)); Rechaza(()=>plato.CalcularImporte(51));
        });
        Caso("Validación de clientes y productos",()=>
        {
            ReglasRestaurante.Cliente("Persona",null,"persona@example.com");
            Rechaza(()=>ReglasRestaurante.Cliente(" ",null,null));
            Rechaza(()=>ReglasRestaurante.Cliente("Persona",null,"correo inválido"));
            Rechaza(()=>ReglasRestaurante.Producto("Agua","Bebidas","Bebida",-1,false));
            Rechaza(()=>ReglasRestaurante.Producto("Agua","Bebidas","Bebida",1.001m,false));
            Rechaza(()=>ReglasRestaurante.Producto("Arroz","Platos","Plato",1,true));
            ReglasRestaurante.Producto("Agua","Bebidas","Bebida",0,false);
        });
        Caso("Intervalos inclusivos y semanas lunes-domingo",()=>
        {
            var fechas=ReglasRestaurante.Fechas(new(2026,10,25,12,0,0),new(2026,10,25));
            Igual(new DateTime(2026,10,25),fechas.Desde); Igual(new DateTime(2026,10,26),fechas.HastaExclusivo);
            Igual(new DateTime(2026,10,19),ReglasRestaurante.InicioPeriodo(new(2026,10,25),"Semana"));
            Igual(new DateTime(2026,10,26),ReglasRestaurante.InicioPeriodo(new(2026,10,26),"Semana"));
            Igual(new DateTime(2026,10,1),ReglasRestaurante.InicioPeriodo(new(2026,10,25),"Mes"));
            Rechaza(()=>ReglasRestaurante.Fechas(new(2026,10,26),new(2026,10,25)));
        });
        Caso("Configuración dañada sin tocar perfil real",()=>
        {
            var archivo=Path.Combine(salida,"configuracion-danada.bin");
            File.WriteAllBytes(archivo,new byte[]{1,2,3,4});
            var config=new ConfiguracionConexion(archivo);
            Igual(false,config.EstaConfigurada); Verdad(config.ErrorCarga is not null,"Falta advertencia de recuperación");
            Rechaza(()=>config.CrearConexion());
        });
        foreach(var crear in new Func<Form>[] {()=>new FormularioAcceso(),()=>new FormularioPrincipal(),()=>new FormularioMesa(),()=>new FormularioDetalleMesa(),()=>new FormularioCatalogos(),()=>new FormularioReportes()})
        {
            using var formulario=crear();
            Caso("Construcción y render de "+formulario.GetType().Name,()=>
            {
                Verdad(formulario.Controls.Count>0,"Designer vacío");
                formulario.ShowInTaskbar=false; formulario.Opacity=0; formulario.Show(); formulario.PerformLayout(); Application.DoEvents();
                using var bitmap=new Bitmap(formulario.Width,formulario.Height);
                formulario.DrawToBitmap(bitmap,new Rectangle(0,0,bitmap.Width,bitmap.Height));
                bitmap.Save(Path.Combine(salida,formulario.GetType().Name+".png"));
                formulario.Close();
            });
        }
        Caso("Mesa editable y acciones dentro del área visible",()=>
        {
            using var form=new FormularioMesa(new Mesa(3,3,6,"Ocupada"));
            Igual(3,form.NumeroMesa); Igual(6,form.Capacidad); Igual("Ocupada",form.EstadoMesa);
            foreach(var boton in form.Controls.OfType<Button>())
                Verdad(form.ClientRectangle.Contains(boton.Bounds),"Botón recortado: "+boton.Text);
        });
        var cadena=Environment.GetEnvironmentVariable("RESTAURANTE_TEST_CONNECTION");
        if(!string.IsNullOrWhiteSpace(cadena)) { Integracion(cadena); PruebasHistorial.Ejecutar(cadena,Caso); PruebasSesion.Ejecutar(cadena,Caso,salida); }
        else Console.WriteLine("PENDIENTE: integración SQL (sin RESTAURANTE_TEST_CONNECTION).");
        Console.WriteLine($"RESULTADO: {correctas} correctas; {fallidas} fallidas.");
        return fallidas==0?0:1;
    }
    private static void Integracion(string cadena)
    {
        var opciones=new SqlConnectionStringBuilder(cadena);
        Verdad(opciones.InitialCatalog.StartsWith("Restaurante_Pruebas_",StringComparison.Ordinal),"Solo se permite una base aislada de pruebas.");
        var repo=new RepositorioRestaurante(new ConexionPrueba(cadena));
        int cliente=0,plato=0,bebida=0,pedido=0;
        Caso("Login válido, inválido e inactivo",()=>
        {
            Verdad(repo.IniciarSesion("admin","Admin123!") is not null,"Login válido rechazado");
            Verdad(repo.IniciarSesion("admin","incorrecta") is null,"Login inválido aceptado");
            Sql(cadena,"UPDATE dbo.Usuarios SET Activo=0 WHERE Usuario=N'admin';");
            Verdad(repo.IniciarSesion("admin","Admin123!") is null,"Inactivo aceptado");
            Sql(cadena,"UPDATE dbo.Usuarios SET Activo=1 WHERE Usuario=N'admin';");
        });
        Caso("Instalación SQL conserva caracteres Unicode",()=> Igual("Café de olla",repo.ListarMenu().Single(p=>p.Nombre.StartsWith("Caf")).Nombre));
        Caso("Configuración persistente del restaurante",()=>
        {
            repo.GuardarRestaurante("Restaurante de prueba","Guatemala");
            Igual("Guatemala",new RepositorioRestaurante(new ConexionPrueba(cadena)).ObtenerRestaurante().Ubicacion);
        });
        Caso("Mantenimiento de clientes y productos",()=>
        {
            repo.GuardarCliente(null,"Cliente prueba","12345","prueba@example.com");
            cliente=repo.ListarClientes().Single(c=>c.Nombre=="Cliente prueba").Id;
            Rechaza(()=>repo.GuardarCliente(null,"Duplicado",null,"prueba@example.com"));
            repo.GuardarCliente(cliente,"Cliente editado","67890","editado@example.com");
            repo.GuardarProducto(null,"Plato prueba","Pruebas","Plato",12.35m,false);
            plato=repo.ListarMenu().Single(p=>p.Nombre=="Plato prueba").Id;
            Rechaza(()=>repo.GuardarProducto(null,"Plato prueba","Pruebas","Plato",12.35m,false));
            repo.GuardarProducto(plato,"Plato prueba","Pruebas","Plato",12.35m,false);
            repo.GuardarProducto(null,"Cortesía","Pruebas","Bebida",0m,false);
            bebida=repo.ListarMenu().Single(p=>p.Nombre=="Cortesía").Id;
        });
        Caso("Mesa: duplicados y validaciones",()=>
        {
            repo.CrearMesa(100,4); Rechaza(()=>repo.CrearMesa(100,4)); Rechaza(()=>repo.CrearMesa(0,4));
            var mesa=repo.ListarMesas().Single(m=>m.Numero==100);
            repo.ActualizarMesa(mesa.Id,100,6,"Disponible"); Igual(6,repo.ListarMesas().Single(m=>m.Id==mesa.Id).Capacidad);
            repo.EliminarMesa(mesa.Id);
        });
        Caso("Pedido, agregado, totales y facturación única",()=>
        {
            var mesa=repo.ListarMesas().Single(m=>m.Numero==1);
            var creado=repo.CrearPedido(new(){MesaId=mesa.Id,ClienteId=cliente,Detalles=[new(plato,2)]});
            pedido=creado.Id; Igual(24.70m,creado.Total);
            Igual(37.05m,repo.AgregarProductoAPedido(pedido,plato,1));
            Rechaza(()=>repo.ActualizarMesa(mesa.Id,1,2,"Disponible"));
            Verdad(repo.FacturarPedido(pedido).StartsWith("F-"),"Sin factura");
            Rechaza(()=>repo.FacturarPedido(pedido)); Rechaza(()=>repo.AgregarProductoAPedido(pedido,plato,1));
            Igual("Disponible",repo.ListarMesas().Single(m=>m.Id==mesa.Id).Estado);
            Rechaza(()=>repo.EliminarMesa(mesa.Id));
        });
        Caso("Cortesía de total cero libera la mesa",()=>
        {
            var mesa=repo.ListarMesas().Single(m=>m.Numero==2);
            var creado=repo.CrearPedido(new(){MesaId=mesa.Id,Detalles=[new(bebida,1)]});
            Igual(0m,creado.Total); repo.FacturarPedido(creado.Id);
            Igual("Disponible",repo.ListarMesas().Single(m=>m.Id==mesa.Id).Estado);
        });
        Caso("Rollback si un producto no existe",()=>
        {
            var antes=repo.ListarPedidos().Count; var mesa=repo.ListarMesas().Single(m=>m.Numero==3);
            Rechaza(()=>repo.CrearPedido(new(){MesaId=mesa.Id,Detalles=[new(plato,1),new(int.MaxValue,1)]}));
            Igual(antes,repo.ListarPedidos().Count); Igual("Disponible",repo.ListarMesas().Single(m=>m.Id==mesa.Id).Estado);
        });
        Caso("Aperturas concurrentes para la misma mesa",()=>
        {
            var mesa=repo.ListarMesas().Single(m=>m.Numero==4);
            var resultados=Task.WhenAll(Enumerable.Range(0,2).Select(_=>Task.Run(()=>
            {
                try { return repo.CrearPedido(new(){MesaId=mesa.Id,Detalles=[new(plato,1)]}).Id; }
                catch(InvalidOperationException) {return 0;}
            }))).GetAwaiter().GetResult();
            Igual(1,resultados.Count(id=>id>0));
        });
        Caso("Facturación concurrente genera una sola factura",()=>
        {
            var abierto=repo.ObtenerPedidoAbierto(repo.ListarMesas().Single(m=>m.Numero==4).Id)!;
            var resultados=Task.WhenAll(Enumerable.Range(0,2).Select(_=>Task.Run(()=>
            {
                try {repo.FacturarPedido(abierto.Id);return true;} catch(InvalidOperationException){return false;}
            }))).GetAwaiter().GetResult();
            Igual(1,resultados.Count(x=>x));
        });
        Caso("Reportes reconciliados y sin pedidos abiertos ni cancelados",()=>
        {
            var hoy=DateTime.Today;
            var mesa=repo.ListarMesas().Single(m=>m.Numero==5);
            var abierto=repo.CrearPedido(new(){MesaId=mesa.Id,Detalles=[new(plato,50)]});
            var cancelado=repo.CrearPedido(new(){MesaId=repo.ListarMesas().Single(m=>m.Numero==6).Id,Detalles=[new(plato,50)]});
            Sql(cadena,$"UPDATE dbo.Pedidos SET Estado=N'Cancelado' WHERE Id={cancelado.Id};");
            var mas=repo.ReportarPlatosMasVendidos(hoy,hoy).Single(p=>p.Nombre=="Plato prueba");
            Igual(4,mas.Cantidad); Igual(49.40m,mas.Total);
            Igual(49.40m,repo.ReportarVentasPeriodo(hoy,hoy,"Día").Sum(v=>v.Total));
            Igual(49.40m,repo.ReportarVentasPeriodo(hoy,hoy,"Semana").Sum(v=>v.Total));
            Igual(49.40m,repo.ReportarVentasPeriodo(hoy,hoy,"Mes").Sum(v=>v.Total));
            Igual(1,repo.ReportarPedidos(hoy,hoy,null,cliente).Count);
            Verdad(repo.ReportarPedidos(hoy,hoy,mesa.Id,null).Any(p=>p.Id==abierto.Id),"Filtro de mesa incorrecto");
            Verdad(repo.ReportarPlatosMasVendidos(hoy.AddDays(1),hoy.AddDays(1)).Count==0,"Filtro de fechas incorrecto");
        });
        Caso("Agrupación semanal entre años y mensual separada",()=>
        {
            var facturas=repo.ListarPedidos().Where(p=>p.NumeroFactura is not null && p.Total>0).OrderBy(p=>p.Id).ToList();
            Sql(cadena,$"UPDATE dbo.Facturas SET Fecha='20251231' WHERE PedidoId={facturas[0].Id}; UPDATE dbo.Facturas SET Fecha='20260101' WHERE PedidoId={facturas[1].Id};");
            var semanas=repo.ReportarVentasPeriodo(new(2025,12,29),new(2026,1,4),"Semana");
            Igual(1,semanas.Count); Igual(new DateTime(2025,12,29),semanas[0].Periodo); Igual(49.40m,semanas[0].Total);
            Igual(2,repo.ReportarVentasPeriodo(new(2025,12,1),new(2026,1,31),"Mes").Count);
            Sql(cadena,"UPDATE dbo.Facturas SET Fecha=SYSDATETIME();");
        });
        Caso("Cambio de estado concurrente no libera una mesa con pedido",()=>
        {
            var mesa=repo.ListarMesas().Single(m=>m.Numero==7);
            var tareas=new[]
            {
                Task.Run(()=>repo.CrearPedido(new(){MesaId=mesa.Id,Detalles=[new(bebida,1)]})),
                Task.Run(()=> {try{repo.ActualizarMesa(mesa.Id,7,6,"Disponible");}catch(InvalidOperationException){} return new PedidoCreado(0,0,null);})
            };
            Task.WhenAll(tareas).GetAwaiter().GetResult();
            Verdad(repo.ObtenerPedidoAbierto(mesa.Id) is not null,"No se abrió el pedido");
            Igual("Ocupada",repo.ListarMesas().Single(m=>m.Id==mesa.Id).Estado);
        });
        Caso("Desconexión SQL produce un error controlable",()=>
        {
            var sinServidor=new RepositorioRestaurante(new ConexionPrueba("Server=127.0.0.1,1;Database=Restaurante_Pruebas_SinServidor;Integrated Security=True;Connect Timeout=1;Encrypt=False;"));
            try{sinServidor.ListarMesas();}catch(SqlException){return;}
            throw new Exception("La conexión de prueba debía fallar.");
        });
        Caso("Pantallas operativas y cierre de sesión",()=>
        {
            using var form=new FormularioPrincipal(repo,new ConexionPrueba(cadena),new UsuarioAutenticado("Prueba","Administrador"));
            bool mostrada=false; form.Shown+=(_,_)=>mostrada=true;
            form.Opacity=0; form.ShowInTaskbar=false; form.Show();
            Esperar(()=>mostrada && Campo<FlowLayoutPanel>(form,"menu").Enabled);
            foreach(var pagina in new[]{"MostrarPedidos","MostrarMenuMesas"})
            {
                var tarea=(Task)form.GetType().GetMethod(pagina,System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!.Invoke(form,null)!;
                Esperar(()=>tarea.IsCompleted); tarea.GetAwaiter().GetResult(); form.PerformLayout();
                using var bmp=new Bitmap(form.Width,form.Height); form.DrawToBitmap(bmp,new Rectangle(0,0,bmp.Width,bmp.Height));
                bmp.Save(Path.Combine(salida,pagina+".png"));
            }
            Campo<Button>(form,"salir").PerformClick(); Igual(true,form.SesionCerrada);
        });
        Caso("Catálogos y reportes cargan datos en la interfaz",()=>
        {
            using(var catalogos=new FormularioCatalogos(repo,true))
            {
                catalogos.Opacity=0; catalogos.ShowInTaskbar=false; catalogos.Show();
                Esperar(()=>catalogos.Enabled && Campo<DataGridView>(catalogos,"tabla").DataSource is not null);
                Capturar(catalogos,"CatalogosClientes");
                Campo<ComboBox>(catalogos,"modo").SelectedItem="Productos";
                Esperar(()=>catalogos.Enabled && Campo<TextBox>(catalogos,"categoria").Visible);
                Capturar(catalogos,"CatalogosProductos");
                Campo<ComboBox>(catalogos,"modo").SelectedItem="Restaurante";
                Esperar(()=>catalogos.Enabled && Campo<TextBox>(catalogos,"ubicacion").Text=="Guatemala");
                Capturar(catalogos,"CatalogosRestaurante"); catalogos.Close();
            }
            using var reportes=new FormularioReportes(repo);
            reportes.Opacity=0; reportes.ShowInTaskbar=false; reportes.Show();
            Esperar(()=>Campo<bool>(reportes,"_listo") && !Campo<bool>(reportes,"_cargando"));
            Capturar(reportes,"ReportesConDatos"); reportes.Close();
        });
        Caso("Desactivación conserva detalles e historial",()=>
        {
            repo.DesactivarCliente(cliente); repo.DesactivarProducto(plato);
            Verdad(!repo.ListarClientes().Any(c=>c.Id==cliente),"Cliente sigue activo");
            Verdad(repo.ListarClientesHistoricos().Any(c=>c.Id==cliente),"Cliente ausente en filtros históricos");
            Verdad(!repo.ListarMenu().Any(p=>p.Id==plato),"Producto sigue activo");
            Igual(3,repo.ListarDetallesPedido(pedido).Sum(d=>d.Cantidad));
            Verdad(repo.ListarPedidos().Any(p=>p.Id==pedido),"Historial perdido");
        });
        Caso("Pedido abierto anterior a los últimos 100 sigue visible",()=>
        {
            var id=repo.ObtenerPedidoAbierto(repo.ListarMesas().Single(m=>m.Numero==5).Id)!.Id;
            Sql(cadena,"INSERT dbo.Pedidos(MesaId,Estado,Total) SELECT TOP(101) (SELECT TOP(1) Id FROM dbo.Mesas),N'Cancelado',0 FROM sys.all_objects;");
            Verdad(repo.ListarPedidos().Any(p=>p.Id==id),"Pedido abierto oculto por límite");
        });
    }
    private static void Capturar(Form form,string nombre)
    {
        form.PerformLayout(); Application.DoEvents();
        using var bitmap=new Bitmap(form.Width,form.Height);
        form.DrawToBitmap(bitmap,new Rectangle(0,0,bitmap.Width,bitmap.Height));
        bitmap.Save(Path.Combine(salida,nombre+".png"));
    }
    private static T Campo<T>(Form form,string nombre) => (T)form.GetType().GetField(nombre,System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!.GetValue(form)!;
    private static void Esperar(Func<bool> terminado)
    {
        var tiempo=System.Diagnostics.Stopwatch.StartNew();
        while(!terminado()) { Application.DoEvents(); Thread.Sleep(10); if(tiempo.Elapsed.TotalSeconds>20) throw new TimeoutException("La interfaz no completó la operación."); }
        Application.DoEvents();
    }
    private static void Sql(string cadena,string sql)
    {
        using var conexion=new SqlConnection(cadena); conexion.Open(); using var comando=new SqlCommand(sql,conexion); comando.ExecuteNonQuery();
    }
    private static void Caso(string nombre,Action accion)
    {
        try {accion(); correctas++; Console.WriteLine("OK: "+nombre);}
        catch(Exception ex) {fallidas++; Console.WriteLine("ERROR: "+nombre+" -> "+ex);}
    }
    private static void Igual<T>(T esperado,T actual) {if(!EqualityComparer<T>.Default.Equals(esperado,actual)) throw new Exception($"Esperado {esperado}; obtenido {actual}");}
    private static void Verdad(bool valor,string mensaje) {if(!valor) throw new Exception(mensaje);}
    private static void Rechaza(Action accion)
    {
        try {accion();} catch(ArgumentException){return;} catch(InvalidOperationException){return;}
        throw new Exception("Se aceptó una operación inválida.");
    }
}
