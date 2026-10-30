namespace Restaurante.Frontend.Formularios;

internal sealed record EstadoVisualMesa(Color Fondo, Color Tinta, string Icono, string Accion)
{
    internal static EstadoVisualMesa De(string estado) => estado switch
    {
        "Disponible" => new(Color.FromArgb(231,243,226), Color.FromArgb(39,89,48), "check", "Abrir pedido"),
        "En limpieza" => new(Color.FromArgb(255,244,208), Color.FromArgb(122,84,8), "brush", "Confirmar limpieza"),
        "Ocupada" => new(Color.FromArgb(253,230,227), Color.FromArgb(151,45,43), "users-round", "Continuar pedido"),
        "Reservada" => new(Color.FromArgb(236,231,250), Color.FromArgb(88,63,140), "calendar", "Revisar mesa"),
        _ => new(Color.FromArgb(237,238,239), Color.FromArgb(75,80,87), "alert", "Revisar estado")
    };
}
