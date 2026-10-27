namespace Restaurante.Frontend.Formularios;
internal static class DisenoFormulario
{
    public static FlowLayoutPanel Preparar(Form form,string titulo)
    {
        form.Text=titulo;form.Size=new Size(760,780);form.MinimumSize=new Size(560,520);form.StartPosition=FormStartPosition.CenterParent;form.Font=new Font("Segoe UI",10);form.AutoScaleMode=AutoScaleMode.Dpi;form.BackColor=Color.FromArgb(248,248,245);
        var panel=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoScroll=true,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new Padding(20)};
        var pie=new FlowLayoutPanel{Dock=DockStyle.Bottom,AutoSize=true,WrapContents=true,Padding=new Padding(16,8,16,8),BackColor=Color.White};panel.Tag=pie;
        form.Controls.Add(panel);form.Controls.Add(pie);panel.Controls.Add(new Label{Text=titulo,AutoSize=true,MaximumSize=new Size(650,0),Font=new Font("Segoe UI",16,FontStyle.Bold)});
        panel.SizeChanged+=(_,_)=>{var ancho=Math.Max(220,panel.ClientSize.Width-55);foreach(Control c in panel.Controls){if(c is TextBox or ComboBox or DataGridView)c.Width=ancho;if(c is Label)c.MaximumSize=new Size(ancho,0);}};
        return panel;
    }
    public static TextBox Campo(FlowLayoutPanel panel,string etiqueta,bool secreto=false)
    {panel.Controls.Add(new Label{Text=etiqueta,AutoSize=true,Margin=new Padding(0,10,0,3)});var c=new TextBox{Width=490,UseSystemPasswordChar=secreto,MaxLength=secreto?128:250};panel.Controls.Add(c);return c;}
    public static ComboBox Opciones(FlowLayoutPanel panel,string etiqueta,params string[] opciones)
    {panel.Controls.Add(new Label{Text=etiqueta,AutoSize=true});var c=new ComboBox{Width=490,DropDownStyle=ComboBoxStyle.DropDownList};c.Items.AddRange(opciones);c.SelectedIndex=0;panel.Controls.Add(c);return c;}
    public static Button Boton(FlowLayoutPanel p,string texto,EventHandler accion)
    {var b=new Button{Text=texto,AutoSize=true,MinimumSize=new Size(150,40),Margin=new Padding(0,4,12,4),BackColor=Color.FromArgb(49,86,62),ForeColor=Color.White,FlatStyle=FlatStyle.Flat};b.Click+=accion;(p.Tag as FlowLayoutPanel??p).Controls.Add(b);return b;}
    public static Label Mensaje(FlowLayoutPanel p)
    {var l=new Label{AutoSize=true,MaximumSize=new Size(650,0),ForeColor=Color.DarkRed};var pie=p.Tag as FlowLayoutPanel??p;pie.SetFlowBreak(l,true);pie.Controls.Add(l);return l;}
}
