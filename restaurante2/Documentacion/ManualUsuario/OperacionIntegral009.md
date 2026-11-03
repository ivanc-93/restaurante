# Manual de operación: mesas, beneficios y cuentas separadas

Actualización 009 · 2 de noviembre de 2026.

**Para la versión 010 consulte [Cuentas identificadas desde la comanda](CuentasIdentificadas010.md). Este suplemento conserva las reglas del flujo anterior para cuentas/pagos que ya estaban divididos.**

## Inicio y atención

Inicio muestra estados de mesas, reservas activas, operaciones pendientes e incidencias. Todos los roles operativos pueden abrir una reserva. Se muestran hasta cinco reservas y un acceso al resto; no se inventan horas de llegada. Las cifras de comprobantes son exclusivas de gerencia y no equivalen al efectivo de caja.

En Mesas, abra el pedido o atienda una reserva. Seleccione un producto en una tarjeta, indique cantidad y pulse **Agregar al pedido**. Se conservan los precios de cada incorporación. El cliente del pedido se selecciona antes del primer producto; el titular de una reserva no se transforma automáticamente en cliente de facturación.

La comanda muestra consumo original, ajustes, pagado y saldo. **Revisar y cobrar** permanece junto al resumen. Las acciones de corrección y preparación están en **Comanda ▾**; las gestiones de cierre y beneficios, en **Resolver / ajustar**. Puede necesitar desplazamiento a escalados altos, sin superponer los controles.

## Descuentos, promociones y cortesías

Haga los ajustes antes de separar cuentas o recibir un abono:

1. Abra Resolver / ajustar → Descuento o cortesía.
2. Seleccione Descuento (5–50 %), Frecuente o Cortesía (100 %).
3. Escriba un motivo. Revise el importe que afectará al consumo original.
4. Gerente o Administrador aplican directamente; Mesero solicita autorización puntual sin cambiar de sesión.

El ajuste reemplaza al vigente: no se acumula. El consumo original y los precios no se borran. Agregar un producto antes de dividir/cobrar invalida el ajuste y exige revisión. Una cortesía cero se cierra como cortesía autorizada, no como efectivo ficticio.

Gerente/Administrador configura **Promoción de clientes frecuentes** desde Inicio: activa/inactiva, visitas completadas mínimas, porcentaje y vigencia. Inicialmente está inactiva. Una visita es un pedido distinto facturado, no legado, con importe positivo y completamente pagado; varios abonos cuentan una sola visita. Se acredita al cliente asociado al pedido, no a cada receptor de recibo. No hay aplicación automática. La promoción vigente se verifica otra vez al aplicar.

En esta versión las promociones y cortesías abarcan la cuenta completa. No hay promociones por categoría/producto ni acumulaciones. Sin costos de productos no se garantiza margen.

## Separar cuentas

En Revisar y cobrar, antes del primer pago, pulse **Separar cuentas**:

- **Por importes:** establezca de 2 a 20 cuentas y distribuya el consumo original. Se propone un reparto igual que puede editar. Todas las partes deben ser positivas y sumar exactamente el original.
- **Por unidades:** asigne cada unidad de producto a un número de cuenta. Deben quedar asignadas todas las unidades, sin repetirse. Para compartir una unidad entre personas use el modo por importes.

Los descuentos se distribuyen proporcionalmente. Los centavos residuales se asignan en el orden de las cuentas; la suma siempre conserva el total. Revise la confirmación: la distribución queda bloqueada y no se eliminan ni trasladan subcuentas después. No divida una cortesía total de cero.

## Cobrar abonos o combinar efectivo y tarjeta

1. Seleccione Cuenta general o una subcuenta.
2. Indique el **importe que se aplicará**, positivo y no mayor al saldo de esa cuenta.
3. Elija Efectivo o Tarjeta.
4. Efectivo: ingrese lo recibido. El cambio es recibido menos aplicado; el cambio no cuenta como ingreso.
5. Tarjeta: declare Confirmado, Rechazado o Incierto según comprobación externa. Introduzca una referencia de operación, nunca número de tarjeta, CVV o PIN. No hay conexión bancaria.
6. Complete Público general o nombre/identificación del receptor de ese abono.
7. Revise y confirme los datos; confirme recepción o comprobación manual cuando corresponda.
8. Pulse **Confirmar abono de Q …** y revise la confirmación final.

Para pago mixto registre, por ejemplo, un abono en efectivo y luego otro en tarjeta sobre el saldo restante. Un rechazo no reduce el saldo. Una tarjeta incierta bloquea nuevos abonos hasta que Gerente/Administrador concilie, o autorice puntualmente al mesero, con motivo y evidencia.

Cada abono conserva su recibo individual. Desde **Abonos y recibos** seleccione un registro y pulse Ver recibo. El resumen final se emite una vez al liquidar el pedido completo. No atribuye todo el pedido al último pagador: los receptores están en los recibos individuales. Todos son documentos internos académicos, no fiscales.

Pagar una sola subcuenta no libera la mesa. Al liquidarse el pedido completo, pasa a En limpieza; después se confirma limpieza para declararla Disponible.

## Restricciones y recuperación

Después de dividir o registrar un pago confirmado/incierto no se modifica el consumo ni se aplican nuevos ajustes. Tampoco se usa cancelación o incidencia para ocultar lo cobrado. Esta versión no incluye devoluciones, reversión de división o condonación de saldos después de abonos: esas excepciones requieren un flujo financiero explícito futuro.

Si se pierde una respuesta, vuelva a Inicio → Operaciones pendientes y recupere **la misma operación**. No genere otra solicitud ni vuelva a pasar la tarjeta. El identificador estable evita duplicados. Los rechazos que quedan en el diario se revisan y descartan mediante el abandono terminal existente; no se borran archivos manualmente.

Los formularios de cierre muestran acciones según el estado. Volver sin cambiar la mesa solo cierra la ventana; no liquida, libera ni elimina deuda.
