using System.Data;
namespace Restaurante.Backend.Servicios;
public sealed partial class RepositorioRestaurante
{
    public void AbandonarOperacion(Guid id)
    {
        if(id==Guid.Empty)throw new ArgumentException("Identificador inválido.");
        using var c=AbrirConexion();using var t=c.BeginTransaction(IsolationLevel.Serializable);
        var actor=SesionEn(c,t);
        using var q=Consulta(c,t,"SELECT Tipo,UsuarioId FROM dbo.OperacionesRestaurante WITH(UPDLOCK,HOLDLOCK) WHERE Id=@id;",("@id",id));
        using(var r=q.ExecuteReader())
        {
            if(r.Read())
            {
                if(r.GetInt32(1)!=actor.Id)throw new UnauthorizedAccessException("La solicitud pertenece a otro usuario.");
                if(r.GetString(0)!="Abandonada")throw new InvalidOperationException("SQL ya registró esta operación. Recupere su resultado original.");
                r.Close();t.Commit();return;
            }
        }
        Ejecutar(c,t,"INSERT dbo.OperacionesRestaurante(Id,Tipo,SolicitudHash,UsuarioId,Resultado)VALUES(@id,N'Abandonada',@hash,@u,N'null');",("@id",id),("@hash",new string('0',64)),("@u",actor.Id));
        Seguridad(c,t,actor.Id,null,"Abandonar solicitud",$"Operación {id}; no cancela pedidos ni consumo");
        t.Commit();
    }
}
