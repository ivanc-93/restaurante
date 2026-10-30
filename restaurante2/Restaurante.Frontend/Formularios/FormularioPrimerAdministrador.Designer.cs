namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioPrimerAdministrador
{
    private TextBox token=null!,nombre=null!,usuario=null!,clave=null!,repetir=null!;private Label mensaje=null!,estadoInicial=null!,etiquetaToken=null!;private Button guardar=null!,prepararWindows=null!;
    private void InitializeComponent()
    {
        var p=DisenoFormulario.Preparar(this,"Crear primer administrador");
        estadoInicial=new Label{Text="Si es tu primera vez, prepara la instalación con tu cuenta Windows autorizada. Después elige tu usuario y contraseña de Sabores. No necesitas buscar un token.",AutoSize=true,MaximumSize=new Size(650,0)};p.Controls.Add(estadoInicial);
        prepararWindows=DisenoFormulario.Boton(p,"Preparar instalación con Windows",PrepararWindows_Click);prepararWindows.Enabled=false;
        etiquetaToken=new Label{Text="Token entregado por un responsable (opcional si preparas con Windows)",AutoSize=true,MaximumSize=new Size(650,0)};p.Controls.Add(etiquetaToken);
        token=new TextBox{UseSystemPasswordChar=true,MaxLength=128,Width=490};p.Controls.Add(token);
        nombre=DisenoFormulario.Campo(p,"Nombre");usuario=DisenoFormulario.Campo(p,"Usuario");clave=DisenoFormulario.Campo(p,"Contraseña de al menos 12 caracteres",true);repetir=DisenoFormulario.Campo(p,"Confirmar contraseña",true);
        p.Controls.Add(new Label{Text=Restaurante.Backend.Servicios.Contrasenas.Ayuda,AutoSize=true,MaximumSize=new Size(650,0)});
        guardar=DisenoFormulario.Boton(p,"Completar instalación de la base",Guardar_Click);mensaje=DisenoFormulario.Mensaje(p);
    }
}
