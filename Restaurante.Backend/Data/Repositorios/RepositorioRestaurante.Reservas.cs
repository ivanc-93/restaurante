using System.Data;
using Microsoft.Data.SqlClient;
using Restaurante.Backend.Dominio;

namespace Restaurante.Backend.Data.Repositorios;
public sealed partial class RepositorioRestaurante
{
    internal static bool TieneReservas(SqlConnection c, SqlTransaction? t)
    {
        using var q = Consulta(c, t, """
            SELECT CASE WHEN OBJECT_ID(N'dbo.ReservasMesa',N'U') IS NOT NULL
            AND NOT EXISTS(SELECT 1 FROM (VALUES(N'Id'),(N'MesaId'),(N'MesaHistorica'),(N'Nombre'),(N'Estado'),(N'Fecha'),(N'UsuarioId'),(N'OperacionId'),(N'PedidoId'),(N'OperacionCierre'),(N'UsuarioCierre'),(N'FechaCierre'),(N'MotivoCierre'),(N'Version')) x(Columna) WHERE COL_LENGTH(N'dbo.ReservasMesa',Columna) IS NULL)
            AND EXISTS(SELECT 1 FROM dbo.VersionEsquema WHERE Version=8)
            AND (SELECT COUNT(*) FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.ReservasMesa') AND name IN(N'UX_Reserva_MesaActiva',N'UX_Reserva_Pedido',N'UX_Reserva_Cierre') AND is_unique=1 AND is_disabled=0 AND has_filter=1)=3
            THEN 1 ELSE 0 END;
            """);
        return (int)q.ExecuteScalar()! == 1;
    }

    private static void ExigirReservas(SqlConnection c, SqlTransaction? t)
    {
        if (!TieneReservas(c, t))
            throw new InvalidOperationException("Reservas requiere la actualización 008 respaldada y autorizada. No se actualizará automáticamente la base.");
    }

    private static bool ReservasHabilitadasEn(SqlConnection c, SqlTransaction? t)
    {
        using var q = Consulta(c, t, "SELECT COUNT(*) FROM dbo.VersionEsquema WHERE Version=8;");
        if ((int)q.ExecuteScalar()! == 0)
            return false;
        ExigirReservas(c, t);
        return true;
    }

    public bool ReservasDisponibles()
    {
        using var c = AbrirConexion();
        SesionEn(c, null);
        return TieneReservas(c, null);
    }

    public IReadOnlyList<ReservaResumen> ListarReservasActivas()
    {
        using var c = AbrirConexion();
        using var t = c.BeginTransaction(IsolationLevel.Serializable);
        SesionEn(c, t);
        ExigirReservas(c, t);
        using var q = Consulta(c, t, "SELECT Id,MesaId,MesaHistorica,Nombre,Estado,Fecha,UsuarioId,Version FROM dbo.ReservasMesa WHERE Estado=N'Activa' ORDER BY MesaHistorica;");
        var resultado = new List<ReservaResumen>();
        using (var r = q.ExecuteReader())
            while (r.Read())
                resultado.Add(LeerReserva(r));
        t.Commit();
        return resultado;
    }

    public ReservaResumen? ConsultarReservaMesa(int mesaId)
    {
        using var c = AbrirConexion();
        using var t = c.BeginTransaction(IsolationLevel.Serializable);
        SesionEn(c, t);
        ExigirReservas(c, t);
        var reserva = ReservaEn(c, t, mesaId);
        t.Commit();
        return reserva;
    }

    private static ReservaResumen LeerReserva(SqlDataReader r) => new(r.GetInt32(0), r.GetInt32(1), r.GetInt32(2), r.GetString(3), r.GetString(4), r.GetDateTime(5), r.GetInt32(6), Convert.ToBase64String((byte[])r[7]));
    private static ReservaResumen? ReservaEn(SqlConnection c, SqlTransaction t, int mesaId)
    {
        using var q = Consulta(c, t, "SELECT Id,MesaId,MesaHistorica,Nombre,Estado,Fecha,UsuarioId,Version FROM dbo.ReservasMesa WITH(UPDLOCK,HOLDLOCK) WHERE MesaId=@m AND Estado=N'Activa';", ("@m", mesaId));
        using var r = q.ExecuteReader();
        return r.Read() ? LeerReserva(r) : null;
    }

    private static void SinPedidoAbierto(SqlConnection c, SqlTransaction t, int mesaId)
    {
        using var q = Consulta(c, t, "SELECT COUNT(*) FROM dbo.Pedidos WITH(UPDLOCK,HOLDLOCK) WHERE MesaId=@m AND Estado IN(N'Abierto',N'En preparación');", ("@m", mesaId));
        if ((int)q.ExecuteScalar()! > 0)
            throw new InvalidOperationException("La mesa tiene un pedido abierto. Actualiza la vista antes de continuar.");
    }

    private static ReservaResumen ExigirReservaActiva(SqlConnection c, SqlTransaction t, int mesaId, int reservaId, string version)
    {
        var reserva = ReservaEn(c, t, mesaId);
        if (reserva is null || reserva.Id != reservaId || reserva.Version != version)
            throw new InvalidOperationException("La reserva cambió o ya fue atendida/cancelada. Actualiza la vista.");
        using var q = Consulta(c, t, "SELECT Estado FROM dbo.Mesas WHERE Id=@m;", ("@m", mesaId));
        if (q.ExecuteScalar() as string != "Reservada")
            throw new InvalidOperationException("El estado de la mesa no coincide con su reserva. Solicita revisión.");
        SinPedidoAbierto(c, t, mesaId);
        return reserva;
    }

