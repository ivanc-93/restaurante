# Sabores — gestión de restaurante

Aplicación C# Windows Forms sobre .NET 8 para mesas, clientes, menú, pedidos, facturación y reportes. La solución contiene dos proyectos: `Restaurante.Frontend` y `Restaurante.Backend`.

## Abrir y preparar

La raíz del repositorio contiene la carpeta `restaurante2`; abra `restaurante2/Restaurante.sln`, no una copia anterior de la solución.

Requisitos: Windows, Visual Studio con desarrollo de escritorio .NET, SDK .NET 8 o posterior y SQL Server. Para compilar desde consola:

```powershell
dotnet restore restaurante2/Restaurante.sln
dotnet build restaurante2/Restaurante.sln --no-restore
```

**Instalación nueva:** ejecute `BaseDatos/CrearBaseDatos.sql` en SQL Server. Crea `RestauranteDB`, las tablas y datos iniciales, incluida la estructura de la versión 3.

**Base existente:** haga un respaldo, seleccione explícitamente la base correspondiente y ejecute en orden `BaseDatos/Migracion_002.sql` y `BaseDatos/Migracion_003.sql`. Si la 002 ya fue aplicada, basta con la 003. La aplicación no ejecuta migraciones automáticamente. El script es transaccional e idempotente; ante duplicados o datos inválidos aborta y explica la causa, sin borrar registros. Si cambia el nombre de la base en una instalación nueva, adapte las referencias `RestauranteDB` del script antes de ejecutarlo.

El script configura las opciones SET requeridas por índices filtrados y la columna calculada del número de factura. La cuenta utilizada para migrar necesita permisos DDL; la cuenta de operación no necesita esos permisos.

## Primer acceso y conexión

Sin conexión guardada se permite la configuración inicial con `admin` / `Admin123!`. Desde **Conexión SQL**, indique servidor, puerto, base de datos y credenciales; dejar vacío el usuario SQL habilita autenticación de Windows. Después de guardar, debe ingresar otra vez y autenticarse contra la base. El script crea el usuario inicial `admin` con esa contraseña; cámbiela en la base antes de utilizar datos reales. Utilice un usuario SQL de mínimo privilegio, no `sa`.

La conexión se guarda mediante DPAPI en el perfil de Windows. Si el archivo está dañado o no puede descifrarse, el programa avisa y permite reconfigurarlo. Una conexión que abre correctamente no garantiza que el esquema esté actualizado: aplique primero la migración.

## Operación

- **Clientes y productos:** registrar, editar y desactivar. No se borran pedidos ni facturas al desactivar. Los clientes activos no pueden compartir un correo no nulo; los productos activos no pueden repetir nombre, tipo y categoría. Se permiten clientes diferentes con el mismo nombre.
- **Restaurante:** el administrador configura nombre y ubicación desde el selector de esa misma pantalla. Estos datos se almacenan en SQL y aparecen en el título principal.
- **Menú y mesas:** consultar menú; crear, editar, visualizar y eliminar mesas sin historial. Una mesa con pedidos asociados no se elimina.
- **Pedidos y facturas:** seleccionar mesa disponible, producto, cantidad y cliente o **Público general**. Abrir el pedido no lo factura.
- **Detalle de mesa:** agregar productos; al terminar, **Facturar y liberar mesa**. Las cantidades van de 1 a 50 por operación. Cada detalle captura nombre, tipo y precio al agregar el producto; conserva esos valores aunque después se renombre, reclasifique, cambie de categoría o precio, o se desactive el producto del menú.
- **Cortesías:** precio cero permitido. Un pedido con detalles puede facturarse por total cero y libera la mesa. Un pedido vacío no puede facturarse. No se agregaron impuestos no especificados: el impuesto sigue siendo cero.
- **Cerrar sesión:** regresa al acceso para iniciar otra sesión. Cerrar la ventana principal termina el programa.

Las aperturas y cambios de estado bloquean la fila de mesa dentro de una transacción. La facturación bloquea el pedido y además existe una restricción única por pedido. Antes de facturar se comprueba que el total coincida con los detalles. No se agregan productos a pedidos cerrados.

## Reportes

El intervalo incluye ambos días. Una fecha inicial posterior a la final se rechaza.

- Ventas por día, semana o mes, calculadas sobre facturas. Las semanas comienzan el lunes; su etiqueta es la fecha de ese lunes, incluso si pertenece al año anterior. Los meses se identifican por su primer día. Las agrupaciones se limitan siempre al intervalo seleccionado.
- Los diez platos más vendidos incluyen únicamente platos de pedidos facturados, por fecha de factura y por identificador de producto y tipo histórico del detalle. Se excluyen bebidas históricas, pedidos abiertos y cancelados. Si un producto tuvo varios nombres, se muestra el último nombre histórico de sus ventas como plato dentro del intervalo (desempate por factura y detalle). Sus cantidades e importes se suman una sola vez por MenuId.
- Pedidos por mesa y cliente: se filtran por fecha del pedido; incluyen estados y facturas. El selector incluye clientes inactivos para consultar su historial y público general. Por eso este reporte no debe sumarse como si todos sus pedidos fueran ventas facturadas.
- Los filtros se actualizan al cambiar; la lista operativa ya no oculta pedidos abiertos por un límite de 100 registros.

Los cálculos son `decimal`. Las consultas SQL del repositorio conservan conexiones independientes y recursos `using`. La interfaz ejecuta esas operaciones en tareas de fondo y aplica sus resultados en el hilo de Windows Forms.

## Organización y POO

