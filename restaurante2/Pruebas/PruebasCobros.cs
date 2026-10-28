using Microsoft.Data.SqlClient;
using Restaurante.Backend.Dominio;
using Restaurante.Backend.Servicios;
using Restaurante.Frontend.Servicios;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

internal static class PruebasCobros
{
    internal static RepositorioRestaurante Repo(string cadena,string usuario="admin",string clave="PruebaAdmin_2026!")
    {
        if(!new SqlConnectionStringBuilder(cadena).InitialCatalog.StartsWith("Restaurante_Pruebas_",StringComparison.Ordinal))throw new Exception("Solo bases aisladas.");
        var repo=new RepositorioRestaurante(new ConexionPrueba(cadena));repo.ValidarEsquema();
        if(repo.IniciarSesion(usuario,clave) is null)throw new Exception("No se pudo autenticar la prueba.");return repo;
    }
    internal static SolicitudCobro Solicitud(CuentaRevision c,string metodo="Efectivo",decimal? entregado=null,string resultado="Confirmado") =>
        new(Guid.NewGuid(),c.PedidoId,c.Version,c.Total,true,"Público general",null,null,null,metodo,entregado??c.Total,resultado,
            metodo=="Tarjeta"?"REF-ACADEMICA-001":null,"Prueba aislada",true,true,metodo=="Cortesía");
    internal static string Facturar(RepositorioRestaurante repo,int pedidoId)
    {
        var c=repo.RevisarCuenta(pedidoId);
        var r=repo.RegistrarCobro(Solicitud(c,c.Total==0?"Cortesía":"Efectivo"));
        repo.GestionarMesa(new(Guid.NewGuid(),c.MesaId,null,null,"Confirmar limpieza","Limpieza comprobada en la prueba",false));
        return r.NumeroComprobante!;
    }
    public static int RecuperarProceso(string archivo,Guid id)
    {
        var repo=Repo(Environment.GetEnvironmentVariable("RESTAURANTE_TEST_CONNECTION")!);
        var pendiente=DiarioOperaciones.Listar(repo).Single(p=>p.Id==id);
        var resultado=DiarioOperaciones.Reanudar(repo,pendiente).GetAwaiter().GetResult();
        File.WriteAllText(archivo,JsonSerializer.Serialize(new{Pid=Environment.ProcessId,Operacion=id,Resultado=resultado,Pendiente=DiarioOperaciones.TienePendiente(repo,id)}));
        return 0;
    }
    public static void Ejecutar(string cadena,Action<string,Action> caso,string salida)
    {
        var repo=Repo(cadena);int numero=12000;
        repo.GuardarProducto(null,"Prueba cobro","Cobros","Plato",10,false);
        var menu=repo.ListarMenu().Single(p=>p.Nombre=="Prueba cobro").Id;
        CuentaRevision Cuenta()
        {
            repo.CrearMesa(++numero,4);var m=repo.ListarMesas().Single(x=>x.Numero==numero);
            return repo.RevisarCuenta(repo.CrearPedido(new(){MesaId=m.Id,Detalles=[new(menu,1)]}).Id);
        }
        void Sql(string q){using var c=new SqlConnection(cadena);c.Open();using var cmd=new SqlCommand("SET QUOTED_IDENTIFIER ON;SET ANSI_NULLS ON;SET ANSI_WARNINGS ON;SET ANSI_PADDING ON;SET ARITHABORT ON;SET CONCAT_NULL_YIELDS_NULL ON;SET NUMERIC_ROUNDABORT OFF;"+q,c);cmd.ExecuteNonQuery();}
        int Cantidad(string q){using var c=new SqlConnection(cadena);c.Open();using var cmd=new SqlCommand(q,c);return (int)cmd.ExecuteScalar()!;}
        void Si(bool condicion,string mensaje){if(!condicion)throw new Exception(mensaje);}
        void Rechaza(Action accion){try{accion();}catch(Exception ex)when(ex is ArgumentException or InvalidOperationException or UnauthorizedAccessException){return;}throw new Exception("Se aceptó una operación inválida.");}
        caso("Cobro: efectivo con cambio y comprobante académico",()=>
        {
            var c=Cuenta();var s=Solicitud(c,entregado:20);var r=repo.RegistrarCobro(s);
            Si(r.Cambio==10&&r.Total==10&&r.Estado=="Confirmado","Cambio incorrecto.");
            Si(repo.ListarMesas().Single(m=>m.Id==c.MesaId).Estado=="En limpieza","No pasó a limpieza.");
            var texto=repo.ConsultarComprobante(c.PedidoId);
            Si(texto.Contains("no válido como documento tributario")&&texto.Contains("Q 10.00")&&texto.Contains("Efectivo"),"Comprobante incompleto.");
            Si(repo.RegistrarCobro(s)==r,"Reintento no recupera el mismo resultado.");
            Si(Cantidad($"SELECT COUNT(*) FROM dbo.PagosManuales WHERE PedidoId={c.PedidoId}")==1,"Pago duplicado.");
            Rechaza(()=>repo.RegistrarCobro(s with{Entregado=30}));
            Rechaza(()=>repo.GestionarMesa(new(Guid.NewGuid(),c.MesaId,c.PedidoId,c.Version,"Cancelar","No debe cancelar un comprobante",true)));
        });
        caso("Cobro: efectivo insuficiente y datos no confirmados no cierran cuenta",()=>
        {
            var c=Cuenta();Rechaza(()=>repo.RegistrarCobro(Solicitud(c,entregado:9)));
            Rechaza(()=>repo.RegistrarCobro(Solicitud(c)with{DatosConfirmados=false}));
            Rechaza(()=>repo.RegistrarCobro(Solicitud(c)with{PublicoGeneral=false,Receptor=" "}));
            Rechaza(()=>repo.RegistrarCobro(Solicitud(c)with{Correo="correo inválido"}));
            Si(repo.ObtenerPedidoAbierto(c.MesaId) is not null,"Cuenta cerrada por validación.");
            Si(Cantidad($"SELECT COUNT(*) FROM dbo.PagosManuales WHERE PedidoId={c.PedidoId}")==0,"Validación dejó pagos parciales.");
        });
        caso("Cobro: versión concurrente exige nueva revisión",()=>
        {
            var c=Cuenta();repo.AgregarProductoAPedido(c.PedidoId,menu,1);
            Rechaza(()=>repo.RegistrarCobro(Solicitud(c)));
            Si(repo.RevisarCuenta(c.PedidoId).Total==20,"Se perdió incorporación concurrente.");
        });
        caso("Cobro: receptor histórico no cambia con el catálogo",()=>
        {
            var c=Cuenta();var s=Solicitud(c)with{PublicoGeneral=false,Receptor="Receptor confirmado",Identificacion="Identificación académica",Direccion="Guatemala",Correo="receptor@example.com"};
            repo.RegistrarCobro(s);repo.GuardarRestaurante("Nombre nuevo","Ubicación nueva");
            var texto=repo.ConsultarComprobante(c.PedidoId);
            Si(texto.Contains("Receptor confirmado")&&texto.Contains("receptor@example.com")&&!texto.Contains("Nombre nuevo"),"Se alteró captura histórica.");
        });
        caso("Cobro: tarjeta requiere comprobación y referencia manual",()=>
        {
            var c=Cuenta();var s=Solicitud(c,"Tarjeta");
            Rechaza(()=>repo.RegistrarCobro(s with{PagoConfirmado=false}));
            Rechaza(()=>repo.RegistrarCobro(s with{Referencia=""}));
            var r=repo.RegistrarCobro(s);Si(r.Estado=="Confirmado"&&r.Cambio==0,"Tarjeta inválida.");
            Si(repo.ConsultarComprobante(c.PedidoId).Contains("Pago registrado por el operador"),"Aparenta verificación bancaria.");
        });
        caso("Cobro: tarjeta rechazada conserva cuenta y permite otro intento",()=>
        {
            var c=Cuenta();var r=repo.RegistrarCobro(Solicitud(c,"Tarjeta",resultado:"Rechazado")with{PagoConfirmado=false});
            Si(r.NumeroComprobante is null&&repo.ObtenerPedidoAbierto(c.MesaId)is not null,"Rechazo cerró cuenta.");
            Si(repo.RegistrarCobro(Solicitud(c)).Estado=="Confirmado","No permitió efectivo posterior.");
        });
        caso("Cobro: tarjeta incierta bloquea repetición, productos y cancelación",()=>
        {
            var c=Cuenta();var s=Solicitud(c,"Tarjeta",resultado:"Incierto")with{PagoConfirmado=false};
            var r=repo.RegistrarCobro(s);Si(r.Estado=="Incierto"&&r.NumeroComprobante is null,"Incierto se cobró.");
            Rechaza(()=>repo.RegistrarCobro(Solicitud(c)));
            Rechaza(()=>repo.AgregarProductoAPedido(c.PedidoId,menu,1));
            Rechaza(()=>repo.GestionarMesa(new(Guid.NewGuid(),c.MesaId,c.PedidoId,c.Version,"Cancelar","Intento incorrecto",true)));
            var op=Guid.NewGuid();var confirmado=repo.ConciliarTarjeta(s.OperacionId,true,"Referencia comprobada con el terminal",op);
            Si(confirmado.NumeroComprobante is not null,"Conciliación no emitió.");
            Si(repo.ConciliarTarjeta(s.OperacionId,true,"Referencia comprobada con el terminal",op)==confirmado,"Conciliación duplicada.");
            Si(repo.RegistrarCobro(s).Estado=="Confirmado","Reintento original no refleja conciliación.");
        });
        caso("Cobro: conciliación rechazada habilita nuevo pago",()=>
        {
            var c=Cuenta();var s=Solicitud(c,"Tarjeta",resultado:"Incierto");repo.RegistrarCobro(s);
            repo.ConciliarTarjeta(s.OperacionId,false,"El terminal confirmó que no se cobró",Guid.NewGuid());
            Si(repo.RegistrarCobro(Solicitud(c)).Estado=="Confirmado","Cuenta bloqueada después de rechazo confirmado.");
        });
        caso("Gestión: cancelación conserva líneas y deja mesa en limpieza",()=>
        {
            var c=Cuenta();Rechaza(()=>repo.GestionarMesa(new(Guid.NewGuid(),c.MesaId,c.PedidoId,c.Version,"Cancelar","Sin declaración",false)));
            var s=new SolicitudGestionMesa(Guid.NewGuid(),c.MesaId,c.PedidoId,c.Version,"Cancelar","Cliente se retiró antes de preparación",true);
            Si(repo.GestionarMesa(s)=="Cancelado"&&repo.GestionarMesa(s)=="Cancelado","Cancelación no recuperable.");
            Si(repo.ListarDetallesPedido(c.PedidoId).Count==1,"Se borraron líneas.");
            Si(repo.ObtenerPedidoAbierto(c.MesaId)is null,"Pedido continúa activo.");
            repo.GestionarMesa(new(Guid.NewGuid(),c.MesaId,null,null,"Confirmar limpieza","Limpieza terminada",false));
            Si(repo.ListarMesas().Single(m=>m.Id==c.MesaId).Estado=="Disponible","Mesa no liberada tras limpieza.");
        });
        caso("Gestión: incidencia conserva deuda y no crea pago ficticio",()=>
        {
            var c=Cuenta();repo.GestionarMesa(new(Guid.NewGuid(),c.MesaId,c.PedidoId,c.Version,"Incidencia","Cliente se retiró con consumo pendiente",false));
            Si(Cantidad($"SELECT COUNT(*) FROM dbo.IncidenciasConsumo WHERE PedidoId={c.PedidoId} AND ImportePendiente=10")==1,"Se perdió el saldo.");
            Si(Cantidad($"SELECT COUNT(*) FROM dbo.PagosManuales WHERE PedidoId={c.PedidoId}")==0,"Se inventó pago.");
            Si(Cantidad($"SELECT COUNT(*) FROM dbo.Facturas WHERE PedidoId={c.PedidoId}")==0,"Se inventó documento.");
        });
        caso("Gestión: mesa inconsistente solo se corrige con motivo y sin pedido",()=>
        {
            repo.CrearMesa(++numero,4);var m=repo.ListarMesas().Single(x=>x.Numero==numero);
            Sql($"UPDATE dbo.Mesas SET Estado=N'Ocupada' WHERE Id={m.Id};");
            Rechaza(()=>repo.GestionarMesa(new(Guid.NewGuid(),m.Id,null,null,"Corregir estado","",false)));
            Si(repo.GestionarMesa(new(Guid.NewGuid(),m.Id,null,null,"Corregir estado","Estado incoherente diagnosticado",false))=="En limpieza","No corrigió.");
            var c=Cuenta();Rechaza(()=>repo.GestionarMesa(new(Guid.NewGuid(),c.MesaId,null,null,"Corregir estado","Tiene cuenta",false)));
        });
        caso("Permisos: empleado no cancela, no corrige ni crea mesas",()=>
        {
            var empleado=Repo(cadena,"segundo","ClavePrueba123!");var c=Cuenta();
            Rechaza(()=>empleado.GestionarMesa(new(Guid.NewGuid(),c.MesaId,c.PedidoId,c.Version,"Cancelar","Empleado sin permiso",true)));
            Rechaza(()=>empleado.CrearMesa(15000,4));
            Si(empleado.RegistrarCobro(Solicitud(c)).Estado=="Confirmado","Empleado no puede registrar cobro normal.");
            empleado.CerrarSesion();Rechaza(()=>empleado.RegistrarCobro(Solicitud(Cuenta())));
        });
        caso("Idempotencia: abrir y agregar no se duplican al perder respuesta",()=>
        {
            repo.CrearMesa(++numero,4);var m=repo.ListarMesas().Single(x=>x.Numero==numero);
            var s=new SolicitudPedido{MesaId=m.Id,Detalles=[new(menu,1)]};var p=repo.CrearPedido(s);
            Si(repo.CrearPedido(s).Id==p.Id,"Se duplicó pedido.");
            var op=Guid.NewGuid();var total=repo.AgregarProductoAPedido(p.Id,menu,1,op);
            Si(repo.AgregarProductoAPedido(p.Id,menu,1,op)==total&&Cantidad($"SELECT COUNT(*) FROM dbo.DetallesPedido WHERE PedidoId={p.Id}")==2 && repo.ListarDetallesPedido(p.Id).Sum(d=>d.Cantidad)==2,"Se duplicó incorporación.");
            Rechaza(()=>repo.AgregarProductoAPedido(p.Id,menu,2,op));
            var nuevo=Repo(cadena);Si(nuevo.AgregarProductoAPedido(p.Id,menu,1,op)==total,"Nuevo repositorio no recuperó operación persistida.");
        });
        caso("Concurrencia: dos cierres solo producen un pago y comprobante",()=>
        {
            var c=Cuenta();var solicitudes=new[]{Solicitud(c),Solicitud(c)};
            var resultados=Task.WhenAll(solicitudes.Select(s=>Task.Run(()=>{try{return Repo(cadena).RegistrarCobro(s).Estado=="Confirmado";}catch(InvalidOperationException){return false;}}))).GetAwaiter().GetResult();
            Si(resultados.Count(x=>x)==1,"Más de un cierre.");
            Si(Cantidad($"SELECT COUNT(*) FROM dbo.PagosManuales WHERE PedidoId={c.PedidoId} AND Estado=N'Confirmado'")==1,"Pagos duplicados.");
        });
        caso("Concurrencia: cancelación y agregado no dejan inconsistencias",()=>
        {
            var c=Cuenta();var tareas=new Action[]{()=>repo.GestionarMesa(new(Guid.NewGuid(),c.MesaId,c.PedidoId,c.Version,"Cancelar","Cancelación concurrente",true)),()=>repo.AgregarProductoAPedido(c.PedidoId,menu,1)};
            Task.WhenAll(tareas.Select(a=>Task.Run(()=>{try{a();}catch(InvalidOperationException){} }))).GetAwaiter().GetResult();
            var actual=repo.RevisarCuenta(c.PedidoId);
            Si(actual.Detalles.Sum(d=>d.Importe)==actual.Total,"Total inconsistente.");
            if(actual.Estado=="Cancelado")Si(actual.Detalles.Count==1,"Agregado después de cancelar.");
        });
        caso("Reportes: cobros, cancelaciones, incidencias y legados separados",()=>
        {
            var filas=repo.ReportarOperaciones(RelojGuatemala.Hoy,RelojGuatemala.Hoy);
            Si(filas.Any(f=>f.Concepto=="Pago Efectivo")&&filas.Any(f=>f.Concepto=="Pago Tarjeta")&&filas.Any(f=>f.Concepto=="Cancelar")&&filas.Any(f=>f.Concepto=="Incidencia"),"Faltan categorías financieras.");
            Si(filas.Where(f=>f.Concepto=="Comprobante legado").All(f=>f.Estado=="Pago no registrado"),"Se inventó cobro legado.");
        });
        caso("API antigua no permite facturar sin pago",()=>Rechaza(()=>repo.FacturarPedido(Cuenta().PedidoId)));
        caso("Recuperación cifrada: pérdida de respuesta después del commit no duplica cobro",()=>
        {
            var c=Cuenta();var s=Solicitud(c);
            try
            {
                Task.Run(()=>DiarioOperaciones.Ejecutar<ResultadoCobro>(repo,"Cobro",s.OperacionId,s,()=>{
                    repo.RegistrarCobro(s);throw new IOException("Fallo simulado de entrega de respuesta después de confirmar SQL.");
                })).GetAwaiter().GetResult();
                throw new Exception("No se inyectó el fallo.");
            }
            catch(IOException){}
            var pendiente=DiarioOperaciones.Listar(repo).Single(x=>x.Id==s.OperacionId);
            var nuevo=Repo(cadena);
            var archivo=Path.Combine(salida,"recuperacion-diario-proceso.json");
            var inicio=new ProcessStartInfo(Environment.ProcessPath!){UseShellExecute=false,CreateNoWindow=true,WindowStyle=ProcessWindowStyle.Hidden,RedirectStandardOutput=true,RedirectStandardError=true};
            if(Path.GetFileNameWithoutExtension(Environment.ProcessPath!).Equals("dotnet",StringComparison.OrdinalIgnoreCase))inicio.ArgumentList.Add(Assembly.GetExecutingAssembly().Location);
            foreach(var argumento in new[]{"--recuperar-diario",archivo,s.OperacionId.ToString()})inicio.ArgumentList.Add(argumento);
            using(var proceso=Process.Start(inicio)!)
            {
                var errores=proceso.StandardError.ReadToEndAsync();var texto=proceso.StandardOutput.ReadToEndAsync();
                if(!proceso.WaitForExit(30000)){proceso.Kill();throw new TimeoutException("La recuperación en otro proceso no terminó.");}
                if(proceso.ExitCode!=0)throw new Exception(errores.GetAwaiter().GetResult()+texto.GetAwaiter().GetResult());
            }
            using(var json=JsonDocument.Parse(File.ReadAllText(archivo)))Si(json.RootElement.GetProperty("Pid").GetInt32()!=Environment.ProcessId&&!json.RootElement.GetProperty("Pendiente").GetBoolean(),"No se recuperó desde otro proceso.");
            Si(!DiarioOperaciones.TienePendiente(nuevo,s.OperacionId),"No se cerró el registro de recuperación.");
            Si(Cantidad($"SELECT COUNT(*) FROM dbo.PagosManuales WHERE PedidoId={c.PedidoId}")==1,"Se volvió a cobrar al recuperar.");
        });
        caso("Recuperación: fallo antes de SQL conserva solicitud y registra abandono terminal al descartarla",()=>
        {
            var c=Cuenta();var s=Solicitud(c);
            try{Task.Run(()=>DiarioOperaciones.Ejecutar<ResultadoCobro>(repo,"Cobro",s.OperacionId,s,()=>throw new IOException("Fallo simulado antes de enviar SQL."))).GetAwaiter().GetResult();}
            catch(IOException){}
            var pendiente=DiarioOperaciones.Listar(repo).Single(x=>x.Id==s.OperacionId);
            Si(!repo.OperacionAplicada(s.OperacionId),"Se registró un cobro no enviado.");
            DiarioOperaciones.DescartarRechazada(repo,pendiente);
            Si(repo.ObtenerPedidoAbierto(c.MesaId)is not null,"Se cerró una cuenta no cobrada.");
        });
        caso("Transacción: fallo al emitir comprobante revierte pago, operación y estado",()=>
        {
            var c=Cuenta();var s=Solicitud(c);
            Sql("EXEC(N'CREATE TRIGGER dbo.PruebaFalloComprobante ON dbo.Facturas AFTER INSERT AS THROW 51999,N''Fallo aislado de emisión'',1;');");
            try
            {
                try{repo.RegistrarCobro(s);throw new Exception("No se inyectó el fallo SQL.");}catch(SqlException ex)when(ex.Number==51999){}
                Si(!repo.OperacionAplicada(s.OperacionId),"Quedó una operación parcial.");
                Si(Cantidad($"SELECT COUNT(*) FROM dbo.PagosManuales WHERE PedidoId={c.PedidoId}")==0,"Quedó un pago sin comprobante.");
                Si(repo.ObtenerPedidoAbierto(c.MesaId)is not null&&repo.ListarMesas().Single(m=>m.Id==c.MesaId).Estado=="Ocupada","Se cerró la mesa aunque falló SQL.");
            }
            finally{Sql("DROP TRIGGER dbo.PruebaFalloComprobante;");}
        });
        caso("Concurrencia: versión detecta cambios aunque el importe no cambie",()=>
        {
            repo.GuardarProducto(null,"Cero versión","Cobros","Bebida",0,false);var cero=repo.ListarMenu().Single(m=>m.Nombre=="Cero versión").Id;
            var c=Cuenta();repo.AgregarProductoAPedido(c.PedidoId,cero,1);
            Si(repo.RevisarCuenta(c.PedidoId).Total==c.Total,"El producto cero alteró importe.");
            Rechaza(()=>repo.RegistrarCobro(Solicitud(c)));
        });
        caso("Permisos: cortesía cero requiere administrador y autorización explícita",()=>
        {
            repo.CrearMesa(++numero,4);var mesa=repo.ListarMesas().Single(m=>m.Numero==numero);var cero=repo.ListarMenu().Single(m=>m.Nombre=="Cero versión").Id;
            var c=repo.RevisarCuenta(repo.CrearPedido(new(){MesaId=mesa.Id,Detalles=[new(cero,1)]}).Id);
            Rechaza(()=>Repo(cadena,"segundo","ClavePrueba123!").RegistrarCobro(Solicitud(c,"Cortesía")));
            Rechaza(()=>repo.RegistrarCobro(Solicitud(c,"Cortesía")with{CortesiaAutorizada=false}));
            Si(repo.RegistrarCobro(Solicitud(c,"Cortesía")).Total==0,"No cerró cortesía válida.");
        });
        caso("Historial: editar cliente y renumerar mesa no altera referencias anteriores",()=>
        {
            repo.GuardarCliente(null,"Cliente histórico de cobro",null,null);var cliente=repo.ListarClientes().Single(c=>c.Nombre=="Cliente histórico de cobro");
            repo.CrearMesa(++numero,4);var mesa=repo.ListarMesas().Single(m=>m.Numero==numero);
            var pedido=repo.CrearPedido(new(){MesaId=mesa.Id,ClienteId=cliente.Id,Detalles=[new(menu,1)]});Facturar(repo,pedido.Id);
            repo.GuardarCliente(cliente.Id,"Cliente editado después",null,null);repo.ActualizarMesa(mesa.Id,++numero,4,"Disponible");
            var antiguo=repo.ListarPedidos().Single(p=>p.Id==pedido.Id);
            Si(antiguo.Cliente=="Cliente histórico de cobro"&&antiguo.NumeroMesa==mesa.Numero,"El historial depende del catálogo actual.");
        });
        caso("Migración: documentos anteriores siguen legados sin pagos inventados",()=>
        {
            var anterior=Environment.GetEnvironmentVariable("RESTAURANTE_TEST_ANTERIOR")??throw new Exception("Falta fixture anterior.");
            var legado=Repo(anterior);var p=legado.ListarPedidos().Single();
            Si(legado.ConsultarComprobante(p.Id).Contains("pago no registrado"),"Se presentó como pagado.");
            using var c=new SqlConnection(anterior);c.Open();using var q=new SqlCommand("SELECT COUNT(*) FROM dbo.PagosManuales;",c);
            Si((int)q.ExecuteScalar()! ==0,"La migración inventó cobros.");
        });
    }
}
