namespace Restaurante.Frontend.Formularios;
partial class FormularioRecuperarClave
{
    private ComboBox metodo=null!;private TextBox usuario=null!,codigo=null!,administrador=null!,claveAdministrador=null!,nueva=null!,confirmacion=null!;
    private Label mensaje=null!;private Button recuperar=null!,consultar=null!,cancelar=null!;private CheckBox mostrar=null!;
    private Control[] camposAdministrador=[],camposCodigo=[],camposNueva=[];
    private void InitializeComponent()
    {
        var p=DisenoFormulario.Preparar(this,"Recuperar acceso a Sabores");
        p.Controls.Add(new Label{Text="Establece una contraseña nueva. La contraseña anterior no puede mostrarse. No se abrirá la operación diaria.",AutoSize=true,MaximumSize=new Size(650,0)});
        metodo=DisenoFormulario.Opciones(p,"¿Cómo verificarás tu identidad?","Con ayuda de otro Administrador","Con mi código de emergencia","No tengo administrador ni código");
        usuario=DisenoFormulario.Campo(p,"Usuario que necesita recuperar acceso");usuario.MaxLength=50;
        var comienzo=p.Controls.Count;
        administrador=DisenoFormulario.Campo(p,"Usuario del otro Administrador");administrador.MaxLength=50;
        claveAdministrador=DisenoFormulario.Campo(p,"Contraseña vigente del otro Administrador",true);
        camposAdministrador=p.Controls.Cast<Control>().Skip(comienzo).ToArray();comienzo=p.Controls.Count;
        codigo=DisenoFormulario.Campo(p,"Código de emergencia guardado previamente",true);codigo.MaxLength=100;
        camposCodigo=p.Controls.Cast<Control>().Skip(comienzo).ToArray();comienzo=p.Controls.Count;
        nueva=DisenoFormulario.Campo(p,"Contraseña nueva (12 a 128 caracteres)",true);confirmacion=DisenoFormulario.Campo(p,"Confirmar contraseña nueva",true);
        p.Controls.Add(new Label{Text=Restaurante.Backend.Servicios.Contrasenas.Ayuda,AutoSize=true,MaximumSize=new Size(650,0)});
        mostrar=new CheckBox{Text="Mostrar contraseñas y código",AutoSize=true};mostrar.CheckedChanged+=(_,_)=>{foreach(var campo in new[]{claveAdministrador,codigo,nueva,confirmacion})campo.UseSystemPasswordChar=!mostrar.Checked;};p.Controls.Add(mostrar);
        camposNueva=p.Controls.Cast<Control>().Skip(comienzo).ToArray();
        recuperar=DisenoFormulario.Boton(p,"Restablecer contraseña",Recuperar_Click);consultar=DisenoFormulario.Boton(p,"Comprobar resultado",Consultar_Click);consultar.Enabled=false;
        cancelar=DisenoFormulario.Boton(p,"Volver al inicio",(_,_)=>Close());mensaje=DisenoFormulario.Mensaje(p);
        metodo.SelectedIndexChanged+=(_,_)=>ActualizarMetodo();AcceptButton=recuperar;
        var tab=0;foreach(Control control in p.Controls)if(control.TabStop)control.TabIndex=tab++;
        ActualizarMetodo();
    }
    private void ActualizarMetodo()
    {
        var ayuda=metodo.SelectedIndex==0;var emergencia=metodo.SelectedIndex==1;
        foreach(var c in camposAdministrador)c.Visible=ayuda;
        foreach(var c in camposCodigo)c.Visible=emergencia;
        foreach(var c in camposNueva)c.Visible=ayuda||emergencia;
        administrador.Enabled=claveAdministrador.Enabled=ayuda;codigo.Enabled=emergencia;
        recuperar.Enabled=ayuda||emergencia;nueva.Enabled=confirmacion.Enabled=usuario.Enabled=ayuda||emergencia;
        claveAdministrador.Clear();codigo.Clear();mensaje.Text=ayuda?"Otro Administrador debe comprobar tu identidad antes de autorizar.":emergencia?"Introduce el código que guardaste fuera del equipo. Cinco fallos pausan su verificación durante cinco minutos.":"Solicita mantenimiento al responsable autorizado de SQL Server. Necesita identificar la base, respaldarla y comprobar su restauración antes de restablecer únicamente tu cuenta. Conectarse a SQL no recupera por sí solo tu usuario. No se ejecutará mantenimiento desde esta pantalla.";
    }
}
