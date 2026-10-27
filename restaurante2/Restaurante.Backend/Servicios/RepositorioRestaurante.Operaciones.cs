using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Restaurante.Backend.Dominio;

namespace Restaurante.Backend.Servicios;

public sealed partial class RepositorioRestaurante
{
    private UsuarioAutenticado? _sesion;
    public UsuarioAutenticado? UsuarioActual => _sesion;
    public string IdentificadorBase {
        get { using var c=configuracion.CrearConexion();return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(c.DataSource.ToUpperInvariant()+"|"+c.Database.ToUpperInvariant()))); }
    }
    public bool BaseEsPruebas { get {using var c=configuracion.CrearConexion();return c.Database.StartsWith("Restaurante_Pruebas_",StringComparison.Ordinal);} }
    public void CerrarSesion() => _sesion = null;
    public bool OperacionAplicada(Guid operacionId)
    {
        var actor=ExigirSesion();using var c=AbrirConexion();
        using var q=Consulta(c,null,"SELECT UsuarioId FROM dbo.OperacionesRestaurante WHERE Id=@id;",("@id",operacionId));
        var usuario=q.ExecuteScalar();
        if(usuario is int id && id!=actor.Id)throw new UnauthorizedAccessException("La operación pertenece a otro usuario.");
        return usuario is int;
    }

    public void ValidarEsquema()
    {
        using var c = AbrirConexion();
        using var q = new SqlCommand("SELECT CASE WHEN OBJECT_ID(N'dbo.VersionEsquema',N'U') IS NULL THEN 0 ELSE 1 END;",c);
        if ((int)q.ExecuteScalar()! == 0) throw new InvalidOperationException("La base necesita la migración 004. Haga un respaldo y ejecute BaseDatos/Migracion_004.sql sobre su base SQL Server. No se actualizará automáticamente.");
        q.CommandText="SELECT COALESCE(MAX(Version),0) FROM dbo.VersionEsquema;";
        if ((int)q.ExecuteScalar()! != 4) throw new InvalidOperationException("Versión de esquema incompatible. Esta aplicación requiere el esquema 4; no se modificará automáticamente.");
    }

    private UsuarioAutenticado ExigirSesion(bool administrador = false)
    {
        var sesion = _sesion ?? throw new UnauthorizedAccessException("Inicie sesión para realizar esta operación.");
        using var c = AbrirConexion();
        using var q = Consulta(c,null,"SELECT Nombre,Rol FROM dbo.Usuarios WHERE Id=@id AND Activo=1;",("@id",sesion.Id));
        using var r = q.ExecuteReader();
        if (!r.Read() || (administrador && r.GetString(1)!="Administrador"))
            throw new UnauthorizedAccessException("Esta operación requiere un administrador activo.");
        return new(r.GetString(0),r.GetString(1),sesion.Id);
    }

    private static SqlCommand Consulta(SqlConnection c,SqlTransaction? t,string sql,params (string Nombre,object? Valor)[] parametros)
    {
        var q = new SqlCommand(sql,c,t);
        foreach (var (nombre,valor) in parametros)
        {
            if (valor is decimal importe) AgregarDecimal(q,nombre,importe,12);
            else q.Parameters.AddWithValue(nombre,valor ?? DBNull.Value);
        }
        return q;
    }
    private static int Ejecutar(SqlConnection c,SqlTransaction t,string sql,params (string Nombre,object? Valor)[] p)
    { using var q=Consulta(c,t,sql,p); return q.ExecuteNonQuery(); }

    private T Mutacion<T>(Guid id,string tipo,object solicitud,int mesaId,bool admin,Func<SqlConnection,SqlTransaction,UsuarioAutenticado,T> accion)
    {
        if(id==Guid.Empty)throw new ArgumentException("La operación necesita un identificador estable.");
        var actor=ExigirSesion(admin);
        var hash=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(solicitud))));
        using var c=AbrirConexion(); using var t=c.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            // All financial/order mutations acquire locks in this order: operation, table, order.
            using(var q=Consulta(c,t,"SELECT Tipo,SolicitudHash,UsuarioId,Resultado FROM dbo.OperacionesRestaurante WITH(UPDLOCK,HOLDLOCK) WHERE Id=@id;",("@id",id)))
            using(var r=q.ExecuteReader())
                if(r.Read())
                {
                    if(r.GetString(0)!=tipo || r.GetString(1)!=hash || r.GetInt32(2)!=actor.Id)
                        throw new InvalidOperationException("El identificador ya pertenece a otra solicitud o usuario. Recupere el resultado original.");
                    var resultado=JsonSerializer.Deserialize<T>(r.GetString(3))!; r.Close(); t.Commit(); return resultado;
                }
            if(mesaId>0)
            {
                using var q=Consulta(c,t,"SELECT Id FROM dbo.Mesas WITH(UPDLOCK,HOLDLOCK) WHERE Id=@mesa;",("@mesa",mesaId));
                if(q.ExecuteScalar() is null)throw new InvalidOperationException("La mesa ya no existe.");
            }
            Ejecutar(c,t,"INSERT dbo.OperacionesRestaurante(Id,Tipo,SolicitudHash,UsuarioId,Resultado)VALUES(@id,@tipo,@hash,@actor,N'');",
                ("@id",id),("@tipo",tipo),("@hash",hash),("@actor",actor.Id));
            var valor=accion(c,t,actor);
            Ejecutar(c,t,"UPDATE dbo.OperacionesRestaurante SET Resultado=@r WHERE Id=@id;",("@r",JsonSerializer.Serialize(valor)),("@id",id));
            t.Commit(); return valor;
        }
        catch { try{t.Rollback();}catch(Exception ex)when(ex is SqlException or InvalidOperationException){} throw; }
    }

    private int MesaDePedido(int pedidoId)
    {
        using var c=AbrirConexion(); using var q=Consulta(c,null,"SELECT MesaId FROM dbo.Pedidos WHERE Id=@id;",("@id",pedidoId));
        return q.ExecuteScalar() is int id ? id : throw new InvalidOperationException("El pedido no existe.");
    }

    private static CuentaRevision LeerCuenta(SqlConnection c,SqlTransaction t,int pedidoId)
    {
        using var q=Consulta(c,t,"""
            SELECT p.MesaId,COALESCE(p.MesaHistorica,m.Numero),p.ClienteId,COALESCE(p.ClienteHistorico,N'Público general'),
                   p.Estado,p.Total,p.VersionCuenta FROM dbo.Pedidos p WITH(UPDLOCK,HOLDLOCK)
            JOIN dbo.Mesas m ON m.Id=p.MesaId WHERE p.Id=@id;
            """,("@id",pedidoId));
        int mesa,numero;int? cliente;string nombre,estado,version;decimal total;
        using(var r=q.ExecuteReader())
        {
            if(!r.Read())throw new InvalidOperationException("El pedido ya no existe.");
            mesa=r.GetInt32(0);numero=r.GetInt32(1);cliente=r.IsDBNull(2)?null:r.GetInt32(2);nombre=r.GetString(3);
            estado=r.GetString(4);total=r.GetDecimal(5);version=Convert.ToBase64String((byte[])r[6]);
        }
        using var d=Consulta(c,t,"SELECT NombreProducto,TipoProducto,Cantidad,PrecioUnitario,Cantidad*PrecioUnitario FROM dbo.DetallesPedido WHERE PedidoId=@id ORDER BY Id;",("@id",pedidoId));
        using var dr=d.ExecuteReader();var detalles=new List<DetallePedidoResumen>();
        while(dr.Read())detalles.Add(new(dr.GetString(0),dr.GetString(1),dr.GetInt32(2),dr.GetDecimal(3),dr.GetDecimal(4)));
        return new(pedidoId,mesa,numero,cliente,nombre,estado,total,version,detalles);
    }
    public CuentaRevision RevisarCuenta(int pedidoId)
    {
        ExigirSesion(); var mesa=MesaDePedido(pedidoId);
        using var c=AbrirConexion();using var t=c.BeginTransaction(IsolationLevel.Serializable);
        using(var q=Consulta(c,t,"SELECT Id FROM dbo.Mesas WITH(UPDLOCK,HOLDLOCK) WHERE Id=@id;",("@id",mesa)))q.ExecuteScalar();
        var cuenta=LeerCuenta(c,t,pedidoId);t.Commit();return cuenta;
    }
    private static void ExigirAbierto(CuentaRevision cuenta)
    { if(cuenta.Estado is not ("Abierto" or "En preparación"))throw new InvalidOperationException("El pedido está cerrado. Actualice la pantalla."); }
    private static void ExigirVersion(CuentaRevision cuenta,string version,decimal? total=null)
    { if(cuenta.Version!=version || total.HasValue&&cuenta.Total!=total)throw new InvalidOperationException("La cuenta cambió. Actualice, revise productos e importe y confirme nuevamente."); }
    private static void SinPagoPendiente(SqlConnection c,SqlTransaction t,int pedidoId)
    {
        using var q=Consulta(c,t,"SELECT COUNT(*) FROM dbo.PagosManuales WHERE PedidoId=@p AND Estado IN(N'Confirmado',N'Incierto');",("@p",pedidoId));
        if((int)q.ExecuteScalar()!>0)throw new InvalidOperationException("Hay un pago confirmado o incierto. Consulte o concilie antes de continuar.");
    }
    private static void Auditar(SqlConnection c,SqlTransaction t,Guid op,UsuarioAutenticado actor,int? pedido,int mesa,string accion,string anterior,string posterior,string motivo,decimal importe)
    {
        Ejecutar(c,t,"""
          INSERT dbo.AuditoriaOperaciones(OperacionId,PedidoId,MesaId,Accion,Anterior,Posterior,Motivo,Importe,UsuarioId)
          VALUES(@op,@pedido,@mesa,@accion,@antes,@despues,@motivo,@importe,@actor);
          """,("@op",op),("@pedido",pedido),("@mesa",mesa),("@accion",accion),("@antes",anterior),("@despues",posterior),("@motivo",motivo),("@importe",importe),("@actor",actor.Id));
    }

    public PedidoCreado CrearPedido(SolicitudPedido solicitud)
    {
        ArgumentNullException.ThrowIfNull(solicitud);
        if(solicitud.MesaId<=0 || solicitud.Detalles is null || solicitud.Detalles.Count==0 || solicitud.Detalles.Any(d=>d.PlatoId<=0||d.Cantidad is <1 or >50))
            throw new ArgumentException("Indique mesa, productos y cantidades de 1 a 50.");
        return Mutacion(solicitud.OperacionId,"AbrirPedido",solicitud,solicitud.MesaId,false,(c,t,actor)=>
        {
            using(var q=Consulta(c,t,"SELECT Estado FROM dbo.Mesas WHERE Id=@id;",("@id",solicitud.MesaId)))
                if(q.ExecuteScalar() as string!="Disponible")throw new InvalidOperationException("La mesa ya no está disponible.");
            string cliente="Público general";
            if(solicitud.ClienteId.HasValue)
            {
                using var q=Consulta(c,t,"SELECT Nombre FROM dbo.Clientes WHERE Id=@id AND Activo=1;",("@id",solicitud.ClienteId));
                cliente=q.ExecuteScalar() as string??throw new InvalidOperationException("El cliente ya no está activo.");
            }
            using var insertar=Consulta(c,t,"""
                INSERT dbo.Pedidos(MesaId,ClienteId,Total,ClienteHistorico,MesaHistorica,CapturaLegada)
                OUTPUT INSERTED.Id SELECT @mesa,@cliente,0,@nombre,Numero,0 FROM dbo.Mesas WHERE Id=@mesa;
                """,("@mesa",solicitud.MesaId),("@cliente",solicitud.ClienteId),("@nombre",cliente));
            var id=(int)insertar.ExecuteScalar()!;
            foreach(var d in solicitud.Detalles)InsertarLinea(c,t,id,d.PlatoId,d.Cantidad);
            Ejecutar(c,t,"UPDATE dbo.Mesas SET Estado=N'Ocupada' WHERE Id=@id;",("@id",solicitud.MesaId));
            using var total=Consulta(c,t,"SELECT Total FROM dbo.Pedidos WHERE Id=@id;",("@id",id));var importe=(decimal)total.ExecuteScalar()!;
            Auditar(c,t,solicitud.OperacionId,actor,id,solicitud.MesaId,"AbrirPedido","Disponible","Abierto","Primer producto registrado",importe);
            return new PedidoCreado(id,importe,null);
        });
    }
    private static decimal InsertarLinea(SqlConnection c,SqlTransaction t,int pedidoId,int menuId,int cantidad)
    {
        using var q=Consulta(c,t,"SELECT Nombre,Tipo,Precio,Categoria,Alcoholica FROM dbo.Menu WHERE Id=@id AND Activo=1;",("@id",menuId));
        ElementoMenu producto;
        using(var r=q.ExecuteReader())
        {
            if(!r.Read())throw new InvalidOperationException("El producto ya no está activo.");
            producto=r.GetString(1)=="Bebida" ? new Bebida(menuId,r.GetString(0),r.GetString(3),r.GetDecimal(2),r.GetBoolean(4))
                : new Plato(menuId,r.GetString(0),r.GetString(3),r.GetDecimal(2));
        }
        var importe=producto.CalcularImporte(cantidad);
        Ejecutar(c,t,"""
            INSERT dbo.DetallesPedido(PedidoId,MenuId,Cantidad,PrecioUnitario,NombreProducto,TipoProducto)VALUES(@p,@m,@n,@precio,@nombre,@tipo);
            UPDATE dbo.Pedidos SET Total=Total+@importe WHERE Id=@p;
            """,("@p",pedidoId),("@m",menuId),("@n",cantidad),("@precio",producto.Precio),("@nombre",producto.Nombre),("@tipo",producto.Tipo),("@importe",importe));
        return importe;
    }
    public decimal AgregarProductoAPedido(int pedidoId,int menuId,int cantidad,Guid? operacionId=null)
    {
        if(menuId<=0||cantidad is <1 or >50)throw new ArgumentException("Producto o cantidad inválidos.");
        var op=operacionId??Guid.NewGuid();var mesa=MesaDePedido(pedidoId);
        return Mutacion(op,"AgregarProducto",new{pedidoId,menuId,cantidad},mesa,false,(c,t,actor)=>
        {
            var cuenta=LeerCuenta(c,t,pedidoId);ExigirAbierto(cuenta);SinPagoPendiente(c,t,pedidoId);
            var importe=InsertarLinea(c,t,pedidoId,menuId,cantidad);
            Auditar(c,t,op,actor,pedidoId,mesa,"AgregarProducto",cuenta.Estado,cuenta.Estado,"Incorporación de producto",importe);
            return cuenta.Total+importe;
        });
    }

    // The old API is retained to fail explicitly rather than silently invent a payment.
    public string FacturarPedido(int pedidoId) => throw new InvalidOperationException("Use Revisar y cobrar: debe confirmar receptor, importe y pago antes de emitir el comprobante interno.");
}
