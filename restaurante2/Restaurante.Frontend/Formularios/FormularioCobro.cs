using Restaurante.Backend.Dominio;
using Restaurante.Backend.Servicios;
using Restaurante.Frontend.Servicios;

namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioCobro : Form
{
    private readonly IRepositorioRestaurante _repo=null!;
    private readonly IDialogosUsuario _dialogos=new DialogosUsuario();
    private readonly CuentaRevision _cuenta=null!;
    private SolicitudCobro? _solicitud;
    private bool _ocupado;
    private bool _ajustandoCampos;
    private int _ultimoAncho;
    public ResultadoCobro? Resultado { get; private set; }
    public FormularioCobro() {InitializeComponent();FormatoPresentacion.Aplicar(this);}
    public FormularioCobro(IRepositorioRestaurante repo,CuentaRevision cuenta,IDialogosUsuario? dialogos=null):this()
    {
        _repo=repo;_cuenta=cuenta;_dialogos=dialogos??new DialogosUsuario();
        tablaCuenta.DataSource=cuenta.Detalles.ToList();
        totalCuenta.Text="Total  "+FormatoPresentacion.Moneda(cuenta.Total);
        detalle.Text=$"Mesa {cuenta.NumeroMesa} · Pedido #{cuenta.PedidoId}\r\n\r\n"+
            string.Join("\r\n",cuenta.Detalles.Select(d=>$"{d.Cantidad} × {d.Producto}\r\n{FormatoPresentacion.Moneda(d.PrecioUnitario)} = {FormatoPresentacion.Moneda(d.Importe)}\r\n"))+
            $"\r\nOriginal: {FormatoPresentacion.Moneda(cuenta.ImporteOriginal)}\r\nDescuento: {FormatoPresentacion.Moneda(cuenta.Descuento)}\r\nTOTAL: {FormatoPresentacion.Moneda(cuenta.Total)}";
        receptor.Text=cuenta.Cliente;publico.Checked=cuenta.ClienteId is null;
        if(cuenta.Total==0)metodo.Items.Add("Cortesía");
        cliente.SelectedIndexChanged+=(_,_)=> {
            if(cliente.SelectedItem is Cliente c && c.Id>0) {publico.Checked=false;receptor.Text=c.Nombre;correo.Text=c.Correo;opcionales.Checked=!string.IsNullOrWhiteSpace(c.Correo);}
        };
        Shown+=async(_,_)=> {try {var clientes=await Task.Run(repo.ListarClientes);if(IsDisposed)return;cliente.DataSource=new List<Cliente>{new(0,"No copiar: introducir receptor",null,null)}.Concat(clientes).ToList();_ultimoAncho=0;AjustarCampos();detalle.SelectionLength=0;}
            catch(Exception ex){mensaje.Text=ex.Message;} };
        FormClosing+=(_,e)=>{if(_ocupado)e.Cancel=true;};
        campos.SizeChanged+=(_,_)=>AjustarCampos();
        pie.SizeChanged+=(_,_)=>{var ancho=Math.Max(120,ClientSize.Width-estructura.Padding.Horizontal-16);if(mensaje.MaximumSize.Width!=ancho){mensaje.MaximumSize=new Size(ancho,(int)(40*DeviceDpi/96f));saldo.MaximumSize=new Size(ancho,0);}};
        Datos_Cambio(this,EventArgs.Empty);
    }
    private void Datos_Cambio(object? sender,EventArgs e)
    {
        if(_cuenta is null)return;
        receptor.Enabled=identificacion.Enabled=!publico.Checked;
        receptor.Visible=identificacion.Visible=campos.Controls["lblReceptor"]!.Visible=campos.Controls["lblIdentificacion"]!.Visible=!publico.Checked;
        direccion.Visible=correo.Visible=campos.Controls["lblDireccion"]!.Visible=campos.Controls["lblCorreo"]!.Visible=opcionales.Checked;
        observacion.Visible=campos.Controls["lblObservacion"]!.Visible=opcionales.Checked||metodo.Text=="Tarjeta";
        resultadoTarjeta.Visible=referencia.Visible=campos.Controls["lblResultado"]!.Visible=campos.Controls["lblReferencia"]!.Visible=metodo.Text=="Tarjeta";
        if(publico.Checked){receptor.Text="Público general";identificacion.Clear();}
        resultadoTarjeta.Enabled=referencia.Enabled=metodo.Text=="Tarjeta";
        entregado.Enabled=metodo.Text=="Efectivo";
        if(metodo.Text=="Tarjeta")entregado.Value=_cuenta.Total;
        if(metodo.Text=="Cortesía")entregado.Value=0;
        var diferencia=entregado.Value-_cuenta.Total;
        saldo.Text=$"Total: {FormatoPresentacion.Moneda(_cuenta.Total)} · "+(diferencia>=0?$"Cambio: {FormatoPresentacion.Moneda(diferencia)}":$"Falta: {FormatoPresentacion.Moneda(-diferencia)}");
        _ultimoAncho=0;AjustarCampos();
        if(!_ocupado)confirmar.Enabled=_solicitud is not null||datosConfirmados.Checked&&metodo.SelectedIndex>=0
            &&(metodo.Text=="Tarjeta"&&resultadoTarjeta.Text is "Rechazado" or "Incierto"||pagoConfirmado.Checked);
    }
    private void AjustarCampos()
    {
        var ancho=Math.Max(120,campos.ClientSize.Width-campos.Padding.Horizontal-SystemInformation.VerticalScrollBarWidth-12);
        if(_ajustandoCampos || ancho==_ultimoAncho)return;
        _ajustandoCampos=true;_ultimoAncho=ancho;campos.SuspendLayout();
        try {foreach(Control campo in campos.Controls)
            if(campo is Label or CheckBox){campo.MaximumSize=new Size(ancho,0);campo.Margin=new Padding(0,6,0,6);}else {campo.Width=ancho;campo.Margin=new Padding(0,0,0,6);}}
        finally{campos.ResumeLayout(true);_ajustandoCampos=false;}
    }
    private void ValidarEntrada()
    {
        void Exigir(bool valido,Control campo,string error){if(valido)return;campos.ScrollControlIntoView(campo);campo.Focus();throw new ArgumentException(error);}
        Exigir(metodo.SelectedIndex>=0,metodo,"Seleccione el método de pago.");
        Exigir(publico.Checked||!string.IsNullOrWhiteSpace(receptor.Text),receptor,"Ingrese el nombre del receptor.");
        Exigir(datosConfirmados.Checked,datosConfirmados,"Confirme que revisó los productos, el importe y el receptor.");
        if(metodo.Text=="Tarjeta")
        {
            Exigir(resultadoTarjeta.SelectedIndex>=0,resultadoTarjeta,"Seleccione el resultado real del terminal externo.");
            if(resultadoTarjeta.Text is "Confirmado" or "Incierto")Exigir(!string.IsNullOrWhiteSpace(referencia.Text),referencia,"Ingrese una referencia externa, sin datos de tarjeta.");
            if(resultadoTarjeta.Text=="Confirmado")Exigir(pagoConfirmado.Checked,pagoConfirmado,"Confirme que comprobó manualmente el pago externo.");
        }
        else
        {
            Exigir(entregado.Value>=_cuenta.Total,entregado,"El efectivo es insuficiente.");
            Exigir(pagoConfirmado.Checked,pagoConfirmado,"Confirme el efectivo recibido o la cortesía autorizada.");
        }
    }
    private async void Confirmar_Click(object? sender,EventArgs e)
    {
        if(_ocupado)return;
        if(_solicitud is null){try{ValidarEntrada();}catch(ArgumentException ex){mensaje.Text=ex.Message;return;}}
        _solicitud??=new(Guid.NewGuid(),_cuenta.PedidoId,_cuenta.Version,_cuenta.Total,publico.Checked,receptor.Text,identificacion.Text,direccion.Text,correo.Text,
            metodo.Text,entregado.Value,resultadoTarjeta.Text,referencia.Text,observacion.Text,datosConfirmados.Checked,pagoConfirmado.Checked,metodo.Text=="Cortesía"&&pagoConfirmado.Checked);
        _ocupado=true;confirmar.Enabled=false;campos.Enabled=false;
        try
        {
            Resultado=await DiarioOperaciones.Ejecutar(_repo,"Cobro",_solicitud.OperacionId,_solicitud,()=>_repo.RegistrarCobro(_solicitud));
            _dialogos.Mostrar(this,Resultado.Estado=="Confirmado"?$"Comprobante {Resultado.NumeroComprobante}. Pago registrado por el operador y cierre con paso de mesa a limpieza. Consulte Mesas para su estado actual.":
                $"Resultado {Resultado.Estado}. La cuenta sigue abierta. No repita un cobro incierto; solicite conciliación.","Resultado de la operación");
            _ocupado=false;DialogResult=DialogResult.OK;Close();
        }
        catch(Exception ex)
        {
            mensaje.Text=ex.Message;
            _dialogos.Mostrar(this,ex.Message,"No se pudo completar el cobro",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            if(_solicitud is not null && !DiarioOperaciones.TienePendiente(_repo,_solicitud.OperacionId))
            { _solicitud=null;campos.Enabled=true;confirmar.Text="Confirmar operación"; }
            else confirmar.Text="Recuperar la misma operación";
        }
        finally{_ocupado=false;confirmar.Enabled=true;}
    }
}
