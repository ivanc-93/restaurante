namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioAutorizar
{
    private TextBox usuario=null!,clave=null!;private Label resumen=null!,mensaje=null!;private Button autorizar=null!;
    private void InitializeComponent()
    {
        var p=DisenoFormulario.Preparar(this,"Autorización puntual del gerente");resumen=new Label{AutoSize=true,MaximumSize=new Size(500,0)};p.Controls.Add(resumen);
        usuario=DisenoFormulario.Campo(p,"Usuario del gerente o administrador");clave=DisenoFormulario.Campo(p,"Contraseña individual",true);
        autorizar=DisenoFormulario.Boton(p,"Autorizar esta operación",Autorizar_Click);DisenoFormulario.Boton(p,"Cancelar",(_,_)=>Close());mensaje=DisenoFormulario.Mensaje(p);
    }
}
