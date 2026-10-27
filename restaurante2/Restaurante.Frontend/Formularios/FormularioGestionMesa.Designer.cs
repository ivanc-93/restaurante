#nullable enable
namespace Restaurante.Frontend.Formularios;
partial class FormularioGestionMesa
{
    private System.ComponentModel.IContainer? components;
    private Label resumen=null!;
    private ComboBox accion=null!;
    private ComboBox resultado=null!;
    private TextBox motivo=null!;
    private CheckBox declarado=null!;
    private CheckBox verificado=null!;
    private Button aplicar=null!;
    private FlowLayoutPanel campos=null!;
    protected override void Dispose(bool disposing){if(disposing)components?.Dispose();base.Dispose(disposing);}
    private void InitializeComponent()
    {
        components=new System.ComponentModel.Container();resumen=new Label();accion=new ComboBox();resultado=new ComboBox();
        motivo=new TextBox();declarado=new CheckBox();verificado=new CheckBox();aplicar=new Button();campos=new FlowLayoutPanel();
        SuspendLayout();
        campos.Dock=DockStyle.Fill;campos.FlowDirection=FlowDirection.TopDown;campos.WrapContents=false;campos.AutoScroll=true;campos.Padding=new Padding(20);
        resumen.AutoSize=true;resumen.MaximumSize=new Size(480,0);
        accion.Width=430;accion.DropDownStyle=ComboBoxStyle.DropDownList;accion.SelectedIndexChanged+=Accion_Cambio;
        resultado.Width=430;resultado.DropDownStyle=ComboBoxStyle.DropDownList;resultado.Items.AddRange(new object[]{"Confirmado","Rechazado"});
        motivo.Width=430;motivo.Height=100;motivo.Multiline=true;motivo.MaxLength=500;motivo.AccessibleName="Motivo y evidencia obligatorios";
        declarado.Text="Declaro que no hubo preparación ni consumo";declarado.AutoSize=true;
        verificado.Text="Comprobé manualmente la evidencia y el resultado";verificado.AutoSize=true;
        aplicar.Text="Confirmar gestión";aplicar.AutoSize=true;aplicar.Enabled=false;aplicar.Click+=Aplicar_Click;
        campos.Controls.Add(resumen);campos.Controls.Add(accion);
        campos.Controls.Add(new Label{Text="Resultado externo (solo conciliación)",AutoSize=true});campos.Controls.Add(resultado);
        campos.Controls.Add(new Label{Text="Motivo / evidencia (obligatorio)",AutoSize=true});campos.Controls.Add(motivo);
        campos.Controls.Add(declarado);campos.Controls.Add(verificado);campos.Controls.Add(aplicar);
        Controls.Add(campos);AutoScaleDimensions=new SizeF(7F,15F);AutoScaleMode=AutoScaleMode.Font;
        ClientSize=new Size(540,530);MinimumSize=new Size(480,420);StartPosition=FormStartPosition.CenterParent;Text="Gestionar cierre de mesa";ResumeLayout(false);
    }
}
