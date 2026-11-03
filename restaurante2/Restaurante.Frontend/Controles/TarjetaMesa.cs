using Restaurante.Backend.Dominio;

namespace Restaurante.Frontend.Controles;
/// <summary>Presenta una mesa; las decisiones y escrituras se realizan al pulsar su acción.</summary>
internal sealed class TarjetaMesa : UserControl
{
    private readonly TableLayoutPanel tlpCuerpo;
    private readonly Label lblEstado;
    private readonly PictureBox picMesa;
    private readonly EstadoVisualMesa estilo;
    internal Mesa Mesa { get; }
    internal Button btnAccionMesa { get; }
    internal Button btnReservarMesa { get; }

    private readonly Label lblNombreReserva;
    private readonly ToolTip ayudaReserva = new();
    internal TarjetaMesa(Mesa mesa, PedidoResumen? pedido, ReservaResumen? reserva = null, bool reservasHabilitadas = false, bool operacionesPendientes = false)
    {
        Mesa = mesa;
        Name = "mesa_" + mesa.Id;
        AccessibleName = $"Mesa {mesa.Numero}, {mesa.Estado}, capacidad {mesa.Capacidad}";
        // Una sola escala de medidas; las filas se adaptan a la altura del texto.
        AutoScaleMode = AutoScaleMode.None;
        BackColor = Color.White;
        Font = Componentes.Fuente;
        BorderStyle = BorderStyle.FixedSingle;
        estilo = EstadoVisualMesa.De(mesa.Estado);
        tlpCuerpo = new TableLayoutPanel
        {
            Name = nameof(tlpCuerpo),
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 7
        };
        tlpCuerpo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        tlpCuerpo.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        for (var fila = 0; fila < 7; fila++)
            tlpCuerpo.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        lblEstado = new Label
        {
            Name = nameof(lblEstado),
            Text = mesa.Estado,
            AutoSize = true,
            Dock = DockStyle.Fill,
            BackColor = estilo.Fondo,
            ForeColor = estilo.Tinta,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ImageAlign = ContentAlignment.MiddleRight
        };
        var lblNumero = new Label
        {
            Name = "lblNumero",
            Text = $"Mesa {mesa.Numero}",
            AutoSize = true,
            Font = new Font("Segoe UI", 21, FontStyle.Bold),
            ForeColor = Componentes.Tinta
        };
        var lblCapacidad = new Label
        {
            Name = "lblCapacidad",
            Text = $"Capacidad: {mesa.Capacidad} personas",
            AutoSize = true
        };
        var lblCuenta = new Label
        {
            Name = "lblCuenta",
            AutoSize = true,
            ForeColor = Componentes.Secundario,
            Text = pedido is null ? "Sin pedido abierto" : $"Pedido #{pedido.Id} · {FormatoPresentacion.Moneda(pedido.Total)}"};
        picMesa = new PictureBox
        {
            Name = nameof(picMesa),
            SizeMode = PictureBoxSizeMode.Zoom,
            TabStop = false,
            Anchor = AnchorStyles.Right | AnchorStyles.Top,
            AccessibleName = "Mesa del restaurante"
        };
        lblNombreReserva = new Label
        {
            Name = nameof(lblNombreReserva),
            AutoSize = false,
            AutoEllipsis = true,
            Dock = DockStyle.Fill,
            Visible = mesa.Estado == "Reservada",
            Text = mesa.Estado == "Reservada" ? reserva is null ? "Reserva anterior: titular pendiente de revisión" : "Reserva: " + reserva.Nombre : "",
            ForeColor = estilo.Tinta,
            TextAlign = ContentAlignment.MiddleLeft
        };
        ayudaReserva.SetToolTip(lblNombreReserva, reserva?.Nombre ?? "Reserva anterior sin titular registrado; solicita revisión.");
        btnReservarMesa = Componentes.Boton("Reservar mesa", false);
        btnReservarMesa.Name = nameof(btnReservarMesa);
        btnReservarMesa.Dock = DockStyle.Fill;
        btnReservarMesa.Visible = mesa.Estado == "Disponible";
        btnReservarMesa.Enabled = reservasHabilitadas && !operacionesPendientes;
        btnReservarMesa.AccessibleName = $"Reservar mesa {mesa.Numero}";
        ayudaReserva.SetToolTip(btnReservarMesa, operacionesPendientes ? "Recupera las operaciones pendientes desde Inicio antes de registrar otra reserva." : reservasHabilitadas ? "Apartar esta mesa para una persona" : "Requiere actualización 008 autorizada; la conexión actual se conserva.");
        btnAccionMesa = Componentes.Boton(mesa.Estado == "Reservada" ? "Ver reserva" : mesa.Estado == "Ocupada" && pedido is null ? "Gestionar estado" : estilo.Accion);
        btnAccionMesa.Name = nameof(btnAccionMesa);
        btnAccionMesa.Dock = DockStyle.Fill;
        btnAccionMesa.BackColor = estilo.Tinta;
        btnAccionMesa.AccessibleName = $"{btnAccionMesa.Text}, mesa {mesa.Numero}";
        tlpCuerpo.Controls.Add(lblEstado, 0, 0);
        tlpCuerpo.SetColumnSpan(lblEstado, 2);
        tlpCuerpo.Controls.Add(lblNumero, 0, 1);
        tlpCuerpo.Controls.Add(lblCapacidad, 0, 2);
        tlpCuerpo.Controls.Add(lblCuenta, 0, 3);
        tlpCuerpo.Controls.Add(picMesa, 1, 1);
        tlpCuerpo.SetRowSpan(picMesa, 3);
        tlpCuerpo.Controls.Add(lblNombreReserva, 0, 4);
        tlpCuerpo.SetColumnSpan(lblNombreReserva, 2);
        tlpCuerpo.Controls.Add(btnAccionMesa, 0, 5);
        tlpCuerpo.SetColumnSpan(btnAccionMesa, 2);
        tlpCuerpo.Controls.Add(btnReservarMesa, 0, 6);
        tlpCuerpo.SetColumnSpan(btnReservarMesa, 2);
        Controls.Add(tlpCuerpo);
        Disposed += (_, _) => ayudaReserva.Dispose();
        AjustarEscala();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        AjustarEscala();
    }

