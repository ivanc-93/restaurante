using Microsoft.Data.SqlClient;
namespace Sabores.Mantenimiento;
public static class ActualizacionRecuperacion
{
    public static string Ejecutar(string cadena,string archivoMigracion)
    {
        var script=File.ReadAllText(archivoMigracion);
        var respaldo=RespaldoRecuperacion.Verificar(cadena);
        using var c=new SqlConnection(cadena);c.Open();using var q=new SqlCommand(script,c){CommandTimeout=120};q.ExecuteNonQuery();return respaldo;
    }
}
