using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Restaurante.Frontend.Formularios;

internal static class Componentes
{
    public static readonly Color Verde = Color.FromArgb(49, 86, 62);
    public static readonly Color VerdeOscuro = Color.FromArgb(38, 67, 48);
    public static readonly Color Fondo = Color.FromArgb(247, 247, 243);
    public static readonly Color Tinta = Color.FromArgb(40, 43, 37);
    public static readonly Color Secundario = Color.FromArgb(93, 100, 91);
    public static readonly Font Fuente = new("Segoe UI", 10f);
    public static readonly Font FuenteTitulo = new("Segoe UI", 24f, FontStyle.Bold);

    public static Label Etiqueta(string texto, int tamano = 10, bool negrita = false)
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
        Height = 44,
        FlatStyle = FlatStyle.Flat,
        FlatAppearance = { BorderSize = primario ? 0 : 1, BorderColor = Color.FromArgb(220, 222, 215) },
        BackColor = primario ? Verde : Color.White,
        ForeColor = primario ? Color.White : Tinta,
        Font = new Font("Segoe UI", 10f, FontStyle.Bold),
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
        tabla.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        tabla.ColumnHeadersHeight = 42;
        tabla.DefaultCellStyle.Font = Fuente;
        tabla.DefaultCellStyle.ForeColor = Tinta;
        tabla.DefaultCellStyle.SelectionBackColor = Color.FromArgb(232, 239, 229);
        tabla.DefaultCellStyle.SelectionForeColor = Tinta;
        tabla.RowTemplate.Height = 38;
        tabla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        tabla.ReadOnly = true;
        tabla.AllowUserToAddRows = false;
        tabla.AllowUserToDeleteRows = false;
        tabla.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        tabla.MultiSelect = false;
    }

    public static void PintarFondoSuave(Control control)
    {
        control.BackColor = Fondo;
        control.Font = Fuente;
    }
}
