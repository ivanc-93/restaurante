using System.Reflection;
using Microsoft.Data.SqlClient;
using Restaurante.Backend.Servicios;
using Restaurante.Frontend.Formularios;
using Restaurante.Frontend.Servicios;

internal static class PruebasInterfazInicial
{
    internal static int Ejecutar(string baseDatos)
    {
        if(!baseDatos.StartsWith("Restaurante_Pruebas_",StringComparison.Ordinal))throw new InvalidOperationException("Solo bases aisladas.");
        var cfg=new ConfiguracionConexionTemporal(ConfiguracionConexion.CrearCadena("localhost",0,baseDatos,"","",true));
        var confirmacion=new ConfirmacionPrueba();
        using var f=new FormularioPrimerAdministrador(cfg,confirmacion);
        f.Show();Esperar(f);
        int aprobadas=0;
        void Ok(bool valor,string nombre){if(!valor)throw new InvalidOperationException(nombre);Console.WriteLine("OK "+nombre);aprobadas++;}
        string? Hash(){using var c=cfg.CrearConexion();c.Open();using var q=new SqlCommand("SELECT TokenInstalacionHash FROM dbo.InstalacionSabores WHERE Id=1;",c);return q.ExecuteScalar() as string;}
        Ok(Campo<Button>(f,"btnPrepararWindows").Enabled,"Windows autorizado habilita preparación");
        var anterior=Hash();
        Campo<Button>(f,"btnPrepararWindows").PerformClick();Esperar(f);
        Ok(confirmacion.Veces==1&&Hash()==anterior,"Confirmación cancelada conserva token anterior");
        confirmacion.Respuesta=DialogResult.Yes;
        Campo<Button>(f,"btnPrepararWindows").PerformClick();Esperar(f);
        Ok(confirmacion.Veces==2&&Hash()!=anterior,"Confirmación aceptada prepara instalación desde interfaz");
        Ok(!Campo<TextBox>(f,"ctToken").Visible&&Campo<TextBox>(f,"ctToken").Text.Length==0,"El usuario no introduce ni visualiza el token generado");
        Ok(Campo<Label>(f,"lblEstadoInicial").Text.Contains("respaldo verificado"),"La interfaz comunica preparación verificada");
        using(var imagen=new Bitmap(f.Width,f.Height))
        {
            f.DrawToBitmap(imagen,new Rectangle(0,0,f.Width,f.Height));
            imagen.Save(Path.Combine(AppContext.BaseDirectory,"primer-inicio-preparado.png"));
        }
        Campo<TextBox>(f,"ctNombre").Text="Cuenta ficticia de pruebas";
        Campo<TextBox>(f,"ctUsuario").Text="administrador_prueba";
        Campo<TextBox>(f,"ctContrasena").Text="corta";Campo<TextBox>(f,"ctRepetir").Text="corta";
        Campo<Button>(f,"btnGuardar").PerformClick();Esperar(f);
        Ok(Campo<Label>(f,"lblMensaje").Text.Contains("12 caracteres")&&Campo<TextBox>(f,"ctNombre").Text=="Cuenta ficticia de pruebas","Validación conserva datos no sensibles");
        Campo<TextBox>(f,"ctContrasena").Text="Frase ficticia aislada 2026";Campo<TextBox>(f,"ctRepetir").Text="Frase ficticia aislada 2026";
        Campo<Button>(f,"btnGuardar").PerformClick();Esperar(f);
        Ok(f.DialogResult==DialogResult.OK,"El mismo botón crea administrador sin entrada manual de token");
        Ok(!new ServicioInstalacion(cfg).NecesitaAdministrador()&&Hash() is null,"Cuenta creada y token consumido en SQL");
        Console.WriteLine($"TOTAL {aprobadas} comprobaciones de interfaz inicial correctas");
        return 0;
    }
    private static T Campo<T>(object f,string nombre)=>(T)f.GetType().GetField(nombre,BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(f)!;
    private static void Esperar(Form f)
    {
        Application.DoEvents();var limite=DateTime.UtcNow.AddSeconds(50);
        while(Campo<bool>(f,"ocupado")&&DateTime.UtcNow<limite){Application.DoEvents();Thread.Sleep(10);}
        if(Campo<bool>(f,"ocupado"))throw new TimeoutException("La interfaz no terminó la operación.");
    }
    private sealed class ConfirmacionPrueba:IDialogosUsuario
    {
        public DialogResult Respuesta=DialogResult.No;public int Veces;
        public DialogResult Mostrar(IWin32Window propietario,string mensaje,string titulo,MessageBoxButtons botones=MessageBoxButtons.OK,MessageBoxIcon icono=MessageBoxIcon.None)
        {Veces++;if(!mensaje.Contains("invalidará"))throw new InvalidOperationException("Falta aviso de invalidación");return Respuesta;}
    }
}
