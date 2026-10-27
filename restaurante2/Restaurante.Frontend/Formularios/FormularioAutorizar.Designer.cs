namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioAutorizar
{
    private TextBox usuario=null!,clave=null!;private Label resumen=null!,mensaje=null!;private Button autorizar=null!,cancelar=null!;private CheckBox mostrarClave=null!;
    private void InitializeComponent()
    {
        var p=DisenoFormulario.Preparar(this,"Autorización puntual del gerente");resumen=new Label{AutoSize=true,MaximumSize=new Size(500,0)};p.Controls.Add(resumen);
        usuario=DisenoFormulario.Campo(p,"Usuario del gerente o administrador");clave=DisenoFormulario.Campo(p,"Contraseña individual",true);
        usuario.MaxLength=50;usuario.TabIndex=0;clave.TabIndex=1;
        mostrarClave=new CheckBox{Text="Mostrar contraseña",AutoSize=true,TabIndex=2};mostrarClave.CheckedChanged+=(_,_)=>clave.UseSystemPasswordChar=!mostrarClave.Checked;p.Controls.Add(mostrarClave);
        p.Controls.Add(new Label{Text="Introduce la contraseña vigente de esa cuenta. No es una contraseña nueva.",AutoSize=true,MaximumSize=new Size(650,0)});
        autorizar=DisenoFormulario.Boton(p,"Autorizar esta operación",Autorizar_Click);autorizar.TabIndex=3;
        cancelar=DisenoFormulario.Boton(p,"Cancelar",Cancelar_Click);cancelar.TabIndex=4;mensaje=DisenoFormulario.Mensaje(p);
        AcceptButton=autorizar;CancelButton=cancelar;
    }
}
