using Restaurante.Backend.Servicios;
using Restaurante.Frontend.Servicios;
namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioPrimerAdministrador:Form
{
    private readonly ServicioInstalacion servicio=null!;
    private readonly ServicioPreparacionInicial preparador=null!;
    private PreparacionPrimerAdministrador? preparada;
    private readonly IDialogosUsuario dialogos=new DialogosUsuario();
    private bool ocupado,puedePreparar,admiteInstalacion=true;
    public FormularioPrimerAdministrador()=>InitializeComponent();
    public FormularioPrimerAdministrador(IConfiguracionConexion cfg,IDialogosUsuario? dialogos=null):this()
    {
        this.dialogos=dialogos??new DialogosUsuario();
        servicio=new(cfg);preparador=new(cfg);
        FormClosing+=(_,e)=>e.Cancel=ocupado;
        FormClosed+=(_,_)=>preparada?.Dispose();
        Shown+=async (_,_)=>await ComprobarEstado();
    }
    private async Task ComprobarEstado()
    {
        Ocupado(true);
        try
        {
            var estado=await Task.Run(()=>preparador.ConsultarEstado());
            admiteInstalacion=estado.Pendiente;puedePreparar=estado.Pendiente&&estado.AutorizadoWindows;
            if(!estado.Pendiente)
            {estadoInicial.Text="Esta base ya tiene usuarios o completó su instalación. Usa el acceso existente. No se reinicializará.";guardar.Enabled=false;return;}
            estadoInicial.Text=estado.AutorizadoWindows
                ? "Tu cuenta Windows tiene autorización administrativa SQL. Pulsa Preparar instalación con Windows; se verificará un respaldo y podrás elegir tu cuenta de Sabores sin introducir un token."
                : "Necesitas al responsable con autenticación Windows y permiso sysadmin de SQL Server para preparar esta instalación. También puedes usar un token vigente entregado por él. Tu usuario de Sabores se crea después.";
            prepararWindows.Enabled=estado.AutorizadoWindows;
        }
        catch(Exception ex){MostrarError(ex);}
        finally{Ocupado(false);}
    }
    private async void PrepararWindows_Click(object? sender,EventArgs e)
    {
        if(ocupado)return;
        Ocupado(true);
        try
        {
            var estado=await Task.Run(()=>preparador.ConsultarEstado());
            if(!estado.Pendiente)throw new InvalidOperationException("La base ya tiene usuarios o completó su instalación. Usa el acceso existente.");
            if(!estado.AutorizadoWindows)throw new UnauthorizedAccessException("Se requiere autenticación Windows con permiso sysadmin de SQL Server.");
            var reemplazar=false;
            if(estado.TieneToken)
            {
                if(dialogos.Mostrar(this,"Existe una preparación anterior. Continuar invalidará su token. Primero se hará y verificará un respaldo de la base. No se borrarán datos ni usuarios. ¿Confirmas reemplazar el token anterior?","Confirmar preparación inicial",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)return;
                reemplazar=true;
            }
            var nueva=await Task.Run(()=>preparador.Preparar(reemplazar));
            preparada?.Dispose();preparada=nueva;
            token.Clear();token.Visible=etiquetaToken.Visible=false;
            estadoInicial.Text="Instalación preparada con Windows y respaldo verificado. Elige tu nombre, usuario y contraseña de Sabores. La autorización se conserva solo durante esta ventana.";
            mensaje.Text="Ya puedes completar la instalación. Si cierras esta ventana antes, necesitarás preparar de nuevo.";
        }
        catch(Exception ex){MostrarError(ex);}
        finally{Ocupado(false);}
    }
    private void Ocupado(bool valor)
    {
        if(valor)ocupado=true;
        guardar.Enabled=!valor&&admiteInstalacion;
        prepararWindows.Enabled=!valor&&puedePreparar&&preparada is null;
        if(!valor)ocupado=false;
    }
    private void MostrarError(Exception ex)
    {
        mensaje.Text=(ex is UnauthorizedAccessException or InvalidOperationException or ArgumentException ? ex.Message : ErroresConexion.Describir(ex))+ErroresConexion.Registrar(ex);
    }
    private async void Guardar_Click(object? sender,EventArgs e)
    {
        if(ocupado)return;
        if((preparada is null&&!ValidacionCredenciales.Campo(token,mensaje,"el token o usa Preparar instalación con Windows"))||!ValidacionCredenciales.Campo(nombre,mensaje,"el nombre")||!ValidacionCredenciales.Campo(usuario,mensaje,"el usuario")||!ValidacionCredenciales.Nueva(clave,mensaje,repetir))return;
        var t=token.Text;var n=nombre.Text;var u=usuario.Text;var c=clave.Text;ocupado=true;guardar.Enabled=false;
        prepararWindows.Enabled=false;
        try{await Task.Run(()=>{if(preparada is not null)preparada.CrearAdministrador(n,u,c);else servicio.CrearPrimerAdministrador(t,n,u,c);});ocupado=false;DialogResult=DialogResult.OK;}
        catch(Exception ex){MostrarError(ex);}
        finally{token.Clear();clave.Clear();repetir.Clear();Ocupado(false);if(DialogResult==DialogResult.OK)Close();}
    }
}
