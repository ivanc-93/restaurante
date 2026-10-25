using Restaurante.Backend.Dominio;
namespace Restaurante.Frontend.Formularios;

public sealed partial class FormularioMesa : Form
{
    public int NumeroMesa => (int)_numero.Value;
    public int Capacidad => (int)_capacidad.Value;
    public string EstadoMesa => _estado.Text;
    public FormularioMesa() => InitializeComponent();
    public FormularioMesa(Mesa? mesa = null, bool soloLectura = false, int siguienteNumero = 1) : this()
    {
        Text = soloLectura ? "Detalle de mesa" : mesa is null ? "Nueva mesa" : "Editar mesa";
        _numero.Value = Math.Clamp(mesa?.Numero ?? siguienteNumero, 1, 10000);
        _capacidad.Value = Math.Clamp(mesa?.Capacidad ?? 4, 1, 100);
        _estado.SelectedItem = mesa?.Estado ?? "Disponible";
        _numero.Enabled = _capacidad.Enabled = !soloLectura;
        _estado.Enabled = mesa is not null && !soloLectura;
        aceptar.Text = soloLectura ? "Cerrar" : "Guardar";
    }
}
