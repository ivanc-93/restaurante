namespace Restaurante.Frontend.Formularios;
partial class FormularioReservaDetalle
{
    private Label lblMesa = null!;
    private Label lblMensaje = null!;
    private TextBox ctTitular = null!;
    private TextBox ctMotivo = null!;
    private Button btnAtenderReserva = null!;
    private Button btnCancelarReserva = null!;
    private Button btnCerrar = null!;
    private void InitializeComponent()
    {
        SuspendLayout();
        AutoScaleDimensions = new SizeF(96, 96);
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = Componentes.Fuente;
        BackColor = Componentes.Fondo;
        Text = "Sabores | Consultar reserva";
        ClientSize = new Size(610, 470);
        MinimumSize = new Size(440, 360);
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
        lblMesa = Componentes.Etiqueta("Reserva de mesa", 20, true);
        lblMesa.Name = nameof(lblMesa);
        var lblTitular = Componentes.Etiqueta("Persona que reserva");
        lblTitular.Name = "lblTitular";
        ctTitular = new TextBox
        {
            Name = nameof(ctTitular),
            ReadOnly = true,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Height = 70,
            Dock = DockStyle.Top,
            AccessibleName = "Nombre completo de la persona que reserva",
            TabIndex = 0
        };
        lblMensaje = Componentes.Etiqueta("Consulta la reserva antes de continuar.");
        lblMensaje.Name = nameof(lblMensaje);
        var lblMotivo = Componentes.Etiqueta("Motivo (obligatorio únicamente al cancelar)");
        lblMotivo.Name = "lblMotivo";
        ctMotivo = new TextBox
        {
            Name = nameof(ctMotivo),
            Multiline = true,
            MaxLength = 500,
            Height = 80,
            Dock = DockStyle.Top,
            TabIndex = 1,
            AccessibleName = "Motivo de cancelación de reserva"
        };
        var flpAcciones = new FlowLayoutPanel
        {
            Name = "flpAcciones",
            AutoSize = true,
            Dock = DockStyle.Top,
            WrapContents = true
        };
        btnAtenderReserva = Componentes.Boton("Atender reserva");
        btnAtenderReserva.Name = nameof(btnAtenderReserva);
        btnAtenderReserva.TabIndex = 2;
        btnAtenderReserva.Click += Atender_Click;
        Iconos.Aplicar(btnAtenderReserva, "plus");
        btnCancelarReserva = Componentes.Boton("Cancelar reserva", false);
        btnCancelarReserva.Name = nameof(btnCancelarReserva);
        btnCancelarReserva.TabIndex = 3;
        btnCancelarReserva.Click += Cancelar_Click;
        btnCerrar = Componentes.Boton("Volver", false);
        btnCerrar.Name = nameof(btnCerrar);
        btnCerrar.TabIndex = 4;
        btnCerrar.DialogResult = DialogResult.Cancel;
        flpAcciones.Controls.AddRange([btnAtenderReserva, btnCancelarReserva, btnCerrar]);
        tlpContenido.Controls.Add(lblMesa);
        tlpContenido.Controls.Add(lblTitular);
        tlpContenido.Controls.Add(ctTitular);
        tlpContenido.Controls.Add(lblMensaje);
        tlpContenido.Controls.Add(lblMotivo);
        tlpContenido.Controls.Add(ctMotivo);
        tlpContenido.SizeChanged += (_, _) =>
        {
            var ancho = Math.Max(180, tlpContenido.ClientSize.Width - 50);
            lblMensaje.MaximumSize = lblMotivo.MaximumSize = new Size(ancho, 0);
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
        CancelButton = btnCerrar;
        ResumeLayout(true);
    }
}
