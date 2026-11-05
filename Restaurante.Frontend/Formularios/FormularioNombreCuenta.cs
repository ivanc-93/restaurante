using Restaurante.Backend.Dominio;

namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioNombreCuenta : Form
{
    private readonly IRepositorioRestaurante repo = null!;
    private readonly ServicioMesa servicio = null!;
    private readonly CuentaOperativa? cuenta;
    private bool ocupado;
    public int CuentaId { get; private set; }

    public FormularioNombreCuenta() => InitializeComponent();
    public FormularioNombreCuenta(IRepositorioRestaurante repo, ServicioMesa servicio, CuentaOperativa? cuenta = null) : this()
    {
        this.repo = repo;
        this.servicio = servicio;
        this.cuenta = cuenta;
        lblResumen.Text = $"Mesa {servicio.Mesa.Numero} · {(cuenta is null ? "Agregar cuenta" : "Identificar cuenta #" + cuenta.Id)}\nEl nombre identifica consumo. No crea un usuario, cliente ni receptor del comprobante.";
        ctNombre.Text = cuenta?.Nombre ?? "";
        chkConfirmado.Checked = cuenta?.NombreConfirmado ?? true;
        btnRetirar.Visible = cuenta is { Id: > 0, Original: 0, Descuento: 0, Pagado: 0, Liquidada: false };
        FormClosing += (_, e) => e.Cancel = ocupado;
    }

    private async void Guardar_Click(object? sender, EventArgs e) => await Guardar(false);
    private async void Retirar_Click(object? sender, EventArgs e) => await Guardar(true);
    private async Task Guardar(bool retirar)
    {
        if (ocupado)
            return;
        try
        {
            if (!retirar)
                ReglasRestaurante.Texto(ctNombre.Text, "Nombre de cuenta", 80);
            var s = new SolicitudCuentaMesa(Guid.NewGuid(), servicio.Mesa.Id, servicio.PedidoId, servicio.VersionPedido, cuenta?.Id ?? 0, cuenta?.Version, retirar ? "Retirar" : cuenta is null ? "Crear" : "Renombrar", ctNombre.Text.Trim(), chkConfirmado.Checked);
            ocupado = true;
            btnGuardar.Enabled = btnRetirar.Enabled = false;
            CuentaId = await DiarioOperaciones.Ejecutar(repo, "CuentaMesa", s.OperacionId, s, () => repo.GestionarCuentaMesa(s));
            ocupado = false;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            lblMensaje.Text = MensajesReserva.Error(ex);
            btnGuardar.Enabled = btnRetirar.Enabled = DiarioOperaciones.ParaPedido(repo, servicio.Mesa.Id, servicio.PedidoId).Count == 0;
        }
        finally
        {
            ocupado = false;
        }
    }
}
