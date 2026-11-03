# Recuperar el código anterior

Referencia anterior: `cb342db76270859e883ad8829011ec1553fcdb63`.
Rama de trabajo: `refactorizacion/estructura-exposicion-20261102`.
Etiqueta previa: `respaldo/antes-estructura-20261102-cb342db`.

Respaldos fuera del repositorio:

`C:\Users\DaniveK\.codex\visualizations\2026\10\25\01a174b9-7292-7271-95db-4bea2c57c13d\Refactorizacion20261102`

- `codigo-inicial-cb342db.zip`: archivos rastreados del estado inicial.
- `historial-inicial.bundle`: historial y referencias, verificado con git bundle verify.

El árbol estaba limpio antes de iniciar. No hubo cambios locales ajenos que guardar aparte. No se hicieron migraciones ni modificaciones de RestauranteDB durante esta reorganización: para volver a este código no se debe restaurar ni borrar la base habitual.

1. Cierra Sabores y detén la depuración antes de cambiar la versión ejecutada.
2. Comprueba `git status`. Conserva cualquier trabajo posterior en un commit o copia independiente; no uses reset --hard ni limpiezas masivas.
3. Para revisar el estado anterior por separado, usa una copia/worktree desde la etiqueta. Alternativamente cambia a `correcciones/auditoria-integral-20261028`, comprobando que apunta al commit de referencia.
4. Recompila la solución; no reutilices un ejecutable que haya quedado de otra versión.
5. No reinicialices usuarios ni cambies la conexión por haber cambiado carpetas de código.
6. Para regresar a las mejoras, cambia a la rama de trabajo y recompila.

Si se pierde el repositorio, `git clone <ruta-del-bundle> <carpeta-nueva>` permite recuperar una copia independiente del historial anterior. El ZIP es otra alternativa para inspección de los archivos. No extraigas a ciegas sobre una carpeta con trabajo sin guardar.

Los .bak generados por las pruebas corresponden a fixtures aisladas; no son un respaldo nuevo de tu base habitual y no deben restaurarse encima de ella. Las bases y copias de prueba se conservan, pendientes de una limpieza explícita futura.
