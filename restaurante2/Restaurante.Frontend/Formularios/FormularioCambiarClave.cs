namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioCambiarClave : Form
{
    private readonly IRepositorioRestaurante repo = null!;
    private bool ocupado;
    public FormularioCambiarClave() => InitializeComponent();
    public FormularioCambiarClave(IRepositorioRestaurante repo) : this()
    {
        this.repo = repo;
        FormClosing += (_, e) => e.Cancel = ocupado;
    }

    private async void Guardar_Click(object? sender, EventArgs e)
    {
        if (ocupado)
            return;
        if (!ValidacionCredenciales.Campo(ctActual, lblMensaje, "la contraseña actual") || !ValidacionCredenciales.Nueva(ctNueva, lblMensaje, ctRepetir, ctActual))
            return;
        var a = ctActual.Text;
        var n = ctNueva.Text;
        ocupado = true;
        btnGuardar.Enabled = false;
        bool terminado = false;
        try
        {
            await Task.Run(() => repo.CambiarMiContrasena(a, n));
            terminado = true;
        }
        catch (Exception ex)
        {
            lblMensaje.Text = ex.Message;
        }
        finally
        {
            ocupado = false;
            btnGuardar.Enabled = true;
            ctActual.Clear();
            ctNueva.Clear();
            ctRepetir.Clear();
        }

        if (terminado)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
