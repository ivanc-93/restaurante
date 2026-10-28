using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
namespace Restaurante.Frontend.Servicios;
internal sealed class DiarioRecuperacion(string baseId,string? carpeta=null)
{
    private string Ruta=>Path.Combine(carpeta??Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"SaboresRestaurante"),"recuperacion-"+Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(baseId)))+".bin");
    public (Guid Id,string Recibo)? Leer()
    {
        if(!File.Exists(Ruta))return null;
        var datos=JsonSerializer.Deserialize<string[]>(ProtectedData.Unprotect(File.ReadAllBytes(Ruta),null,DataProtectionScope.CurrentUser));
        if(datos is not {Length:2}||!Guid.TryParse(datos[0],out var id)||datos[1].Length!=64)throw new InvalidOperationException("El comprobante local de recuperación no es válido. Solicita asistencia antes de repetir.");
        return(id,datos[1]);
    }
    public void Guardar(Guid id,string recibo)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Ruta)!);
        var datos=ProtectedData.Protect(JsonSerializer.SerializeToUtf8Bytes(new[]{id.ToString(),recibo}),null,DataProtectionScope.CurrentUser);
        File.WriteAllBytes(Ruta+".tmp",datos);File.Move(Ruta+".tmp",Ruta,true);
    }
    public void Borrar(){if(File.Exists(Ruta))File.Delete(Ruta);}
}
