# Respaldo y recuperación de las correcciones del 28 de octubre

## Código preservado

Rama inicial mejoras/recuperacion-contrasenas-20261027; commit inicial 2c427e525963927f044a5261f7079b080e1f7d11. Rama de trabajo correcciones/auditoria-integral-20261028. Etiqueta local de respaldo respaldo/auditoria-20261028-2c427e5. El archivo Program.cs de Frontend ya figuraba modificado al comenzar y se conservó sin incorporarlo a las correcciones.

Existe un ZIP local de los 261 archivos versionados o no ignorados iniciales, verificado sin errores y con manifiesto SHA256. El ZIP y los respaldos SQL están fuera de Git. No publiques conexion.bin, respaldos, bases ni diarios cifrados.

## Base actualizada

Destino autorizado: (localdb)\MSSQLLocalDB / RestauranteDB, autenticación Windows. Se obtuvo un respaldo COPY_ONLY con CHECKSUM; se ejecutó VERIFYONLY, una restauración con nombre/archivos nuevos y CHECKDB. Se ensayó 006/007 en la copia antes de actualizar. Las 17 tablas existentes conservaron conteos y checksums dentro de una transacción de actualización. La base quedó en esquema 7; no se reinicializaron usuarios ni contraseñas.

Los nombres exactos de los archivos .bak y las copias restauradas se conservan en los manifiestos locales de la entrega. No hay borrado automático ni se usó RESTORE WITH REPLACE.

## Recuperación del código

Para consultar el punto inicial usa git show respaldo/auditoria-20261028-2c427e5. Para preparar un regreso, conserva primero el estado actual en un respaldo y trabaja en otra rama o checkout; no uses reset --hard sobre trabajo pendiente. El ZIP conserva los bytes iniciales de archivos locales, incluso si no estaban confirmados.

El código anterior admite esquema 5/6 y no debe conectarse directamente a la base 7 para seguir operando. Además, ese código desconoce las correcciones de unidades: no basta con borrar la fila VersionEsquema para hacerlo compatible.

## Recuperación de la base

1. Detén nuevas escrituras y respalda también la base actual antes de intervenir.
2. Restaura el .bak previo en OTRA base y archivos nuevos, sin sobrescribir RestauranteDB.
3. Ejecuta CHECKDB y revisa esquema, conteos, usuarios y comprobantes.
4. Antes de volver al código antiguo, concilia cualquier venta, pago, corrección, incidencia, usuario o contraseña cambiados después del respaldo. Restaurar directamente sobre la base actual perdería esos cambios.
5. Configura el equipo con el destino recuperado mediante el procedimiento administrativo, sin compartir el archivo DPAPI entre perfiles.
6. Conserva la base 7 y el historial para diagnóstico. No borres tablas o movimientos para simular una reversión.

## Equipo y mantenimiento

Para actualizar otras bases existentes, cierra las aplicaciones operativas y usa BaseDatos/InstalarOActualizar.ps1 con -ConFuncionesAuditadas y una ruta accesible al servicio SQL. El script respalda, restaura y ensaya antes de aplicar la cadena hasta 007. En servidores remotos, las rutas de datos y respaldo pertenecen al servidor.

Para una base nueva sin usuarios fijos, desde PowerShell 7 o posterior, InstalarNuevaBase.ps1 admite -ConOperaciones, que incluye recuperación y operaciones del esquema 7. Requiere un token de instalación privado y solo admite una base inexistente. No uses CrearBaseDatos.sql por separado para reinicializar una base del grupo.

La retención queda bajo control del responsable: inventariar copia y .bak, proteger su acceso, comprobar espacio en el servidor y conservar una copia verificada anterior al despliegue. No se impuso una duración ni se eliminan bases por prefijo; la política de borrado requiere una decisión posterior del equipo.
