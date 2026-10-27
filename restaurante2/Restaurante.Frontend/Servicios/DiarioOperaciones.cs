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
            var bytes=ProtectedData.Unprotect(File.ReadAllBytes(archivo),null,DataProtectionScope.CurrentUser);
            var p=JsonSerializer.Deserialize<OperacionPendienteLocal>(Encoding.UTF8.GetString(bytes))??throw new IOException("Registro de recuperación ilegible. Conserve el archivo y solicite revisión.");
            if(p.Base!=repo.IdentificadorBase||p.UsuarioId!=repo.UsuarioActual?.Id)throw new IOException("Registro de recuperación incompatible con la sesión.");
            resultado.Add(p);
        }
        return resultado;
    }
    public static async Task<T> Ejecutar<T>(IRepositorioRestaurante repo,string tipo,Guid id,object datos,Func<T> accion)
    {
        Directory.CreateDirectory(Carpeta(repo));
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
        catch(Exception ex)when(ex is ArgumentException or InvalidOperationException or UnauthorizedAccessException)
        {
            // Delete only a conclusively rejected operation. Unknown SQL outcomes remain recoverable.
            try{if(!await Task.Run(()=>repo.OperacionAplicada(id)))File.Delete(ruta);}catch{ /* Keep the encrypted request. */ }
            throw;
        }
    }
    public static async Task<string> Reanudar(IRepositorioRestaurante repo,OperacionPendienteLocal p)
    {
        switch(p.Tipo)
        {
            case "Cobro":
                var cobro=JsonSerializer.Deserialize<SolicitudCobro>(p.Datos)!;
                return JsonSerializer.Serialize(await Ejecutar(repo,p.Tipo,p.Id,cobro,()=>repo.RegistrarCobro(cobro)));
            case "Abrir":
                var abrir=JsonSerializer.Deserialize<SolicitudPedido>(p.Datos)!;
                return JsonSerializer.Serialize(await Ejecutar(repo,p.Tipo,p.Id,abrir,()=>repo.CrearPedido(abrir)));
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
                return JsonSerializer.Serialize(await Ejecutar(repo,p.Tipo,p.Id,conciliacion,()=>repo.ConciliarTarjeta(conciliacion.OperacionIncierta,conciliacion.Confirmado,conciliacion.Motivo,conciliacion.OperacionId)));
            default:throw new InvalidOperationException("Tipo de operación pendiente desconocido. No se ejecutó nada.");
        }
    }
    public static void DescartarRechazada(IRepositorioRestaurante repo,OperacionPendienteLocal p)
    {
        if(repo.OperacionAplicada(p.Id))throw new InvalidOperationException("La operación existe en SQL. Recupere su resultado, no la descarte.");
        File.Delete(Ruta(repo,p.Id));
    }
}
