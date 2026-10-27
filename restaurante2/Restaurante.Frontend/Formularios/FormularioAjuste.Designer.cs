namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioAjuste
{
    private Label resumen=null!,mensaje=null!;private ComboBox tipo=null!;private NumericUpDown descuento=null!;private TextBox motivo=null!;private Button aplicar=null!;
    private void InitializeComponent()
    {
        var p=DisenoFormulario.Preparar(this,"Descuento o cortesía antes del cobro");resumen=new Label{AutoSize=true,MaximumSize=new Size(500,0)};p.Controls.Add(resumen);
        tipo=DisenoFormulario.Opciones(p,"Tipo de ajuste","Descuento","Cortesía");p.Controls.Add(new Label{Text="Descuento en Q sobre el importe original",AutoSize=true});descuento=new NumericUpDown{DecimalPlaces=2,Maximum=9999999999.99m,Width=250};p.Controls.Add(descuento);motivo=DisenoFormulario.Campo(p,"Motivo obligatorio");
        aplicar=DisenoFormulario.Boton(p,"Solicitar y aplicar ajuste",Aplicar_Click);mensaje=DisenoFormulario.Mensaje(p);tipo.SelectedIndexChanged+=(_,_)=>{descuento.Enabled=tipo.Text!="Cortesía";};
    }
}