    protected override void OnDpiChangedAfterParent(EventArgs e)
    {
        base.OnDpiChangedAfterParent(e);
        AjustarEscala();
    }

    private void AjustarEscala()
    {
        if (tlpCuerpo is null)
            return;
        var escala = DeviceDpi / 96f;
        int Px(int valor) => (int)Math.Ceiling(valor * escala);
        tlpCuerpo.Padding = new Padding(Px(12));
        Margin = new Padding(0, 0, Px(14), Px(14));
        lblEstado.Padding = new Padding(Px(6));
        lblEstado.Margin = new Padding(0, 0, 0, Px(5));
        lblEstado.Image = Iconos.Obtener(estilo.Icono, DeviceDpi);
        picMesa.Size = new Size(Px(48), Px(48));
        // La ilustración central usa el PNG de 48 px, no amplía el de 24 px en DPI base.
        picMesa.Image = Iconos.Obtener("armchair", Math.Max(192, DeviceDpi));
        lblNombreReserva.Height = Px(38);
        lblNombreReserva.MinimumSize = new Size(0, Px(38));
        btnReservarMesa.MinimumSize = new Size(0, Px(42));
        Iconos.Aplicar(btnReservarMesa, "calendar");
        btnAccionMesa.MinimumSize = new Size(0, Px(44));
        btnAccionMesa.Margin = new Padding(0, Px(8), 0, 0);
        Iconos.Aplicar(btnAccionMesa, estilo.Icono);
    }

    internal int ObtenerAlturaPreferida(int ancho) => Math.Max((int)Math.Ceiling(205 * DeviceDpi / 96f), tlpCuerpo.GetPreferredSize(new Size(ancho - 2, 0)).Height + 2);
}
