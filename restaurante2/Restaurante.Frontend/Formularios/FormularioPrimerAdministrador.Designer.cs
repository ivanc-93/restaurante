namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioPrimerAdministrador
{
    private TextBox token=null!,nombre=null!,usuario=null!,clave=null!,repetir=null!;private Label mensaje=null!;private Button guardar=null!;
    private void InitializeComponent()
    {
        var p=DisenoFormulario.Preparar(this,"Crear primer administrador");
        token=DisenoFormulario.Campo(p,"Token de instalación entregado por el responsable",true);nombre=DisenoFormulario.Campo(p,"Nombre");usuario=DisenoFormulario.Campo(p,"Usuario");clave=DisenoFormulario.Campo(p,"Contraseña de al menos 12 caracteres",true);repetir=DisenoFormulario.Campo(p,"Confirmar contraseña",true);
        p.Controls.Add(new Label{Text=Restaurante.Backend.Servicios.Contrasenas.Ayuda,AutoSize=true,MaximumSize=new Size(650,0)});
        guardar=DisenoFormulario.Boton(p,"Completar instalación de la base",Guardar_Click);mensaje=DisenoFormulario.Mensaje(p);
    }
}
