# Sabores — sistema de gestión de restaurante

Solución .NET con exactamente dos proyectos:

- `Restaurante.Frontend`: aplicación de escritorio Windows Forms (`net8.0-windows`).
- `Restaurante.Backend`: biblioteca de clases con dominio, interfaces, conexión y operaciones SQL.

El acceso SQL es síncrono con `SqlConnection`, `SqlCommand` y `SqlDataReader`; no utiliza métodos asíncronos, Entity Framework, `DataAdapter` ni `DataSet`.

## Inicio rápido

1. Ejecuta `BaseDatos\CrearBaseDatos.sql` en SQL Server Management Studio.
2. Abre `Restaurante.sln` en Visual Studio y ejecuta el proyecto `Restaurante.Frontend`.
3. Inicia la configuración inicial con `admin` / `Admin123!`.
4. En **Administración → Conexión SQL**, configura la **IP del servidor**, el puerto (1433 por defecto), el nombre de base de datos, usuario SQL y contraseña. La aplicación prueba la conexión antes de guardarla.
5. Inicia sesión nuevamente con el usuario `admin` creado en la base de datos.

Las credenciales se protegen con DPAPI en el perfil del usuario de Windows.

Ejemplo de cadena de conexión:

```text
Server=192.168.1.100,1433;Database=RestauranteDB;User Id=restaurante_app;Password=TuClaveSegura;Encrypt=True;TrustServerCertificate=True;
```

El ejemplo es ilustrativo: reemplaza IP, usuario y contraseña con los de tu servidor. Para autenticación de Windows deja vacío el usuario SQL. En producción crea un login SQL de mínimo privilegio; no uses la cuenta `sa`.

## Funcionalidades

- Inicio y cierre de sesión, con rol administrador.
- Panel de ventas, pedidos, mesas y productos.
- Consulta de mesas, clientes, menú y pedidos.
- Gestión completa de mesas: crear, editar, visualizar y eliminar (sin borrar mesas con pedidos asociados).
- Registro transaccional de pedidos abiertos, agregar platos o bebidas al pedido desde el detalle de la mesa y facturar/liberar la mesa cuando se retiren los comensales.
- Reportes de ventas por fecha y productos más vendidos.
- Configuración administrativa de la conexión SQL Server.

Para atender una mesa, abre un pedido desde **Pedidos y facturas**; la factura no se genera al iniciarlo. En **Menú y mesas → Mesas → Visualizar** puedes añadir bebidas y platillos al pedido abierto. Cuando los comensales se retiren, pulsa **Facturar y liberar mesa** en el detalle de la mesa o factura el pedido seleccionado desde la lista. Se genera una sola factura y la mesa vuelve a estar disponible.

`ElementoMenu` es una clase abstracta; `Plato` y `Bebida` heredan y permiten polimorfismo. Las interfaces de conexión y repositorio separan la UI de la biblioteca backend.

## Diagramas y diseño

- `Diagramas\DiagramaBaseDatos.mmd`: diagrama entidad-relación Mermaid, editable en draw.io.
- `Diagramas\DiagramaClases.mmd`: diagrama de clases Mermaid, editable en draw.io.
- `Diagramas\DiagramaBaseDatos.png` y `Diagramas\DiagramaClases.png`: exportaciones PNG.
- `Diagramas\MockupFigma.svg`: mockup vectorial editable al importarlo en Figma; no es un archivo binario `.fig`.
