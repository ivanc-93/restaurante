using Microsoft.Data.SqlClient;

namespace Restaurante.Backend.Data;
/// <summary>Comprueba capacidades del esquema sin migrar ni escribir datos.</summary>
public static class ValidadorEsquemaSql
{
    public static void Validar(SqlConnection c)
    {
        using var q = new SqlCommand("SELECT CASE WHEN OBJECT_ID(N'dbo.VersionEsquema',N'U') IS NULL THEN 0 ELSE 1 END;", c);
        if ((int)q.ExecuteScalar()! == 0)
            throw new InvalidOperationException("La base necesita la migración 005. Haga un respaldo y ejecute BaseDatos/Migracion_005.sql sobre su base SQL Server. No se actualizará automáticamente.");
        q.CommandText = "SELECT COALESCE(MAX(Version),0) FROM dbo.VersionEsquema;";
        if ((int)q.ExecuteScalar()!is not (5 or 6 or 7 or 8))
            throw new InvalidOperationException("Versión de esquema incompatible. Esta aplicación admite esquemas 5, 6, 7 y 8; no se modificará automáticamente.");
        q.CommandText = "SELECT COUNT(*) FROM dbo.VersionEsquema WHERE Version=5;";
        if ((int)q.ExecuteScalar()! != 1)
            throw new InvalidOperationException("Falta el esquema base 5. Solicita revisión técnica.");
        q.CommandText = "SELECT CASE WHEN OBJECT_ID(N'dbo.AutorizacionesOperacion',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.AjustesPedido',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.AuditoriaSeguridad',N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Usuarios',N'VersionSeguridad') IS NOT NULL AND COL_LENGTH(N'dbo.InstalacionSabores',N'TokenInstalacionHash') IS NOT NULL AND COL_LENGTH(N'dbo.Pedidos',N'ImporteOriginal') IS NOT NULL THEN 1 ELSE 0 END;";
        if ((int)q.ExecuteScalar()! != 1)
            throw new InvalidOperationException("La estructura está incompleta. Solicite revisión de la instalación; no se guardará esta conexión.");
        q.CommandText = "SELECT COALESCE(MAX(Version),0) FROM dbo.VersionEsquema;";
        var version = (int)q.ExecuteScalar()!;
        if (version >= 6)
            ExigirRecuperacion(c, null);
        if (version >= 7)
            ExigirCorrecciones(c, null);
        if (version >= 8 && !RepositorioRestaurante.TieneReservas(c, null))
            throw new InvalidOperationException("La estructura de reservas 008 está incompleta. Solicita revisión sin reinicializar la base.");
    }

    internal static bool TieneRecuperacion(Microsoft.Data.SqlClient.SqlConnection c, Microsoft.Data.SqlClient.SqlTransaction? t)
    {
        using var q = Consulta(c, t, """
          SELECT CASE WHEN NOT EXISTS(
            SELECT 1 FROM (VALUES
              (N'RecuperacionInstalacion',N'Id'),(N'RecuperacionInstalacion',N'InstalacionId'),
              (N'CodigosRecuperacion',N'UsuarioId'),(N'CodigosRecuperacion',N'Verificador'),(N'CodigosRecuperacion',N'VersionSeguridad'),
              (N'CodigosRecuperacion',N'Consumido'),(N'CodigosRecuperacion',N'Confirmado'),(N'CodigosRecuperacion',N'Intentos'),(N'CodigosRecuperacion',N'PausaHasta'),(N'CodigosRecuperacion',N'Fecha'),
              (N'OperacionesRecuperacion',N'Id'),(N'OperacionesRecuperacion',N'UsuarioId'),(N'OperacionesRecuperacion',N'Prueba'),(N'OperacionesRecuperacion',N'Mecanismo'),(N'OperacionesRecuperacion',N'Fecha')
            ) x(Tabla,Columna) WHERE COL_LENGTH(N'dbo.'+Tabla,Columna) IS NULL
          ) THEN 1 ELSE 0 END;
          """);
        if ((int)q.ExecuteScalar()! != 1)
            return false;
        q.CommandText = "SELECT COUNT(*) FROM dbo.VersionEsquema WHERE Version=6;";
        if ((int)q.ExecuteScalar()! != 1)
            return false;
        q.CommandText = "SELECT COUNT(*) FROM dbo.RecuperacionInstalacion WHERE Id=1;";
        return (int)q.ExecuteScalar()! == 1;
    }

    internal static void ExigirRecuperacion(SqlConnection c, SqlTransaction? t)
    {
        if (!TieneRecuperacion(c, t))
            throw new InvalidOperationException("La recuperación no está disponible: requiere esquema 006 completo. Solicita respaldo y restauración verificados antes de actualizar; no se reinicializará la base.");
    }

    internal static bool TieneCorrecciones(SqlConnection c, SqlTransaction? t)
    {
        using var q = Consulta(c, t, "SELECT CASE WHEN OBJECT_ID(N'dbo.CorreccionesLinea',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.ResolucionesIncidencia',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.DetallesPedidoVigentes',N'V') IS NOT NULL THEN 1 ELSE 0 END;");
        return (int)q.ExecuteScalar()! == 1;
    }

    internal static void ExigirCorrecciones(SqlConnection c, SqlTransaction? t)
    {
        if (!TieneCorrecciones(c, t))
            throw new InvalidOperationException("Esta función necesita la actualización 007 respaldada. No se modificará automáticamente la base.");
    }

    private static SqlCommand Consulta(SqlConnection c, SqlTransaction? t, string sql) => new(sql, c, t);
}
