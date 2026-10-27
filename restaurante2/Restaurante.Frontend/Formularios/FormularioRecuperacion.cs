using Restaurante.Backend.Servicios;
using Restaurante.Frontend.Servicios;
namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioRecuperacion:Form
{
    private readonly IRepositorioRestaurante _repo=null!;
    private bool _ocupado;
    public FormularioRecuperacion(){InitializeComponent();FormatoPresentacion.Aplicar(this);}
    public FormularioRecuperacion(IRepositorioRestaurante repo):this()
    {_repo=repo;Shown+=(_,_)=>Cargar();FormClosing+=(_,e)=>{if(_ocupado)e.Cancel=true;};}
    private void Cargar()
    {
        try{lista.DataSource=DiarioOperaciones.Listar(_repo).ToList();recuperar.Enabled=descartar.Enabled=lista.Items.Count>0;if(lista.Items.Count==0)detalle.Text="No hay operaciones pendientes para este usuario y esta base.";}
        catch(Exception ex){detalle.Text=ex.Message;recuperar.Enabled=descartar.Enabled=false;}
    }
    private void Seleccion_Cambio(object? sender,EventArgs e)
    {if(lista.SelectedItem is OperacionPendienteLocal p)detalle.Text=$"Tipo: {p.Tipo}\r\nIdentificador interno de operación: {p.Id}\r\n\r\n{p.Datos}\r\n\r\nSe reutiliza el mismo identificador. Si SQL ya la aplicó, se recuperará su resultado sin repetirla.";}
    private async void Recuperar_Click(object? sender,EventArgs e)
    {
        if(_ocupado||lista.SelectedItem is not OperacionPendienteLocal p)return;
        if(MessageBox.Show(this,"Revisa los datos mostrados. ¿Recuperar o completar exactamente esta operación?","Recuperar",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)return;
        _ocupado=true;recuperar.Enabled=descartar.Enabled=false;
        try{var r=await DiarioOperaciones.Reanudar(_repo,p);MessageBox.Show(this,r,"Resultado recuperado");Cargar();}
        catch(Exception ex){detalle.Text=ex.Message;}finally{_ocupado=false;recuperar.Enabled=descartar.Enabled=lista.Items.Count>0;}
    }
    private async void Descartar_Click(object? sender,EventArgs e)
    {
        if(_ocupado||lista.SelectedItem is not OperacionPendienteLocal p)return;
        if(MessageBox.Show(this,"Solo se eliminará la solicitud local si SQL confirma que no fue aplicada. ¿Continuar?","Descartar solicitud no aplicada",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)return;
        _ocupado=true;recuperar.Enabled=descartar.Enabled=false;
        try{await Task.Run(()=>DiarioOperaciones.DescartarRechazada(_repo,p));Cargar();}catch(Exception ex){detalle.Text=ex.Message;}
        finally{_ocupado=false;recuperar.Enabled=descartar.Enabled=lista.Items.Count>0;}
    }
}
