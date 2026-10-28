using System.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;
using Restaurante.Backend.Dominio;

namespace Restaurante.Backend.Servicios;

public sealed partial class RepositorioRestaurante(IConfiguracionConexion configuracion) : IRepositorioRestaurante
{
    public UsuarioAutenticado? IniciarSesion(string usuario, string contrasena)
    {
        _sesion = null;
        if (!configuracion.EstaConfigurada) return null;
        return _sesion = VerificarAcceso(usuario,contrasena,true);
    }

    public ResumenRestaurante ObtenerResumen()
    {
        var actor=ExigirSesion();
        using var conexion = AbrirConexion();
        using var comando = new SqlCommand("""
            SELECT
             (SELECT COUNT(*) FROM dbo.Mesas WHERE Estado = 'Disponible'),
             (SELECT COUNT(*) FROM dbo.Pedidos WHERE Estado IN ('Abierto','En preparación')),
             (SELECT COALESCE(SUM(Total),0) FROM dbo.Facturas WHERE CAST(Fecha AS date)=CAST(DATEADD(hour,-6,SYSUTCDATETIME()) AS date)),
             (SELECT COUNT(*) FROM dbo.Menu WHERE Activo=1);
            """, conexion);
        using var lector = comando.ExecuteReader();
        lector.Read();
        return new(lector.GetInt32(0), lector.GetInt32(1), actor.Rol is "Administrador" or "Gerente" ? lector.GetDecimal(2) : 0, lector.GetInt32(3));
    }

    public IReadOnlyList<Mesa> ListarMesas()
    {
        ExigirSesion();
        var resultado = new List<Mesa>();
        using var conexion = AbrirConexion();
        using var comando = new SqlCommand(
            "SELECT Id, Numero, Capacidad, Estado FROM dbo.Mesas ORDER BY Numero;", conexion);
        using var lector = comando.ExecuteReader();
        while (lector.Read())
            resultado.Add(new(lector.GetInt32(0), lector.GetInt32(1), lector.GetInt32(2), lector.GetString(3)));
        return resultado;
    }

