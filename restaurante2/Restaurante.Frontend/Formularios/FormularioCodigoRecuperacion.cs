using Restaurante.Backend.Servicios;

namespace Restaurante.Frontend.Formularios;
public sealed partial class FormularioCodigoRecuperacion : Form
{
    private readonly IRepositorioRestaurante repo;
    private bool ocupado;
    public FormularioCodigoRecuperacion(IRepositorioRestaurante repositorio)
    {
        repo = repositorio;
        InitializeComponent();
        FormClosing += (_, e) => e.Cancel = ocupado;
        FormClosed += (_, _) =>
        {
            ctContrasena.Clear();
            ctCodigo.Clear();
        };
    }

    private async void Generar_Click(object? sender, EventArgs e)
    {
        if (ocupado)
            return;
        if (string.IsNullOrEmpty(ctContrasena.Text))
        {
            lblMensaje.Text = "Escribe tu contraseña vigente de Administrador, no el código de emergencia.";
            ctContrasena.Focus();
            return;
        }

        var c = ctContrasena.Text;
        ocupado = true;
        btnGenerar.Enabled = btnContinuar.Enabled = ctContrasena.Enabled = chkGuardado.Enabled = false;
        UseWaitCursor = true;
        Restaurante.Backend.Dominio.ResultadoGeneracionCodigo? resultado = null;
        string? error = null;
        try
        {
            resultado = await Task.Run(() => repo.IntentarGenerarCodigoRecuperacion(c)).ConfigureAwait(false);
        }
        catch (Microsoft.Data.SqlClient.SqlException)
        {
            error = "No se confirmó la generación con SQL. No confirmes un código anterior: comprueba la conexión y genera un reemplazo para guardar el último obtenido.";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.TraceError("Generación de código: {0}", ex.GetType().Name);
            error = "No se pudo completar la generación por un problema técnico. No se confirmó el resultado; solicita asistencia sin enviar tu contraseña ni códigos.";
        }

        c = "";
        DisenoFormulario.EnInterfaz(this, () =>
        {
            ocupado = false;
            btnGenerar.Enabled = ctContrasena.Enabled = chkGuardado.Enabled = true;
            UseWaitCursor = false;
            ctContrasena.Clear();
            if (error is not null)
            {
                ctCodigo.Clear();
                chkGuardado.Checked = false;
                lblMensaje.Text = error;
            }
            else if (resultado is not null)
            {
                lblMensaje.Text = resultado.Mensaje;
                if (resultado.Generado)
                {
                    ctCodigo.Text = resultado.Codigo;
                    chkGuardado.Checked = false;
                }
                else
                    ctContrasena.Focus();
            }

            btnContinuar.Enabled = chkGuardado.Checked && ctCodigo.Text.Length > 0;
        });
    }

    private async void Confirmar_Click(object? sender, EventArgs e)
    {
        if (ocupado || !chkGuardado.Checked || ctCodigo.Text.Length == 0)
            return;
        var valor = ctCodigo.Text;
        ocupado = true;
        btnContinuar.Enabled = btnGenerar.Enabled = false;
        Restaurante.Backend.Dominio.ResultadoRecuperacion? resultado = null;
        try
        {
            resultado = await Task.Run(() => repo.IntentarConfirmarCodigoRecuperacion(valor)).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.TraceError("Confirmación de custodia: {0}", ex.GetType().Name);
        }

        DisenoFormulario.EnInterfaz(this, () =>
        {
            ocupado = false;
            if (resultado?.Correcto == true)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                lblMensaje.Text = resultado?.Mensaje ?? "No se confirmó el código con SQL. Comprueba la conexión y vuelve a confirmar antes de cerrar.";
                btnContinuar.Enabled = chkGuardado.Checked;
                btnGenerar.Enabled = true;
            }
        });
    }
}
