#nullable enable
namespace Restaurante.Frontend.Formularios;
partial class FormularioAcceso
{
    private System.ComponentModel.IContainer? components;
    private TextBox _usuario=null!,_contrasena=null!;
    private Label _mensaje=null!,titulo=null!,usuarioEtiqueta=null!,claveEtiqueta=null!,ayuda=null!;
    private Button entrar=null!,conexion=null!;
    private CheckBox mostrarClave=null!;
    private TableLayoutPanel estructura=null!;
    protected override void Dispose(bool disposing){if(disposing)components?.Dispose();base.Dispose(disposing);}
    private void InitializeComponent()
    {
        components=new System.ComponentModel.Container();
        SuspendLayout();
        AutoScaleDimensions=new SizeF(96F,96F);AutoScaleMode=AutoScaleMode.Dpi;
        Font=new Font("Segoe UI",10F);BackColor=Color.FromArgb(246,247,243);
        Text="Sabores | Acceso";ClientSize=new Size(480,610);MinimumSize=new Size(360,360);
        StartPosition=FormStartPosition.CenterScreen;
        estructura=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=6,Padding=new Padding(24,12,24,0),AutoScroll=true};
        estructura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        for(var i=0;i<6;i++)estructura.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        ayuda=new Label{Text="SABORES  ·  Gestión de restaurante",AutoSize=true,ForeColor=Color.FromArgb(49,86,62),Margin=new Padding(0,0,0,6)};
        titulo=new Label{Text="Iniciar sesión",AutoSize=true,Font=new Font("Segoe UI",18F,FontStyle.Bold),Margin=new Padding(0,0,0,4)};
        usuarioEtiqueta=new Label{Text="Usuario",AutoSize=true,Margin=new Padding(0,0,0,6)};
        _usuario=new TextBox{Dock=DockStyle.Top,MaxLength=50,TabIndex=0,AccessibleName="Usuario de Sabores",Margin=new Padding(0,0,0,6)};
        claveEtiqueta=new Label{Text="Contraseña",AutoSize=true,Margin=new Padding(0,0,0,6)};
        _contrasena=new TextBox{Dock=DockStyle.Top,MaxLength=128,UseSystemPasswordChar=true,TabIndex=1,AccessibleName="Contraseña de Sabores",Margin=new Padding(0,0,0,4)};
        mostrarClave=new CheckBox{Text="Mostrar contraseña",AutoSize=true,TabIndex=2,Margin=new Padding(0,0,0,4)};
        mostrarClave.CheckedChanged+=(_,_)=>_contrasena.UseSystemPasswordChar=!mostrarClave.Checked;
        _mensaje=new Label{Text="Usa tu cuenta individual para continuar.",AutoSize=true,MinimumSize=new Size(0,24),ForeColor=Color.FromArgb(92,103,98),Margin=new Padding(0,4,0,10)};
        entrar=new Button{Text="Entrar al sistema",AutoSize=true,Dock=DockStyle.Top,MinimumSize=new Size(0,36),BackColor=Color.FromArgb(49,86,62),ForeColor=Color.White,FlatStyle=FlatStyle.Flat,TabIndex=3,Margin=new Padding(0,0,0,6)};
        entrar.Click+=Entrar_Click;
        conexion=new Button{Text="Configurar o recuperar conexión",AutoSize=true,Dock=DockStyle.Top,MinimumSize=new Size(0,36),BackColor=Color.White,ForeColor=Color.FromArgb(49,86,62),FlatStyle=FlatStyle.Flat,TabIndex=4,Margin=new Padding(0,0,0,4)};
        conexion.Click+=Conexion_Click;
        var cabeceraClave=new TableLayoutPanel{Dock=DockStyle.Top,AutoSize=true,ColumnCount=2,RowCount=1,Margin=new Padding(0,0,0,4),TabIndex=2};
        cabeceraClave.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));cabeceraClave.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        cabeceraClave.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mostrarClave.Margin=new Padding(0);claveEtiqueta.Margin=new Padding(0);
        cabeceraClave.Controls.Add(claveEtiqueta,0,0);cabeceraClave.Controls.Add(mostrarClave,1,0);
        Control[] controles=[ayuda,titulo,usuarioEtiqueta,_usuario,cabeceraClave,_contrasena];
        for(var i=0;i<controles.Length;i++)estructura.Controls.Add(controles[i],0,i);
        var pie=new TableLayoutPanel{Dock=DockStyle.Fill,AutoSize=true,ColumnCount=1,RowCount=3,Padding=new Padding(24,8,24,12)};
        pie.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        pie.Controls.Add(_mensaje,0,0);pie.Controls.Add(entrar,0,1);pie.Controls.Add(conexion,0,2);
        pie.SizeChanged+=(_,_)=>_mensaje.MaximumSize=new Size(Math.Max(180,pie.ClientSize.Width-pie.Padding.Horizontal),0);
        var contenedor=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=2};
        contenedor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));contenedor.RowStyles.Add(new RowStyle(SizeType.Percent,100));contenedor.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contenedor.Controls.Add(estructura,0,0);contenedor.Controls.Add(pie,0,1);
        estructura.SizeChanged+=(_,_)=>{var ancho=Math.Max(180,estructura.ClientSize.Width-estructura.Padding.Horizontal-SystemInformation.VerticalScrollBarWidth);foreach(var label in estructura.Controls.OfType<Label>())label.MaximumSize=new Size(ancho,0);};
        Controls.Add(contenedor);AcceptButton=entrar;
        ResumeLayout(true);
    }
}
