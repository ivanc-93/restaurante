namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioCambiarClave
{
    private TextBox actual=null!,nueva=null!,repetir=null!;private Label mensaje=null!;private Button guardar=null!;
    private void InitializeComponent()
    {
        var p=DisenoFormulario.Preparar(this,"Cambiar mi contraseña");
        actual=DisenoFormulario.Campo(p,"Contraseña actual",true);nueva=DisenoFormulario.Campo(p,"Nueva contraseña de 12 a 128 caracteres",true);repetir=DisenoFormulario.Campo(p,"Repetir nueva contraseña",true);
        p.Controls.Add(new Label{Text=Restaurante.Backend.Servicios.Contrasenas.Ayuda,AutoSize=true,MaximumSize=new Size(650,0)});
        guardar=DisenoFormulario.Boton(p,"Cambiar y volver al acceso",Guardar_Click);mensaje=DisenoFormulario.Mensaje(p);
    }
}
