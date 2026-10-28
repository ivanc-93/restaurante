namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioResolverIncidencia
{
    private Label resumen=null!,mensaje=null!;private ComboBox desenlace=null!,metodo=null!;private TextBox motivo=null!,receptor=null!,identificacion=null!,direccion=null!,correo=null!,referencia=null!;private NumericUpDown entregado=null!;private CheckBox publico=null!,confirmacion=null!;private Button resolver=null!;
    private void InitializeComponent()
    {
        var p=DisenoFormulario.Preparar(this,"Resolver incidencia");resumen=new Label{AutoSize=true,MaximumSize=new Size(600,0)};p.Controls.Add(resumen);
        desenlace=DisenoFormulario.Opciones(p,"Desenlace","Cobro posterior","Pérdida","Condonación");
        motivo=DisenoFormulario.Campo(p,"Motivo y evidencia del desenlace");
        publico=new CheckBox{Text="Comprobante a público general",Checked=true,AutoSize=true};p.Controls.Add(publico);
        receptor=DisenoFormulario.Campo(p,"Nombre del receptor identificado");identificacion=DisenoFormulario.Campo(p,"Identificación (sin validación SAT)");direccion=DisenoFormulario.Campo(p,"Dirección");correo=DisenoFormulario.Campo(p,"Correo opcional");
        metodo=DisenoFormulario.Opciones(p,"Pago manual confirmado; sin conexión bancaria","Efectivo","Tarjeta");
        p.Controls.Add(new Label{Text="Recibido en Q (tarjeta: importe exacto)",AutoSize=true});entregado=new NumericUpDown{DecimalPlaces=2,Maximum=9999999999.99m,Width=300};p.Controls.Add(entregado);
        referencia=DisenoFormulario.Campo(p,"Referencia externa si es tarjeta");
        confirmacion=new CheckBox{Text="Revisé los datos; si hay cobro, comprobé su recepción completa",AutoSize=true};p.Controls.Add(confirmacion);p.Controls.Add(new Label{AutoSize=false,Size=new Size(1,32),TabStop=false});
        void Actualizar(){var pago=desenlace.Text=="Cobro posterior";publico.Enabled=metodo.Enabled=entregado.Enabled=referencia.Enabled=pago;receptor.Enabled=identificacion.Enabled=direccion.Enabled=correo.Enabled=pago&&!publico.Checked;}
        publico.CheckedChanged+=(_,_)=>Actualizar();desenlace.SelectedIndexChanged+=(_,_)=>Actualizar();Actualizar();
        resolver=DisenoFormulario.Boton(p,"Registrar resolución",Resolver_Click);var cerrar=DisenoFormulario.Boton(p,"Cerrar",(_,_)=>Close());CancelButton=cerrar;mensaje=DisenoFormulario.Mensaje(p);
    }
}
