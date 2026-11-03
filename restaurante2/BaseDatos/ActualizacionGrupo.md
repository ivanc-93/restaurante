# Actualización de funciones del esquema 7

Para las correcciones de octubre de 2026 consulte Pruebas/CorreccionesAuditoria20261028.md y Pruebas/RecuperacionAuditoria20261028.md. InstalarOActualizar.ps1 admite -ConFuncionesAuditadas para ensayar y aplicar hasta 007 con respaldo/restauración. InstalarNuevaBase.ps1 admite -ConOperaciones para una base nueva sin usuarios fijos. La aplicación no migra al iniciar. Los apartados siguientes conservan el procedimiento histórico hasta 005.

# Actualización vigente del esquema 5

Consulte [Instalación usuarios y autorizaciones](../Pruebas/RolesYAcceso.md). InstalarOActualizar.ps1 ahora ensaya y aplica 002 003 004 y 005, con respaldo y restauración comprobados. Para bases nuevas sin credenciales fijas utilice InstalarNuevaBase.ps1 con token de un solo uso. La aplicación requiere esquema 5 y no migra al iniciar.

La conexión ahora es un formulario independiente accesible desde el acceso o Configuración. No se permite acceso operativo con credenciales fijas sin SQL. Los apartados de respaldos siguientes conservan la evidencia histórica de esquema 4.

# SQL Server: instalación y actualización del grupo

## Conexión

Se conserva `Microsoft.Data.SqlClient`, la configuración por integrante y el archivo DPAPI `conexion.bin`. No hay una cadena compartida ni contraseñas en Git. La versión de SSMS no determina la del motor.

Por instrucción del usuario, este equipo usa **(localdb)\MSSQLLocalDB / RestauranteDB / autenticación de Windows**. LocalDB es Microsoft SQL Server. La aplicación no impone esa instancia a los demás integrantes. La única diferencia respecto del constructor inicial es que una instancia LocalDB no recibe el sufijo de puerto TCP; los demás servidores conservan el mecanismo original.

La configuración se realiza desde Configuración → Avanzado · Conexión SQL. Usuario SQL vacío significa autenticación Windows. Para LocalDB el puerto no se utiliza. Cada integrante debe instalar y actualizar su base, además de configurar su conexión. No compartir `conexion.bin` entre perfiles.

La aplicación requiere esquema 5 y solo lo comprueba: **no ejecuta migraciones al iniciar**. Si falta actualizar, muestra el procedimiento antes de admitir la sesión de operación.

## Procedimiento con autenticación Windows

Cerrar las aplicaciones que escriban sobre la base. Desde la raíz del repositorio, elegir una carpeta de respaldo accesible para el servicio SQL:

```powershell
./restaurante2/BaseDatos/InstalarOActualizar.ps1 `
  -Servidor '(localdb)\MSSQLLocalDB' `
  -BaseDatos RestauranteDB `
  -RutaRespaldos 'C:\RespaldosSabores'
