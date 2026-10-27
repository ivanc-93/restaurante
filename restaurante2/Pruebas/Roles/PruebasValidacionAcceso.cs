using System.Reflection;
using Restaurante.Backend.Servicios;
using Restaurante.Frontend.Formularios;
namespace PruebasRoles;
internal static class PruebasValidacionAcceso
{
 public static void Ejecutar(string dir,Action<bool,string> ok)
 {
    Directory.CreateDirectory(dir);ApplicationConfiguration.Initialize();
    ok(Contrasenas.ErrorValidacion(new string('a',12)) is null,"Mínimo 12 sin requisitos inventados");
    ok(Contrasenas.ErrorValidacion(new string('a',128)) is null,"Máximo 128 válido");
    ok(Contrasenas.ErrorValidacion(new string('a',129)) is not null,"Más de 128 rechazado");
    ok(Contrasenas.ErrorValidacion("   ") is not null,"Espacios rechazados");
    using(var f=new FormularioCambiarClave())
    {
        Mostrar(f);Campo<TextBox>(f,"actual").Text="antigua";Campo<TextBox>(f,"nueva").Text="breve";Campo<TextBox>(f,"repetir").Text="breve";Campo<Button>(f,"guardar").PerformClick();
        ok(Campo<Label>(f,"mensaje").Text.Contains("12 caracteres"),"Cambio valida antes del servicio");
        Campo<TextBox>(f,"actual").Clear();Campo<TextBox>(f,"nueva").Clear();Campo<TextBox>(f,"repetir").Clear();Capturar(f,dir,"ClaveCorta");
        Campo<TextBox>(f,"actual").Text="antigua";Campo<TextBox>(f,"nueva").Text="EjemploSoloPruebas2026!";Campo<TextBox>(f,"repetir").Text="distinta";Campo<Button>(f,"guardar").PerformClick();
        ok(Campo<Label>(f,"mensaje").Text.Contains("no coinciden"),"Corrección y reintento sin reiniciar");
        Campo<TextBox>(f,"actual").Clear();Campo<TextBox>(f,"nueva").Clear();Campo<TextBox>(f,"repetir").Clear();Capturar(f,dir,"CambioValidacion");
    }
    using(var f=new FormularioUsuarios())
    {
        Mostrar(f);Campo<TextBox>(f,"nombre").Text="Persona de prueba";Campo<TextBox>(f,"usuario").Text="prueba";Campo<TextBox>(f,"clave").Text="breve";Campo<Button>(f,"guardar").PerformClick();
        ok(Campo<Label>(f,"mensaje").Text.Contains("12 caracteres")&&Campo<TextBox>(f,"nombre").Text=="Persona de prueba","Crear usuario valida y conserva nombre");
    }
    using(var f=new FormularioPrimerAdministrador())
    {
        Mostrar(f);foreach(var n in new[]{"token","nombre","usuario"})Campo<TextBox>(f,n).Text="DatosFicticiosDePrueba";Campo<TextBox>(f,"clave").Text="breve";Campo<TextBox>(f,"repetir").Text="breve";Campo<Button>(f,"guardar").PerformClick();
        ok(Campo<Label>(f,"mensaje").Text.Contains("12 caracteres"),"Administrador inicial valida antes de SQL");
    }
    using(var f=new FormularioAcceso())
    {
        Mostrar(f);Console.WriteLine($"ENTORNO DPI observado {f.DeviceDpi}; tamaños de ventana en píxeles, sin cambiar escalado de Windows.");Campo<Button>(f,"entrar").PerformClick();ok(Campo<Label>(f,"_mensaje").Text.Contains("usuario"),"Usuario vacío no consulta SQL");
        Campo<TextBox>(f,"_usuario").Text="prueba";Campo<Button>(f,"entrar").PerformClick();ok(Campo<Label>(f,"_mensaje").Text.Contains("contraseña actual"),"Contraseña vacía tiene mensaje específico");
        Campo<TextBox>(f,"_contrasena").Text="breve";Campo<CheckBox>(f,"mostrarClave").Checked=true;ok(!Campo<TextBox>(f,"_contrasena").UseSystemPasswordChar,"Mostrar contraseña");Campo<CheckBox>(f,"mostrarClave").Checked=false;ok(Campo<TextBox>(f,"_contrasena").UseSystemPasswordChar,"Ocultar contraseña");Campo<TextBox>(f,"_contrasena").Clear();
        foreach(var t in new[]{new Size(1280,720),new Size(1366,768)})
        {
            f.Size=t;Campo<Label>(f,"_mensaje").Text="No se pudo verificar el acceso. Comprueba la conexión y vuelve a intentarlo. La configuración anterior se conserva.";f.PerformLayout();Application.DoEvents();
            var p=Campo<TableLayoutPanel>(f,"estructura");var controles=p.Controls.Cast<Control>().Where(c=>c.Visible).ToArray();
            ok(!controles.Any(a=>controles.Any(b=>a!=b&&a.Bounds.IntersectsWith(b.Bounds))),"Sin superposición con mensaje largo "+t.Width);
            foreach(var nombre in new[]{"entrar","conexion","_mensaje","mostrarClave"})
            {var c=Campo<Control>(f,nombre);var rect=f.RectangleToClient(c.RectangleToScreen(c.ClientRectangle));ok(f.ClientRectangle.Contains(rect)&&c.Parent!.ClientRectangle.Contains(c.Bounds),"Control visible sin recorte "+nombre+" en "+t.Width);}
            Capturar(f,dir,"Acceso-"+t.Width);
        }
        ok(f.AcceptButton==Campo<Button>(f,"entrar")&&Campo<TextBox>(f,"_usuario").TabIndex==0&&Campo<Button>(f,"conexion").TabIndex==4,"Enter y tabulación configurados");
    }
 }
 static T Campo<T>(Form f,string n)=>(T)f.GetType().GetField(n,BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(f)!;
 static void Mostrar(Form f){f.StartPosition=FormStartPosition.Manual;f.Location=new Point(-20000,-20000);f.ShowInTaskbar=false;f.Show();Application.DoEvents();}
 static void Capturar(Form f,string dir,string n){using var b=new Bitmap(f.Width,f.Height);f.DrawToBitmap(b,new Rectangle(0,0,b.Width,b.Height));b.Save(Path.Combine(dir,n+".png"));}
}
