namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioUsuarios
{
    private DataGridView lista=null!;private TextBox nombre=null!,usuario=null!,clave=null!,confirmarClave=null!;private ComboBox rol=null!;private CheckBox activo=null!;private Label mensaje=null!;private Button guardar=null!;
    private void InitializeComponent()
    {
        var p=DisenoFormulario.Preparar(this,"Usuarios y roles");lista=new DataGridView{Width=570,Height=180,ReadOnly=true,AutoGenerateColumns=true,SelectionMode=DataGridViewSelectionMode.FullRowSelect,MultiSelect=false,AllowUserToAddRows=false};p.Controls.Add(lista);
        DisenoFormulario.Boton(p,"Editar seleccionado",Editar_Click);DisenoFormulario.Boton(p,"Nuevo usuario",async(_,_)=>{Limpiar();await Recargar();});
        DisenoFormulario.Boton(p,"Ver auditoría",Auditoria_Click);
        DisenoFormulario.Boton(p,"Mi código de emergencia",(_,_)=>{using var f=new FormularioCodigoRecuperacion(repo);f.ShowDialog(this);});
        nombre=DisenoFormulario.Campo(p,"Nombre");usuario=DisenoFormulario.Campo(p,"Usuario individual");rol=DisenoFormulario.Opciones(p,"Rol","Mesero","Gerente","Administrador");activo=new CheckBox{Text="Usuario activo",Checked=true,AutoSize=true};p.Controls.Add(activo);
        clave=DisenoFormulario.Campo(p,"Contraseña temporal de 12 caracteres o más; vacío conserva la existente",true);confirmarClave=DisenoFormulario.Campo(p,"Confirmar contraseña temporal si se cambia",true);p.Controls.Add(new Label{Text=Restaurante.Backend.Servicios.Contrasenas.Ayuda,AutoSize=true,MaximumSize=new Size(650,0)});guardar=DisenoFormulario.Boton(p,"Guardar usuario",Guardar_Click);mensaje=DisenoFormulario.Mensaje(p);
    }
}
