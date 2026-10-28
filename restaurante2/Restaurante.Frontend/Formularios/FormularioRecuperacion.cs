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
        try{lista.DataSource=DiarioOperaciones.Listar(_repo).ToList();recuperar.Enabled=descartar.Enabled=lista.SelectedItem is OperacionPendienteLocal {Tipo:not "Ilegible"};if(lista.Items.Count==0)detalle.Text="No hay operaciones pendientes para este usuario y esta base.";}
        catch(Exception ex){detalle.Text=ex.Message;recuperar.Enabled=descartar.Enabled=false;}
    }
    private void Seleccion_Cambio(object? sender,EventArgs e)
    {if(lista.SelectedItem is OperacionPendienteLocal p){detalle.Text=PresentacionPendiente.Resumen(p);recuperar.Enabled=descartar.Enabled=p.Tipo!="Ilegible";}}
    private async void Recuperar_Click(object? sender,EventArgs e)
    {
        if(_ocupado||lista.SelectedItem is not OperacionPendienteLocal p)return;
        if(MessageBox.Show(this,"Revisa los datos mostrados. ¿Recuperar o completar exactamente esta operación?","Recuperar",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)return;
        _ocupado=true;lista.Enabled=false;recuperar.Enabled=descartar.Enabled=false;
        try{
            if(!await Task.Run(()=>_repo.OperacionAplicada(p.Id)) && PresentacionPendiente.Autorizacion(p) is {} autorizacion)
            {
                if(!FormularioAutorizar.Solicitar(this,_repo,p.Id,autorizacion.Tipo,autorizacion.Datos,PresentacionPendiente.Resumen(p)))return;
            }
            var r=await DiarioOperaciones.Reanudar(_repo,p);MessageBox.Show(this,r,"Resultado recuperado");Cargar();}
        catch(Exception ex){detalle.Text=ex.Message;}finally{_ocupado=false;lista.Enabled=true;recuperar.Enabled=descartar.Enabled=lista.SelectedItem is OperacionPendienteLocal {Tipo:not "Ilegible"};}
    }
    private async void Descartar_Click(object? sender,EventArgs e)
    {
        if(_ocupado||lista.SelectedItem is not OperacionPendienteLocal p)return;
        if(MessageBox.Show(this,"SQL registrará el abandono definitivo de esta solicitud. Si otra instancia ya la aplicó, deberá recuperarse. No cancela pedidos ni consumo. ¿Continuar?","Descartar solicitud no aplicada",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)return;
        _ocupado=true;lista.Enabled=false;recuperar.Enabled=descartar.Enabled=false;
        try{await Task.Run(()=>DiarioOperaciones.DescartarRechazada(_repo,p));Cargar();}catch(Exception ex){detalle.Text=ex.Message;}
        finally{_ocupado=false;lista.Enabled=true;recuperar.Enabled=descartar.Enabled=lista.SelectedItem is OperacionPendienteLocal {Tipo:not "Ilegible"};}
    }
}
