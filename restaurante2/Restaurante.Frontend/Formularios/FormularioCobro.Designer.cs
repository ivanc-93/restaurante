#nullable enable
namespace Restaurante.Frontend.Formularios;
partial class FormularioCobro
{
    private System.ComponentModel.IContainer? components;
    private TextBox detalle = null!;
    private ComboBox cliente = null!;
    private CheckBox publico = null!;
    private CheckBox opcionales = null!;
    private TextBox receptor = null!;
    private TextBox identificacion = null!;
    private TextBox direccion = null!;
    private TextBox correo = null!;
    private ComboBox metodo = null!;
    private ComboBox resultadoTarjeta = null!;
    private NumericUpDown entregado = null!;
    private TextBox referencia = null!;
    private TextBox observacion = null!;
    private CheckBox datosConfirmados = null!;
    private CheckBox pagoConfirmado = null!;
    private Label saldo = null!;
    private Label mensaje = null!;
    private Button confirmar = null!;
    private TableLayoutPanel estructura = null!;
    private FlowLayoutPanel campos = null!;
    private TableLayoutPanel pie = null!;
    protected override void Dispose(bool disposing) { if(disposing)components?.Dispose(); base.Dispose(disposing); }
    private void InitializeComponent()
    {
        components=new System.ComponentModel.Container(); detalle=new TextBox();cliente=new ComboBox();publico=new CheckBox();opcionales=new CheckBox();
        receptor=new TextBox();identificacion=new TextBox();direccion=new TextBox();correo=new TextBox();metodo=new ComboBox();
        resultadoTarjeta=new ComboBox();entregado=new NumericUpDown();referencia=new TextBox();observacion=new TextBox();
        datosConfirmados=new CheckBox();pagoConfirmado=new CheckBox();saldo=new Label();mensaje=new Label();confirmar=new Button();
        estructura=new TableLayoutPanel();campos=new FlowLayoutPanel();pie=new TableLayoutPanel();
        ((System.ComponentModel.ISupportInitialize)entregado).BeginInit();SuspendLayout();
        estructura.Dock=DockStyle.Fill;estructura.ColumnCount=2;estructura.RowCount=2;estructura.Padding=new Padding(16);
        estructura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,45));estructura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,55));
        estructura.RowStyles.Add(new RowStyle(SizeType.Percent,100));estructura.RowStyles.Add(new RowStyle(SizeType.Absolute,110));
        detalle.Dock=DockStyle.Fill;detalle.Multiline=true;detalle.ReadOnly=true;detalle.ScrollBars=ScrollBars.Vertical;
        detalle.BackColor=Color.White;detalle.AccessibleName="Detalle y total de la cuenta";detalle.TabStop=false;
        campos.Dock=DockStyle.Fill;campos.FlowDirection=FlowDirection.TopDown;campos.WrapContents=false;campos.AutoScroll=true;campos.Padding=new Padding(12);
        cliente.DropDownStyle=ComboBoxStyle.DropDownList;cliente.Width=330;cliente.AccessibleName="Copiar datos de cliente del catálogo";
        publico.Text="Público general / sin identificación";publico.AutoSize=true;publico.CheckedChanged+=Datos_Cambio;
        opcionales.Text="Agregar dirección, correo u observación (opcional)";opcionales.AutoSize=true;opcionales.CheckedChanged+=Datos_Cambio;
        receptor.Width=330;receptor.MaxLength=120;receptor.AccessibleName="Nombre o razón social del receptor";
        identificacion.Width=330;identificacion.MaxLength=40;identificacion.AccessibleName="NIT u otra identificación opcional";
        direccion.Width=330;direccion.MaxLength=250;direccion.AccessibleName="Dirección opcional";
        correo.Width=330;correo.MaxLength=150;correo.AccessibleName="Correo opcional";
        metodo.DropDownStyle=ComboBoxStyle.DropDownList;metodo.Width=330;metodo.Items.AddRange(new object[]{"Efectivo","Tarjeta"});metodo.SelectedIndexChanged+=Datos_Cambio;
        resultadoTarjeta.DropDownStyle=ComboBoxStyle.DropDownList;resultadoTarjeta.Width=330;
        resultadoTarjeta.Items.AddRange(new object[]{"Confirmado","Rechazado","Incierto"});resultadoTarjeta.AccessibleName="Resultado declarado del pago externo";
        resultadoTarjeta.SelectedIndexChanged+=Datos_Cambio;
        entregado.DecimalPlaces=2;entregado.Maximum=9999999999.99M;entregado.Width=330;entregado.ValueChanged+=Datos_Cambio;entregado.AccessibleName="Importe entregado o pagado externamente";
        referencia.Width=330;referencia.MaxLength=100;referencia.AccessibleName="Referencia externa sin datos de tarjeta";
        observacion.Width=330;observacion.MaxLength=250;observacion.AccessibleName="Observación del pago";
        datosConfirmados.Text="Revisé productos, importe y datos del receptor";datosConfirmados.AutoSize=true;
        pagoConfirmado.Text="Confirmo recepción del efectivo o comprobación manual del pago";pagoConfirmado.AutoSize=true;
        campos.Controls.Add(new Label {Text="Copiar cliente existente (opcional)",AutoSize=true});campos.Controls.Add(cliente);campos.Controls.Add(publico);
        campos.Controls.Add(new Label{Name="lblReceptor",Text="Nombre del receptor",AutoSize=true});campos.Controls.Add(receptor);
        campos.Controls.Add(new Label{Name="lblIdentificacion",Text="NIT / identificación (opcional, sin validación SAT)",AutoSize=true});campos.Controls.Add(identificacion);
        campos.Controls.Add(opcionales);
        campos.Controls.Add(new Label{Name="lblDireccion",Text="Dirección (opcional)",AutoSize=true});campos.Controls.Add(direccion);
        campos.Controls.Add(new Label{Name="lblCorreo",Text="Correo (opcional)",AutoSize=true});campos.Controls.Add(correo);
        campos.Controls.Add(new Label{Text="Método de pago manual",AutoSize=true});campos.Controls.Add(metodo);
        campos.Controls.Add(new Label{Name="lblResultado",Text="Resultado del terminal externo (solo tarjeta)",AutoSize=true});campos.Controls.Add(resultadoTarjeta);
        campos.Controls.Add(new Label{Text="Entregado en efectivo / importe externo",AutoSize=true});campos.Controls.Add(entregado);
        campos.Controls.Add(new Label{Name="lblReferencia",Text="Referencia externa (sin número de tarjeta)",AutoSize=true});campos.Controls.Add(referencia);
        campos.Controls.Add(new Label{Name="lblObservacion",Text="Observación",AutoSize=true});campos.Controls.Add(observacion);
        campos.Controls.Add(datosConfirmados);campos.Controls.Add(pagoConfirmado);
        pie.Dock=DockStyle.Fill;pie.ColumnCount=1;pie.RowCount=3;pie.AutoSize=false;
        pie.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        pie.RowStyles.Add(new RowStyle(SizeType.AutoSize));pie.RowStyles.Add(new RowStyle(SizeType.AutoSize));pie.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        saldo.AutoSize=true;saldo.Font=new Font("Segoe UI",13,FontStyle.Bold);
        mensaje.AutoSize=true;mensaje.AutoEllipsis=true;mensaje.MaximumSize=new Size(850,70);mensaje.Text="Comprobante interno académico — no válido como documento tributario. Sin conexión SAT ni bancaria.";
        confirmar.Text="Confirmar operación";confirmar.AutoSize=false;confirmar.Height=44;confirmar.Dock=DockStyle.Top;confirmar.Click+=Confirmar_Click;
        pie.Controls.Add(saldo,0,0);pie.Controls.Add(mensaje,0,1);pie.Controls.Add(confirmar,0,2);
        estructura.Controls.Add(detalle,0,0);estructura.Controls.Add(campos,1,0);estructura.Controls.Add(pie,0,1);estructura.SetColumnSpan(pie,2);
        Controls.Add(estructura);AutoScaleDimensions=new SizeF(7F,15F);AutoScaleMode=AutoScaleMode.Font;
        ClientSize=new Size(1000,700);MinimumSize=new Size(550,320);StartPosition=FormStartPosition.CenterParent;Text="Revisar y cobrar · GTQ";
        ((System.ComponentModel.ISupportInitialize)entregado).EndInit();ResumeLayout(false);
    }
}
