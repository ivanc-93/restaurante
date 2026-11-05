using System.Data;
using Restaurante.Backend.Dominio;

namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioDivision : Form
{
    private readonly IRepositorioRestaurante repo = null!;
    private readonly CuentaRevision cuenta = null!;
    private readonly DataTable partes = new();
    private readonly DataTable unidades = new();
    private bool ocupado;
    public FormularioDivision() => InitializeComponent();
    public FormularioDivision(IRepositorioRestaurante repo, CuentaRevision cuenta) : this()
    {
        this.repo = repo;
        this.cuenta = cuenta;
        lblResumen.Text = $"Pedido #{cuenta.PedidoId} · consumo {MonedaGuatemala.Formatear(cuenta.ImporteOriginal)}\nDistribuye todo el consumo. El descuento se repartirá proporcionalmente; no se duplican productos. Después de confirmar no podrás cambiar la distribución ni el consumo.";
        partes.Columns.Add("Cuenta", typeof(string));
        partes.Columns.Add("Original", typeof(decimal));
        dgvPartes.DataSource = partes;
        unidades.Columns.Add("DetalleId", typeof(int));
        unidades.Columns.Add("Producto", typeof(string));
        unidades.Columns.Add("Precio", typeof(decimal));
        unidades.Columns.Add("Cuenta", typeof(int));
        dgvUnidades.DataSource = unidades;
        foreach (DataGridViewColumn col in dgvUnidades.Columns)
            col.ReadOnly = col.Name != "Cuenta";
        dgvUnidades.Columns["DetalleId"].Visible = false;
        Shown += async (_, _) =>
        {
            try
            {
                var lineas = await Task.Run(() => repo.ListarLineasCorregibles(cuenta.PedidoId));
                foreach (var l in lineas)
                    for (int i = 0; i < l.CantidadVigente; i++)
                        unidades.Rows.Add(l.Id, l.Producto, l.PrecioUnitario, 1);
                CrearPartes();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = MensajesReserva.Error(ex);
                btnGuardar.Enabled = false;
            }
        };
        dgvPartes.DataError += (_, e) =>
        {
            e.ThrowException = false;
            lblMensaje.Text = "Escribe un nombre y un importe numérico válido con hasta dos decimales.";
        };
        dgvUnidades.DataError += (_, e) =>
        {
            e.ThrowException = false;
            lblMensaje.Text = "Selecciona un número de cuenta válido.";
        };
        FormClosing += (_, e) => e.Cancel = ocupado;
    }

    private void CrearPartes()
    {
        if (cuenta is null)
            return;
        partes.Rows.Clear();
        var n = (int)nudPersonas.Value;
        var valor = decimal.Truncate(cuenta.ImporteOriginal / n * 100) / 100;
        var resto = (int)((cuenta.ImporteOriginal - valor * n) * 100);
        for (int i = 0; i < n; i++)
            partes.Rows.Add($"Cuenta {i + 1}", valor + (i < resto ? .01m : 0));
    }

    private async void Guardar_Click(object? sender, EventArgs e)
    {
        if (ocupado)
            return;
        try
        {
            Validate();
            dgvPartes.EndEdit();
            dgvUnidades.EndEdit();
            var ps = partes.Rows.Cast<DataRow>().Select(r => new ParteCuenta((string)r[0], Convert.ToDecimal(r[1]))).ToArray();
            AsignacionProducto[] productos = [];
            if (cbModo.SelectedIndex == 1)
            {
                productos = unidades.Rows.Cast<DataRow>().GroupBy(r => (Cuenta: Convert.ToInt32(r[3]) - 1, Detalle: Convert.ToInt32(r[0]))).Select(g => new AsignacionProducto(g.Key.Cuenta, g.Key.Detalle, g.Count())).ToArray();
                var importes = new decimal[ps.Length];
                foreach (DataRow u in unidades.Rows)
                {
                    var i = Convert.ToInt32(u[3]) - 1;
                    if (i < 0 || i >= ps.Length)
                        throw new ArgumentException("Cada unidad debe asignarse a un número de cuenta existente.");
                    importes[i] += Convert.ToDecimal(u[2]);
                }

                ps = ps.Select((p, i) => p with { Original = importes[i] }).ToArray();
            }

            if (ps.Any(p => p.Original <= 0) || ps.Sum(p => p.Original) != cuenta.ImporteOriginal)
                throw new ArgumentException("Todas las cuentas deben tener importe positivo y sumar el consumo original.");
            if (MessageBox.Show(this, string.Join("\n", ps.Select(p => $"{p.Nombre}: {MonedaGuatemala.Formatear(p.Original)}")) + "\n¿Confirmar distribución completa?", "Separar cuentas", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            var s = new SolicitudDivision(Guid.NewGuid(), cuenta.PedidoId, cuenta.Version, cuenta.Total, ps, productos);
            ocupado = true;
            btnGuardar.Enabled = false;
            await DiarioOperaciones.Ejecutar(repo, "Division", s.OperacionId, s, () => repo.DividirCuenta(s));
            ocupado = false;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            lblMensaje.Text = MensajesReserva.Error(ex);
            btnGuardar.Enabled = repo is not null && DiarioOperaciones.Listar(repo).Count == 0;
        }
        finally
        {
            ocupado = false;
        }
    }
}