Cada clase, record e interfaz tiene un archivo propio. Los formularios usan `.cs` y `.Designer.cs` parciales, con constructor sin argumentos para diseño y constructor con dependencias para ejecución. Las consultas no se ejecutan en el constructor de diseño. No se necesitan `.resx` porque actualmente no hay recursos localizados ni binarios asociados a controles.

`ElementoMenu` es abstracta; `Plato` y `Bebida` implementan `Tipo`. El repositorio construye ambos como `ElementoMenu` y utiliza su cálculo monetario. Las interfaces `IRepositorioRestaurante` e `IConfiguracionConexion` separan consumidores e implementaciones. `IDialogosUsuario` aísla las notificaciones y confirmaciones para probar los mismos eventos de los formularios sin bloquear el ejecutor con ventanas modales; en ejecución normal `DialogosUsuario` sigue mostrando MessageBox. `CicloSesion` contiene el ciclo de login, ventana principal y logout compartido por el ejecutable y las pruebas. `ReglasRestaurante` centraliza validaciones y límites temporales. SQL permanece en el Backend.

Los diagramas editables están en `Diagramas/DiagramaClases.mmd` y `Diagramas/DiagramaBaseDatos.mmd`; las imágenes PNG son vistas generales de sus relaciones. `MockupFigma.svg` es una referencia de diseño previa, no una captura de la versión actual.

## Pruebas reproducibles

```powershell
# Reglas y construcción de formularios, sin base de datos:
./restaurante2/Pruebas/Verificar.ps1

# Integración completa en una base NUEVA y aislada de LocalDB:
./restaurante2/Pruebas/Verificar.ps1 -ConSql
```

Se necesita la instancia `(localdb)\MSSQLLocalDB` y `sqlcmd` para la segunda opción. Cada ejecución crea una base `Restaurante_Pruebas_<fecha>_<id>`, repite la migración y conserva la base para inspección. No utiliza ni altera `RestauranteDB`. El ejecutor temporal, resultados e imágenes se guardan en `.codex-build`, excluido de Git; la solución entregada conserva dos proyectos.

Se verifican cálculos, validaciones, configuración dañada, formularios, mantenimiento, duplicados, rollback, concurrencia, cortesías, historial, filtros y reconciliación de reportes. El render de formularios no equivale a abrirlos en el diseñador de Visual Studio; esa apertura y una revisión manual en varios monitores siguen siendo comprobaciones de entrega.

## Revertir

La rama de esta etapa es `correcciones/auditoria-final`, creada desde `correcciones/revision-restaurante` en `14bce2d`; `main` conserva el punto de partida `90868cb`.

Con el árbol limpio, `git switch correcciones/revision-restaurante` permite volver al estado anterior a esta auditoría (14bce2d). `git switch main` lleva a la versión original 90868cb, sin las correcciones de la primera etapa. Para deshacer un commit manteniendo historial: `git revert <hash>`. No use `reset --hard` para recuperar cambios.

**Git no revierte SQL.** Antes de migrar una base real, obtenga un respaldo completo con SQL Server y verifique una restauración en otra base. Si necesita regresar de versión, restaure ese respaldo en una base separada y apunte la versión anterior a ella. Considere los datos nuevos registrados después del respaldo antes de decidir una recuperación. No se proporciona un script de borrado de tablas como reversión automática.

No se incluyen el informe académico, video ni enlaces de entrega: deben incorporarse como entregables del grupo.


## Historial de productos y migración 003

`NombreProducto` y `TipoProducto` se guardan en cada fila de `DetallesPedido`, junto a `MenuId` y `PrecioUnitario`. Se capturan dentro de la transacción al crear el pedido o agregar un producto. Los detalles y más vendidos consultan esos valores; no reclasifican ventas según el catálogo actual. No se alteran los importes de pedidos o facturas previos.

Para registros anteriores sin captura, la migración solo puede copiar nombre y tipo del catálogo **disponible al migrar**. No reconstruye renombres ni cambios de tipo anteriores desconocidos. Repetirla no sobrescribe valores ya capturados. Ante valores incompatibles o inválidos revierte su transacción y comunica el motivo.

Respalde antes de migrar y actualice la base antes de ejecutar la nueva aplicación. La versión anterior no incluye los campos nuevos en sus INSERT; volver solo al código viejo puede impedir registrar detalles en un esquema 3. Para una reversión operativa, utilice el respaldo previo en una base separada y resuelva los datos posteriores; no elimine las columnas históricas automáticamente.

## Prueba aislada del ejecutable

La ejecución normal sin argumentos mantiene la configuración existente. Para una instancia de pruebas, establezca `RESTAURANTE_PRUEBAS_CONEXION` con autenticación integrada y una base cuyo nombre comience por `Restaurante_Pruebas_`, y ejecute:

```powershell
dotnet run --project restaurante2/Restaurante.Frontend -- --pruebas
```

Este modo no lee ni guarda `conexion.bin`; rechaza bases con nombres reales, autenticación SQL y AttachDBFilename. No sustituye ni migra automáticamente ninguna base. Nunca publique credenciales en archivos de configuración o comandos versionados.

La suite crea bases nuevas con prefijo de prueba y una fixture del esquema anterior. Los resultados de sesiones quedan en `sesion-proceso-1.json` y `sesion-proceso-2.json` dentro de `.codex-build/Verificador/Resultados`. Registran PID, pedido, factura, importe, cantidad de logins y DPI. El proceso 1 ingresa como administrador, crea un pedido de dos productos mediante controles, factura, hace logout e ingresa como otro usuario; el proceso 2 vuelve a ingresar y recupera la factura del mismo pedido.

La matriz trazable de requisitos está en `Requerimientos.md`. Los pendientes académicos y límites de la comprobación se mantienen explícitos; no se certifica que el proyecto esté listo para entregar por el solo hecho de compilar.
