# Requisitos y trazabilidad

Esta matriz convierte el enunciado aportado por el usuario en requisitos verificables. No acredita la entrega académica ni la autoría del grupo. La implementación se encuentra en la rama `correcciones/auditoria-final`.

## Funcionales del enunciado

| ID | Requisito / aceptación | Pantalla | Implementación / tablas | Evidencia de prueba |
|---|---|---|---|---|
| RF01 | Guardar nombre y ubicación y recuperarlos con otra conexión | Catálogos → Restaurante | GuardarRestaurante / ObtenerRestaurante; Restaurante | Configuración persistente del restaurante |
| RF02 | Crear, editar y consultar mesas; rechazar números duplicados y eliminar solo sin historial | Menú y mesas | CrearMesa / ActualizarMesa / EliminarMesa; Mesas | Mesa: duplicados y validaciones |
| RF03 | Registrar, editar y desactivar platos y bebidas con precios válidos | Catálogos → Productos | GuardarProducto / DesactivarProducto; Menu | Mantenimiento; validación de productos |
| RF04 | Registrar, editar y desactivar clientes sin borrar su historial | Catálogos → Clientes | GuardarCliente / DesactivarCliente; Clientes | Mantenimiento y desactivación conserva historial |
| RF05 | Abrir pedido en mesa disponible y asignar cliente o público general | Pedidos y facturas | CrearPedido; Pedidos, Mesas, Clientes | Pedido y recorrido de interfaz |
| RF06 | Agregar varios productos con cantidades válidas y total decimal exacto | Detalle de mesa | AgregarProductoAPedido; DetallesPedido | Pedido y recorrido de dos productos |
| RF07 | Emitir una sola factura, cerrar pedido y liberar mesa | Detalle / Pedidos y facturas | FacturarPedido; Facturas, Pedidos, Mesas | Facturación única, concurrente y cortesía cero |
| RF08 | Reportar ventas por día | Reportes → Ventas | ReportarVentas / ReportarVentasPeriodo; Facturas | Reportes reconciliados |
| RF09 | Reportar ventas semanales de lunes a domingo | Reportes → Ventas | InicioPeriodo / ReportarVentasPeriodo | Agrupación semanal entre años |
| RF10 | Reportar ventas por mes | Reportes → Ventas | InicioPeriodo / ReportarVentasPeriodo | Agrupación mensual separada |
| RF11 | Filtrar pedidos por mesa, cliente y fechas; incluir clientes inactivos en historial | Reportes → Pedidos | ReportarPedidos / ListarClientesHistoricos | Filtros y conservación de historial |
| RF12 | Informar platos vendidos por unidades sobre facturas del intervalo | Reportes → Platos | ReportarPlatosMasVendidos; identidad histórica en DetallesPedido | Cinco casos de regresión histórica |
| RF13 | Login válido, inválido/inactivo rechazado y logout con nueva sesión | Acceso / Principal | IniciarSesion / CicloSesion; Usuarios | Login de repositorio y recorrido con dos usuarios |
| RF14 | Menú principal, información base, movimientos y consultas accesibles | Principal, catálogos, servicio y reportes | Métodos Mostrar* y servicios del Backend | Carga de páginas y recorrido de interfaz |
| RF15 | Guardar la información del negocio en BD y recuperarla al reiniciar | Aplicación completa | Repositorio y SQL Server | Recuperación de factura en otro proceso |

## Técnicos y requisitos adicionales del usuario

| ID | Requisito | Evidencia / aceptación | Estado |
|---|---|---|---|
| RT01 | Windows Forms | UseWindowsForms, seis formularios, compilación Debug/Release | Implementado; compilar en cada entrega |
| RT02 | POO: abstracción, encapsulamiento, herencia y polimorfismo | ElementoMenu, Plato/Bebida, propiedades y objetos; cálculo real en CrearPedido | Implementado y probado |
| RT03 | Interfaces reales | IRepositorioRestaurante, IConfiguracionConexion; IDialogosUsuario como límite de presentación | Implementado |
| RT04 | Una clase, record o interfaz por archivo | Fuentes individuales; formularios partial como excepción | Implementado |
| RT05 | Designer editable | InitializeComponent, controles, eventos y Dispose; constructor de diseño sin SQL | Construcción/render probados; edición dentro de Visual Studio pendiente |
| RT06 | Validaciones y seguridad de datos | Parámetros SQL, relaciones, restricciones, using y transacciones | Pruebas de rechazo, rollback y concurrencia |
| RT07 | Identidad histórica de productos | Nombre, tipo y precio capturados por detalle; edición del catálogo no reclasifica ventas | Regresión H1 corregida |
| RT08 | Migración preservando datos | 003 transaccional, repetible y sin sobrescribir capturas; fixture de versión 2 | Probado en bases aisladas |
| RT09 | Sesiones y persistencia independientes de la configuración real | CicloSesion y ConfiguracionConexionPruebas | Dos procesos y dos usuarios en pruebas aisladas |
| RT10 | Controles accesibles en tamaños y escalas disponibles | Prueba de acción de pedido en tamaño mínimo y captura del DPI disponible | No certifica todas las escalas/monitores; ensayo manual pendiente |

## Reglas históricas explícitas

- RF06/RT07 capturan por cada agregado: el mismo producto puede tener líneas de diferentes nombres, tipos o precios.
- RF12 agrupa por MenuId. Solo suma detalles capturados como Plato de pedidos facturados. El rótulo es el último nombre histórico vendido como plato dentro del intervalo; se desempata por factura y detalle.
- El cambio posterior de nombre/tipo/categoría/precio o la baja no actualizan capturas ni importes anteriores.
- Al actualizar datos anteriores a la 003, el catálogo disponible es la única referencia para inicializar valores faltantes. No se garantiza reconstruir cambios desconocidos anteriores.
- Impresión/PDF de factura, cancelaciones y retirada de líneas no se convierten aquí en requisitos obligatorios: no aparecen expresamente en la guía aportada.

## Entregables del grupo: no verificables con los archivos actuales

| ID | Entregable | Información necesaria |
|---|---|---|
| EA01 | Investigación del funcionamiento de un restaurante | Fuentes y desarrollo del grupo |
| EA02 | Informe académico formal | Estructura, contenido y autores reales |
| EA03 | Video con todos los integrantes | Video/enlace y permisos |
| EA04 | Cinco integrantes y participación | Nombres y evidencias verificables |
| EA05 | Enlaces a solución, documentación y video | Acceso comprobado para el docente |
| EA06 | Defensa de 30 minutos ±5 y una entrega por grupo | Guion, responsables y ensayo |

Fecha indicada por la guía: 1 de noviembre de 2026 o, en su defecto, último día de clases. La alternativa confirmada por el docente no se ha verificado. No se inventan autores, participación, fuentes o enlaces de entrega.