```

Para otro integrante, cambiar servidor, base y ruta. En un servidor remoto, la ruta de respaldo/restauración es del servidor, no de la computadora que ejecuta SSMS. El servicio necesita permiso para escribirla. El script no instala SQL Server ni cambia sus opciones de seguridad.

El procedimiento:

1. Si la base no existe, crea el esquema inicial y los datos de demostración mediante `CrearBaseDatos.sql`.
2. Realiza un `BACKUP DATABASE` con `COPY_ONLY` y `CHECKSUM`.
3. Restaura realmente el respaldo con otro nombre y archivos nuevos, sin `REPLACE`.
4. Compara conteos y checksums de las tablas dbo entre la base original y la restaurada. Si cambian durante el proceso, se detiene; usar una ventana sin escrituras.
5. Prueba las migraciones 002, 003 y 004 en la copia, y repite 004 para comprobar idempotencia.
6. Solo después aplica la misma cadena al destino.
7. Conserva el `.bak`, la base restaurada y un registro JSON del resultado.

Si falla cualquier paso, no continuar manualmente sin revisar. No elimina bases ni corrige inconsistencias silenciosamente. Los checksums son comprobaciones adicionales al respaldo/restauración, no hashes criptográficos.

## Si el integrante usa autenticación SQL

El script anterior usa Windows. Para autenticación SQL, abrir SSMS con la conexión propia y realizar el mismo procedimiento: respaldo, restauración en base separada, comparación y ensayo de migraciones en la copia. Ejecutar después 002 → 003 → 004 → 005 sobre la base elegida. Los scripts de migración no cambian automáticamente a otra base; confirmar el destino antes de ejecutarlos.

Para instalaciones nuevas, `CrearBaseDatos.sql` crea `RestauranteDB`; después ejecutar las migraciones hasta 005. No basta con instalar la versión 3. La cuenta usada para actualizar necesita permisos DDL y de respaldo/restauración; no publicar sus credenciales.

## Esquema 4

- `Pedidos`: versión de concurrencia, captura histórica de cliente/mesa y marca de legado.
- `Facturas`: comprobantes internos, datos del receptor y restaurante capturados, operador, GTQ y marca de legado. El nombre de tabla se conserva por compatibilidad.
- `PagosManuales`: efectivo, tarjeta y cortesía cero; confirmado, rechazado o incierto.
- `OperacionesRestaurante`: identificadores únicos, hash de solicitud, actor y resultado recuperable.
- `AuditoriaOperaciones`: motivo, estados, importe, responsable y fecha.
- `IncidenciasConsumo`: importe pendiente sin inventar cobro ni borrar el consumo.
- `VersionEsquema`: versión requerida por el ejecutable.
- Estados adicionales: mesa `En limpieza`, pedido `Incidencia`. Índice único de pedido activo por mesa.

Los datos anteriores se conservan. Las capturas iniciales solo reflejan información conocida al migrar; no reconstruyen datos anteriores desconocidos. Los comprobantes previos quedan legados y sin pago registrado. No se insertan pagos para hacerlos parecer cobrados.

## Respaldos de este trabajo

En `.codex-build/RespaldosSQL`, excluido de Git:

- `RestauranteDB_20261026_190122_9ec39f.bak`: antes de migración 004, esquema 3; restauración real y comparación de ocho tablas.
- `RestauranteDB_20261026_193352_cec63e.bak`: esquema 4; restauración real y comparación de trece tablas.
- Registros JSON homónimos con servidor, base restaurada, fecha y resultado.

La instancia comprobada informó motor 17.0.4025.3, Express Edition. Las pruebas de negocio se ejecutan en bases `Restaurante_Pruebas_*`, no en `RestauranteDB`.

## Recuperar código y datos por separado

Código anterior: rama `respaldo/antes-cobros-20261026`, commit `e410f934f1942cdd165b714ffe8a21c35900c1c4`. Para revisarlo sin descartar la implementación:

```powershell
git status --short
git worktree add --detach ../sabores-antes-cobros respaldo/antes-cobros-20261026
```

Usar una ruta nueva; el comando no se ejecutó durante este trabajo. No usar `reset --hard`.

Para regresar a datos anteriores, conservar primero la base actual y restaurar el `.bak` compatible en **otra base**, con archivos distintos. Consultar primero `RESTORE FILELISTONLY` y usar sus nombres lógicos en `WITH MOVE`. No usar `WITH REPLACE` sobre la base actual. Seleccionar explícitamente la base restaurada en una configuración compatible con el código que se ejecutará.

Volver al código anterior no revierte SQL. El código anterior no conoce los nuevos pagos y estados; no usarlo contra el esquema nuevo para operar ni eliminar columnas para forzar compatibilidad. Las operaciones posteriores al respaldo deben conciliarse antes de una reversión operativa.
