using Restaurante.Backend.Servicios;
namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioCodigoRecuperacion:Form
{
    private readonly IRepositorioRestaurante repo;private bool ocupado;
    public FormularioCodigoRecuperacion(IRepositorioRestaurante repositorio){repo=repositorio;InitializeComponent();FormClosing+=(_,e)=>e.Cancel=ocupado;FormClosed+=(_,_)=>{clave.Clear();codigo.Clear();};}
    private async void Generar_Click(object? sender,EventArgs e)
    {
        if(ocupado)return;
        if(string.IsNullOrEmpty(clave.Text)){mensaje.Text="Escribe tu contraseña vigente de Administrador, no el código de emergencia.";clave.Focus();return;}
        var c=clave.Text;ocupado=true;generar.Enabled=continuar.Enabled=clave.Enabled=guardado.Enabled=false;UseWaitCursor=true;
        Restaurante.Backend.Dominio.ResultadoGeneracionCodigo? resultado=null;string? error=null;
        try{resultado=await Task.Run(()=>repo.IntentarGenerarCodigoRecuperacion(c)).ConfigureAwait(false);}
        catch(Microsoft.Data.SqlClient.SqlException){error="No se confirmó la generación con SQL. No confirmes un código anterior: comprueba la conexión y genera un reemplazo para guardar el último obtenido.";}
        catch(Exception ex){System.Diagnostics.Trace.TraceError("Generación de código: {0}",ex.GetType().Name);error="No se pudo completar la generación por un problema técnico. No se confirmó el resultado; solicita asistencia sin enviar tu contraseña ni códigos.";}
        c="";
        DisenoFormulario.EnInterfaz(this,()=>
        {
            ocupado=false;generar.Enabled=clave.Enabled=guardado.Enabled=true;UseWaitCursor=false;clave.Clear();
            if(error is not null){codigo.Clear();guardado.Checked=false;mensaje.Text=error;}
            else if(resultado is not null)
            {
                mensaje.Text=resultado.Mensaje;
                if(resultado.Generado){codigo.Text=resultado.Codigo;guardado.Checked=false;}
                else clave.Focus();
            }
            continuar.Enabled=guardado.Checked&&codigo.Text.Length>0;
        });
    }
    private async void Confirmar_Click(object? sender,EventArgs e)
    {
        if(ocupado||!guardado.Checked||codigo.Text.Length==0)return;
        var valor=codigo.Text;ocupado=true;continuar.Enabled=generar.Enabled=false;
        Restaurante.Backend.Dominio.ResultadoRecuperacion? resultado=null;
        try{resultado=await Task.Run(()=>repo.IntentarConfirmarCodigoRecuperacion(valor)).ConfigureAwait(false);}
        catch(Exception ex){System.Diagnostics.Trace.TraceError("Confirmación de custodia: {0}",ex.GetType().Name);}
        DisenoFormulario.EnInterfaz(this,()=>{ocupado=false;if(resultado?.Correcto==true){DialogResult=DialogResult.OK;Close();}else{mensaje.Text=resultado?.Mensaje??"No se confirmó el código con SQL. Comprueba la conexión y vuelve a confirmar antes de cerrar.";continuar.Enabled=guardado.Checked;generar.Enabled=true;}});
    }
}
