using Restaurante.Backend.Dominio;
using Restaurante.Backend.Servicios;
using Restaurante.Frontend.Servicios;

namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioGestionMesa : Form
{
    private readonly IRepositorioRestaurante _repo = null!;
    private readonly Mesa _mesa = null!;
    private readonly IDialogosUsuario _dialogos = new DialogosUsuario();
    private CuentaRevision? _cuenta;
    private Guid? _incierta;
    private bool _ocupado;
    private readonly Guid _operacion = Guid.NewGuid();
    public FormularioGestionMesa()
    {
        InitializeComponent();
        FormatoPresentacion.Aplicar(this);
    }

    public FormularioGestionMesa(IRepositorioRestaurante repo, Mesa mesa, IDialogosUsuario? dialogos = null) : this()
    {
        _repo = repo;
        _mesa = mesa;
        _dialogos = dialogos ?? new DialogosUsuario();
        Shown += async (_, _) =>
        {
            _ocupado = true;
            try
            {
                var datos = await Task.Run(() =>
                {
                    var m = repo.ListarMesas().Single(m => m.Id == mesa.Id);
                    var p = repo.ObtenerPedidoAbierto(m.Id);
                    return (m, cuenta: p is null ? null : repo.RevisarCuenta(p.Id), incierta: p is null ? null : repo.ObtenerPagoIncierto(p.Id));
                });
                _cuenta = datos.cuenta;
                _incierta = datos.incierta;
                if (_cuenta is not null && _incierta is null)
                {
                    var btnAjuste = new Button
                    {
                        Text = "Descuento o cortesía",
                        AutoSize = true,
                        Name = "btnAjuste"
                    };
                    flpCampos.Controls.Add(btnAjuste);
                    btnAjuste.Click += (_, _) =>
                    {
                        using var f = new FormularioAjuste(_repo, _cuenta);
                        f.ShowDialog(this);
                        DialogResult = DialogResult.OK;
                        Close();
                    };
                }

                lblResumen.Text = $"Mesa {datos.m.Numero} · {datos.m.Estado}\n" + (_cuenta is null ? "Sin pedido activo." : $"Pedido #{_cuenta.PedidoId} · {FormatoPresentacion.Moneda(_cuenta.Total)}") + "\nSi hubo preparación o consumo, registre una incidencia; no borre el importe.";
                var admin = true; // meseros pueden solicitar autorización puntual; el servicio decide.
                if (_incierta.HasValue)
                {
                    var s = repo.ConsultarSolicitudCobro(_incierta.Value);
                    lblResumen.Text += $"\nTarjeta incierta: {FormatoPresentacion.Moneda(s.TotalEsperado)}\nReferencia: {s.Referencia}\nObservación: {s.Observacion}";
                    if (admin)
                        cbAccion.Items.Add("Conciliar tarjeta");
                }
                else if (_cuenta is not null && admin)
                    cbAccion.Items.AddRange(new object[] { "Cancelar", "Incidencia" });
                else if (_cuenta is null && datos.m.Estado == "Ocupada" && admin)
                    cbAccion.Items.Add("Corregir estado");
                else if (_cuenta is null && datos.m.Estado == "En limpieza")
                    cbAccion.Items.Add("Confirmar limpieza");
                if (cbAccion.Items.Count == 0)
                    lblResumen.Text += "\nNo hay acciones disponibles para tu rol. Solicita a un administrador resolver la cuenta.";
                else
                    cbAccion.SelectedIndex = 0;
                btnAplicar.Enabled = cbAccion.Items.Count > 0;
            }
            catch (Exception ex)
            {
                lblResumen.Text = ex.Message;
            }
            finally
            {
                _ocupado = false;
            }
        };
        FormClosing += (_, e) =>
        {
            if (_ocupado)
                e.Cancel = true;
        };
    }

    private void Accion_Cambio(object? sender, EventArgs e)
    {
        chkDeclarado.Visible = cbAccion.Text == "Cancelar";
        cbResultado.Enabled = chkVerificado.Visible = cbAccion.Text == "Conciliar tarjeta";
        if (cbAccion.Text == "Confirmar limpieza" && string.IsNullOrWhiteSpace(ctMotivo.Text))
            ctMotivo.Text = "Limpieza terminada y mesa revisada";
    }

    private async void Aplicar_Click(object? sender, EventArgs e)
    {
        if (_ocupado)
            return;
        try
        {
            ReglasRestaurante.Texto(ctMotivo.Text, "Motivo", 500);
            if (cbAccion.Text == "Conciliar tarjeta" && (!chkVerificado.Checked || cbResultado.SelectedIndex < 0))
                throw new ArgumentException("Selecciona el resultado y confirma su comprobación manual.");
            if (cbAccion.Text == "Cancelar" && !chkDeclarado.Checked)
                throw new ArgumentException("Declara que no hubo preparación ni consumo, o registra incidencia.");
            if (_dialogos.Mostrar(this, $"¿Confirmar {cbAccion.Text} para la mesa {_mesa.Numero}?\n{ctMotivo.Text}", "Confirmar gestión", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            _ocupado = true;
            flpCampos.Enabled = false;
            string texto;
            if (cbAccion.Text == "Conciliar tarjeta")
            {
                var aprobado = cbResultado.Text == "Confirmado";
                var razon = ctMotivo.Text;
                var solicitud = new SolicitudConciliacion(_operacion, _incierta!.Value, aprobado, razon);
                if (!FormularioAutorizar.Solicitar(this, _repo, _operacion, "ConciliarTarjeta", solicitud, lblResumen.Text + "\nResultado: " + cbResultado.Text + "\nMotivo: " + razon))
                    return;
                var r = await DiarioOperaciones.Ejecutar(_repo, "Conciliar", _operacion, solicitud, () => _repo.ConciliarTarjeta(solicitud.OperacionIncierta, solicitud.Confirmado, solicitud.Motivo, solicitud.OperacionId));
                texto = $"Conciliación manual: {r.Estado}. " + (r.NumeroComprobante is null ? "La cuenta sigue abierta." : $"Comprobante {r.NumeroComprobante}; mesa en limpieza.");
            }
            else
            {
                var s = new SolicitudGestionMesa(_operacion, _mesa.Id, _cuenta?.PedidoId, _cuenta?.Version, cbAccion.Text, ctMotivo.Text, chkDeclarado.Checked);
                if (s.Accion != "Confirmar limpieza" && !FormularioAutorizar.Solicitar(this, _repo, _operacion, "GestionMesa", s, lblResumen.Text + "\nAcción: " + s.Accion + "\nMotivo: " + s.Motivo))
                    return;
                texto = await DiarioOperaciones.Ejecutar(_repo, "Gestionar", _operacion, s, () => _repo.GestionarMesa(s));
            }

            _dialogos.Mostrar(this, texto, "Gestión registrada");
            _ocupado = false;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            _dialogos.Mostrar(this, ex.Message, "No se pudo completar la gestión", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            _ocupado = false;
            flpCampos.Enabled = true;
            if (DiarioOperaciones.TienePendiente(_repo, _operacion))
            {
                foreach (Control campo in flpCampos.Controls)
                    campo.Enabled = false;
                lblResumen.Enabled = true;
                lblResumen.Text = "Existe una operación pendiente. Cierre esta ventana y use Recuperar operaciones desde Inicio. No repita la gestión.";
            }
        }
    }
}
