# Activación autorizada de reservas — 2 de noviembre de 2026

El usuario autorizó explícitamente la modificación después del diagnóstico del botón deshabilitado. Se aplicó la migración 008 a RestauranteDB en (localdb)\MSSQLLocalDB.

- Respaldo COPY_ONLY/CHECKSUM conservado en la carpeta externa Reservas20261102/RespaldoHabitual008.
- Restauración real: Restaurante_Pruebas_Restauracion_20261102_215028_7d5be1.
- 23 tablas verificadas mediante conteos/checksum antes del ensayo.
- Migraciones ensayadas en la copia; última migración repetida; CHECKDB antes y después del ensayo.
- Destino final: versión 8, tabla de reservas con 14 columnas y 3 índices únicos filtrados habilitados.
- CHECKDB del destino final sin errores reportados.
- No se crearon reservas ni se generó consumo para probar sobre la base habitual.
- Sabores se volvió a abrir en Acceso. La interacción con una sesión real queda a cargo del usuario; puede reservar una mesa disponible si no tiene operaciones pendientes.

El primer intento se detuvo al crear el respaldo: SQL Server no veía la ruta LocalAppData desde su proceso. Se utilizó la carpeta externa ya validada y visible para SQL. No se eludió la comprobación de respaldo; la actualización se hizo únicamente después de restaurarlo y verificarlo.

El instalador ahora incluye el código de salida y el detalle SQL en el error administrativo, para distinguir fallos de conexión de fallos de respaldo. Este cambio solo mejora el diagnóstico y no altera la secuencia de migración.

Los documentos de entrega inicial describen el estado previo, cuando faltaba autorización; este registro actualiza ese estado. No se publicó ni se mezcló con main. Para recuperación después de 008 consulte BaseDatos/Reservas008.md: no basta regresar al ejecutable anterior ni eliminar la versión del esquema.
