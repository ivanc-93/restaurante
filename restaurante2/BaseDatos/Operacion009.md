# Migración y recuperación de operación 009

La autorización actual del usuario permite implementar los cambios y actualizar el destino habitual en una rama, conservando respaldo. No permite publicar ni mezclar con main.

## Antes de actualizar

1. Cierre las instancias de Sabores y conserve la rama `mejoras/operacion-cuentas-20261102`.
2. Conserve el punto `respaldo/antes-operacion-integral-20261102-928ef17`, el ZIP y el bundle de la carpeta externa OperacionIntegral20261102.
3. Ejecute el instalador con `-ConCobranzaParcial`. Esta opción implica 006, 007, 008 y 009 después de las versiones base; sin la opción no se habilita 009.
4. El instalador respalda COPY_ONLY/CHECKSUM, restaura una base aislada con archivos diferentes, compara las tablas previas, ejecuta CHECKDB, ensaya las migraciones y repite la última antes de tocar el destino.

Ejemplo para este equipo, desde la carpeta restaurante2:

```powershell
.\BaseDatos\InstalarOActualizar.ps1 `
  -Servidor '(localdb)\MSSQLLocalDB' `
  -BaseDatos 'RestauranteDB' `
  -RutaRespaldos 'C:\Users\DaniveK\.codex\visualizations\2026\10\25\01a174b9-7292-7271-95db-4bea2c57c13d\OperacionIntegral20261102\RespaldoHabitual009' `
  -ConCobranzaParcial
```

No ejecute el comando de nuevo para intentar resolver un error sin leerlo. Los archivos de respaldo y configuración no se publican en Git. El ejecutable no migra automáticamente una base al abrirla.

## Efectos de 009

Añade CuentasPedido, UnidadesCuenta, AbonosPedido y ReglaFrecuente, y los campos Porcentaje/Promocion de AjustesPedido. Conserva pedidos, usuarios, reservas, pagos anteriores, precios e historial. La regla de clientes frecuentes comienza inactiva; nadie recibe descuentos automáticamente. Las cuentas existentes sin abonos/división continúan su operación, y los DTO anteriores conservan su serialización para recuperación.

El código admite esquemas 5 a 9. Las nuevas pantallas de abonos se activan al detectar 009 completo; una estructura 009 incompleta se rechaza, no se interpreta como permiso para volver al cobro anterior.

## Recuperación

Antes de migrar puede abrir una copia del código del tag anterior sin tocar los datos. Después de activar 009, **no basta volver al ejecutable anterior**, que admite hasta 8. No elimine filas de VersionEsquema ni tablas para hacerlo arrancar.

Si necesita retroceder, detenga las escrituras, respalde también el estado actual y restaure primero el respaldo previo en otra base con archivos separados. Valide CHECKDB y datos antes de configurar una copia del código anterior contra ella. Sustituir el destino habitual requiere decidir qué hacer con los movimientos posteriores al respaldo: restaurarlo sin conciliarlos los perdería. No se automatizó una restauración destructiva sobre RestauranteDB.

La evidencia de activación del destino, una vez realizada, queda en Pruebas/OperacionIntegral/Activacion_20261102.md y en el JSON externo del instalador.
