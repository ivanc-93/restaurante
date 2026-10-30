using Microsoft.Data.SqlClient;
using Restaurante.Backend.Servicios;
using System.Security.Cryptography;
using System.Text;

internal static class PruebasPrimeraInstalacion
{
    internal static int Ejecutar(string baseDatos)
    {
        if(!baseDatos.StartsWith("Restaurante_Pruebas_",StringComparison.Ordinal))throw new InvalidOperationException("Solo bases aisladas.");
        var cadena=ConfiguracionConexion.CrearCadena("localhost",0,baseDatos,"","",true);
        var cfg=new ConfiguracionConexionTemporal(cadena);
        var servicio=new ServicioPreparacionInicial(cfg);
        var instalacion=new ServicioInstalacion(cfg);
        int aprobadas=0;
        void Ok(bool resultado,string nombre){if(!resultado)throw new InvalidOperationException(nombre);aprobadas++;Console.WriteLine("OK "+nombre);}
        void Rechaza(Action accion,string nombre){try{accion();}catch(Exception e)when(e is InvalidOperationException or UnauthorizedAccessException){Ok(true,nombre);return;}throw new InvalidOperationException(nombre);}
        void Sql(string texto){using var c=new SqlConnection(cadena);c.Open();using var q=new SqlCommand(texto,c);q.ExecuteNonQuery();}
        string? Hash(){using var c=new SqlConnection(cadena);c.Open();using var q=new SqlCommand("SELECT TokenInstalacionHash FROM dbo.InstalacionSabores WHERE Id=1;",c);return q.ExecuteScalar() as string;}
        string HashDe(string token)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        const string clavePrueba="Frase de prueba aislada 2026";

        var estado=servicio.ConsultarEstado();
        Ok(estado.Pendiente&&estado.AutorizadoWindows,"Instalación pendiente y Windows autorizado detectados");
        // Exercise absence of provisioning only in this fixture.
        Sql("UPDATE dbo.InstalacionSabores SET TokenInstalacionHash=NULL WHERE Id=1;");
        Rechaza(()=>instalacion.CrearPrimerAdministrador("inventado","Prueba","primero",clavePrueba),"Sin aprovisionamiento no crea usuario");
        using(var preparada=servicio.Preparar(false))
        {
            Ok(Hash()?.Length==64,"Generación segura persiste solamente hash SHA256");
            Ok(preparada.RutaRespaldo.EndsWith(".bak"),"Respaldo SQL verificado antes de aprovisionar");
        }
        var previo=Hash();
        Rechaza(()=>servicio.Preparar(false),"Token perdido exige confirmación antes de reemplazar");
        Ok(Hash()==previo,"Negarse al reemplazo conserva token anterior");
        var antiguo=Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        Sql("UPDATE dbo.InstalacionSabores SET TokenInstalacionHash='"+HashDe(antiguo)+"' WHERE Id=1;");
        using var vigente=servicio.Preparar(true);
        Ok(Hash()!=HashDe(antiguo),"Reemplazo autorizado cambia el hash");
        Rechaza(()=>instalacion.CrearPrimerAdministrador(antiguo,"Prueba","primero",clavePrueba),"Token anterior invalidado");
        Rechaza(()=>instalacion.CrearPrimerAdministrador("incorrecto","Prueba","primero",clavePrueba),"Token incorrecto rechazado");
        Sql("CREATE USER [SaboresSinPermiso] WITHOUT LOGIN;");
        var restringida=new ConexionSinPermisos(cadena);
        Rechaza(()=>new ServicioPreparacionInicial(restringida).Preparar(true),"Windows sin permiso sysadmin no aprovisiona");
        var carreras=Enumerable.Range(0,2).Select(i=>Task.Run(()=>
        {
            try{vigente.CrearAdministrador("Administrador aislado","primero"+i,clavePrueba);return true;}
            catch(Exception e)when(e is InvalidOperationException or UnauthorizedAccessException){return false;}
        })).ToArray();
        Task.WaitAll(carreras);
        Ok(carreras.Count(t=>t.Result)==1,"Dos creaciones simultáneas producen un solo administrador");
        Ok(!servicio.ConsultarEstado().Pendiente&&Hash() is null,"Instalación completada consume token");
        Rechaza(()=>servicio.Preparar(true),"Base con usuarios no se reinicializa");
        Rechaza(()=>instalacion.CrearPrimerAdministrador(antiguo,"Intruso","otro",clavePrueba),"Instalación finalizada rechaza otro primer administrador");
        Console.WriteLine($"TOTAL {aprobadas} comprobaciones de primera instalación correctas");
        return 0;
    }

    private sealed class ConexionSinPermisos(string cadena):IConfiguracionConexion
    {
        public bool EstaConfigurada=>true;
        public SqlConnection CrearConexion()
        {
            // EXECUTE AS must not leave an impersonated session in the shared pool.
            var conexion=new SqlConnection(new SqlConnectionStringBuilder(cadena){Pooling=false}.ConnectionString);
            conexion.StateChange+=(_,e)=>{if(e.CurrentState==System.Data.ConnectionState.Open){using var q=new SqlCommand("EXECUTE AS USER=N'SaboresSinPermiso';",conexion);q.ExecuteNonQuery();}};
            return conexion;
        }
        public void ProbarYGuardar(string servidor,int puerto,string baseDatos,string usuario,string contrasena,bool confiarCertificado)=>throw new InvalidOperationException();
    }
}
