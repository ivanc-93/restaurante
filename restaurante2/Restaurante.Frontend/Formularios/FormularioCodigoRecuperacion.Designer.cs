namespace Restaurante.Frontend.Formularios;
partial class FormularioCodigoRecuperacion
{
    private TextBox clave=null!,codigo=null!;private Label mensaje=null!;private Button generar=null!,continuar=null!;private CheckBox guardado=null!;
    private void InitializeComponent()
    {
        var p=DisenoFormulario.Preparar(this,"Código de emergencia del Administrador");
        p.Controls.Add(new Label{Text="Guarda este código fuera del equipo, en un gestor seguro o papel protegido. Permite recuperar tu cuenta si olvidas la contraseña. Quien lo posea podría tomar el control de tu cuenta. Generar otro revoca el anterior.",AutoSize=true,MaximumSize=new Size(650,0)});
        clave=DisenoFormulario.Campo(p,"Confirma tu contraseña vigente",true);
        codigo=DisenoFormulario.Campo(p,"Código nuevo: se muestra únicamente en esta ventana",true);codigo.ReadOnly=true;
        var mostrar=new CheckBox{Text="Mostrar código de emergencia",AutoSize=true};mostrar.CheckedChanged+=(_,_)=>codigo.UseSystemPasswordChar=!mostrar.Checked;p.Controls.Add(mostrar);
        guardado=new CheckBox{Text="He guardado el código en un lugar seguro fuera del equipo",AutoSize=true};p.Controls.Add(guardado);guardado.CheckedChanged+=(_,_)=>continuar.Enabled=guardado.Checked&&codigo.Text.Length>0;
        generar=DisenoFormulario.Boton(p,"Generar código",Generar_Click);continuar=DisenoFormulario.Boton(p,"He guardado el código",Confirmar_Click);continuar.Enabled=false;
        DisenoFormulario.Boton(p,"Cancelar",(_,_)=>Close());mensaje=DisenoFormulario.Mensaje(p);AcceptButton=generar;
    }
}
