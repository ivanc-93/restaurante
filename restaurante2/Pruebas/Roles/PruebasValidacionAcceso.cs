using Restaurante.Backend.Aplicacion;
using Restaurante.Backend.Aplicacion.Contratos;
using Restaurante.Backend.Configuration;
using Restaurante.Backend.Data;
using Restaurante.Backend.Data.Repositorios;
using Restaurante.Backend.Dominio.Reglas;
using Restaurante.Backend.Seguridad;
using Restaurante.Frontend.Presentacion;
using Restaurante.Frontend.Controles;
using Restaurante.Frontend.Infraestructura.Recuperacion;
using System.Reflection;

using Restaurante.Frontend.Formularios;
namespace PruebasRoles;
internal static class PruebasValidacionAcceso
{
 public static void Ejecutar(string dir,Action<bool,string> ok)
 {
    Directory.CreateDirectory(dir);global::ApplicationConfiguration.Initialize();
    ok(Contrasenas.ErrorValidacion(new string('a',12)) is null,"Mínimo 12 sin requisitos inventados");
    ok(Contrasenas.ErrorValidacion(new string('a',128)) is null,"Máximo 128 válido");
    ok(Contrasenas.ErrorValidacion(new string('a',129)) is not null,"Más de 128 rechazado");
    ok(Contrasenas.ErrorValidacion("   ") is not null,"Espacios rechazados");
    using(var f=new FormularioCambiarClave())
    {
        Mostrar(f);Campo<TextBox>(f,"ctActual").Text="antigua";Campo<TextBox>(f,"ctNueva").Text="breve";Campo<TextBox>(f,"ctRepetir").Text="breve";Campo<Button>(f,"btnGuardar").PerformClick();
        ok(Campo<Label>(f,"lblMensaje").Text.Contains("12 caracteres"),"Cambio valida antes del servicio");
        Campo<TextBox>(f,"ctActual").Clear();Campo<TextBox>(f,"ctNueva").Clear();Campo<TextBox>(f,"ctRepetir").Clear();Capturar(f,dir,"ClaveCorta");
        Campo<TextBox>(f,"ctActual").Text="antigua";Campo<TextBox>(f,"ctNueva").Text="EjemploSoloPruebas2026!";Campo<TextBox>(f,"ctRepetir").Text="distinta";Campo<Button>(f,"btnGuardar").PerformClick();
        ok(Campo<Label>(f,"lblMensaje").Text.Contains("no coinciden"),"Corrección y reintento sin reiniciar");
        Campo<TextBox>(f,"ctActual").Clear();Campo<TextBox>(f,"ctNueva").Clear();Campo<TextBox>(f,"ctRepetir").Clear();Capturar(f,dir,"CambioValidacion");
    }
    using(var f=new FormularioUsuarios())
    {
        Mostrar(f);Campo<TextBox>(f,"ctNombre").Text="Persona de prueba";Campo<TextBox>(f,"ctUsuario").Text="prueba";Campo<TextBox>(f,"ctContrasena").Text="breve";Campo<Button>(f,"btnGuardar").PerformClick();
        ok(Campo<Label>(f,"lblMensaje").Text.Contains("12 caracteres")&&Campo<TextBox>(f,"ctNombre").Text=="Persona de prueba","Crear usuario valida y conserva nombre");
    }
    using(var f=new FormularioPrimerAdministrador())
    {
        Mostrar(f);foreach(var n in new[]{"ctToken","ctNombre","ctUsuario"})Campo<TextBox>(f,n).Text="DatosFicticiosDePrueba";Campo<TextBox>(f,"ctContrasena").Text="breve";Campo<TextBox>(f,"ctRepetir").Text="breve";Campo<Button>(f,"btnGuardar").PerformClick();
        ok(Campo<Label>(f,"lblMensaje").Text.Contains("12 caracteres"),"Administrador inicial valida antes de SQL");
    }
    using(var f=new FormularioAcceso())
    {
        Mostrar(f);Console.WriteLine($"ENTORNO DPI observado {f.DeviceDpi}; tamaños de ventana en píxeles, sin cambiar escalado de Windows.");Campo<Button>(f,"btnEntrarSistema").PerformClick();ok(Campo<Label>(f,"lblMensaje").Text.Contains("usuario"),"Usuario vacío no consulta SQL");
        Campo<TextBox>(f,"ctUsuario").Text="prueba";Campo<Button>(f,"btnEntrarSistema").PerformClick();ok(Campo<Label>(f,"lblMensaje").Text.Contains("contraseña actual"),"Contraseña vacía tiene mensaje específico");
        Campo<TextBox>(f,"ctContrasena").Text="breve";Campo<CheckBox>(f,"chkMostrarContrasena").Checked=true;ok(!Campo<TextBox>(f,"ctContrasena").UseSystemPasswordChar,"Mostrar contraseña");Campo<CheckBox>(f,"chkMostrarContrasena").Checked=false;ok(Campo<TextBox>(f,"ctContrasena").UseSystemPasswordChar,"Ocultar contraseña");Campo<TextBox>(f,"ctContrasena").Clear();
        foreach(var t in new[]{new Size(1280,720),new Size(1366,768)})
        {
            f.Size=t;Campo<Label>(f,"lblMensaje").Text="No se pudo verificar el acceso. Comprueba la conexión y vuelve a intentarlo. La configuración anterior se conserva.";f.PerformLayout();Application.DoEvents();
            var p=Campo<TableLayoutPanel>(f,"tlpEstructura");var controles=p.Controls.Cast<Control>().Where(c=>c.Visible).ToArray();
            ok(!controles.Any(a=>controles.Any(b=>a!=b&&a.Bounds.IntersectsWith(b.Bounds))),"Sin superposición con mensaje largo "+t.Width);
            foreach(var nombre in new[]{"btnEntrarSistema","btnConfigurarConexion","lblMensaje","chkMostrarContrasena"})
            {var c=Campo<Control>(f,nombre);var rect=f.RectangleToClient(c.RectangleToScreen(c.ClientRectangle));ok(f.ClientRectangle.Contains(rect)&&c.Parent!.ClientRectangle.Contains(c.Bounds),"Control visible sin recorte "+nombre+" en "+t.Width);}
            Capturar(f,dir,"Acceso-"+t.Width);
        }
        ok(f.AcceptButton==Campo<Button>(f,"btnEntrarSistema")&&Campo<TextBox>(f,"ctUsuario").TabIndex==0&&Campo<Button>(f,"btnConfigurarConexion").TabIndex==5,"Enter y tabulación configurados");
    }
 }
 static T Campo<T>(Form f,string n)=>(T)f.GetType().GetField(n,BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(f)!;
 static void Mostrar(Form f){f.StartPosition=FormStartPosition.Manual;f.Location=new Point(-20000,-20000);f.ShowInTaskbar=false;f.Show();Application.DoEvents();}
 static void Capturar(Form f,string dir,string n){using var b=new Bitmap(f.Width,f.Height);f.DrawToBitmap(b,new Rectangle(0,0,b.Width,b.Height));b.Save(Path.Combine(dir,n+".png"));}
}
