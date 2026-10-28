using Restaurante.Backend.Servicios;
namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioCodigoRecuperacion:Form
{
    private readonly IRepositorioRestaurante repo;private bool ocupado;
    public FormularioCodigoRecuperacion(IRepositorioRestaurante repositorio){repo=repositorio;InitializeComponent();FormClosing+=(_,e)=>e.Cancel=ocupado;FormClosed+=(_,_)=>{clave.Clear();codigo.Clear();};}
    private async void Generar_Click(object? sender,EventArgs e)
    {
        if(ocupado||string.IsNullOrEmpty(clave.Text)){mensaje.Text="Introduce tu contraseña vigente de Administrador.";return;}
        var c=clave.Text;ocupado=true;generar.Enabled=false;codigo.Clear();guardado.Checked=false;
        string texto,valor="";
        try{valor=await Task.Run(()=>repo.GenerarCodigoRecuperacion(c)).ConfigureAwait(false);texto="El código anterior ya no funciona. Guarda este reemplazo: no podrás volver a consultarlo.";}
        catch(Exception ex){texto=ex is Microsoft.Data.SqlClient.SqlException?"No se confirmó la generación con SQL. Cuando se recupere la conexión, genera un código nuevo y guarda el último obtenido.":ex is UnauthorizedAccessException or InvalidOperationException?ex.Message:"No se pudo generar el código. Solicita asistencia.";}
        DisenoFormulario.EnInterfaz(this,()=>{codigo.Text=valor;mensaje.Text=texto;clave.Clear();ocupado=false;generar.Enabled=true;});
    }
    private async void Confirmar_Click(object? sender,EventArgs e)
    {
        if(ocupado||!guardado.Checked||codigo.Text.Length==0)return;
        var valor=codigo.Text;ocupado=true;continuar.Enabled=generar.Enabled=false;
        bool correcto=false;
        try{await Task.Run(()=>repo.ConfirmarCodigoRecuperacion(valor)).ConfigureAwait(false);correcto=true;}
        catch(Exception ex){System.Diagnostics.Trace.TraceError("Confirmación de custodia: {0}",ex.GetType().Name);}
        DisenoFormulario.EnInterfaz(this,()=>{ocupado=false;if(correcto){DialogResult=DialogResult.OK;Close();}else{mensaje.Text="No se pudo confirmar el código guardado. Comprueba la conexión y vuelve a confirmar.";continuar.Enabled=guardado.Checked;generar.Enabled=true;}});
    }
}
