namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioComprobante : Form
{
    public FormularioComprobante(string contenido) : this()
    {
        ctContenido.Text = contenido;
    }

    public FormularioComprobante()
    {
        InitializeComponent();
        FormatoPresentacion.Aplicar(this);
    }

    public FormularioComprobante(IRepositorioRestaurante repo, int pedidoId) : this()
    {
        Shown += async (_, _) =>
        {
            try
            {
                ctContenido.Text = await Task.Run(() => repo.ConsultarComprobante(pedidoId));
            }
            catch (Exception ex)
            {
                ctContenido.Text = ex.Message;
            }
        };
    }
}
