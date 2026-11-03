namespace Restaurante.Frontend.Infraestructura;
/// <summary>Conserva el hilo de controles al regresar de una ventana modal antes de una operación asíncrona.</summary>
internal static class ContextoInterfaz
{
    internal static void AsegurarActual()
    {
        if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA || SynchronizationContext.Current is WindowsFormsSynchronizationContext)
            return;
        if (Application.OpenForms.Cast<Form>().Any(f => f.IsHandleCreated && !f.IsDisposed && !f.InvokeRequired))
            SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
    }

    internal static Task Invocar(Control control, Func<Task> accion)
    {
        var completada = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        if (control.IsDisposed)
            return Task.CompletedTask;
        control.BeginInvoke(new Action(async () =>
        {
            try
            {
                AsegurarActual();
                await accion();
                completada.SetResult();
            }
            catch (Exception ex)
            {
                completada.SetException(ex);
            }
        }));
        return completada.Task;
    }
}
