using System.Globalization;

namespace Restaurante.Frontend.Formularios;

internal static class FormatoPresentacion
{
    // Restaurant in Guatemala: presentation only, no conversion of stored amounts.
    private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-GT");
    public static string Moneda(decimal importe) => Restaurante.Backend.Servicios.MonedaGuatemala.Formatear(importe);
    public static void Aplicar(Form formulario)
    {
        formulario.BackColor = Componentes.Fondo;
        formulario.Font = Componentes.Fuente;
        AplicarControles(formulario);
    }
    private static void AplicarControles(Control padre)
    {
        foreach (Control control in padre.Controls)
        {
            if (control.Font.FontFamily.Name == "Georgia")
                control.Font = new Font("Segoe UI", control.Font.Size, control.Font.Style);
            if (control is Button boton)
            {
                boton.FlatStyle = FlatStyle.Flat;
                boton.FlatAppearance.BorderColor = Color.FromArgb(210, 218, 208);
                boton.MinimumSize = new Size(0, (int)Math.Ceiling(44 * boton.DeviceDpi / 96f));
                boton.Cursor = Cursors.Hand;
            }
            if (control is DataGridView tabla)
            {
                Componentes.EstilizarTabla(tabla);
                tabla.RowHeadersVisible = false;
                tabla.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                tabla.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                tabla.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
                tabla.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                foreach (DataGridViewColumn columna in tabla.Columns)
                    if (columna.Name is "Precio" or "PrecioUnitario" or "Importe" or "Total" or "Subtotal" or "Impuesto")
                    {
                        columna.DefaultCellStyle.Format = "'Q '#,##0.00";
                        columna.DefaultCellStyle.FormatProvider = Cultura;
                        columna.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    }
                foreach (DataGridViewColumn columna in tabla.Columns)
                {
                    columna.HeaderText = columna.Name switch { "NumeroMesa" => "Mesa", "NumeroFactura" => "Factura", "PrecioUnitario" => "Precio unitario", _ => columna.HeaderText };
                    columna.MinimumWidth = 65;
                    if (columna.Name == "Producto") columna.FillWeight = 200;
                    if (columna.Name is "Tipo" or "PrecioUnitario" && tabla.Columns.Contains("Producto")) columna.Visible = false;
                    }
            }
            AplicarControles(control);
        }
    }
}
