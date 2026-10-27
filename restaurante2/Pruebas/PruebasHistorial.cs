using System.Data;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Restaurante.Backend.Dominio;
using Restaurante.Backend.Servicios;

internal static class PruebasHistorial
{
    public static void Ejecutar(string cadena, Action<string,Action> caso)
    {
        if (!new SqlConnectionStringBuilder(cadena).InitialCatalog.StartsWith("Restaurante_Pruebas_",StringComparison.Ordinal))
            throw new InvalidOperationException("Se requiere una base aislada.");
        var repo=PruebasCobros.Repo(cadena);
        var hoy=DateTime.Today;
        int numero=9000;
        int Producto(string nombre,string tipo,decimal precio)
        {
            repo.GuardarProducto(null,nombre,"Historial",tipo,precio,false);
            return repo.ListarMenu().Single(p=>p.Nombre==nombre).Id;
        }
        PedidoCreado Pedido(int menu,int cantidad,bool facturar=true)
        {
            repo.CrearMesa(++numero,4);
            var mesa=repo.ListarMesas().Single(m=>m.Numero==numero);
            var pedido=repo.CrearPedido(new(){MesaId=mesa.Id,Detalles=[new(menu,cantidad)]});
            if(facturar) PruebasCobros.Facturar(repo,pedido.Id);
            return pedido;
        }
        caso("Historial: plato facturado conserva tipo, nombre e importe tras edición y baja",()=>
        {
            var id=Producto("Histórico plato A","Plato",10m);
            var pedido=Pedido(id,2);
            repo.GuardarProducto(id,"Catálogo bebida A","Otra categoría","Bebida",99m,false);
            repo.DesactivarProducto(id);
            var detalle=repo.ListarDetallesPedido(pedido.Id).Single();
            Igual("Histórico plato A",detalle.Producto); Igual("Plato",detalle.Tipo); Igual(20m,detalle.Importe);
            var venta=repo.ReportarPlatosMasVendidos(hoy,hoy).Single(p=>p.Nombre=="Histórico plato A");
            Igual(2,venta.Cantidad); Igual(20m,venta.Total);
        });
        caso("Historial: bebida vendida no se convierte retroactivamente en plato",()=>
        {
            var id=Producto("Histórica bebida B","Bebida",5m);
            var bebida=Pedido(id,4);
            repo.GuardarProducto(id,"Nuevo plato B","Historial","Plato",7m,false);
            Pedido(id,3);
            Igual("Bebida",repo.ListarDetallesPedido(bebida.Id).Single().Tipo);
            var venta=repo.ReportarPlatosMasVendidos(hoy,hoy).Single(p=>p.Nombre=="Nuevo plato B");
            Igual(3,venta.Cantidad); Igual(21m,venta.Total);
        });
        caso("Historial: distintas versiones se capturan al agregar y se agrupan una vez",()=>
        {
            var id=Producto("Nombre C anterior","Plato",10m);
            var pedido=Pedido(id,2,false);
            repo.GuardarProducto(id,"Nombre C nuevo","Historial","Plato",12.5m,false);
            Igual(57.5m,repo.AgregarProductoAPedido(pedido.Id,id,3));
            PruebasCobros.Facturar(repo,pedido.Id);
            var detalles=repo.ListarDetallesPedido(pedido.Id);
            Igual(2,detalles.Count); Igual(20m,detalles.Single(d=>d.Producto=="Nombre C anterior").Importe);
            Igual(37.5m,detalles.Single(d=>d.Producto=="Nombre C nuevo").Importe);
            var ventas=repo.ReportarPlatosMasVendidos(hoy,hoy);
            Igual(1,ventas.Count(v=>v.Nombre=="Nombre C nuevo"));
            Igual(5,ventas.Single(v=>v.Nombre=="Nombre C nuevo").Cantidad);
            Igual(57.5m,ventas.Single(v=>v.Nombre=="Nombre C nuevo").Total);
            repo.GuardarProducto(id,"Nombre C mañana","Historial","Plato",13m,false);
            var siguiente=Pedido(id,1);
            Sql(cadena,$"UPDATE dbo.Facturas SET Fecha=DATEADD(day,1,CAST(GETDATE() AS date)) WHERE PedidoId={siguiente.Id};");
            Igual(5,repo.ReportarPlatosMasVendidos(hoy,hoy).Single(v=>v.Nombre=="Nombre C nuevo").Cantidad);
            Igual(1,repo.ReportarPlatosMasVendidos(hoy.AddDays(1),hoy.AddDays(1)).Single(v=>v.Nombre=="Nombre C mañana").Cantidad);
            var intervalo=repo.ReportarPlatosMasVendidos(hoy,hoy.AddDays(1)).Single(v=>v.Nombre=="Nombre C mañana");
            Igual(6,intervalo.Cantidad); Igual(70.5m,intervalo.Total);
        });
        caso("Historial: migración repetida no sobrescribe capturas ni cambia importes",()=>
        {
            var ruta=Environment.GetEnvironmentVariable("RESTAURANTE_MIGRACION_003") ?? throw new Exception("Falta ruta de migración.");
            var antes=Escalar(cadena,"SELECT CONCAT(COUNT(*),'|',SUM(Cantidad*PrecioUnitario),'|',CHECKSUM_AGG(BINARY_CHECKSUM(Id,MenuId,NombreProducto,TipoProducto,PrecioUnitario,Cantidad))) FROM dbo.DetallesPedido;");
            Migrar(cadena,File.ReadAllText(ruta)); Migrar(cadena,File.ReadAllText(ruta));
            Igual(antes,Escalar(cadena,"SELECT CONCAT(COUNT(*),'|',SUM(Cantidad*PrecioUnitario),'|',CHECKSUM_AGG(BINARY_CHECKSUM(Id,MenuId,NombreProducto,TipoProducto,PrecioUnitario,Cantidad))) FROM dbo.DetallesPedido;"));
        });
        var anterior=Environment.GetEnvironmentVariable("RESTAURANTE_TEST_ANTERIOR");
        if(!string.IsNullOrEmpty(anterior)) caso("Historial: actualización de versión 2 conserva factura e inicializa solo lo conocido",()=>
        {
            var legado=new RepositorioRestaurante(new ConexionPrueba(anterior));
            var pedido=legado.ListarPedidos().Single(); var detalle=legado.ListarDetallesPedido(pedido.Id).Single();
            Igual("Producto legado",detalle.Producto); Igual("Plato",detalle.Tipo); Igual(20m,detalle.Importe); Igual(20m,pedido.Total);
            Igual("20.00",Escalar(anterior,"SELECT CONVERT(varchar(30),Total) FROM dbo.Facturas;"));
            Igual("2",Escalar(anterior,"SELECT COUNT(*) FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.DetallesPedido') AND name IN(N'NombreProducto',N'TipoProducto') AND is_nullable=0;"));
            var ruta=Environment.GetEnvironmentVariable("RESTAURANTE_MIGRACION_003")!;
            Migrar(anterior,File.ReadAllText(ruta));
            Igual("Producto legado",legado.ListarDetallesPedido(pedido.Id).Single().Producto);
        });
    }
    private static void Migrar(string cadena,string sql)
    {
        using var conexion=new SqlConnection(cadena); conexion.Open();
        foreach(var lote in Regex.Split(sql,@"^\s*GO\s*$",RegexOptions.Multiline|RegexOptions.IgnoreCase))
        {
            if(string.IsNullOrWhiteSpace(lote))continue;
            using var comando=new SqlCommand(lote,conexion); comando.ExecuteNonQuery();
        }
    }
    private static string Escalar(string cadena,string sql)
    {
        using var conexion=new SqlConnection(cadena); conexion.Open();
        using var comando=new SqlCommand(sql,conexion); return Convert.ToString(comando.ExecuteScalar(),System.Globalization.CultureInfo.InvariantCulture)!;
    }
    private static void Sql(string cadena,string sql)
    {
        using var conexion=new SqlConnection(cadena); conexion.Open();using var comando=new SqlCommand(sql,conexion);comando.ExecuteNonQuery();
    }
    private static void Igual<T>(T esperado,T actual)
    {
        if(!EqualityComparer<T>.Default.Equals(esperado,actual)) throw new Exception($"Esperado {esperado}; obtenido {actual}");
    }
}
