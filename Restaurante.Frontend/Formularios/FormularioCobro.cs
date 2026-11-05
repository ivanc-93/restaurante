using Restaurante.Backend.Dominio;
using Restaurante.Frontend.Servicios;

namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioCobro : Form
{
    private readonly IRepositorioRestaurante _repo = null!;
    private readonly IDialogosUsuario _dialogos = new DialogosUsuario();
    private readonly CuentaRevision _cuenta = null!;
    private SolicitudCobro? _solicitud;
    private bool _ocupado;
    private bool _ajustandoCampos;
    private int _ultimoAncho;
    public ResultadoCobro? Resultado { get; private set; }

    public FormularioCobro()
    {
        InitializeComponent();
        FormatoPresentacion.Aplicar(this);
    }

    public FormularioCobro(IRepositorioRestaurante repo, CuentaRevision cuenta, IDialogosUsuario? dialogos = null) : this()
    {
        _repo = repo;
        _cuenta = cuenta;
        _dialogos = dialogos ?? new DialogosUsuario();
        dgvTablaCuenta.DataSource = cuenta.Detalles.ToList();
        lblTotalCuenta.Text = "Total  " + FormatoPresentacion.Moneda(cuenta.Total);
        ctDetalle.Text = $"Mesa {cuenta.NumeroMesa} · Pedido #{cuenta.PedidoId}\r\n\r\n" + string.Join("\r\n", cuenta.Detalles.Select(d => $"{d.Cantidad} × {d.Producto}\r\n{FormatoPresentacion.Moneda(d.PrecioUnitario)} = {FormatoPresentacion.Moneda(d.Importe)}\r\n")) + $"\r\nOriginal: {FormatoPresentacion.Moneda(cuenta.ImporteOriginal)}\r\nDescuento: {FormatoPresentacion.Moneda(cuenta.Descuento)}\r\nTOTAL: {FormatoPresentacion.Moneda(cuenta.Total)}";
        ctReceptor.Text = cuenta.Cliente;
        chkPublico.Checked = cuenta.ClienteId is null;
        if (cuenta.Total == 0)
        {
            cbMetodo.Items.Add("Cortesía");
            cbMetodo.SelectedItem = "Cortesía";
        }

        cbCliente.SelectedIndexChanged += (_, _) =>
        {
            if (cbCliente.SelectedItem is Cliente c && c.Id > 0)
            {
                chkPublico.Checked = false;
                ctReceptor.Text = c.Nombre;
                ctCorreo.Text = c.Correo;
                chkOpcionales.Checked = !string.IsNullOrWhiteSpace(c.Correo);
            }
        };
        Shown += async (_, _) =>
        {
            try
            {
                var clientes = await Task.Run(repo.ListarClientes);
                if (IsDisposed)
                    return;
                cbCliente.DataSource = new List<Cliente>
                {
                    new(0, "No copiar: introducir receptor", null, null)
                }.Concat(clientes).ToList();
                _ultimoAncho = 0;
                AjustarCampos();
                ctDetalle.SelectionLength = 0;
            }
            catch (Exception ex)
            {
                lblMensaje.Text = ex.Message;
            }
        };
        FormClosing += (_, e) =>
        {
            if (_ocupado)
                e.Cancel = true;
        };
        flpCampos.SizeChanged += (_, _) => AjustarCampos();
        tlpPie.SizeChanged += (_, _) =>
        {
            var ancho = Math.Max(120, ClientSize.Width - tlpEstructura.Padding.Horizontal - 16);
            if (lblMensaje.MaximumSize.Width != ancho)
            {
                lblMensaje.MaximumSize = new Size(ancho, (int)(40 * DeviceDpi / 96f));
                lblSaldo.MaximumSize = new Size(ancho, 0);
            }
        };
        Datos_Cambio(this, EventArgs.Empty);
    }

    private void Datos_Cambio(object? sender, EventArgs e)
    {
        if (_cuenta is null)
            return;
        ctReceptor.Enabled = ctIdentificacion.Enabled = !chkPublico.Checked;
        ctReceptor.Visible = ctIdentificacion.Visible = lblReceptor.Visible = lblIdentificacion.Visible = !chkPublico.Checked;
        ctDireccion.Visible = ctCorreo.Visible = lblDireccion.Visible = lblCorreo.Visible = chkOpcionales.Checked;
        ctObservacion.Visible = lblObservacion.Visible = chkOpcionales.Checked || cbMetodo.Text == "Tarjeta";
        cbResultadoTarjeta.Visible = ctReferencia.Visible = lblResultado.Visible = lblReferencia.Visible = cbMetodo.Text == "Tarjeta";
        if (chkPublico.Checked)
        {
            ctReceptor.Text = "Público general";
            ctIdentificacion.Clear();
        }

        cbResultadoTarjeta.Enabled = ctReferencia.Enabled = cbMetodo.Text == "Tarjeta";
        nudEntregado.Enabled = cbMetodo.Text == "Efectivo";
        if (cbMetodo.Text == "Tarjeta")
            nudEntregado.Value = _cuenta.Total;
        if (cbMetodo.Text == "Cortesía")
            nudEntregado.Value = 0;
        var diferencia = nudEntregado.Value - _cuenta.Total;
        lblSaldo.Text = $"Total: {FormatoPresentacion.Moneda(_cuenta.Total)} · " + (diferencia >= 0 ? $"Cambio: {FormatoPresentacion.Moneda(diferencia)}" : $"Falta: {FormatoPresentacion.Moneda(-diferencia)}");
        _ultimoAncho = 0;
        AjustarCampos();
        if (!_ocupado)
            btnConfirmar.Enabled = _solicitud is not null || chkDatosConfirmados.Checked && cbMetodo.SelectedIndex >= 0 && (cbMetodo.Text == "Tarjeta" && cbResultadoTarjeta.Text is "Rechazado" or "Incierto" || chkPagoConfirmado.Checked);
    }

    private void AjustarCampos()
    {
        var ancho = Math.Max(120, flpCampos.ClientSize.Width - flpCampos.Padding.Horizontal - SystemInformation.VerticalScrollBarWidth - 12);
        if (_ajustandoCampos || ancho == _ultimoAncho)
            return;
        _ajustandoCampos = true;
        _ultimoAncho = ancho;
        flpCampos.SuspendLayout();
        try
        {
            foreach (Control campo in flpCampos.Controls)
                if (campo is Label or CheckBox)
                {
                    campo.MaximumSize = new Size(ancho, 0);
                    campo.Margin = new Padding(0, 6, 0, 6);
                }
                else
                {
                    campo.Width = ancho;
                    campo.Margin = new Padding(0, 0, 0, 6);
                }
        }
        finally
        {
            flpCampos.ResumeLayout(true);
            _ajustandoCampos = false;
        }
    }

    private void ValidarEntrada()
    {
        void Exigir(bool valido, Control campo, string error)
        {
            if (valido)
                return;
            flpCampos.ScrollControlIntoView(campo);
            campo.Focus();
            throw new ArgumentException(error);
        }

        Exigir(cbMetodo.SelectedIndex >= 0, cbMetodo, "Seleccione el método de pago.");
        Exigir(chkPublico.Checked || !string.IsNullOrWhiteSpace(ctReceptor.Text), ctReceptor, "Ingrese el nombre del receptor.");
        Exigir(chkDatosConfirmados.Checked, chkDatosConfirmados, "Confirme que revisó los productos, el importe y el receptor.");
        if (cbMetodo.Text == "Tarjeta")
        {
            Exigir(cbResultadoTarjeta.SelectedIndex >= 0, cbResultadoTarjeta, "Seleccione el resultado real del terminal externo.");
            if (cbResultadoTarjeta.Text is "Confirmado" or "Incierto")
                Exigir(!string.IsNullOrWhiteSpace(ctReferencia.Text), ctReferencia, "Ingrese una referencia externa, sin datos de tarjeta.");
            if (cbResultadoTarjeta.Text == "Confirmado")
                Exigir(chkPagoConfirmado.Checked, chkPagoConfirmado, "Confirme que comprobó manualmente el pago externo.");
        }
        else
        {
            Exigir(nudEntregado.Value >= _cuenta.Total, nudEntregado, "El efectivo es insuficiente.");
            Exigir(chkPagoConfirmado.Checked, chkPagoConfirmado, "Confirme el efectivo recibido o la cortesía autorizada.");
        }
    }

    private async void Confirmar_Click(object? sender, EventArgs e)
    {
        if (_ocupado)
            return;
        if (_solicitud is null)
        {
            try
            {
                ValidarEntrada();
            }
            catch (ArgumentException ex)
            {
                lblMensaje.Text = ex.Message;
                return;
            }
        }

        _solicitud ??= new(Guid.NewGuid(), _cuenta.PedidoId, _cuenta.Version, _cuenta.Total, chkPublico.Checked, ctReceptor.Text, ctIdentificacion.Text, ctDireccion.Text, ctCorreo.Text, cbMetodo.Text, nudEntregado.Value, cbResultadoTarjeta.Text, ctReferencia.Text, ctObservacion.Text, chkDatosConfirmados.Checked, chkPagoConfirmado.Checked, cbMetodo.Text == "Cortesía" && chkPagoConfirmado.Checked);
        _ocupado = true;
        btnConfirmar.Enabled = false;
        flpCampos.Enabled = false;
        try
        {
            Resultado = await DiarioOperaciones.Ejecutar(_repo, "Cobro", _solicitud.OperacionId, _solicitud, () => _repo.RegistrarCobro(_solicitud));
            _dialogos.Mostrar(this, Resultado.Estado == "Confirmado" ? $"Comprobante {Resultado.NumeroComprobante}. Pago registrado por el operador y cierre con paso de mesa a limpieza. Consulte Mesas para su estado actual." : $"Resultado {Resultado.Estado}. La cuenta sigue abierta. No repita un cobro incierto; solicite conciliación.", "Resultado de la operación");
            _ocupado = false;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            lblMensaje.Text = ex.Message;
            _dialogos.Mostrar(this, ex.Message, "No se pudo completar el cobro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            if (_solicitud is not null && !DiarioOperaciones.TienePendiente(_repo, _solicitud.OperacionId))
            {
                _solicitud = null;
                flpCampos.Enabled = true;
                btnConfirmar.Text = "Confirmar operación";
            }
            else
                btnConfirmar.Text = "Recuperar la misma operación";
        }
        finally
        {
            _ocupado = false;
            btnConfirmar.Enabled = true;
        }
    }
}
