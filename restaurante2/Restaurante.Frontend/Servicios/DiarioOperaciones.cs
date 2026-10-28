using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Restaurante.Backend.Dominio;
using Restaurante.Backend.Servicios;

namespace Restaurante.Frontend.Servicios;
internal static class DiarioOperaciones
{
    private static string Carpeta(IRepositorioRestaurante repo) => repo.BaseEsPruebas
        ? Path.Combine(Path.GetTempPath(),"SaboresPruebas","Operaciones")
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"SaboresRestaurante","Operaciones");
    private static string Prefijo(IRepositorioRestaurante repo)=>repo.IdentificadorBase+"_"+(repo.UsuarioActual?.Id??throw new UnauthorizedAccessException("Inicie sesión."))+"_";
    private static string Ruta(IRepositorioRestaurante repo,Guid id)=>Path.Combine(Carpeta(repo),Prefijo(repo)+id.ToString("N")+".bin");
    public static bool TienePendiente(IRepositorioRestaurante repo,Guid id)=>File.Exists(Ruta(repo,id));
    public static IReadOnlyList<OperacionPendienteLocal> Listar(IRepositorioRestaurante repo)
    {
        var carpeta=Carpeta(repo);if(!Directory.Exists(carpeta))return [];
        var resultado=new List<OperacionPendienteLocal>();
        foreach(var archivo in Directory.EnumerateFiles(carpeta,Prefijo(repo)+"*.bin"))
        {
            try
            {
            var bytes=ProtectedData.Unprotect(File.ReadAllBytes(archivo),null,DataProtectionScope.CurrentUser);
            var p=JsonSerializer.Deserialize<OperacionPendienteLocal>(Encoding.UTF8.GetString(bytes))??throw new IOException("Registro de recuperación ilegible. Conserve el archivo y solicite revisión.");
            if(p.Id==Guid.Empty||Path.GetFileName(archivo)!=Path.GetFileName(Ruta(repo,p.Id))||p.Base!=repo.IdentificadorBase||p.UsuarioId!=repo.UsuarioActual?.Id)throw new IOException("Registro de recuperación incompatible con la sesión.");
            ValidarDatos(p);
            resultado.Add(p);
            }
            catch(Exception ex)when(ex is IOException or CryptographicException or JsonException or UnauthorizedAccessException)
            {
                var nombre=Path.GetFileNameWithoutExtension(archivo);
                Guid.TryParseExact(nombre.Split('_').Last(),"N",out var id);
                resultado.Add(new(id,repo.IdentificadorBase,repo.UsuarioActual!.Id,"Ilegible",Path.GetFileName(archivo)));
            }
        }
        return resultado;
    }
    private static void ValidarDatos(OperacionPendienteLocal p)
    {
        var id=p.Tipo switch
        {
            "Abrir"=>JsonSerializer.Deserialize<SolicitudPedido>(p.Datos)?.OperacionId,
            "Agregar"=>JsonSerializer.Deserialize<SolicitudIncorporacion>(p.Datos)?.OperacionId,
            "Cobro"=>JsonSerializer.Deserialize<SolicitudCobro>(p.Datos)?.OperacionId,
            "Ajuste"=>JsonSerializer.Deserialize<SolicitudAjuste>(p.Datos)?.OperacionId,
            "Gestionar"=>JsonSerializer.Deserialize<SolicitudGestionMesa>(p.Datos)?.OperacionId,
            "Conciliar"=>JsonSerializer.Deserialize<SolicitudConciliacion>(p.Datos)?.OperacionId,
            "Corregir"=>JsonSerializer.Deserialize<SolicitudCorreccionLinea>(p.Datos)?.OperacionId,
            "Resolver"=>JsonSerializer.Deserialize<SolicitudResolucionIncidencia>(p.Datos)?.OperacionId,
            "Preparacion"=>JsonSerializer.Deserialize<int>(p.Datos)>0?p.Id:Guid.Empty,
            _=>Guid.Empty
        };
        if(id!=p.Id)throw new IOException("El contenido no corresponde al identificador del registro. Conserve el archivo.");
    }
    public static async Task<T> Ejecutar<T>(IRepositorioRestaurante repo,string tipo,Guid id,object datos,Func<T> accion)
    {
        Directory.CreateDirectory(Carpeta(repo));
        // Exclusive OS file handle coordinates this operation between processes in the same profile.
        using var bloqueo=Bloquear(repo,id);
        var p=new OperacionPendienteLocal(id,repo.IdentificadorBase,repo.UsuarioActual?.Id??throw new UnauthorizedAccessException(),tipo,JsonSerializer.Serialize(datos));
        var ruta=Ruta(repo,id);
        if(File.Exists(ruta))
        {
            var anterior=Listar(repo).Single(x=>x.Id==id);
            if(anterior!=p)throw new InvalidOperationException("Existe una operación pendiente con datos distintos. Recupérela desde Inicio.");
        }
        else
        {
            var protegido=ProtectedData.Protect(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(p)),null,DataProtectionScope.CurrentUser);
            var temporal=ruta+"."+Guid.NewGuid().ToString("N")+".tmp";
            File.WriteAllBytes(temporal,protegido);File.Move(temporal,ruta,false);
        }
        try { var resultado=await Task.Run(accion);File.Delete(ruta);return resultado; }
        catch { /* Unknown/rejected requests remain until SQL records a terminal abandonment. */ throw; }
    }
    private static FileStream Bloquear(IRepositorioRestaurante repo,Guid id)
    {
        Directory.CreateDirectory(Carpeta(repo));
        try{return new FileStream(Ruta(repo,id)+".lock",FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None);}
        catch(IOException ex){throw new InvalidOperationException("Otra ventana está procesando esta solicitud. Espera y recupera el mismo identificador.",ex);}
    }
    public static async Task<string> Reanudar(IRepositorioRestaurante repo,OperacionPendienteLocal p)
    {
        switch(p.Tipo)
        {
            case "Corregir":
                var correccion=JsonSerializer.Deserialize<SolicitudCorreccionLinea>(p.Datos)!;
                return MonedaGuatemala.Formatear(await Ejecutar(repo,p.Tipo,p.Id,correccion,()=>repo.CorregirLinea(correccion)));
            case "Preparacion":
                var preparacion=JsonSerializer.Deserialize<int>(p.Datos);
                return await Ejecutar(repo,p.Tipo,p.Id,preparacion,()=>repo.MarcarPreparacion(preparacion,p.Id));
            case "Resolver":
                var resolucion=JsonSerializer.Deserialize<SolicitudResolucionIncidencia>(p.Datos)!;
                return await Ejecutar(repo,p.Tipo,p.Id,resolucion,()=>repo.ResolverIncidencia(resolucion));
            case "Cobro":
                var cobro=JsonSerializer.Deserialize<SolicitudCobro>(p.Datos)!;
                return ResumenCobro(await Ejecutar(repo,p.Tipo,p.Id,cobro,()=>repo.RegistrarCobro(cobro)));
            case "Abrir":
                var abrir=JsonSerializer.Deserialize<SolicitudPedido>(p.Datos)!;
                var pedido=await Ejecutar(repo,p.Tipo,p.Id,abrir,()=>repo.CrearPedido(abrir));
                return $"Pedido #{pedido.Id} registrado. No se duplicó la solicitud.";
            case "Agregar":
                var agregar=JsonSerializer.Deserialize<SolicitudIncorporacion>(p.Datos)!;
                return MonedaGuatemala.Formatear(await Ejecutar(repo,p.Tipo,p.Id,agregar,()=>repo.AgregarProductoAPedido(agregar.PedidoId,agregar.MenuId,agregar.Cantidad,agregar.OperacionId)));
            case "Ajuste":
                var ajuste=JsonSerializer.Deserialize<SolicitudAjuste>(p.Datos)!;
                return MonedaGuatemala.Formatear(await Ejecutar(repo,p.Tipo,p.Id,ajuste,()=>repo.AplicarAjuste(ajuste)));
            case "Gestionar":
                var gestion=JsonSerializer.Deserialize<SolicitudGestionMesa>(p.Datos)!;
                return await Ejecutar(repo,p.Tipo,p.Id,gestion,()=>repo.GestionarMesa(gestion));
            case "Conciliar":
                var conciliacion=JsonSerializer.Deserialize<SolicitudConciliacion>(p.Datos)!;
                return ResumenCobro(await Ejecutar(repo,p.Tipo,p.Id,conciliacion,()=>repo.ConciliarTarjeta(conciliacion.OperacionIncierta,conciliacion.Confirmado,conciliacion.Motivo,conciliacion.OperacionId)));
            default:throw new InvalidOperationException("Tipo de operación pendiente desconocido. No se ejecutó nada.");
        }
    }
    private static string ResumenCobro(ResultadoCobro r)=>$"Estado: {r.Estado}. Total: {MonedaGuatemala.Formatear(r.Total)}. Comprobante: {r.NumeroComprobante??"no emitido"}. Cambio: {MonedaGuatemala.Formatear(r.Cambio)}.";
    public static void DescartarRechazada(IRepositorioRestaurante repo,OperacionPendienteLocal p)
    {
        if(p.Tipo=="Ilegible")throw new InvalidOperationException("Conserve el archivo ilegible y solicite revisión técnica. No se eliminará automáticamente.");
        using var bloqueo=Bloquear(repo,p.Id);
        repo.AbandonarOperacion(p.Id);
        File.Delete(Ruta(repo,p.Id));
    }
}
