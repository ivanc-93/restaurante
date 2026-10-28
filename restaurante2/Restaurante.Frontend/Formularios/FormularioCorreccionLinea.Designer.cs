namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioCorreccionLinea
{
    private ComboBox lineas=null!;private NumericUpDown retirar=null!;private TextBox motivo=null!;private CheckBox declaracion=null!;private Button aplicar=null!;private Label mensaje=null!;
    private void InitializeComponent()
    {
        var p=DisenoFormulario.Preparar(this,"Corregir captura de comanda");
        p.Controls.Add(new Label{Text="Selecciona una línea. La corrección requiere Gerente/Administrador y no elimina la captura original.",AutoSize=true,MaximumSize=new Size(600,0)});
        lineas=DisenoFormulario.Opciones(p,"Producto");lineas.DisplayMember="Producto";lineas.FormattingEnabled=true;lineas.Format+=(_,e)=>{if(e.ListItem is Restaurante.Backend.Dominio.LineaPedidoCorregible l)e.Value=$"{l.Producto} · {l.CantidadVigente} vigentes de {l.CantidadOriginal} capturadas";};
        p.Controls.Add(new Label{Text="Unidades a retirar",AutoSize=true});retirar=new NumericUpDown{Minimum=1,Maximum=50,Value=1};p.Controls.Add(retirar);
        declaracion=new CheckBox{Text="Confirmo que no hubo preparación ni consumo de estas unidades",AutoSize=true};p.Controls.Add(declaracion);
        motivo=DisenoFormulario.Campo(p,"Motivo de la corrección");
        aplicar=DisenoFormulario.Boton(p,"Solicitar autorización y corregir",Aplicar_Click);
        var cerrar=DisenoFormulario.Boton(p,"Cerrar",(_,_)=>Close());CancelButton=cerrar;
        mensaje=DisenoFormulario.Mensaje(p);
    }
}
