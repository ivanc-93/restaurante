namespace Sabores.Mantenimiento;
// Mantiene la entrada de la herramienta técnica; la garantía de respaldo es compartida.
public static class RespaldoRecuperacion
{
    public static string Verificar(string cadena) => RespaldoVerificadoSql.Verificar(cadena);
}
