using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Restaurante.Frontend.Formularios;

internal static class Componentes
{
    public static readonly Color Verde = Color.FromArgb(48, 78, 59);
    public static readonly Color VerdeOscuro = Color.FromArgb(38, 67, 48);
    public static readonly Color Fondo = Color.FromArgb(246, 243, 236);
    public static readonly Color Terracota = Color.FromArgb(183, 90, 60);
    public static readonly Color Borde = Color.FromArgb(220, 223, 215);
    public static readonly Color Seleccion = Color.FromArgb(231, 238, 224);
    public static readonly Color Tinta = Color.FromArgb(40, 43, 37);
    public static readonly Color Secundario = Color.FromArgb(93, 100, 91);
    public static readonly Font Fuente = new("Segoe UI", 12f);
    public static readonly Font FuenteTitulo = new("Segoe UI", 24f, FontStyle.Bold);

    public static Label Etiqueta(string texto, int tamano = 12, bool negrita = false)
    {
        return new Label
        {
            Text = texto,
            AutoSize = true,
            ForeColor = Tinta,
            Font = new Font("Segoe UI", tamano, negrita ? FontStyle.Bold : FontStyle.Regular),
            Margin = new Padding(0, 7, 0, 7)
        };
    }

    public static TextBox Campo(bool secreto = false) => new()
    {
        Font = Fuente,
        BorderStyle = BorderStyle.FixedSingle,
        Height = 37,
        UseSystemPasswordChar = secreto,
        Dock = DockStyle.Fill,
        Margin = new Padding(0, 4, 0, 13)
    };

    public static Button Boton(string texto, bool primario = true) => new()
    {
        AutoSize = true,
        Text = texto,
        Height = 48,
        MinimumSize = new Size(0, 48),
        FlatStyle = FlatStyle.Flat,
        FlatAppearance = { BorderSize = primario ? 0 : 1, BorderColor = Color.FromArgb(220, 222, 215) },
        BackColor = primario ? Verde : Color.White,
        ForeColor = primario ? Color.White : Tinta,
        Font = new Font("Segoe UI", 12f, FontStyle.Bold),
        Cursor = Cursors.Hand,
        Margin = new Padding(0, 6, 0, 6),
        Padding = new Padding(10, 0, 10, 0)
    };

    public static Panel Tarjeta() => new()
    {
        BackColor = Color.White,
        Padding = new Padding(22),
        Margin = new Padding(0, 0, 14, 14),
        BorderStyle = BorderStyle.FixedSingle
    };

    public static void EstilizarTabla(DataGridView tabla)
    {
        tabla.BackgroundColor = Color.White;
        tabla.BorderStyle = BorderStyle.None;
        tabla.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        tabla.GridColor = Color.FromArgb(238, 239, 233);
        tabla.EnableHeadersVisualStyles = false;
        tabla.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(246, 247, 242);
        tabla.ColumnHeadersDefaultCellStyle.ForeColor = Secundario;
        tabla.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(246,247,242);
        tabla.ColumnHeadersDefaultCellStyle.SelectionForeColor = Tinta;
        tabla.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        tabla.ColumnHeadersHeight = 48;
        tabla.DefaultCellStyle.Font = Fuente;
        tabla.DefaultCellStyle.ForeColor = Tinta;
        tabla.DefaultCellStyle.Padding = new Padding(8,8,8,8);
        tabla.DefaultCellStyle.SelectionBackColor = Seleccion;
        tabla.DefaultCellStyle.SelectionForeColor = Tinta;
        tabla.RowTemplate.Height = 48;
        tabla.RowTemplate.MinimumHeight = 46;
        tabla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        tabla.ReadOnly = true;
        tabla.AllowUserToAddRows = false;
        tabla.AllowUserToDeleteRows = false;
        tabla.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        tabla.MultiSelect = false;
        tabla.RowHeadersVisible = false;
    }

    public static void EstilizarBoton(Button boton, bool primario = false)
    {
        boton.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        boton.FlatStyle = FlatStyle.Flat;
        boton.FlatAppearance.BorderColor = Borde;
        boton.BackColor = primario ? Verde : Color.White;
        boton.ForeColor = primario ? Color.White : Tinta;
        boton.MinimumSize = new Size(0, 48);
        boton.Padding = new Padding(12, 4, 12, 4);
        boton.Cursor = Cursors.Hand;
        boton.AccessibleName = boton.Text;
    }

    public static void PintarFondoSuave(Control control)
    {
        control.BackColor = Fondo;
        control.Font = Fuente;
    }
}
