using System.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;
using Restaurante.Backend.Dominio;

namespace Restaurante.Backend.Servicios;

public sealed class RepositorioRestaurante(IConfiguracionConexion configuracion) : IRepositorioRestaurante
{
    public UsuarioAutenticado? IniciarSesion(string usuario, string contrasena)
    {
        if (!configuracion.EstaConfigurada) return null;
        using var conexion = AbrirConexion();
        using var comando = new SqlCommand("""
            SELECT Nombre, Rol, ContrasenaHash, Sal
            FROM dbo.Usuarios WHERE Usuario = @usuario AND Activo = 1;
            """, conexion);
        comando.Parameters.Add("@usuario", SqlDbType.NVarChar, 50).Value = usuario.Trim();
        using var lector = comando.ExecuteReader();
        if (!lector.Read()) return null;
        var hash = Convert.ToHexString(SHA256.HashData(
            Encoding.UTF8.GetBytes($"{lector.GetString(3)}:{contrasena}")));
        return CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(lector.GetString(2)), Encoding.ASCII.GetBytes(hash))
            ? new UsuarioAutenticado(lector.GetString(0), lector.GetString(1))
            : null;
    }

    public ResumenRestaurante ObtenerResumen()
    {
        using var conexion = AbrirConexion();
        using var comando = new SqlCommand("""
            SELECT
             (SELECT COUNT(*) FROM dbo.Mesas WHERE Estado = 'Disponible'),
             (SELECT COUNT(*) FROM dbo.Pedidos WHERE Estado IN ('Abierto','En preparación')),
             (SELECT COALESCE(SUM(Total),0) FROM dbo.Facturas WHERE CAST(Fecha AS date)=CAST(GETDATE() AS date)),
             (SELECT COUNT(*) FROM dbo.Menu WHERE Activo=1);
            """, conexion);
        using var lector = comando.ExecuteReader();
        lector.Read();
        return new(lector.GetInt32(0), lector.GetInt32(1), lector.GetDecimal(2), lector.GetInt32(3));
    }

    public IReadOnlyList<Mesa> ListarMesas()
    {
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
        ValidarMesa(numero, capacidad);
        using var conexion = AbrirConexion();
        using var comando = new SqlCommand("""
            INSERT dbo.Mesas (Numero, Capacidad, Estado)
            VALUES (@numero, @capacidad, N'Disponible');
            """, conexion);
        comando.Parameters.Add("@numero", SqlDbType.Int).Value = numero;
        comando.Parameters.Add("@capacidad", SqlDbType.Int).Value = capacidad;
        try { comando.ExecuteNonQuery(); }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            throw new InvalidOperationException($"Ya existe una mesa con el número {numero}.", ex);
        }
    }

    public void ActualizarMesa(int id, int numero, int capacidad, string estado)
    {
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id), "Seleccione una mesa válida.");
        ValidarMesa(numero, capacidad);
        if (estado is not ("Disponible" or "Ocupada" or "Reservada"))
            throw new ArgumentException("El estado de la mesa no es válido.", nameof(estado));

        using var conexion = AbrirConexion();
        if (estado == "Disponible")
        {
            using var pedidosAbiertos = new SqlCommand("""
                SELECT COUNT(*) FROM dbo.Pedidos
                WHERE MesaId = @id AND Estado IN (N'Abierto', N'En preparación');
                """, conexion);
            pedidosAbiertos.Parameters.Add("@id", SqlDbType.Int).Value = id;
            if ((int)pedidosAbiertos.ExecuteScalar()! > 0)
                throw new InvalidOperationException(
                    "No se puede marcar disponible una mesa que todavía tiene pedidos abiertos.");
        }

        using var comando = new SqlCommand("""
            UPDATE dbo.Mesas
            SET Numero = @numero, Capacidad = @capacidad, Estado = @estado
            WHERE Id = @id;
            """, conexion);
        comando.Parameters.Add("@id", SqlDbType.Int).Value = id;
        comando.Parameters.Add("@numero", SqlDbType.Int).Value = numero;
        comando.Parameters.Add("@capacidad", SqlDbType.Int).Value = capacidad;
        comando.Parameters.Add("@estado", SqlDbType.NVarChar, 20).Value = estado;
        try
        {
            if (comando.ExecuteNonQuery() == 0)
                throw new InvalidOperationException("La mesa seleccionada ya no existe.");
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            throw new InvalidOperationException($"Ya existe otra mesa con el número {numero}.", ex);
        }
    }

    public void EliminarMesa(int id)
    {
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id), "Seleccione una mesa válida.");
        using var conexion = AbrirConexion();
        using var comando = new SqlCommand("DELETE dbo.Mesas WHERE Id = @id;", conexion);
        comando.Parameters.Add("@id", SqlDbType.Int).Value = id;
        try
        {
            if (comando.ExecuteNonQuery() == 0)
                throw new InvalidOperationException("La mesa seleccionada ya no existe.");
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

    public IReadOnlyList<Cliente> ListarClientes()
    {
        var resultado = new List<Cliente>();
        using var conexion = AbrirConexion();
        using var comando = new SqlCommand("""
            SELECT Id, Nombre, Telefono, Correo FROM dbo.Clientes
            WHERE Activo=1 ORDER BY Nombre;
            """, conexion);
        using var lector = comando.ExecuteReader();
        while (lector.Read())
            resultado.Add(new(lector.GetInt32(0), lector.GetString(1),
                lector.IsDBNull(2) ? null : lector.GetString(2),
                lector.IsDBNull(3) ? null : lector.GetString(3)));
        return resultado;
    }

    public IReadOnlyList<PedidoResumen> ListarPedidos()
    {
        var resultado = new List<PedidoResumen>();
        using var conexion = AbrirConexion();
        using var comando = new SqlCommand("""
            SELECT TOP (100) p.Id, m.Numero, COALESCE(c.Nombre,'Público general'),
             p.Fecha, p.Estado, p.Total, f.NumeroFactura
            FROM dbo.Pedidos p JOIN dbo.Mesas m ON m.Id=p.MesaId
            LEFT JOIN dbo.Clientes c ON c.Id=p.ClienteId
            LEFT JOIN dbo.Facturas f ON f.PedidoId=p.Id
            ORDER BY p.Fecha DESC;
            """, conexion);
        using var lector = comando.ExecuteReader();
        while (lector.Read())
            resultado.Add(new(lector.GetInt32(0), lector.GetInt32(1), lector.GetString(2),
                lector.GetDateTime(3), lector.GetString(4), lector.GetDecimal(5),
                lector.IsDBNull(6) ? null : lector.GetString(6)));
        return resultado;
    }

    public PedidoResumen? ObtenerPedidoAbierto(int mesaId)
    {
        using var conexion = AbrirConexion();
        using var comando = new SqlCommand("""
            SELECT TOP (1) p.Id, m.Numero, COALESCE(c.Nombre,N'Público general'),
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
        if (pedidoId <= 0) throw new ArgumentOutOfRangeException(nameof(pedidoId));
        var resultado = new List<DetallePedidoResumen>();
        using var conexion = AbrirConexion();
        using var comando = new SqlCommand("""
            SELECT m.Nombre, m.Tipo, SUM(d.Cantidad),
                d.PrecioUnitario, SUM(d.Cantidad*d.PrecioUnitario)
            FROM dbo.DetallesPedido d
            JOIN dbo.Menu m ON m.Id=d.MenuId
            WHERE d.PedidoId=@pedido
            GROUP BY m.Nombre,m.Tipo,d.PrecioUnitario
            ORDER BY m.Nombre;
            """, conexion);
        comando.Parameters.Add("@pedido", SqlDbType.Int).Value = pedidoId;
        using var lector = comando.ExecuteReader();
        while (lector.Read())
            resultado.Add(new(lector.GetString(0), lector.GetString(1), lector.GetInt32(2),
                lector.GetDecimal(3), lector.GetDecimal(4)));
        return resultado;
    }

    public PedidoCreado CrearPedido(SolicitudPedido solicitud)
    {
        if (solicitud.MesaId <= 0 || solicitud.Detalles.Count == 0 ||
            solicitud.Detalles.Any(x => x.PlatoId <= 0 || x.Cantidad is < 1 or > 50))
            throw new ArgumentException("Seleccione una mesa y productos con cantidades válidas.");

        using var conexion = AbrirConexion();
        using var transaccion = conexion.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            using (var mesa = new SqlCommand(
                "SELECT Estado FROM dbo.Mesas WITH (UPDLOCK, ROWLOCK) WHERE Id=@id;",
                conexion, transaccion))
            {
                mesa.Parameters.Add("@id", SqlDbType.Int).Value = solicitud.MesaId;
                var estado = mesa.ExecuteScalar() as string;
                if (estado is null) throw new InvalidOperationException("La mesa indicada no existe.");
                if (estado != "Disponible") throw new InvalidOperationException("La mesa no está disponible.");
            }

            var detalles = new List<(DetallePedidoSolicitud Solicitud, decimal Precio)>();
            foreach (var detalle in solicitud.Detalles)
            {
                using var precio = new SqlCommand(
                    "SELECT Precio FROM dbo.Menu WHERE Id=@id AND Activo=1;",
                    conexion, transaccion);
                precio.Parameters.Add("@id", SqlDbType.Int).Value = detalle.PlatoId;
                var valor = precio.ExecuteScalar();
                if (valor is null) throw new InvalidOperationException($"El producto {detalle.PlatoId} no está disponible.");
                detalles.Add((detalle, (decimal)valor));
            }

            var total = detalles.Sum(x => x.Solicitud.Cantidad * x.Precio);
            int pedidoId;
            using (var pedido = new SqlCommand("""
                INSERT dbo.Pedidos (MesaId, ClienteId, Estado, Total)
                OUTPUT INSERTED.Id VALUES (@mesa,@cliente,N'Abierto',@total);
                """, conexion, transaccion))
            {
                pedido.Parameters.Add("@mesa", SqlDbType.Int).Value = solicitud.MesaId;
                pedido.Parameters.Add("@cliente", SqlDbType.Int).Value =
                    (object?)solicitud.ClienteId ?? DBNull.Value;
                AgregarDecimal(pedido, "@total", total, 12);
                pedidoId = (int)pedido.ExecuteScalar()!;
            }

            foreach (var detalle in detalles)
            {
                using var linea = new SqlCommand("""
                    INSERT dbo.DetallesPedido (PedidoId,MenuId,Cantidad,PrecioUnitario)
                    VALUES (@pedido,@menu,@cantidad,@precio);
                    """, conexion, transaccion);
                linea.Parameters.Add("@pedido", SqlDbType.Int).Value = pedidoId;
                linea.Parameters.Add("@menu", SqlDbType.Int).Value = detalle.Solicitud.PlatoId;
                linea.Parameters.Add("@cantidad", SqlDbType.Int).Value = detalle.Solicitud.Cantidad;
                AgregarDecimal(linea, "@precio", detalle.Precio, 10);
                linea.ExecuteNonQuery();
            }

            using (var actualizar = new SqlCommand(
                "UPDATE dbo.Mesas SET Estado='Ocupada' WHERE Id=@id;",
                conexion, transaccion))
            {
                actualizar.Parameters.Add("@id", SqlDbType.Int).Value = solicitud.MesaId;
                actualizar.ExecuteNonQuery();
            }
            transaccion.Commit();
            return new(pedidoId, total, null);
        }
        catch
        {
            transaccion.Rollback();
            throw;
        }
    }

    public decimal AgregarProductoAPedido(int pedidoId, int menuId, int cantidad)
    {
        if (pedidoId <= 0) throw new ArgumentOutOfRangeException(nameof(pedidoId));
        if (menuId <= 0) throw new ArgumentOutOfRangeException(nameof(menuId));
        if (cantidad is < 1 or > 50)
            throw new ArgumentOutOfRangeException(nameof(cantidad), "La cantidad debe estar entre 1 y 50.");

        using var conexion = AbrirConexion();
        using var transaccion = conexion.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            using var pedido = new SqlCommand("""
                SELECT Estado FROM dbo.Pedidos WITH (UPDLOCK, ROWLOCK)
                WHERE Id=@pedido;
                """, conexion, transaccion);
            pedido.Parameters.Add("@pedido", SqlDbType.Int).Value = pedidoId;
            var estado = pedido.ExecuteScalar() as string;
            if (estado is null) throw new InvalidOperationException("El pedido ya no existe.");
            if (estado is not ("Abierto" or "En preparación"))
                throw new InvalidOperationException("No se pueden añadir productos a un pedido facturado o cerrado.");

            using var precio = new SqlCommand(
                "SELECT Precio FROM dbo.Menu WHERE Id=@menu AND Activo=1;",
                conexion, transaccion);
            precio.Parameters.Add("@menu", SqlDbType.Int).Value = menuId;
            var valor = precio.ExecuteScalar();
            if (valor is null) throw new InvalidOperationException("El producto ya no está disponible.");
            var precioUnitario = (decimal)valor;

            using (var detalle = new SqlCommand("""
                INSERT dbo.DetallesPedido (PedidoId,MenuId,Cantidad,PrecioUnitario)
                VALUES (@pedido,@menu,@cantidad,@precio);
                """, conexion, transaccion))
            {
                detalle.Parameters.Add("@pedido", SqlDbType.Int).Value = pedidoId;
                detalle.Parameters.Add("@menu", SqlDbType.Int).Value = menuId;
                detalle.Parameters.Add("@cantidad", SqlDbType.Int).Value = cantidad;
                AgregarDecimal(detalle, "@precio", precioUnitario, 10);
                detalle.ExecuteNonQuery();
            }

            using (var total = new SqlCommand("""
                UPDATE dbo.Pedidos
                SET Total = Total + @importe
                WHERE Id=@pedido AND Estado IN (N'Abierto',N'En preparación');
                """, conexion, transaccion))
            {
                total.Parameters.Add("@pedido", SqlDbType.Int).Value = pedidoId;
                AgregarDecimal(total, "@importe", precioUnitario * cantidad, 12);
                if (total.ExecuteNonQuery() != 1)
                    throw new InvalidOperationException("El pedido dejó de estar abierto.");
            }

            var nuevoTotal = ObtenerTotalPedido(conexion, transaccion, pedidoId);
            transaccion.Commit();
            return nuevoTotal;
        }
        catch
        {
            transaccion.Rollback();
            throw;
        }
    }

    public string FacturarPedido(int pedidoId)
    {
        if (pedidoId <= 0) throw new ArgumentOutOfRangeException(nameof(pedidoId));
        using var conexion = AbrirConexion();
        using var transaccion = conexion.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            int mesaId;
            decimal total;
            using (var pedido = new SqlCommand("""
                SELECT MesaId, Total, Estado FROM dbo.Pedidos WITH (UPDLOCK, ROWLOCK)
                WHERE Id=@pedido;
                """, conexion, transaccion))
            {
                pedido.Parameters.Add("@pedido", SqlDbType.Int).Value = pedidoId;
                using var lector = pedido.ExecuteReader();
                if (!lector.Read()) throw new InvalidOperationException("El pedido ya no existe.");
                mesaId = lector.GetInt32(0);
                total = lector.GetDecimal(1);
                var estado = lector.GetString(2);
                if (estado is not ("Abierto" or "En preparación"))
                    throw new InvalidOperationException("Este pedido ya fue facturado o cerrado.");
            }

            if (total <= 0)
                throw new InvalidOperationException("Agrega al menos un producto antes de facturar el pedido.");

            string numeroFactura;
            using (var factura = new SqlCommand("""
                INSERT dbo.Facturas (PedidoId,Subtotal,Impuesto,Total)
                OUTPUT INSERTED.NumeroFactura VALUES (@pedido,@subtotal,0,@total);
                """, conexion, transaccion))
            {
                factura.Parameters.Add("@pedido", SqlDbType.Int).Value = pedidoId;
                AgregarDecimal(factura, "@subtotal", total, 12);
                AgregarDecimal(factura, "@total", total, 12);
                numeroFactura = (string)factura.ExecuteScalar()!;
            }

            using (var cerrarPedido = new SqlCommand("""
                UPDATE dbo.Pedidos SET Estado=N'Facturado'
                WHERE Id=@pedido AND Estado IN (N'Abierto',N'En preparación');
                """, conexion, transaccion))
            {
                cerrarPedido.Parameters.Add("@pedido", SqlDbType.Int).Value = pedidoId;
                if (cerrarPedido.ExecuteNonQuery() != 1)
                    throw new InvalidOperationException("El pedido dejó de estar abierto.");
            }

            using (var liberarMesa = new SqlCommand("""
                UPDATE dbo.Mesas SET Estado=N'Disponible'
                WHERE Id=@mesa AND NOT EXISTS
                    (SELECT 1 FROM dbo.Pedidos WHERE MesaId=@mesa
                     AND Estado IN (N'Abierto',N'En preparación'));
                """, conexion, transaccion))
            {
                liberarMesa.Parameters.Add("@mesa", SqlDbType.Int).Value = mesaId;
                liberarMesa.ExecuteNonQuery();
            }

            transaccion.Commit();
            return numeroFactura;
        }
        catch
        {
            transaccion.Rollback();
            throw;
        }
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
        var resultado = new List<VentaDiaria>();
        using var conexion = AbrirConexion();
        using var comando = new SqlCommand("""
            SELECT CAST(Fecha AS date), COUNT(*), SUM(Total) FROM dbo.Facturas
            WHERE Fecha>=@desde AND Fecha<@hasta
            GROUP BY CAST(Fecha AS date) ORDER BY CAST(Fecha AS date);
            """, conexion);
        comando.Parameters.Add("@desde", SqlDbType.DateTime2).Value = desde.Date;
        comando.Parameters.Add("@hasta", SqlDbType.DateTime2).Value = hasta.Date.AddDays(1);
        using var lector = comando.ExecuteReader();
        while (lector.Read())
            resultado.Add(new(lector.GetDateTime(0), lector.GetInt32(1), lector.GetDecimal(2)));
        return resultado;
    }

    public IReadOnlyList<PlatoVendido> ReportarPlatosMasVendidos()
    {
        var resultado = new List<PlatoVendido>();
        using var conexion = AbrirConexion();
        using var comando = new SqlCommand("""
            SELECT TOP (10) m.Nombre, SUM(d.Cantidad), SUM(d.Cantidad*d.PrecioUnitario)
            FROM dbo.DetallesPedido d JOIN dbo.Menu m ON m.Id=d.MenuId
            GROUP BY m.Nombre ORDER BY SUM(d.Cantidad) DESC;
            """, conexion);
        using var lector = comando.ExecuteReader();
        while (lector.Read())
            resultado.Add(new(lector.GetString(0), lector.GetInt32(1), lector.GetDecimal(2)));
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
