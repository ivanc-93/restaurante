using Microsoft.Data.SqlClient;
namespace Sabores.Mantenimiento;
internal static class Program
{
    private static string Secreto()
    {
        var s=new System.Text.StringBuilder();ConsoleKeyInfo k;
        while((k=Console.ReadKey(true)).Key!=ConsoleKey.Enter){if(k.Key==ConsoleKey.Backspace){if(s.Length>0)s.Length--;}else if(!char.IsControl(k.KeyChar)&&s.Length<129)s.Append(k.KeyChar);}
        Console.WriteLine();return s.ToString();
    }
    private static int Main(string[] args)
    {
        if(args.Length==3&&args[0]=="--actualizar"&&!Console.IsInputRedirected)
        {
            try
            {
                Console.WriteLine($"Activar recuperación en {args[1]} / {args[2]}. Detén la operación diaria. Se hará respaldo COPY_ONLY, restauración aislada y CHECKDB antes de migrar.");
                Console.Write($"Confirma escribiendo ACTUALIZAR {args[2]}: ");if(Console.ReadLine()!=$"ACTUALIZAR {args[2]}")return 1;
                var cadena=new SqlConnectionStringBuilder{DataSource=args[1],InitialCatalog=args[2],IntegratedSecurity=true,Encrypt=true,TrustServerCertificate=true}.ConnectionString;
                var bak=ActualizacionRecuperacion.Ejecutar(cadena,Path.Combine(AppContext.BaseDirectory,"Migracion_006.sql"));Console.WriteLine($"Recuperación activada. Respaldo restaurado y verificado: {bak}");return 0;
            }
            catch(Exception ex){Console.Error.WriteLine(ex is SqlException?"No se confirmó la actualización con SQL. Conserve el respaldo y solicite revisión antes de continuar.":ex.Message);return 1;}
        }
        if(args.Length!=2||Console.IsInputRedirected){Console.WriteLine("Uso interactivo: MantenimientoRecuperacion <servidor> <base>, o --actualizar <servidor> <base>. No acepta contraseñas por argumentos ni entrada redirigida.");return 1;}
        try
        {
            Console.WriteLine($"Destino: {args[0]} / {args[1]}. Este procedimiento respalda y restaura una copia aislada antes de cambiar UNA cuenta. Debe contar con autorización del responsable del restaurante y detener la operación diaria.");
            Console.Write("Usuario Administrador objetivo: ");var u=Console.ReadLine()??"";
            Console.Write("Responsable autorizado: ");var responsable=Console.ReadLine()??"";Console.Write("Motivo: ");var motivo=Console.ReadLine()??"";
            Console.Write($"Confirma escribiendo RESTABLECER {args[1]} {u}: ");if(Console.ReadLine()!=$"RESTABLECER {args[1]} {u}")return 1;
            Console.Write("Contraseña temporal (oculta): ");var clave=Secreto();Console.Write("Confirmación (oculta): ");if(clave!=Secreto())throw new ArgumentException("Las contraseñas no coinciden.");
            var cb=new SqlConnectionStringBuilder{DataSource=args[0],InitialCatalog=args[1],IntegratedSecurity=true,Encrypt=true,TrustServerCertificate=true};var op=Guid.NewGuid();Console.WriteLine($"Identificador de intervención: {op}. Consérvalo si se pierde la respuesta.");
            var bak=RestablecimientoAdministrativo.Ejecutar(cb.ConnectionString,u,clave,responsable,motivo,op);Console.WriteLine($"Restablecimiento completado; cambio obligatorio al ingresar. Respaldo verificado: {bak}. Copia aislada conservada para diagnóstico.");return 0;
        }
        catch(Exception ex){Console.Error.WriteLine(ex is SqlException?"No se confirmó la intervención con SQL. No la repitas sin comprobar su identificador original.":ex.Message);return 1;}
    }
}
