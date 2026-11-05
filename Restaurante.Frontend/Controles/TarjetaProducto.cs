using Restaurante.Backend.Dominio;

namespace Restaurante.Frontend.Controles;
internal sealed class TarjetaProducto : UserControl
{
    private readonly Label nombre, precio, categoria;
    private readonly ToolTip ayuda = new();
    private readonly PictureBox picIlustracion;
    private readonly ElementoMenu producto;
    private Bitmap? ilustracion;
    internal event EventHandler? Seleccionar;
    internal TarjetaProducto(ElementoMenu producto)
    {
        this.producto = producto;
        Name = "producto_" + producto.Id;
        AutoScaleMode = AutoScaleMode.None;
        BackColor = Color.White;
        BorderStyle = BorderStyle.FixedSingle;
        Cursor = Cursors.Hand;
        var p = new TableLayoutPanel
        {
            Name = "tlpProducto",
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 1,
            RowCount = 4
        };
        p.ColumnStyles.Add(new(SizeType.Percent, 100));
        p.RowStyles.Add(new(SizeType.Percent, 52));
        p.RowStyles.Add(new(SizeType.Percent, 48));
        p.RowStyles.Add(new(SizeType.AutoSize));
        p.RowStyles.Add(new(SizeType.AutoSize));
        nombre = new()
        {
            Name = "lblNombreProducto",
            Text = producto.Nombre,
            Dock = DockStyle.Fill,
            AutoEllipsis = true,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Componentes.Tinta
        };
        categoria = new()
        {
            Name = "lblCategoriaProducto",
            Text = producto.Categoria,
            Dock = DockStyle.Top,
            AutoEllipsis = true,
            Font = new Font("Segoe UI", 9),
            ForeColor = Componentes.Secundario,
            Height = 24
        };
        precio = new()
        {
            Name = "lblPrecioProducto",
            Text = MonedaGuatemala.Formatear(producto.Precio),
            AutoSize = true,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = Componentes.Verde
        };
        picIlustracion = new PictureBox
        {
            Name = "picIlustracionProducto",
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            TabStop = false,
            AccessibleName = "Ilustración orientativa de categoría; no es fotografía del producto",
            BackColor = Color.FromArgb(249, 247, 241)
        };
        picIlustracion.SizeChanged += (_, _) => DibujarIlustracion();
        p.Controls.Add(picIlustracion, 0, 0);
        p.Controls.Add(nombre, 0, 1);
        p.Controls.Add(categoria, 0, 2);
        p.Controls.Add(precio, 0, 3);
        Controls.Add(p);
        AccessibleName = $"{producto.Nombre}, {precio.Text}. Seleccionar producto";
        TabStop = true;
        foreach (Control c in new Control[]
        {
            this,
            p,
            nombre,
            categoria,
            precio,
            picIlustracion
        }

        )
        {
            c.Click += (_, _) => Seleccionar?.Invoke(this, EventArgs.Empty);
            ayuda.SetToolTip(c, AccessibleName);
        }

        KeyDown += (_, e) =>
        {
            if (e.KeyCode is Keys.Enter or Keys.Space)
            {
                Seleccionar?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
            }
        };
        Disposed += (_, _) =>
        {
            ayuda.Dispose();
            ilustracion?.Dispose();
        };
    }

    private void DibujarIlustracion()
    {
        if (picIlustracion.Width <= 0 || picIlustracion.Height <= 0 || ilustracion?.Size == picIlustracion.ClientSize)
            return;
        var nueva = new Bitmap(picIlustracion.Width, picIlustracion.Height);
        using (var grafico = Graphics.FromImage(nueva))
            IlustracionesSabores.Producto(grafico, new Rectangle(Point.Empty, nueva.Size), producto.Tipo, producto.Nombre);
        var anterior = ilustracion;
        picIlustracion.Image = ilustracion = nueva;
        anterior?.Dispose();
    }

    internal void Marcar(bool seleccionada)
    {
        BackColor = seleccionada ? Color.FromArgb(225, 239, 228) : Color.White;
        nombre.BackColor = precio.BackColor = categoria.BackColor = BackColor;
    }
}
