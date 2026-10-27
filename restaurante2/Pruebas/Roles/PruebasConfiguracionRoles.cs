using Microsoft.Data.SqlClient;
using Restaurante.Backend.Servicios;
namespace PruebasRoles;
internal static class PruebasConfiguracionRoles
{
 public static void Ejecutar(string cadena,Action<bool,string> ok)
 {
    var b=new SqlConnectionStringBuilder(cadena);if(!b.InitialCatalog.StartsWith("Restaurante_Pruebas_"))throw new Exception("Solo pruebas aisladas.");
    var carpeta=Path.Combine(Path.GetTempPath(),"SaboresRoles_"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(carpeta);
    var ruta=Path.Combine(carpeta,"conexion.bin");var cfg=new ConfiguracionConexion(ruta);
    ok(!cfg.EstaConfigurada,"Equipo sin configuración no tiene acceso inicial implícito");
    try{cfg.ProbarYGuardar("",1433,b.InitialCatalog,"","",true);throw new Exception("Aceptó servidor vacío");}catch(ArgumentException){ok(!File.Exists(ruta),"Servidor vacío se rechaza antes de guardar");}
    cfg.ProbarYGuardar(b.DataSource,1433,b.InitialCatalog,"","",true);
    var original=File.ReadAllBytes(ruta);ok(new ConfiguracionConexion(ruta).EstaConfigurada,"Configuración protegida reabre en el mismo perfil");
    try{cfg.ProbarYGuardar(b.DataSource,1433,"Restaurante_Pruebas_Inexistente_"+Guid.NewGuid().ToString("N"),"","",true);throw new Exception("Aceptó base inexistente");}catch(SqlException){ok(original.SequenceEqual(File.ReadAllBytes(ruta)),"Fallo SQL conserva configuración válida");}
    using var c=new SqlConnection(cadena);c.Open();using var q=new SqlCommand("DELETE dbo.VersionEsquema WHERE Version=5;",c);q.ExecuteNonQuery();
    try
    {
        try{cfg.ProbarYGuardar(b.DataSource,1433,b.InitialCatalog,"","",true);throw new Exception("Aceptó esquema antiguo");}
        catch(InvalidOperationException){ok(original.SequenceEqual(File.ReadAllBytes(ruta)),"Esquema incompatible no reemplaza configuración válida");}
    }
    finally{q.CommandText="INSERT dbo.VersionEsquema(Version)VALUES(5);";q.ExecuteNonQuery();}
 }
}
