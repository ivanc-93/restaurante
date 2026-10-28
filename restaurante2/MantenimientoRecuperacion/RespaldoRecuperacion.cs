using Microsoft.Data.SqlClient;
namespace Sabores.Mantenimiento;
public static class RespaldoRecuperacion
{
    public static string Verificar(string cadena)
    {
        using var c=new SqlConnection(cadena);c.Open();var nombre=c.Database;
        if(new[]{"master","model","msdb","tempdb"}.Contains(nombre,StringComparer.OrdinalIgnoreCase))throw new InvalidOperationException("No admite bases del sistema.");
        using var q=new SqlCommand("SELECT IS_SRVROLEMEMBER('sysadmin');",c){CommandTimeout=120};
        if(Convert.ToInt32(q.ExecuteScalar())!=1)throw new UnauthorizedAccessException("Se requiere autoridad administrativa SQL para respaldo y restauración aislada.");
        q.CommandText="SELECT name,physical_name,CONVERT(int,type) FROM sys.database_files ORDER BY file_id;";var archivos=new List<(string Nombre,string Ruta,int Tipo)>();using(var r=q.ExecuteReader())while(r.Read())archivos.Add((r.GetString(0),r.GetString(1),r.GetInt32(2)));
        if(archivos.Any(a=>a.Tipo is not(0 or 1)))throw new InvalidOperationException("Tipo de archivo no soportado: requiere revisión técnica.");
        var carpeta=Path.GetDirectoryName(archivos[0].Ruta)!;var sufijo=Guid.NewGuid().ToString("N");var copia="Sabores_Validacion_"+sufijo;var bak=Path.Combine(carpeta,"Sabores_Recuperacion_"+sufijo+".bak");
        static string Ident(string v)=>"["+v.Replace("]","]]")+"]";static string Literal(string v)=>"N'"+v.Replace("'","''")+"'";
        q.CommandText=$"BACKUP DATABASE {Ident(nombre)} TO DISK={Literal(bak)} WITH COPY_ONLY,CHECKSUM; RESTORE VERIFYONLY FROM DISK={Literal(bak)} WITH CHECKSUM;";q.ExecuteNonQuery();
        var movimientos=string.Join(",",archivos.Select((a,i)=>$"MOVE {Literal(a.Nombre)} TO {Literal(Path.Combine(carpeta,copia+"_"+i+(a.Tipo==1?".ldf":".mdf")))}"));
        q.CommandText=$"RESTORE DATABASE {Ident(copia)} FROM DISK={Literal(bak)} WITH {movimientos},RECOVERY,CHECKSUM; DBCC CHECKDB({Literal(copia)}) WITH NO_INFOMSGS;";q.ExecuteNonQuery();
        var cb=new SqlConnectionStringBuilder(cadena){InitialCatalog=copia};using var clon=new SqlConnection(cb.ConnectionString);clon.Open();
        using var invalidar=new SqlCommand("IF OBJECT_ID(N'dbo.CodigosRecuperacion',N'U') IS NOT NULL BEGIN UPDATE dbo.CodigosRecuperacion SET Consumido=1; UPDATE dbo.RecuperacionInstalacion SET InstalacionId=NEWID(); END; SELECT COUNT(*) FROM dbo.Usuarios;",clon);
        var usuarios=Convert.ToInt32(invalidar.ExecuteScalar());q.CommandText="SELECT COUNT(*) FROM dbo.Usuarios;";
        if(usuarios!=Convert.ToInt32(q.ExecuteScalar()))throw new InvalidOperationException("Cambió el número de usuarios durante la verificación. Repite en mantenimiento exclusivo.");
        return bak;
    }
}
