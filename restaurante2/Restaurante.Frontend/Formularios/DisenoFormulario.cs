namespace Restaurante.Frontend.Formularios;
internal static class DisenoFormulario
{
    public static void EnInterfaz(Form form,Action accion)
    {
        if(form.IsDisposed||!form.IsHandleCreated)return;
        if(!form.InvokeRequired){accion();return;}
        try{form.BeginInvoke((Action)(()=>{if(!form.IsDisposed)accion();}));}
        catch(InvalidOperationException)when(form.IsDisposed||!form.IsHandleCreated){/* La ventana ya terminó; no se actualizan controles destruidos. */}
    }
    public static FlowLayoutPanel Preparar(Form form,string titulo)
    {
        form.Text=titulo;form.AutoScaleMode=AutoScaleMode.Dpi;form.AutoScaleDimensions=new SizeF(96,96);form.Font=new Font("Segoe UI",10);form.Size=new Size(760,780);form.MinimumSize=new Size(560,520);form.StartPosition=FormStartPosition.CenterParent;form.BackColor=Color.FromArgb(248,248,245);
        form.Shown+=(_,_)=>{
            var area=Screen.FromControl(form).WorkingArea;
            form.MinimumSize=new Size(Math.Min(form.MinimumSize.Width,area.Width),Math.Min(form.MinimumSize.Height,area.Height));
            form.Size=new Size(Math.Min(form.Width,area.Width),Math.Min(form.Height,area.Height));
            form.Location=new Point(Math.Clamp(form.Left,area.Left,Math.Max(area.Left,area.Right-form.Width)),Math.Clamp(form.Top,area.Top,Math.Max(area.Top,area.Bottom-form.Height)));
        };
        var panel=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoScroll=true,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new Padding(20)};
        var pie=new FlowLayoutPanel{Dock=DockStyle.Bottom,AutoSize=true,WrapContents=true,Padding=new Padding(16,8,16,8),BackColor=Color.White};panel.Tag=pie;
        form.Controls.Add(panel);form.Controls.Add(pie);panel.Controls.Add(new Label{Text=titulo,AutoSize=true,MaximumSize=new Size(650,0),Font=new Font("Segoe UI",16,FontStyle.Bold)});
        void AjustarAnchos()
        {
            var ancho=Math.Max(160,panel.ClientSize.Width-panel.Padding.Horizontal-SystemInformation.VerticalScrollBarWidth-8);
            foreach(Control c in panel.Controls)
            {
                if(c is TextBox or ComboBox or DataGridView)c.Width=ancho;
                if(c is Label)c.MaximumSize=new Size(ancho,0);
                if(c is CheckBox cb)
                {
                    cb.AutoSize=false;cb.MaximumSize=Size.Empty;cb.Width=ancho;
                    cb.Height=Math.Max(32,TextRenderer.MeasureText(cb.Text,cb.Font,new Size(Math.Max(100,ancho-28),int.MaxValue),TextFormatFlags.WordBreak).Height+12);
                }
            }
            var anchoPie=Math.Max(160,pie.ClientSize.Width-pie.Padding.Horizontal-8);
            foreach(Control c in pie.Controls)if(c is Label)c.MaximumSize=new Size(anchoPie,0);
        }
        panel.SizeChanged+=(_,_)=>AjustarAnchos();panel.ControlAdded+=(_,_)=>AjustarAnchos();
        pie.SizeChanged+=(_,_)=>AjustarAnchos();form.Shown+=(_,_)=>AjustarAnchos();
        return panel;
    }
    public static TextBox Campo(FlowLayoutPanel panel,string etiqueta,bool secreto=false)
    {panel.Controls.Add(new Label{Text=etiqueta,AutoSize=true,Margin=new Padding(0,10,0,3)});var c=new TextBox{Width=490,UseSystemPasswordChar=secreto,MaxLength=secreto?128:250};panel.Controls.Add(c);return c;}
    public static ComboBox Opciones(FlowLayoutPanel panel,string etiqueta,params string[] opciones)
    {panel.Controls.Add(new Label{Text=etiqueta,AutoSize=true});var c=new ComboBox{Width=490,DropDownStyle=ComboBoxStyle.DropDownList};c.Items.AddRange(opciones);if(c.Items.Count>0)c.SelectedIndex=0;panel.Controls.Add(c);return c;}
    public static Button Boton(FlowLayoutPanel p,string texto,EventHandler accion)
    {var b=new Button{Text=texto,AutoSize=true,MinimumSize=new Size(150,40),Margin=new Padding(0,4,12,4),BackColor=Color.FromArgb(49,86,62),ForeColor=Color.White,FlatStyle=FlatStyle.Flat};b.Click+=accion;(p.Tag as FlowLayoutPanel??p).Controls.Add(b);return b;}
    public static Label Mensaje(FlowLayoutPanel p)
    {var l=new Label{AutoSize=true,MaximumSize=new Size(650,0),ForeColor=Color.DarkRed};var pie=p.Tag as FlowLayoutPanel??p;pie.SetFlowBreak(l,true);pie.Controls.Add(l);return l;}
}
