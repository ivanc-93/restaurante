namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioConexion
{
    private TextBox servidor=null!,baseDatos=null!,sqlUsuario=null!,sqlClave=null!,admin=null!,adminClave=null!;
    private ComboBox autenticacion=null!;private NumericUpDown puerto=null!;private CheckBox certificado=null!;private Label mensaje=null!;private Button probar=null!,guardar=null!;
    private void InitializeComponent()
    {
        var p=DisenoFormulario.Preparar(this,"Preparar conexión del equipo");
        p.Controls.Add(new Label{Text="Conecte una base preparada. Este asistente no crea ni migra bases y no permite vender.",AutoSize=true,MaximumSize=new Size(500,0)});
        servidor=DisenoFormulario.Campo(p,"Servidor SQL");servidor.Text="localhost";
        baseDatos=DisenoFormulario.Campo(p,"Base de datos");baseDatos.Text="RestauranteDB";
        autenticacion=DisenoFormulario.Opciones(p,"Autenticación de SQL","Windows","SQL Server");
        puerto=new NumericUpDown{Minimum=0,Maximum=65535,Value=0,Width=180};p.Controls.Add(new Label{Text="Puerto TCP opcional · 0 usa la conexión automática del servidor",AutoSize=true});p.Controls.Add(puerto);
        sqlUsuario=DisenoFormulario.Campo(p,"Usuario SQL solo para autenticación SQL");sqlClave=DisenoFormulario.Campo(p,"Contraseña SQL",true);
        certificado=new CheckBox{Text="Confiar en certificado SQL del entorno local",Checked=true,AutoSize=true};p.Controls.Add(certificado);
        admin=DisenoFormulario.Campo(p,"Administrador de Sabores de esta base");adminClave=DisenoFormulario.Campo(p,"Contraseña de Sabores para guardar el destino",true);
        probar=DisenoFormulario.Boton(p,"Probar conexión y esquema",Probar_Click);guardar=DisenoFormulario.Boton(p,"Guardar conexión validada",Guardar_Click);mensaje=DisenoFormulario.Mensaje(p);
        autenticacion.SelectedIndexChanged+=(_,_)=>ActualizarCampos();servidor.TextChanged+=(_,_)=>ActualizarCampos();ActualizarCampos();
    }
    private void ActualizarCampos(){sqlUsuario.Enabled=sqlClave.Enabled=autenticacion.SelectedIndex==1;puerto.Enabled=!servidor.Text.Trim().StartsWith("(localdb)\\",StringComparison.OrdinalIgnoreCase);}
}