    public ReservaResumen ReservarMesa(SolicitudReserva solicitud)
    {
        ArgumentNullException.ThrowIfNull(solicitud);
        if (solicitud.MesaId <= 0)
            throw new ArgumentException("Selecciona una mesa válida.");
        var nombre = ReglasRestaurante.Texto(solicitud.Nombre, "Nombre de la persona que reserva", 120);
        if (nombre.Any(char.IsControl))
            throw new ArgumentException("El nombre no debe contener saltos de línea ni caracteres de control.");
        return Mutacion(solicitud.OperacionId, "ReservarMesa", solicitud, solicitud.MesaId, false, (c, t, actor) =>
        {
            ExigirReservas(c, t);
            using (var estado = Consulta(c, t, "SELECT Estado FROM dbo.Mesas WHERE Id=@m;", ("@m", solicitud.MesaId)))
                if (estado.ExecuteScalar() as string != "Disponible")
                    throw new InvalidOperationException("La mesa ya no está disponible. Actualiza la vista.");
            SinPedidoAbierto(c, t, solicitud.MesaId);
            if (ReservaEn(c, t, solicitud.MesaId)is not null)
                throw new InvalidOperationException("La mesa ya tiene una reserva activa. Actualiza la vista.");
            Ejecutar(c, t, "INSERT dbo.ReservasMesa(MesaId,MesaHistorica,Nombre,Estado,UsuarioId,OperacionId) SELECT Id,Numero,@nombre,N'Activa',@u,@op FROM dbo.Mesas WHERE Id=@m;UPDATE dbo.Mesas SET Estado=N'Reservada' WHERE Id=@m;", ("@nombre", nombre), ("@u", actor.Id), ("@op", solicitud.OperacionId), ("@m", solicitud.MesaId));
            Auditar(c, t, solicitud.OperacionId, actor, null, solicitud.MesaId, "ReservarMesa", "Disponible", "Reservada", "Reserva inmediata registrada", 0);
            return ReservaEn(c, t, solicitud.MesaId)!;
        });
    }

    public string CancelarReserva(SolicitudCancelacionReserva solicitud)
    {
        ArgumentNullException.ThrowIfNull(solicitud);
        ReglasRestaurante.Texto(solicitud.Motivo, "Motivo de cancelación", 500);
        return Mutacion(solicitud.OperacionId, "CancelarReserva", solicitud, solicitud.MesaId, true, (c, t, actor) =>
        {
            ExigirReservas(c, t);
            ExigirReservaActiva(c, t, solicitud.MesaId, solicitud.ReservaId, solicitud.Version);
            Ejecutar(c, t, "UPDATE dbo.ReservasMesa SET Estado=N'Cancelada',OperacionCierre=@op,UsuarioCierre=@u,FechaCierre=DATEADD(hour,-6,SYSUTCDATETIME()),MotivoCierre=@motivo WHERE Id=@r;UPDATE dbo.Mesas SET Estado=N'Disponible' WHERE Id=@m;", ("@op", solicitud.OperacionId), ("@u", actor.Id), ("@motivo", solicitud.Motivo.Trim()), ("@r", solicitud.ReservaId), ("@m", solicitud.MesaId));
            Auditar(c, t, solicitud.OperacionId, actor, null, solicitud.MesaId, "CancelarReserva", "Reservada", "Disponible", solicitud.Motivo.Trim(), 0);
            return "Reserva cancelada. La mesa está disponible y el historial se conserva.";
        });
    }

    public PedidoCreado AtenderReserva(SolicitudAtencionReserva solicitud)
    {
        ArgumentNullException.ThrowIfNull(solicitud);
        if (solicitud.MesaId <= 0 || solicitud.Detalles is null || solicitud.Detalles.Count == 0 || solicitud.Detalles.Any(d => d.PlatoId <= 0 || d.Cantidad is < 1 or > 50))
            throw new ArgumentException("Selecciona productos y cantidades de 1 a 50. Abrir la ventana no crea consumo.");
        return Mutacion(solicitud.OperacionId, "AtenderReserva", solicitud, solicitud.MesaId, false, (c, t, actor) =>
        {
            ExigirReservas(c, t);
            ExigirReservaActiva(c, t, solicitud.MesaId, solicitud.ReservaId, solicitud.Version);
            var pedido = CrearPedidoConConsumo(c, t, actor, solicitud.MesaId, solicitud.ClienteId, solicitud.Detalles, solicitud.OperacionId, "Reservada");
            Ejecutar(c, t, "UPDATE dbo.ReservasMesa SET Estado=N'Atendida',PedidoId=@p,OperacionCierre=@op,UsuarioCierre=@u,FechaCierre=DATEADD(hour,-6,SYSUTCDATETIME()),MotivoCierre=N'Primer producto registrado' WHERE Id=@r;", ("@p", pedido.Id), ("@op", solicitud.OperacionId), ("@u", actor.Id), ("@r", solicitud.ReservaId));
            Auditar(c, t, solicitud.OperacionId, actor, pedido.Id, solicitud.MesaId, "AtenderReserva", "Reservada", "Ocupada", "Reserva vinculada al primer consumo", pedido.Total);
            return pedido;
        });
    }
}
