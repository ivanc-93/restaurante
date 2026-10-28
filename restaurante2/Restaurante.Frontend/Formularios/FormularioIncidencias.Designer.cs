namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioIncidencias
{
    private DataGridView tabla=null!;private Button resolver=null!;private Label mensaje=null!;private ComboBox filtro=null!;
    private void InitializeComponent()
    {
        var p=DisenoFormulario.Preparar(this,"Incidencias de consumo");
        filtro=DisenoFormulario.Opciones(p,"Mostrar","Pendientes","Todas");filtro.SelectedIndexChanged+=async(_,_)=>await Cargar();
        tabla=new DataGridView{Width=650,Height=280,ReadOnly=true,AutoGenerateColumns=true,AllowUserToAddRows=false,MultiSelect=false,SelectionMode=DataGridViewSelectionMode.FullRowSelect};p.Controls.Add(tabla);Componentes.EstilizarTabla(tabla);
        resolver=DisenoFormulario.Boton(p,"Resolver incidencia seleccionada",Resolver_Click);
        DisenoFormulario.Boton(p,"Actualizar",async(_,_)=>await Cargar());
        var cerrar=DisenoFormulario.Boton(p,"Cerrar",(_,_)=>Close());CancelButton=cerrar;mensaje=DisenoFormulario.Mensaje(p);
    }
}
