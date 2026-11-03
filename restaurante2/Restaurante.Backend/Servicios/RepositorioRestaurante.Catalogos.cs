using System.Data;
using Microsoft.Data.SqlClient;
using Restaurante.Backend.Dominio;
namespace Restaurante.Backend.Servicios;
public sealed partial class RepositorioRestaurante
{
    public IReadOnlyList<RegistroInactivo> ListarInactivos(bool productos)
    {
        ExigirSesion(true);using var c=AbrirConexion();
        using var q=Consulta(c,null,productos?"SELECT Id,Nombre,Categoria+N' · '+Tipo FROM dbo.Menu WHERE Activo=0 ORDER BY Nombre;":"SELECT Id,Nombre,COALESCE(Correo,Telefono,N'Sin datos de contacto') FROM dbo.Clientes WHERE Activo=0 ORDER BY Nombre;");
        using var r=q.ExecuteReader();var lista=new List<RegistroInactivo>();
        while(r.Read())lista.Add(new(r.GetInt32(0),r.GetString(1),r.GetString(2)));return lista;
    }
    public void ReactivarRegistro(bool productos,int id)
    {
        if(id<=0)throw new ArgumentException("Selecciona un registro.");
        using var c=AbrirConexion();using var t=c.BeginTransaction(IsolationLevel.Serializable);SesionEn(c,t,gerente:true);
        try {
            using var q=Consulta(c,t,productos?"UPDATE dbo.Menu SET Activo=1 WHERE Id=@id AND Activo=0;":"UPDATE dbo.Clientes SET Activo=1 WHERE Id=@id AND Activo=0;",("@id",id));
            if(q.ExecuteNonQuery()!=1)throw new InvalidOperationException("El registro ya fue reactivado. Actualiza el listado.");
            t.Commit();
        } catch(SqlException ex) when(ex.Number is 2601 or 2627) {
            throw new InvalidOperationException("Ya existe un registro activo con esos datos. Revisa el duplicado antes de reactivar; el historial se conserva.",ex);
        }
    }
}
