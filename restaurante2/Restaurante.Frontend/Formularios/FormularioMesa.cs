using Restaurante.Backend.Dominio;

namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioMesa : Form
{
    public int NumeroMesa => (int)nudNumero.Value;
    public int Capacidad => (int)nudCapacidad.Value;
    public string EstadoMesa => cbEstado.Text;

    public FormularioMesa() => InitializeComponent();
    public FormularioMesa(Mesa? mesa = null, bool soloLectura = false, int siguienteNumero = 1) : this()
    {
        Text = soloLectura ? "Detalle de mesa" : mesa is null ? "Nueva mesa" : "Editar mesa";
        nudNumero.Value = Math.Max(mesa?.Numero ?? siguienteNumero, 1);
        nudCapacidad.Value = Math.Clamp(mesa?.Capacidad ?? 4, 1, 100);
        cbEstado.SelectedItem = mesa?.Estado ?? "Disponible";
        nudNumero.Enabled = nudCapacidad.Enabled = !soloLectura;
        cbEstado.Enabled = mesa is not null && !soloLectura && mesa.Estado == "Disponible";
        if (mesa?.Estado != "Reservada")
            cbEstado.Items.Remove("Reservada");
        if (mesa?.Estado == "Reservada")
            nudNumero.Enabled = false;
        btnAceptar.Text = soloLectura ? "Cerrar" : "Guardar";
    }
}
