using System.Text.Json;
using System.Security.Cryptography;
using Microsoft.Data.SqlClient;
using Restaurante.Backend.Dominio;
using Restaurante.Backend.Servicios;
using Restaurante.Frontend.Formularios;
using Restaurante.Frontend.Servicios;
namespace PruebasRoles;
internal static class PruebasAuditoria
{
    private const string Clave="PruebaPersonal_2026!",Nueva="NuevaRecuperacion2026!";
    public static void Ejecutar(string cadena,RepositorioRestaurante admin,string dir,Action<bool,string> ok)
    {
        Directory.CreateDirectory(dir);
        object? Sql(string texto,params (string,object?)[] valores){using var c=new SqlConnection(cadena);c.Open();using var q=new SqlCommand(texto,c){CommandTimeout=60};foreach(var(n,v)in valores)q.Parameters.AddWithValue(n,v??DBNull.Value);return q.ExecuteScalar();}
        RepositorioRestaurante Repo(string u,string clave=Clave){var repo=new RepositorioRestaurante(new ConfiguracionConexionTemporal(cadena));if(repo.IniciarSesion(u,clave) is null)throw new Exception("Fixture no autenticó");return repo;}
        void Deniega(Action a,string texto){try{a();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or UnauthorizedAccessException){ok(true,texto);return;}throw new Exception("NO rechazó: "+texto);}
        string suf=Guid.NewGuid().ToString("N")[..8];
        if(Convert.ToInt32(Sql("SELECT MAX(Version) FROM dbo.VersionEsquema"))==5)ok(!admin.RecuperacionDisponible(),"Esquema 5 no ofrece recuperación inexistente");
        var baseSql=Path.GetFullPath("restaurante2/BaseDatos");
        Sql(File.ReadAllText(Path.Combine(baseSql,"Migracion_006.sql")));Sql(File.ReadAllText(Path.Combine(baseSql,"Migracion_006.sql")));
        admin.ValidarEsquema();ok(admin.RecuperacionDisponible(),"006 completa e idempotente habilita recuperación");
        Sql("ALTER TABLE dbo.CodigosRecuperacion DROP COLUMN PausaHasta;");
        ok(!admin.RecuperacionDisponible(),"006 incompleta no se anuncia como disponible");Deniega(admin.ValidarEsquema,"006 incompleta rechaza validación de conexión");
        Sql("ALTER TABLE dbo.CodigosRecuperacion ADD PausaHasta datetime2 NULL;");
        Sql(File.ReadAllText(Path.Combine(baseSql,"Migracion_007.sql")));Sql(File.ReadAllText(Path.Combine(baseSql,"Migracion_007.sql")));
        admin.ValidarEsquema();ok(admin.CorreccionesDisponibles(),"007 aditiva e idempotente habilita correcciones");
        foreach(var rol in new[]{"Mesero","Gerente"}){var u=rol+suf;admin.GuardarUsuario(null,u,u,rol,true,Clave);Sql("UPDATE dbo.Usuarios SET CambioObligatorio=0 WHERE Usuario=@u;",("@u",u));}
        var mesero=Repo("Mesero"+suf);var gerente=Repo("Gerente"+suf);
        var numero=300000+Random.Shared.Next(100000);admin.CrearMesa(numero,4);var mesa=admin.ListarMesas().Single(x=>x.Numero==numero);var producto=admin.ListarMenu().First(x=>x.Precio>0);
        PedidoCreado Abrir(int unidades=2)=>mesero.CrearPedido(new SolicitudPedido{MesaId=mesa.Id,Detalles=[new(producto.Id,unidades)]});
        SolicitudCobro Efectivo(int id,Guid? op=null){var c=admin.RevisarCuenta(id);return new(op??Guid.NewGuid(),id,c.Version,c.Total,true,"Público general",null,null,null,"Efectivo",c.Total,"",null,null,true,true);}
        void Limpiar()=>mesero.GestionarMesa(new(Guid.NewGuid(),mesa.Id,null,null,"Confirmar limpieza","Limpieza de prueba",false));
        void Autorizar(Guid id,string tipo,object datos)=>mesero.AutorizarOperacion(id,tipo,JsonSerializer.Serialize(datos,datos.GetType()),"Gerente"+suf,Clave);
        var pedido=Abrir();ok(admin.RevisarCuenta(pedido.Id).Total==producto.Precio*2,"Administrador revisa cuenta abierta");ok(gerente.RevisarCuenta(pedido.Id).Total>0,"Gerente revisa cuenta abierta");ok(mesero.RevisarCuenta(pedido.Id).Total>0,"Mesero revisa cuenta abierta");
        admin.RegistrarCobro(Efectivo(pedido.Id));Deniega(()=>mesero.RevisarCuenta(pedido.Id),"Mesero no revisa cerrado facturado por otro operador");Deniega(()=>mesero.ListarDetallesPedido(pedido.Id),"Misma restricción en detalles");Deniega(()=>mesero.ConsultarComprobante(pedido.Id),"Misma restricción en comprobante");ok(gerente.RevisarCuenta(pedido.Id).Estado=="Facturado","Gerente conserva lectura de cerrados");
        admin.ActualizarMesa(mesa.Id,numero+1000000,6,"En limpieza");ok(admin.ListarMesas().Single(x=>x.Id==mesa.Id).Estado=="En limpieza","Editar metadatos no libera mesa en limpieza");Deniega(()=>admin.ActualizarMesa(mesa.Id,numero+1000000,6,"Disponible"),"Catálogo no evita confirmación de limpieza");
        ApplicationConfiguration.Initialize();using(var f=new FormularioMesa(new Mesa(mesa.Id,10001,6,"En limpieza"))){ok(f.NumeroMesa==10001&&f.EstadoMesa=="En limpieza","Editor conserva número 10001 y estado real");}
        Limpiar();pedido=Abrir(3);var cuenta=mesero.RevisarCuenta(pedido.Id);var linea=mesero.ListarLineasCorregibles(pedido.Id).Single();
        var correccion=new SolicitudCorreccionLinea(Guid.NewGuid(),pedido.Id,linea.Id,1,cuenta.Version,cuenta.Total,"Error de captura ficticio",true);
        Deniega(()=>mesero.CorregirLinea(correccion),"Corrección de mesero sin autorización rechazada");Autorizar(correccion.OperacionId,"CorreccionLinea",correccion);
        ok(mesero.CorregirLinea(correccion)==producto.Precio*2,"Corrección autorizada reduce unidades vigentes");ok(mesero.CorregirLinea(correccion)==producto.Precio*2,"Replay de corrección no vuelve a retirar");
        linea=mesero.ListarLineasCorregibles(pedido.Id).Single();ok(linea.CantidadOriginal==3&&linea.CantidadVigente==2,"Captura original e historial conservados");ok(mesero.ListarDetallesPedido(pedido.Id).Single().Cantidad==2,"Cuenta muestra unidades vigentes");
        Deniega(()=>admin.CorregirLinea(correccion with{OperacionId=Guid.NewGuid(),CantidadRetirada=5}),"Cantidad excesiva no se aplica");
        var op=Guid.NewGuid();var preparacion=mesero.MarcarPreparacion(pedido.Id,op);ok(preparacion=="En preparación"&&mesero.MarcarPreparacion(pedido.Id,op)==preparacion,"Preparación idempotente declarada");
        cuenta=admin.RevisarCuenta(pedido.Id);Deniega(()=>admin.CorregirLinea(correccion with{OperacionId=Guid.NewGuid(),Version=cuenta.Version,TotalEsperado=cuenta.Total}),"No se oculta consumo después de preparación");
        Deniega(()=>gerente.GestionarMesa(new(Guid.NewGuid(),mesa.Id,pedido.Id,cuenta.Version,"Cancelar","Intento tras preparar",true)),"Declaración no permite cancelar preparación ya registrada");
        var incidencia=new SolicitudGestionMesa(Guid.NewGuid(),mesa.Id,pedido.Id,cuenta.Version,"Incidencia","Consumo pendiente ficticio",false);gerente.GestionarMesa(incidencia);
        Deniega(()=>mesero.ResolverIncidencia(new(Guid.NewGuid(),pedido.Id,"Pérdida","Pérdida ficticia",null)),"Mesero no resuelve incidencias");
        Limpiar();var nuevo=Abrir(1);var estadoAntes=admin.ListarMesas().Single(x=>x.Id==mesa.Id).Estado;
        var resolver=new SolicitudResolucionIncidencia(Guid.NewGuid(),pedido.Id,"Cobro posterior","Recepción completa ficticia",null);resolver=resolver with{Cobro=Efectivo(pedido.Id,resolver.OperacionId)};
        var resultado=gerente.ResolverIncidencia(resolver);ok(resultado.Contains("comprobante"),"Cobro posterior emite comprobante académico");ok(gerente.ResolverIncidencia(resolver)==resultado,"Replay de resolución no duplica cobro");ok(admin.ListarMesas().Single(x=>x.Id==mesa.Id).Estado==estadoAntes,"Cobro de incidencia no libera mesa reutilizada");
        ok(admin.ListarIncidencias().Single(x=>x.PedidoId==pedido.Id).Estado=="Resuelta","Incidencia tiene desenlace auditable");
        Deniega(()=>gerente.ResolverIncidencia(resolver with{OperacionId=Guid.NewGuid(),Desenlace="Pérdida",Cobro=null}),"Incidencia resuelta no admite otro desenlace");
        foreach(var desenlace in new[]{"Pérdida","Condonación"})
        {
            cuenta=admin.RevisarCuenta(nuevo.Id);gerente.GestionarMesa(new(Guid.NewGuid(),mesa.Id,nuevo.Id,cuenta.Version,"Incidencia","Incidencia ficticia",false));
            gerente.ResolverIncidencia(new(Guid.NewGuid(),nuevo.Id,desenlace,"Desenlace ficticio autorizado",null));
            ok(Convert.ToInt32(Sql("SELECT COUNT(*) FROM dbo.PagosManuales WHERE PedidoId=@p;",("@p",nuevo.Id)))==0,desenlace+" no inventa efectivo ni tarjeta");
            Limpiar();nuevo=Abrir(1);
        }
        cuenta=admin.RevisarCuenta(nuevo.Id);gerente.GestionarMesa(new(Guid.NewGuid(),mesa.Id,nuevo.Id,cuenta.Version,"Incidencia","Prueba de tarjeta posterior",false));
        var idTarjeta=Guid.NewGuid();var efectivoCorto=Efectivo(nuevo.Id,idTarjeta) with{Entregado=cuenta.Total-1};
        Deniega(()=>gerente.ResolverIncidencia(new(idTarjeta,nuevo.Id,"Cobro posterior","Importe insuficiente ficticio",efectivoCorto)),"Cobro posterior rechaza efectivo insuficiente");
        var tarjeta=efectivoCorto with{Entregado=cuenta.Total,Metodo="Tarjeta",ResultadoTarjeta="Incierto",Referencia="Referencia ficticia"};
        Deniega(()=>gerente.ResolverIncidencia(new(idTarjeta,nuevo.Id,"Cobro posterior","Resultado incierto ficticio",tarjeta)),"Cobro posterior no inventa confirmación de tarjeta incierta");
        tarjeta=tarjeta with{ResultadoTarjeta="Confirmado"};gerente.ResolverIncidencia(new(idTarjeta,nuevo.Id,"Cobro posterior","Tarjeta manual comprobada",tarjeta));
        ok(Convert.ToInt32(Sql("SELECT COUNT(*) FROM dbo.PagosManuales WHERE PedidoId=@p AND Metodo=N'Tarjeta' AND Estado=N'Confirmado';",("@p",nuevo.Id)))==1,"Incidencia admite tarjeta manual confirmada con referencia");
        Limpiar();admin.GuardarProducto(null,"Producto cero "+suf,"Pruebas","Plato",0,false);var gratis=admin.ListarMenu().Single(x=>x.Nombre=="Producto cero "+suf);
        var cero=mesero.CrearPedido(new SolicitudPedido{MesaId=mesa.Id,Detalles=[new(gratis.Id,1)]});cuenta=admin.RevisarCuenta(cero.Id);gerente.GestionarMesa(new(Guid.NewGuid(),mesa.Id,cero.Id,cuenta.Version,"Incidencia","Saldo cero ficticio",false));
        var idCero=Guid.NewGuid();var pagoCero=Efectivo(cero.Id,idCero);Deniega(()=>gerente.ResolverIncidencia(new(idCero,cero.Id,"Cobro posterior","Intento cero",pagoCero)),"Saldo cero no registra efectivo o tarjeta ficticios");
        gerente.ResolverIncidencia(new(Guid.NewGuid(),cero.Id,"Condonación","Cierre de saldo cero sin pago",null));ok(Convert.ToInt32(Sql("SELECT COUNT(*) FROM dbo.PagosManuales WHERE PedidoId=@p;",("@p",cero.Id)))==0,"Saldo cero se resuelve conservando historial sin pago ficticio");
        Limpiar();nuevo=Abrir(1);
        var cliente="Cliente"+suf;mesero.GuardarCliente(null,cliente,null,null);var cid=mesero.ListarClientes().Single(x=>x.Nombre==cliente).Id;
        Deniega(()=>mesero.GuardarCliente(cid,"Modificado",null,null),"Mesero crea receptor pero no edita clientes existentes");gerente.GuardarCliente(cid,cliente+" autorizado",null,null);ok(true,"Gerente edita clientes");
        Deniega(()=>ValidacionAjuste.Comprobar("Descuento",100,50),"Ajuste excesivo se valida antes de autorización");Deniega(()=>ValidacionAjuste.Comprobar("Descuento",50,50),"Descuento completo requiere cortesía");ValidacionAjuste.Comprobar("Cortesía",50,50);ok(true,"Cortesía válida conserva política");
        cuenta=admin.RevisarCuenta(nuevo.Id);var lcon=admin.ListarLineasCorregibles(nuevo.Id).Single();
        var concurrente=new SolicitudCorreccionLinea(Guid.NewGuid(),nuevo.Id,lcon.Id,1,cuenta.Version,cuenta.Total,"Corrección concurrente ficticia",true);
        using(var barrera=new Barrier(2))
        {
            Task<bool> Intentar(SolicitudCorreccionLinea c)=>Task.Run(()=>{barrera.SignalAndWait();try{admin.CorregirLinea(c);return true;}catch(InvalidOperationException){return false;}});
            var intentos=new[]{Intentar(concurrente),Intentar(concurrente with{OperacionId=Guid.NewGuid()})};Task.WaitAll(intentos);ok(intentos.Count(x=>x.Result)==1,"Correcciones concurrentes con versión idéntica aplican una sola vez");
        }
        var idAbandono=Guid.NewGuid();mesero.AbandonarOperacion(idAbandono);mesero.AbandonarOperacion(idAbandono);
        Deniega(()=>mesero.AgregarProductoAPedido(nuevo.Id,producto.Id,1,idAbandono),"Abandono terminal impide envío posterior");
        var idEnVuelo=Guid.NewGuid();using(var barrera=new Barrier(2))
        {
            var a=Task.Run(()=>{barrera.SignalAndWait();try{Repo("Mesero"+suf).AgregarProductoAPedido(nuevo.Id,producto.Id,1,idEnVuelo);return "aplicada";}catch(InvalidOperationException){return "abandonada";}});
            var b=Task.Run(()=>{barrera.SignalAndWait();try{Repo("Mesero"+suf).AbandonarOperacion(idEnVuelo);return "abandonada";}catch(InvalidOperationException){return "aplicada";}});
            Task.WaitAll(a,b);ok(a.Result==b.Result,"Carrera SQL aplicar/abandonar tiene un solo resultado terminal");
        }
        SynchronizationContext.SetSynchronizationContext(null); // Headless runner has no WinForms message loop.
        var idPerdida=Guid.NewGuid();var original=mesero.RevisarCuenta(nuevo.Id).Total;var perdida=new SolicitudIncorporacion(idPerdida,nuevo.Id,producto.Id,1);
        try{DiarioOperaciones.Ejecutar(mesero,"Agregar",idPerdida,perdida,()=>{mesero.AgregarProductoAPedido(nuevo.Id,producto.Id,1,idPerdida);return Fallar("Respuesta perdida simulada después de commit");}).GetAwaiter().GetResult();}catch(IOException){}
        var pendiente=DiarioOperaciones.Listar(mesero).Single(x=>x.Id==idPerdida);DiarioOperaciones.Reanudar(mesero,pendiente).GetAwaiter().GetResult();ok(mesero.RevisarCuenta(nuevo.Id).Total==original+producto.Precio,"Respuesta perdida se recupera sin doble incorporación");
        var corrupto=Guid.NewGuid();var carpeta=Path.Combine(Path.GetTempPath(),"SaboresPruebas","Operaciones");Directory.CreateDirectory(carpeta);var archivo=Path.Combine(carpeta,mesero.IdentificadorBase+"_"+mesero.UsuarioActual!.Id+"_"+corrupto.ToString("N")+".bin");File.WriteAllBytes(archivo,[1,2,3]);
        ok(DiarioOperaciones.Listar(mesero).Any(x=>x.Id==corrupto&&x.Tipo=="Ilegible"),"Diario dañado se identifica y conserva sin bloquear listado");
        var formatoMalo=Guid.NewGuid();var falso=new OperacionPendienteLocal(formatoMalo,mesero.IdentificadorBase,mesero.UsuarioActual.Id,"Cobro","{invalid");
        var rutaMala=Path.Combine(carpeta,mesero.IdentificadorBase+"_"+mesero.UsuarioActual.Id+"_"+formatoMalo.ToString("N")+".bin");
        File.WriteAllBytes(rutaMala,ProtectedData.Protect(JsonSerializer.SerializeToUtf8Bytes(falso),null,DataProtectionScope.CurrentUser));
        ok(DiarioOperaciones.Listar(mesero).Any(x=>x.Id==formatoMalo&&x.Tipo=="Ilegible"),"Diario cifrado con solicitud interna inválida también se conserva como ilegible");
        var noEnviado=Guid.NewGuid();try{DiarioOperaciones.Ejecutar(mesero,"Agregar",noEnviado,new SolicitudIncorporacion(noEnviado,nuevo.Id,producto.Id,1),()=>{return Fallar("Antes de enviar");}).GetAwaiter().GetResult();}catch(IOException){}
        ok(DiarioOperaciones.Listar(mesero).Any(x=>x.Id==noEnviado)&&File.Exists(archivo),"Un registro válido sigue visible junto al dañado");DiarioOperaciones.DescartarRechazada(mesero,DiarioOperaciones.Listar(mesero).Single(x=>x.Id==noEnviado));ok(!DiarioOperaciones.TienePendiente(mesero,noEnviado),"Descarte deja tombstone SQL antes de retirar diario");
        var recibo=Convert.ToHexString(RandomNumberGenerator.GetBytes(32));var admin2="AdminRec"+suf;admin.GuardarUsuario(null,admin2,admin2,"Administrador",true,Clave);Sql("UPDATE dbo.Usuarios SET CambioObligatorio=0 WHERE Usuario=@u;",("@u",admin2));
        var publico=new RepositorioRestaurante(new ConfiguracionConexionTemporal(cadena));var recuperar=Guid.NewGuid();ok(publico.RecuperarConAdministrador(recuperar,recibo,admin2,"admin",Clave,Nueva).Correcto,"Recuperación por otro administrador válida");
        ok(publico.RecuperarConAdministrador(recuperar,recibo,admin2,"admin",Clave,Nueva).Correcto,"Replay original de recuperación confirma resultado");
        ok(!publico.RecuperarConAdministrador(recuperar,recibo,"admin","admin",Clave,Nueva).Correcto,"Replay con destino distinto no informa éxito");
        ok(!publico.RecuperarConAdministrador(recuperar,recibo,admin2,admin2,Nueva,Nueva).Correcto,"Replay con autorizador distinto no informa éxito");
        ok(!publico.RecuperarConCodigo(recuperar,recibo,admin2,"inventado",Nueva).Correcto,"Replay con mecanismo distinto rechazado");
        ok(!publico.RecuperarConAdministrador(recuperar,recibo,admin2,"admin",Clave,"OtraContrasena2026!").Correcto,"Replay con contraseña distinta no informa éxito");
        ok(Repo(admin2,Nueva).UsuarioActual is not null,"Replay incompatible no cambia la contraseña original");
        var cuentaCodigo=Repo(admin2,Nueva);int excepcionesAcceso=0;
        EventHandler<System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs> observar=(_,e)=>{if(e.Exception is UnauthorizedAccessException)excepcionesAcceso++;};
        AppDomain.CurrentDomain.FirstChanceException+=observar;
        try
        {
            ok(publico.IntentarGenerarCodigoRecuperacion(Clave).Estado==EstadoGeneracionCodigo.SesionModificada,"Generación sin sesión devuelve mensaje sin excepción de acceso");
            ok(gerente.IntentarGenerarCodigoRecuperacion(Clave).Estado==EstadoGeneracionCodigo.SinPermiso,"Gerente recibe mensaje de permiso sin excepción");
            ok(cuentaCodigo.IntentarGenerarCodigoRecuperacion("").Estado==EstadoGeneracionCodigo.DatosIncompletos,"Generación vacía explica que requiere contraseña vigente");
            var generado=cuentaCodigo.IntentarGenerarCodigoRecuperacion(Nueva);ok(generado.Generado&&generado.Codigo?.Length==64,"Administrador genera código protegido y recibe siguiente paso");
            ok(!cuentaCodigo.IntentarConfirmarCodigoRecuperacion("invalido").Correcto,"Confirmación inválida devuelve un mensaje sin interrumpir el flujo");
            ok(cuentaCodigo.IntentarConfirmarCodigoRecuperacion(generado.Codigo!).Correcto,"Código válido se confirma sin cambiar la contraseña");var verificador=(byte[])Sql("SELECT Verificador FROM dbo.CodigosRecuperacion WHERE UsuarioId=@u;",("@u",cuentaCodigo.UsuarioActual!.Id))!;
            var error=cuentaCodigo.IntentarGenerarCodigoRecuperacion("IncorrectaFicticia");ok(error.Estado==EstadoGeneracionCodigo.CredencialesNoVerificadas&&error.Codigo is null,"Contraseña incorrecta produce mensaje y no devuelve un código");
            ok(verificador.SequenceEqual((byte[])Sql("SELECT Verificador FROM dbo.CodigosRecuperacion WHERE UsuarioId=@u;",("@u",cuentaCodigo.UsuarioActual.Id))!),"Contraseña incorrecta no revoca el código vigente");
            for(var i=0;i<4;i++)cuentaCodigo.IntentarGenerarCodigoRecuperacion("IncorrectaFicticia");
            ok(cuentaCodigo.IntentarGenerarCodigoRecuperacion(Nueva).Estado==EstadoGeneracionCodigo.CuentaBloqueada,"Bloqueo temporal se explica sin lanzar excepción de autorización");
            var temporal="AdminTemporal"+suf;admin.GuardarUsuario(null,temporal,temporal,"Administrador",true,Clave);var conTemporal=Repo(temporal);
            ok(conTemporal.IntentarGenerarCodigoRecuperacion(Clave).Estado==EstadoGeneracionCodigo.CambioObligatorio,"Administrador temporal recibe instrucción de cambiar contraseña antes de generar");
            var datos=admin.ListarUsuarios().Single(x=>x.Usuario==temporal);admin.GuardarUsuario(datos.Id,datos.Nombre,datos.Usuario,"Administrador",false,null);
            ok(conTemporal.IntentarGenerarCodigoRecuperacion(Clave).Estado==EstadoGeneracionCodigo.SesionModificada,"Desactivación durante sesión devuelve instrucción de volver a ingresar");
        }
        finally{AppDomain.CurrentDomain.FirstChanceException-=observar;}
        ok(excepcionesAcceso==0,"Los rechazos esperados de generación no lanzan UnauthorizedAccessException");
        admin.GuardarUsuario(mesero.UsuarioActual!.Id,"Mesero desactivado","Mesero"+suf,"Mesero",false,null);Deniega(()=>mesero.RevisarCuenta(nuevo.Id),"Desactivación invalida lectura en sesión existente");
        ok(Convert.ToInt32(Sql("SELECT COUNT(*) FROM dbo.Facturas GROUP BY PedidoId HAVING COUNT(*)>1"))==0,"No hay comprobantes duplicados");
        ok(Convert.ToInt32(Sql("SELECT COUNT(*) FROM dbo.Pedidos p WHERE p.ImporteOriginal<>COALESCE((SELECT SUM(d.Cantidad*d.PrecioUnitario) FROM dbo.DetallesPedidoVigentes d WHERE d.PedidoId=p.Id),0) OR p.Total<>p.ImporteOriginal-p.Descuento"))==0,"Totales coinciden con unidades vigentes y descuentos");
        using(var bloqueo=new SqlConnection(cadena))
        {
            bloqueo.Open();using var t=bloqueo.BeginTransaction();using(var q=new SqlCommand("UPDATE dbo.Usuarios WITH(TABLOCKX) SET Nombre=Nombre WHERE Usuario=N'admin';",bloqueo,t))q.ExecuteNonQuery();
            using var f=new FormularioUsuarios(admin);f.Show();Application.DoEvents();f.Close();ok(!f.IsDisposed,"Cerrar durante lectura de usuarios espera una finalización segura");
            t.Rollback();var limite=DateTime.UtcNow.AddSeconds(15);
            while(DateTime.UtcNow<limite&&!((System.Windows.Forms.Button)typeof(FormularioUsuarios).GetField("guardar",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!.GetValue(f)!).Enabled){Application.DoEvents();Thread.Sleep(10);}
            Application.DoEvents();f.Close();ok(f.IsDisposed,"Lectura finalizada permite cerrar sin tocar controles destruidos");
        }
        SynchronizationContext.SetSynchronizationContext(null);
        File.WriteAllText(Path.Combine(dir,"resumen.txt"),"Pruebas auditadas completadas. Base ficticia conservada. Archivos dañados de prueba conservados; no son del perfil habitual.");
    }
    private static decimal Fallar(string mensaje)=>throw new IOException(mensaje);
    public static void Visual(string cadena,RepositorioRestaurante admin,string tipo)
    {
        ApplicationConfiguration.Initialize();
        if(tipo=="resolucion"&&!admin.ListarIncidencias().Any(x=>x.Estado=="Pendiente"))
        {
            var n=admin.ListarMesas().Max(x=>x.Numero)+1;admin.CrearMesa(n,2);var m=admin.ListarMesas().Single(x=>x.Numero==n);var producto=admin.ListarMenu().First(x=>x.Precio>0);
            var p=admin.CrearPedido(new SolicitudPedido{MesaId=m.Id,Detalles=[new(producto.Id,1)]});var c=admin.RevisarCuenta(p.Id);
            admin.GestionarMesa(new(Guid.NewGuid(),m.Id,p.Id,c.Version,"Incidencia","Fixture visual ficticia",false));
        }
        var mesa=admin.ListarMesas().First();
        if(tipo=="mesa"){Application.Run(new FormularioMesa(new Mesa(mesa.Id,10001,4,"En limpieza")));return;}
        if(tipo=="usuarios"){Application.Run(new FormularioUsuarios(admin));return;}
        if(tipo=="incidencias"){Application.Run(new FormularioIncidencias(admin));return;}
        var pedido=admin.ListarPedidos().First(x=>x.Estado=="Abierto");
        if(tipo=="correccion")Application.Run(new FormularioCorreccionLinea(admin,admin.RevisarCuenta(pedido.Id)));
        else if(tipo=="resolucion")Application.Run(new FormularioResolverIncidencia(admin,admin.RevisarCuenta(admin.ListarIncidencias().First(x=>x.Estado=="Pendiente").PedidoId)));
        else Application.Run(new FormularioRecuperacion(admin));
    }
}
