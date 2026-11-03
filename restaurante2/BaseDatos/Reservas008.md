# Activación y recuperación — esquema 008

## Estado de esta entrega

La migración está preparada y probada. **No se ejecutó contra RestauranteDB ni se cambió la conexión habitual.** La política de atención y cancelación sí fue aprobada. La autorización de migrar la base habitual sigue siendo un paso distinto, exigido por el prompt de esta entrega.

Script: [Migracion_008.sql](Migracion_008.sql). Añade exclusivamente la estructura de reservas y la versión 8; conserva los registros existentes. Las mesas que ya estaban Reservadas sin titular se muestran como diagnóstico, sin inventar datos.

## Ensayos ejecutados

- Migración 007 → 008 y repetición sobre copia restaurada y sobre fixtures de reservas.
- Respaldo COPY_ONLY/CHECKSUM, restauración real con archivos separados, comparación de conteos/checksum y DBCC CHECKDB.
- Instalador `InstalarOActualizar.ps1 -ConReservas`: actualización 7 → 8 ensayada en Restaurante_Pruebas_Reservas_Auditoria_20261102, con restauración en Restaurante_Pruebas_Restauracion_20261102_212723_17b58c.
- Repetición del instalador sobre una fixture ya en 8.
- Instalación nueva con `InstalarNuevaBase.ps1 -ConReservas` sobre Restaurante_Pruebas_Reservas_Instalador_20261102, sin usuarios de demostración.

No se eliminaron las bases ni los respaldos de estos ensayos. Las evidencias se encuentran en Pruebas/EvidenciasReservas20261102 y en la carpeta local externa Reservas20261102.

## Comando preparado — ejecutar SOLO después de autorización específica

Detenga todas las instancias de Sabores y confirme que el destino es el correcto. Desde la carpeta restaurante2 de este repositorio:

```powershell
.\BaseDatos\InstalarOActualizar.ps1 `
  -Servidor '(localdb)\MSSQLLocalDB' `
  -BaseDatos 'RestauranteDB' `
  -RutaRespaldos "$env:LOCALAPPDATA\Sabores\Respaldos\Reservas008" `
  -ConReservas
```

Este comando escribe en la base indicada. No es una consulta. Antes de actualizarla, crea un respaldo, restaura una base aislada, compara tablas, ejecuta CHECKDB y ensaya la cadena de migraciones con repetición de la última. Si falla, se detiene; no continúe manualmente ni cambie el nombre de la base para eludir el error. La comparación de checksum es una comprobación de consistencia práctica, no una prueba criptográfica de igualdad.

Conserve el archivo .bak y el .json emitidos, y los archivos del clon. Tras actualizar, compruebe versión 8, validación de esquema, reserva/atención/cancelación y los registros anteriores. En pruebas operativas posteriores use datos de ensayo identificables y autorizados.

## Reversión

Antes de migrar la base habitual puede regresar al código del tag `respaldo/antes-reservas-20261102-2c2dbde` en una copia separada, conservando el trabajo actual. No use reset destructivo.

Después de activar 008 **no basta volver al ejecutable anterior**: la versión anterior rechaza el esquema 8. No elimine filas de VersionEsquema ni la tabla ReservasMesa. Detenga las escrituras y conserve también un respaldo del estado actual antes de recuperar.

Restaure primero el respaldo previo en una base con otro nombre y archivos separados, valide CHECKDB y datos, y abra una copia del código anterior contra esa copia validada mediante su configuración. La sustitución del destino habitual requiere una decisión explícita: restaurar el respaldo previo pierde las operaciones posteriores al respaldo si no se concilian. No se automatizó esa sustitución destructiva.

No publique respaldos SQL ni archivos de configuración local en GitHub.
