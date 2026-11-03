namespace Restaurante.Frontend.Formularios;
partial class FormularioReservarMesa
{
    private TextBox ctNombreReserva = null!;
    private Label lblMesa = null!;
    private Label lblMensaje = null!;
    private CheckBox chkConfirmar = null!;
    private Button btnReservarMesa = null!;
    private Button btnCancelar = null!;
    private void InitializeComponent()
    {
        SuspendLayout();
        AutoScaleDimensions = new SizeF(96, 96);
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = Componentes.Fuente;
        BackColor = Componentes.Fondo;
        Text = "Sabores | Reservar mesa";
        ClientSize = new Size(530, 370);
        MinimumSize = new Size(400, 300);
        StartPosition = FormStartPosition.CenterParent;
        var tlpContenido = new TableLayoutPanel
        {
            Name = "tlpContenido",
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            AutoScroll = true,
            ColumnCount = 1,
            RowCount = 7
        };
        tlpContenido.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (var fila = 0; fila < 7; fila++)
            tlpContenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var lblTitulo = Componentes.Etiqueta("Reservar mesa", 20, true);
        lblTitulo.Name = "lblTitulo";
        lblMesa = Componentes.Etiqueta("Mesa seleccionada");
        lblMesa.Name = nameof(lblMesa);
        var lblNombre = Componentes.Etiqueta("Nombre de la persona que reserva");
        lblNombre.Name = "lblNombre";
        ctNombreReserva = Componentes.Campo();
        ctNombreReserva.Name = nameof(ctNombreReserva);
        ctNombreReserva.MaxLength = 120;
        ctNombreReserva.TabIndex = 0;
        ctNombreReserva.AccessibleName = "Nombre de la persona que reserva";
        chkConfirmar = new CheckBox
        {
            Name = nameof(chkConfirmar),
            Text = "Confirmo esta reserva",
            AutoSize = true,
            TabIndex = 1
        };
        lblMensaje = Componentes.Etiqueta("Reserva inmediata. No registra consumo ni pago.");
        lblMensaje.Name = nameof(lblMensaje);
        lblMensaje.ForeColor = Componentes.Secundario;
        var flpAcciones = new FlowLayoutPanel
        {
            Name = "flpAcciones",
            AutoSize = true,
            Dock = DockStyle.Top,
            WrapContents = true
        };
        btnReservarMesa = Componentes.Boton("Confirmar reserva");
        btnReservarMesa.Name = nameof(btnReservarMesa);
        btnReservarMesa.TabIndex = 2;
        Iconos.Aplicar(btnReservarMesa, "calendar");
        btnReservarMesa.Click += Reservar_Click;
        btnCancelar = Componentes.Boton("Volver", false);
        btnCancelar.Name = nameof(btnCancelar);
        btnCancelar.DialogResult = DialogResult.Cancel;
        btnCancelar.TabIndex = 3;
        flpAcciones.Controls.AddRange([btnReservarMesa, btnCancelar]);
        tlpContenido.Controls.Add(lblTitulo);
        tlpContenido.Controls.Add(lblMesa);
        tlpContenido.Controls.Add(lblNombre);
        tlpContenido.Controls.Add(ctNombreReserva);
        tlpContenido.Controls.Add(chkConfirmar);
        tlpContenido.Controls.Add(lblMensaje);
        tlpContenido.SizeChanged += (_, _) =>
        {
            var ancho = Math.Max(150, tlpContenido.ClientSize.Width - 50);
            lblMensaje.MaximumSize = chkConfirmar.MaximumSize = new Size(ancho, 0);
        };
        flpAcciones.Padding = new Padding(20, 0, 20, 12);
        var tlpEstructura = new TableLayoutPanel
        {
            Name = "tlpEstructura",
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        tlpEstructura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        tlpEstructura.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        tlpEstructura.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tlpEstructura.Controls.Add(tlpContenido, 0, 0);
        tlpEstructura.Controls.Add(flpAcciones, 0, 1);
        Controls.Add(tlpEstructura);
        AcceptButton = btnReservarMesa;
        CancelButton = btnCancelar;
        ResumeLayout(true);
    }
}
