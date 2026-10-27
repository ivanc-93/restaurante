#nullable enable
namespace Restaurante.Frontend.Formularios;
partial class FormularioComprobante
{
    private System.ComponentModel.IContainer? components;
    private TextBox contenido=null!;
    protected override void Dispose(bool disposing){if(disposing)components?.Dispose();base.Dispose(disposing);}
    private void InitializeComponent()
    {
        components=new System.ComponentModel.Container();contenido=new TextBox();SuspendLayout();
        contenido.Dock=DockStyle.Fill;contenido.Multiline=true;contenido.ReadOnly=true;contenido.ScrollBars=ScrollBars.Both;
        contenido.BackColor=Color.White;contenido.Font=new Font("Segoe UI",11);contenido.AccessibleName="Comprobante interno académico";
        Controls.Add(contenido);AutoScaleDimensions=new SizeF(7F,15F);AutoScaleMode=AutoScaleMode.Font;ClientSize=new Size(720,650);
        MinimumSize=new Size(450,350);StartPosition=FormStartPosition.CenterParent;Text="Consultar comprobante interno";ResumeLayout(false);
    }
}