    public void CrearMesa(int numero, int capacidad)
    {
        ExigirSesion(true);
        ValidarMesa(numero, capacidad);
        using var conexion = AbrirConexion();
        using var transaccion = conexion.BeginTransaction(IsolationLevel.Serializable);
        SesionEn(conexion,transaccion,gerente:true);
        using var comando = new SqlCommand("""
            INSERT dbo.Mesas (Numero, Capacidad, Estado)
            VALUES (@numero, @capacidad, N'Disponible');
            """, conexion, transaccion);
        comando.Parameters.Add("@numero", SqlDbType.Int).Value = numero;
        comando.Parameters.Add("@capacidad", SqlDbType.Int).Value = capacidad;
        try { comando.ExecuteNonQuery(); transaccion.Commit(); }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            throw new InvalidOperationException($"Ya existe una mesa con el número {numero}.", ex);
        }
    }

    public void ActualizarMesa(int id, int numero, int capacidad, string estado)
    {
        ExigirSesion(true);
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id), "Seleccione una mesa válida.");
        ValidarMesa(numero, capacidad);
        if (estado is not ("Disponible" or "Ocupada" or "Reservada" or "En limpieza"))
            throw new ArgumentException("El estado de la mesa no es válido.", nameof(estado));

        using var conexion = AbrirConexion();
        using var transaccion = conexion.BeginTransaction(IsolationLevel.Serializable);
        SesionEn(conexion,transaccion,gerente:true);
        using (var bloquear = new SqlCommand("SELECT Id FROM dbo.Mesas WITH (UPDLOCK, ROWLOCK) WHERE Id=@id;", conexion, transaccion))
        {
            bloquear.Parameters.Add("@id", SqlDbType.Int).Value = id;
            if (bloquear.ExecuteScalar() is null) throw new InvalidOperationException("La mesa ya no existe.");
        }
        using (var actual = new SqlCommand("SELECT Estado FROM dbo.Mesas WHERE Id=@id;", conexion, transaccion))
        {
            actual.Parameters.AddWithValue("@id",id);
            var anterior=(string)actual.ExecuteScalar()!;
            if ((anterior is "Ocupada" or "En limpieza") && estado!=anterior)
                throw new InvalidOperationException("Utilice Gestionar cierre para resolver el estado con motivo y auditoría.");
            if (estado=="Ocupada" && anterior!="Ocupada")
                throw new InvalidOperationException("Una mesa se ocupa al registrar el primer producto.");
        }
        if (estado != "Ocupada")
        {
            using var pedidosAbiertos = new SqlCommand("""
                SELECT COUNT(*) FROM dbo.Pedidos
                WHERE MesaId = @id AND Estado IN (N'Abierto', N'En preparación');
                """, conexion, transaccion);
            pedidosAbiertos.Parameters.Add("@id", SqlDbType.Int).Value = id;
            if ((int)pedidosAbiertos.ExecuteScalar()! > 0)
                throw new InvalidOperationException(
                    "No se puede marcar disponible una mesa que todavía tiene pedidos abiertos.");
        }
        using (var renumerar = new SqlCommand("SELECT COUNT(*) FROM dbo.Pedidos p JOIN dbo.Mesas m ON m.Id=p.MesaId WHERE p.MesaId=@id AND m.Numero<>@numero AND p.Estado IN(N'Abierto',N'En preparación');",conexion,transaccion))
        {
            renumerar.Parameters.AddWithValue("@id",id);renumerar.Parameters.AddWithValue("@numero",numero);
            if((int)renumerar.ExecuteScalar()!>0)throw new InvalidOperationException("No renumere una mesa con pedido activo. Resuelva la atención primero.");
        }

        using var comando = new SqlCommand("""
            UPDATE dbo.Mesas
            SET Numero = @numero, Capacidad = @capacidad, Estado = @estado
            WHERE Id = @id;
            """, conexion, transaccion);
        comando.Parameters.Add("@id", SqlDbType.Int).Value = id;
        comando.Parameters.Add("@numero", SqlDbType.Int).Value = numero;
        comando.Parameters.Add("@capacidad", SqlDbType.Int).Value = capacidad;
        comando.Parameters.Add("@estado", SqlDbType.NVarChar, 20).Value = estado;
        try
        {
            if (comando.ExecuteNonQuery() == 0)
                throw new InvalidOperationException("La mesa seleccionada ya no existe.");
            transaccion.Commit();
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            throw new InvalidOperationException($"Ya existe otra mesa con el número {numero}.", ex);
        }
    }

    public void EliminarMesa(int id)
    {
        ExigirSesion(true);
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id), "Seleccione una mesa válida.");
        using var conexion = AbrirConexion();
        using var transaccion = conexion.BeginTransaction(IsolationLevel.Serializable);
        SesionEn(conexion,transaccion,gerente:true);
        using var comando = new SqlCommand("DELETE dbo.Mesas WHERE Id = @id;", conexion, transaccion);
        comando.Parameters.Add("@id", SqlDbType.Int).Value = id;
        try
        {
            if (comando.ExecuteNonQuery() == 0)
                throw new InvalidOperationException("La mesa seleccionada ya no existe.");
            transaccion.Commit();
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            throw new InvalidOperationException(
                "No se puede eliminar esta mesa porque tiene pedidos asociados. Puedes editar su estado en lugar de eliminarla.", ex);
        }
    }

    private static void ValidarMesa(int numero, int capacidad)
    {
        if (numero <= 0) throw new ArgumentOutOfRangeException(nameof(numero), "El número de mesa debe ser mayor que cero.");
        if (capacidad is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(capacidad), "La capacidad debe estar entre 1 y 100 personas.");
    }

    public IReadOnlyList<ElementoMenu> ListarMenu()
    {
        ExigirSesion();
        var resultado = new List<ElementoMenu>();
        using var conexion = AbrirConexion();
        using var comando = new SqlCommand("""
            SELECT Id, Nombre, Tipo, Categoria, Precio, Alcoholica
            FROM dbo.Menu WHERE Activo=1 ORDER BY Categoria, Nombre;
            """, conexion);
        using var lector = comando.ExecuteReader();
        while (lector.Read())
        {
            var id = lector.GetInt32(0);
            var nombre = lector.GetString(1);
            var categoria = lector.GetString(3);
            var precio = lector.GetDecimal(4);
            resultado.Add(lector.GetString(2) == "Bebida"
                ? new Bebida(id, nombre, categoria, precio, lector.GetBoolean(5))
                : new Plato(id, nombre, categoria, precio));
        }
        return resultado;
    }

    public IReadOnlyList<Cliente> ListarClientes() => LeerClientes(false);
    public IReadOnlyList<Cliente> ListarClientesHistoricos() {ExigirSesion(true);return LeerClientes(true);}

    private IReadOnlyList<Cliente> LeerClientes(bool incluirInactivos)
    {
        ExigirSesion();
        var resultado = new List<Cliente>();
        using var conexion = AbrirConexion();
        using var comando = new SqlCommand("""
            SELECT Id, Nombre, Telefono, Correo FROM dbo.Clientes
            WHERE @todos=1 OR Activo=1 ORDER BY Nombre;
            """, conexion);
        comando.Parameters.Add("@todos",SqlDbType.Bit).Value=incluirInactivos;
        using var lector = comando.ExecuteReader();
        while (lector.Read())
            resultado.Add(new(lector.GetInt32(0), lector.GetString(1),
                lector.IsDBNull(2) ? null : lector.GetString(2),
                lector.IsDBNull(3) ? null : lector.GetString(3)));
        return resultado;
    }

    public IReadOnlyList<PedidoResumen> ListarPedidos()
    {
        var actor=ExigirSesion();
        var resultado = new List<PedidoResumen>();
        using var conexion = AbrirConexion();
        using var comando = new SqlCommand("""
            SELECT p.Id, COALESCE(p.MesaHistorica,m.Numero), COALESCE(p.ClienteHistorico,N'Público general'),
             p.Fecha, p.Estado, p.Total, f.NumeroFactura
            FROM dbo.Pedidos p JOIN dbo.Mesas m ON m.Id=p.MesaId
            LEFT JOIN dbo.Clientes c ON c.Id=p.ClienteId
            LEFT JOIN dbo.Facturas f ON f.PedidoId=p.Id
            WHERE @gerente=1 OR p.Estado IN(N'Abierto',N'En preparación') OR f.UsuarioId=@actor
            ORDER BY p.Fecha DESC;
            """, conexion);
        comando.Parameters.AddWithValue("@gerente",actor.Rol is "Administrador" or "Gerente");
        comando.Parameters.AddWithValue("@actor",actor.Id);
        using var lector = comando.ExecuteReader();
        while (lector.Read())
            resultado.Add(new(lector.GetInt32(0), lector.GetInt32(1), lector.GetString(2),
                lector.GetDateTime(3), lector.GetString(4), lector.GetDecimal(5),
                lector.IsDBNull(6) ? null : lector.GetString(6)));
        return resultado;
    }

    public PedidoResumen? ObtenerPedidoAbierto(int mesaId)
    {
        ExigirSesion();
        using var conexion = AbrirConexion();
        using var comando = new SqlCommand("""
            SELECT TOP (1) p.Id, COALESCE(p.MesaHistorica,m.Numero), COALESCE(p.ClienteHistorico,N'Público general'),
                p.Fecha, p.Estado, p.Total, f.NumeroFactura
            FROM dbo.Pedidos p
            JOIN dbo.Mesas m ON m.Id=p.MesaId
            LEFT JOIN dbo.Clientes c ON c.Id=p.ClienteId
            LEFT JOIN dbo.Facturas f ON f.PedidoId=p.Id
            WHERE p.MesaId=@mesa AND p.Estado IN (N'Abierto',N'En preparación')
            ORDER BY p.Fecha DESC, p.Id DESC;
            """, conexion);
        comando.Parameters.Add("@mesa", SqlDbType.Int).Value = mesaId;
        using var lector = comando.ExecuteReader();
        return lector.Read()
            ? new PedidoResumen(lector.GetInt32(0), lector.GetInt32(1), lector.GetString(2),
                lector.GetDateTime(3), lector.GetString(4), lector.GetDecimal(5),
                lector.IsDBNull(6) ? null : lector.GetString(6))
            : null;
    }

    public IReadOnlyList<DetallePedidoResumen> ListarDetallesPedido(int pedidoId)
    {
        ExigirLecturaPedido(pedidoId);
        if (pedidoId <= 0) throw new ArgumentOutOfRangeException(nameof(pedidoId));
        var resultado = new List<DetallePedidoResumen>();
        using var conexion = AbrirConexion();
        using var comando = new SqlCommand($"""
            SELECT d.NombreProducto, d.TipoProducto, SUM(d.Cantidad),
                d.PrecioUnitario, SUM(d.Cantidad*d.PrecioUnitario)
            FROM {TablaDetalles(conexion,null)} d
            WHERE d.PedidoId=@pedido
            GROUP BY d.MenuId,d.NombreProducto,d.TipoProducto,d.PrecioUnitario
            ORDER BY d.NombreProducto;
            """, conexion);
        comando.Parameters.Add("@pedido", SqlDbType.Int).Value = pedidoId;
        using var lector = comando.ExecuteReader();
        while (lector.Read())
            resultado.Add(new(lector.GetString(0), lector.GetString(1), lector.GetInt32(2),
                lector.GetDecimal(3), lector.GetDecimal(4)));
        return resultado;
    }

    private static decimal ObtenerTotalPedido(SqlConnection conexion, SqlTransaction transaccion, int pedidoId)
    {
        using var comando = new SqlCommand(
            "SELECT Total FROM dbo.Pedidos WHERE Id=@pedido;", conexion, transaccion);
        comando.Parameters.Add("@pedido", SqlDbType.Int).Value = pedidoId;
        return (decimal)comando.ExecuteScalar()!;
    }

    public IReadOnlyList<VentaDiaria> ReportarVentas(DateTime desde, DateTime hasta)
    {
        ExigirSesion(true);
        var fechas = ReglasRestaurante.Fechas(desde, hasta);
        var resultado = new List<VentaDiaria>();
        using var conexion = AbrirConexion();
        using var comando = new SqlCommand("""
            SELECT CAST(Fecha AS date), COUNT(*), SUM(Total) FROM dbo.Facturas
            WHERE Fecha>=@desde AND Fecha<@hasta
            GROUP BY CAST(Fecha AS date) ORDER BY CAST(Fecha AS date);
            """, conexion);
        comando.Parameters.Add("@desde", SqlDbType.DateTime2).Value = fechas.Desde;
        comando.Parameters.Add("@hasta", SqlDbType.DateTime2).Value = fechas.HastaExclusivo;
        using var lector = comando.ExecuteReader();
        while (lector.Read())
            resultado.Add(new(lector.GetDateTime(0), lector.GetInt32(1), lector.GetDecimal(2)));
        return resultado;
    }

    public IReadOnlyList<PlatoVendido> ReportarPlatosMasVendidos(DateTime desde, DateTime hasta)
    {
        ExigirSesion(true);
        var fechas = ReglasRestaurante.Fechas(desde, hasta);
        var resultado = new List<PlatoVendido>();
        using var conexion = AbrirConexion();
        using var comando = new SqlCommand($"""
            WITH Ventas AS
            (
                SELECT d.MenuId, d.NombreProducto, d.Cantidad, d.PrecioUnitario,
                    ROW_NUMBER() OVER (PARTITION BY d.MenuId ORDER BY f.Fecha DESC,f.Id DESC,d.Id DESC) AS Orden
                FROM {TablaDetalles(conexion,null)} d
                JOIN dbo.Pedidos p ON p.Id=d.PedidoId
                JOIN dbo.Facturas f ON f.PedidoId=p.Id
                WHERE p.Estado=N'Facturado' AND d.TipoProducto=N'Plato'
                    AND f.Fecha>=@desde AND f.Fecha<@hasta
            )
            SELECT TOP (10) MAX(CASE WHEN Orden=1 THEN NombreProducto END),
                SUM(Cantidad),SUM(Cantidad*PrecioUnitario)
            FROM Ventas
            GROUP BY MenuId ORDER BY SUM(Cantidad) DESC,MenuId;
            """, conexion);
        comando.Parameters.Add("@desde", SqlDbType.DateTime2).Value = fechas.Desde;
        comando.Parameters.Add("@hasta", SqlDbType.DateTime2).Value = fechas.HastaExclusivo;
        using var lector = comando.ExecuteReader();
        while (lector.Read())
            resultado.Add(new(lector.GetString(0), lector.GetInt32(1), lector.GetDecimal(2)));
        return resultado;
    }

    public DatosRestaurante ObtenerRestaurante()
    {
        ExigirSesion();
        using var conexion = AbrirConexion();
        using var comando = new SqlCommand("SELECT Nombre,Ubicacion FROM dbo.Restaurante WHERE Id=1;", conexion);
        using var lector = comando.ExecuteReader();
        return lector.Read() ? new(lector.GetString(0), lector.GetString(1)) : new("Sabores", "Pendiente de configurar");
    }

    public void GuardarRestaurante(string nombre, string ubicacion)
    {
        ExigirSesion(true);
        nombre = ReglasRestaurante.Texto(nombre, "Nombre", 120);
        ubicacion = ReglasRestaurante.Texto(ubicacion, "Ubicación", 250);
        using var conexion = AbrirConexion();
        using var transaccion = conexion.BeginTransaction(IsolationLevel.Serializable);
        SesionEn(conexion,transaccion,gerente:true);
        using var comando = new SqlCommand("UPDATE dbo.Restaurante SET Nombre=@nombre,Ubicacion=@ubicacion WHERE Id=1;", conexion, transaccion);
        comando.Parameters.Add("@nombre", SqlDbType.NVarChar,120).Value = nombre;
        comando.Parameters.Add("@ubicacion", SqlDbType.NVarChar,250).Value = ubicacion;
        if (comando.ExecuteNonQuery()!=1) throw new InvalidOperationException("Ejecute la migración SQL para configurar el restaurante.");
        transaccion.Commit();
    }

    public void GuardarCliente(int? id, string nombre, string? telefono, string? correo)
    {
        ExigirSesion();
        ReglasRestaurante.Cliente(nombre, telefono, correo);
        using var conexion = AbrirConexion();
        using var transaccion = conexion.BeginTransaction(IsolationLevel.Serializable);
        SesionEn(conexion,transaccion,gerente:id.HasValue);
        using var comando = new SqlCommand(id.HasValue
            ? "UPDATE dbo.Clientes SET Nombre=@nombre,Telefono=@telefono,Correo=@correo WHERE Id=@id AND Activo=1;"
            : "INSERT dbo.Clientes(Nombre,Telefono,Correo) VALUES(@nombre,@telefono,@correo);", conexion, transaccion);
        comando.Parameters.Add("@nombre", SqlDbType.NVarChar,120).Value = nombre.Trim();
        comando.Parameters.Add("@telefono", SqlDbType.NVarChar,30).Value = string.IsNullOrWhiteSpace(telefono) ? DBNull.Value : telefono.Trim();
        comando.Parameters.Add("@correo", SqlDbType.NVarChar,150).Value = string.IsNullOrWhiteSpace(correo) ? DBNull.Value : correo.Trim();
        comando.Parameters.Add("@id", SqlDbType.Int).Value = (object?)id ?? DBNull.Value;
                try { if (comando.ExecuteNonQuery()!=1) throw new InvalidOperationException("El cliente ya no está activo."); transaccion.Commit(); }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        { throw new InvalidOperationException("Ya existe un cliente activo con ese correo.",ex); }
    }

    public void DesactivarCliente(int id) => Desactivar("Cliente", id);
    public void DesactivarProducto(int id) => Desactivar("Producto", id);

    private void Desactivar(string entidad, int id)
    {
        ExigirSesion(true);
        if (id<=0) throw new ArgumentException("Seleccione un registro válido.");
        using var conexion = AbrirConexion();
        using var transaccion = conexion.BeginTransaction(IsolationLevel.Serializable);
        SesionEn(conexion,transaccion,gerente:true);
        // SQL choices are fixed; never interpolate user-supplied table names.
        using var comando = new SqlCommand(entidad == "Cliente"
            ? "UPDATE dbo.Clientes SET Activo=0 WHERE Id=@id AND Activo=1;"
            : "UPDATE dbo.Menu SET Activo=0 WHERE Id=@id AND Activo=1;", conexion, transaccion);
        comando.Parameters.Add("@id", SqlDbType.Int).Value=id;
        if (comando.ExecuteNonQuery()!=1) throw new InvalidOperationException("El registro ya no está activo.");
        transaccion.Commit();
    }

    public void GuardarProducto(int? id, string nombre, string categoria, string tipo, decimal precio, bool alcoholica)
    {
        ExigirSesion(true);
        ReglasRestaurante.Producto(nombre, categoria, tipo, precio, alcoholica);
        using var conexion = AbrirConexion();
        using var transaccion = conexion.BeginTransaction(IsolationLevel.Serializable);
        SesionEn(conexion,transaccion,gerente:true);
        using var comando = new SqlCommand(id.HasValue
            ? "UPDATE dbo.Menu SET Nombre=@nombre,Categoria=@categoria,Tipo=@tipo,Precio=@precio,Alcoholica=@alcoholica WHERE Id=@id AND Activo=1;"
            : "INSERT dbo.Menu(Nombre,Categoria,Tipo,Precio,Alcoholica) VALUES(@nombre,@categoria,@tipo,@precio,@alcoholica);", conexion, transaccion);
        comando.Parameters.Add("@nombre", SqlDbType.NVarChar,120).Value=nombre.Trim();
        comando.Parameters.Add("@categoria", SqlDbType.NVarChar,60).Value=categoria.Trim();
        comando.Parameters.Add("@tipo", SqlDbType.NVarChar,20).Value=tipo;
        comando.Parameters.Add("@alcoholica", SqlDbType.Bit).Value=alcoholica;
        comando.Parameters.Add("@id", SqlDbType.Int).Value=(object?)id ?? DBNull.Value;
        AgregarDecimal(comando,"@precio",precio,10);
                try { if (comando.ExecuteNonQuery()!=1) throw new InvalidOperationException("El producto ya no está activo."); transaccion.Commit(); }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        { throw new InvalidOperationException("Ya existe un producto activo con ese nombre, tipo y categoría.",ex); }
    }

    public IReadOnlyList<VentaPeriodo> ReportarVentasPeriodo(DateTime desde, DateTime hasta, string agrupacion)
    {
        ExigirSesion(true);
        ReglasRestaurante.InicioPeriodo(desde, agrupacion);
        return ReportarVentas(desde,hasta)
            .GroupBy(v => ReglasRestaurante.InicioPeriodo(v.Fecha,agrupacion))
            .OrderBy(g => g.Key)
            .Select(g => new VentaPeriodo(g.Key,g.Sum(v=>v.Pedidos),g.Sum(v=>v.Total))).ToList();
    }

    public IReadOnlyList<PedidoResumen> ReportarPedidos(DateTime desde, DateTime hasta, int? mesaId, int? clienteId)
    {
        ExigirSesion(true);
        var fechas=ReglasRestaurante.Fechas(desde,hasta);
        using var conexion=AbrirConexion();
        using var comando=new SqlCommand("""
            SELECT p.Id,COALESCE(p.MesaHistorica,m.Numero),COALESCE(p.ClienteHistorico,N'Público general'),p.Fecha,p.Estado,p.Total,f.NumeroFactura
            FROM dbo.Pedidos p JOIN dbo.Mesas m ON m.Id=p.MesaId
            LEFT JOIN dbo.Clientes c ON c.Id=p.ClienteId
            LEFT JOIN dbo.Facturas f ON f.PedidoId=p.Id
            WHERE p.Fecha>=@desde AND p.Fecha<@hasta AND (@mesa IS NULL OR p.MesaId=@mesa)
              AND (@cliente IS NULL OR p.ClienteId=@cliente OR (@cliente=0 AND p.ClienteId IS NULL))
            ORDER BY p.Fecha DESC,p.Id DESC;
            """,conexion);
        comando.Parameters.Add("@desde",SqlDbType.DateTime2).Value=fechas.Desde;
        comando.Parameters.Add("@hasta",SqlDbType.DateTime2).Value=fechas.HastaExclusivo;
        comando.Parameters.Add("@mesa",SqlDbType.Int).Value=(object?)mesaId ?? DBNull.Value;
        comando.Parameters.Add("@cliente",SqlDbType.Int).Value=(object?)clienteId ?? DBNull.Value;
        using var lector=comando.ExecuteReader();
        var resultado=new List<PedidoResumen>();
        while(lector.Read()) resultado.Add(new(lector.GetInt32(0),lector.GetInt32(1),lector.GetString(2),lector.GetDateTime(3),lector.GetString(4),lector.GetDecimal(5),lector.IsDBNull(6)?null:lector.GetString(6)));
        return resultado;
    }

    private SqlConnection AbrirConexion()
    {
        var conexion = configuracion.CrearConexion();
        try { conexion.Open(); return conexion; }
        catch { conexion.Dispose(); throw; }
    }

    private static void AgregarDecimal(SqlCommand comando, string nombre, decimal valor, byte precision)
    {
        var parametro = comando.Parameters.Add(nombre, SqlDbType.Decimal);
        parametro.Precision = precision;
        parametro.Scale = 2;
        parametro.Value = valor;
    }
}
