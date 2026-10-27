#nullable enable
namespace Restaurante.Frontend.Formularios;
partial class FormularioRecuperacion
{
    private System.ComponentModel.IContainer? components;
    private ListBox lista=null!;
    private TextBox detalle=null!;
    private Button recuperar=null!;
    private Button descartar=null!;
    private TableLayoutPanel estructura=null!;
    protected override void Dispose(bool disposing){if(disposing)components?.Dispose();base.Dispose(disposing);}
    private void InitializeComponent()
    {
        components=new System.ComponentModel.Container();lista=new ListBox();detalle=new TextBox();recuperar=new Button();descartar=new Button();estructura=new TableLayoutPanel();
        SuspendLayout();estructura.Dock=DockStyle.Fill;estructura.Padding=new Padding(16);estructura.ColumnCount=1;estructura.RowCount=4;
        estructura.RowStyles.Add(new RowStyle(SizeType.Percent,30));estructura.RowStyles.Add(new RowStyle(SizeType.Percent,70));
        estructura.RowStyles.Add(new RowStyle(SizeType.AutoSize));estructura.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        lista.Dock=DockStyle.Fill;lista.DisplayMember="Tipo";lista.SelectedIndexChanged+=Seleccion_Cambio;
        detalle.Dock=DockStyle.Fill;detalle.Multiline=true;detalle.ReadOnly=true;detalle.ScrollBars=ScrollBars.Vertical;
        recuperar.Text="Recuperar la misma operación";recuperar.AutoSize=true;recuperar.Dock=DockStyle.Fill;recuperar.Click+=Recuperar_Click;
        descartar.Text="Descartar solo si SQL confirma que no fue aplicada";descartar.AutoSize=true;descartar.Dock=DockStyle.Fill;descartar.Click+=Descartar_Click;
        estructura.Controls.Add(lista,0,0);estructura.Controls.Add(detalle,0,1);estructura.Controls.Add(recuperar,0,2);estructura.Controls.Add(descartar,0,3);
        Controls.Add(estructura);AutoScaleDimensions=new SizeF(7F,15F);AutoScaleMode=AutoScaleMode.Font;ClientSize=new Size(760,580);MinimumSize=new Size(500,380);StartPosition=FormStartPosition.CenterParent;Text="Recuperación de operaciones locales";ResumeLayout(false);
    }
}
