using Microsoft.Data.SqlClient;
using Restaurante.Backend.Servicios;
using Restaurante.Backend.Dominio;
namespace PruebasRoles;
internal static class PruebasConcurrenciaRoles
{
 public static void Ejecutar(Action<bool,string> ok)
 {
    var cadena=Environment.GetEnvironmentVariable("SABORES_INSTALACION_PRUEBAS")??throw new Exception("Defina SABORES_INSTALACION_PRUEBAS con una base nueva aislada.");
    if(!new SqlConnectionStringBuilder(cadena).InitialCatalog.StartsWith("Restaurante_Pruebas_"))throw new Exception("Destino de instalación no aislado.");
    var token=Environment.GetEnvironmentVariable("SABORES_TOKEN_PRUEBAS")??throw new Exception("Falta token de prueba.");
    var cfg=new ConfiguracionConexionTemporal(cadena);var servicio=new ServicioInstalacion(cfg);
    ok(servicio.NecesitaAdministrador(),"Instalación nueva detectada sin usuarios");
    try{servicio.CrearPrimerAdministrador("incorrecto","X","intruso","PruebaPersonal_2026!");throw new Exception("Aceptó token incorrecto");}catch(UnauthorizedAccessException){ok(true,"Token incorrecto rechazado");}
    ok(servicio.NecesitaAdministrador(),"Intento inicial rechazado conserva instalación pendiente");
    using var barrera=new Barrier(2);
    Task<bool> Crear(string u)=>Task.Run(()=>{barrera.SignalAndWait(TimeSpan.FromSeconds(20));try{new ServicioInstalacion(cfg).CrearPrimerAdministrador(token,u,u,"PruebaPersonal_2026!");return true;}catch(InvalidOperationException){return false;}});
    var creaciones=new[]{Crear("admin_a"),Crear("admin_b")};Task.WaitAll(creaciones);
    ok(creaciones.Count(t=>t.Result)==1,"Dos equipos crean exactamente un primer administrador");
    ok(!servicio.NecesitaAdministrador(),"Instalación completada no se reinicializa");
    var ganador=creaciones[0].Result?"admin_a":"admin_b";
    var primero=new RepositorioRestaurante(cfg);ok(primero.IniciarSesion(ganador,"PruebaPersonal_2026!")?.Rol=="Administrador","Primer administrador utiliza contraseña individual");
    primero.GuardarUsuario(null,"Segundo administrador","segundo_admin","Administrador",true,"PruebaTemporal_2026!");
    var segundo=new RepositorioRestaurante(cfg);segundo.IniciarSesion("segundo_admin","PruebaTemporal_2026!");segundo.CambiarMiContrasena("PruebaTemporal_2026!","PruebaPersonal_2026!");segundo.IniciarSesion("segundo_admin","PruebaPersonal_2026!");
    var usuarios=primero.ListarUsuarios();var a=usuarios.Single(u=>u.Usuario==ganador);var b=usuarios.Single(u=>u.Usuario=="segundo_admin");
    using var limite=new Barrier(2);
    Task<bool> Degradar(RepositorioRestaurante r,UsuarioGestion u)=>Task.Run(()=>{limite.SignalAndWait(TimeSpan.FromSeconds(20));try{r.GuardarUsuario(u.Id,u.Nombre,u.Usuario,"Mesero",true,null);return true;}catch(InvalidOperationException){return false;}catch(UnauthorizedAccessException){return false;}});
    var cambios=new[]{Degradar(primero,a),Degradar(segundo,b)};Task.WaitAll(cambios);
    ok(cambios.Count(t=>t.Result)==1,"Degradaciones simultáneas conservan un administrador");
    using var c=cfg.CrearConexion();c.Open();using var q=new SqlCommand("SELECT COUNT(*) FROM dbo.Usuarios WHERE Rol=N'Administrador' AND Activo=1;",c);
    ok((int)q.ExecuteScalar()! ==1,"Último administrador permanece en SQL");
 }
}
