using Restaurante.Backend.Servicios;
namespace Restaurante.Frontend.Formularios;
internal static class ValidacionCredenciales
{
    public static bool Campo(TextBox campo,Label mensaje,string nombre)
    {
        if(!string.IsNullOrWhiteSpace(campo.Text))return true;
        mensaje.Text=$"Completa {nombre}.";campo.Focus();return false;
    }
    public static bool Nueva(TextBox clave,Label mensaje,TextBox? confirmar=null,TextBox? actual=null)
    {
        var error=Contrasenas.ErrorValidacion(clave.Text);
        if(error is not null){mensaje.Text=error;clave.Focus();return false;}
        if(actual is not null&&clave.Text==actual.Text){mensaje.Text="La contraseña nueva debe ser diferente de la actual.";clave.Focus();return false;}
        if(confirmar is not null&&clave.Text!=confirmar.Text){mensaje.Text="Las contraseñas nuevas no coinciden. Repite exactamente la contraseña nueva.";confirmar.Focus();return false;}
        mensaje.Text="";return true;
    }
}
