using Restaurante.Backend.Dominio;

namespace Restaurante.Frontend.Formularios;
internal sealed class TarjetaMesa : UserControl
{
    internal Mesa Mesa { get; }
    internal Button btnAccionMesa { get; }

    internal TarjetaMesa(Mesa mesa, PedidoResumen? pedido)
    {
        Mesa = mesa;
        Name = "mesa_" + mesa.Id;
        AccessibleName = $"Mesa {mesa.Numero}, {mesa.Estado}, capacidad {mesa.Capacidad}";
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoScaleDimensions = new SizeF(96, 96);
        BackColor = Color.White;
        Font = Componentes.Fuente;
        Size = new Size(270, 270);
        Margin = new Padding(0, 0, 16, 16);
        BorderStyle = BorderStyle.FixedSingle;
        var estilo = EstadoVisualMesa.De(mesa.Estado);
        var tlpCuerpo = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(14),
            Name = "tlpCuerpo"
        };
        tlpCuerpo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        tlpCuerpo.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        tlpCuerpo.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
        tlpCuerpo.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        tlpCuerpo.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        tlpCuerpo.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        var lblEstado = new Label
        {
            Text = "  " + mesa.Estado,
            Dock = DockStyle.Fill,
            BackColor = estilo.Fondo,
            ForeColor = estilo.Tinta,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Image = Iconos.Obtener(estilo.Icono),
            ImageAlign = ContentAlignment.MiddleRight,
            Padding = new Padding(4),
            Name = "lblEstado"
        };
        var lblNumero = new Label
        {
            Text = "Mesa " + mesa.Numero,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 25, FontStyle.Bold),
            ForeColor = Componentes.Tinta,
            Name = "lblNumero"
        };
        var lblCapacidad = new Label
        {
            Text = $"Capacidad: {mesa.Capacidad} personas",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Name = "lblCapacidad"
        };
        var lblCuenta = new Label
        {
            Text = pedido is null ? "Sin pedido abierto" : $"Pedido #{pedido.Id} · {FormatoPresentacion.Moneda(pedido.Total)}",
            Dock = DockStyle.Fill,
            ForeColor = Componentes.Secundario,
            AutoEllipsis = true,
            Name = "lblCuenta"
        };
        btnAccionMesa = Componentes.Boton(mesa.Estado == "Ocupada" && pedido is null ? "Gestionar estado" : estilo.Accion);
        btnAccionMesa.Name = "btnAccionMesa";
        btnAccionMesa.Name = "accionMesa";
        btnAccionMesa.Dock = DockStyle.Fill;
        btnAccionMesa.Margin = new Padding(0, 4, 0, 0);
        btnAccionMesa.BackColor = estilo.Tinta;
        btnAccionMesa.AccessibleName = $"{btnAccionMesa.Text}, mesa {mesa.Numero}";
        Iconos.Aplicar(btnAccionMesa, estilo.Icono);
        tlpCuerpo.Controls.Add(lblEstado, 0, 0);
        tlpCuerpo.Controls.Add(lblNumero, 0, 1);
        tlpCuerpo.Controls.Add(lblCapacidad, 0, 2);
        tlpCuerpo.Controls.Add(lblCuenta, 0, 3);
        tlpCuerpo.Controls.Add(btnAccionMesa, 0, 4);
        Controls.Add(tlpCuerpo);
    }
}
