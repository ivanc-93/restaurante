using Restaurante.Backend.Dominio;

namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioProductoCuenta : Form
{
    private readonly IRepositorioRestaurante repo = null!;
    private readonly ServicioMesa servicio = null!;
    private readonly CuentaOperativa origen = null!;
    private readonly CuentaRevision revision = null!;
    private bool ocupado;
    public FormularioProductoCuenta() => InitializeComponent();
    public FormularioProductoCuenta(IRepositorioRestaurante repo, ServicioMesa servicio, CuentaOperativa origen, CuentaRevision revision, bool corregir = false) : this()
    {
        this.repo = repo;
        this.servicio = servicio;
        this.origen = origen;
        this.revision = revision;
        lblResumen.Text = $"Mesa {servicio.Mesa.Numero} · {origen.Nombre} (# {origen.Id})\nMover cambia quién paga; corregir retira unidades sin preparación ni consumo. Son acciones diferentes.";
        dgvProductos.DataSource = origen.Productos.ToList();
        cbDestino.DataSource = servicio.Cuentas.Where(x => x.Id > 0 && x.Id != origen.Id && !x.Liquidada && x.Pagado == 0).ToList();
        cbDestino.DisplayMember = "Etiqueta";
        cbAccion.Items.Add("Mover a otra cuenta");
        if (revision.Estado == "Abierto")
            cbAccion.Items.Add("Corregir captura");
        cbAccion.SelectedItem = corregir && revision.Estado == "Abierto" ? "Corregir captura" : "Mover a otra cuenta";
        FormClosing += (_, e) => e.Cancel = ocupado;
        FormatoPresentacion.Aplicar(this);
        if (dgvProductos.Columns.Contains("Id"))
            dgvProductos.Columns["Id"].Visible = false;
    }

    private async void Aplicar_Click(object? sender, EventArgs e)
    {
        if (ocupado)
            return;
        try
        {
            if (dgvProductos.CurrentRow?.DataBoundItem is not DetalleCuenta linea)
                throw new ArgumentException("Selecciona el producto de esta cuenta.");
            ReglasRestaurante.Texto(ctMotivo.Text, "Motivo", 400);
            var cantidad = (int)nudCantidad.Value;
            if (cantidad > linea.Cantidad)
                throw new ArgumentException("La cantidad supera las unidades de esta cuenta.");
            if (cbAccion.Text == "Corregir captura")
            {
                var s = new SolicitudCorreccionCuenta(Guid.NewGuid(), revision.PedidoId, revision.Version, revision.Total, origen.Id, linea.Id, cantidad, ctMotivo.Text.Trim(), chkSinPreparacion.Checked);
                if (!s.SinPreparacionDeclarada)
                    throw new ArgumentException("Declara que no hubo preparación ni consumo.");
                if (!FormularioAutorizar.Solicitar(this, repo, s.OperacionId, "CorreccionCuenta", s, $"{origen.Nombre}: retirar {cantidad} × {linea.Producto}\n{s.Motivo}"))
                    return;
                ocupado = true;
                btnAplicar.Enabled = false;
                await DiarioOperaciones.Ejecutar(repo, "CorreccionCuenta", s.OperacionId, s, () => repo.CorregirCuenta(s));
            }
            else
            {
                if (cbDestino.SelectedItem is not CuentaOperativa destino)
                    throw new ArgumentException("Elige la cuenta de destino.");
                var s = new SolicitudTraspasoCuenta(Guid.NewGuid(), revision.PedidoId, revision.Version, origen.Id, destino.Id, linea.Id, cantidad, ctMotivo.Text.Trim());
                ocupado = true;
                btnAplicar.Enabled = false;
                await DiarioOperaciones.Ejecutar(repo, "TraspasoCuenta", s.OperacionId, s, () => repo.TraspasarCuenta(s));
            }

            ocupado = false;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            lblMensaje.Text = MensajesReserva.Error(ex);
            btnAplicar.Enabled = DiarioOperaciones.ParaPedido(repo, servicio.Mesa.Id, servicio.PedidoId).Count == 0;
        }
        finally
        {
            ocupado = false;
        }
    }
}
