# Arquitectura y recorrido de operación 009

## Matriz aplicada

| Acción | Mesero / Empleado legado | Gerente | Administrador |
|---|---|---|---|
| Consultar mesas/reservas, atender y crear comandas | Sí | Sí | Sí |
| Dividir cuenta antes del cobro y registrar abonos | Sí | Sí | Sí |
| Consultar recibos y estado permitido del pedido | Sí | Sí | Sí |
| Aplicar descuento, promoción o cortesía | Autorización puntual | Directo, con motivo | Directo, con motivo |
| Conciliar abono incierto | Autorización puntual | Directo, con evidencia | Directo, con evidencia |
| Configurar promoción frecuente | No | Sí | Sí |
| Indicadores financieros de Inicio / reportes | No | Sí | Sí |
| Administrar usuarios, roles y conexión SQL | No | No | Sí |

Los permisos se revalidan en los servicios, no dependen de ocultar botones. La desactivación/cambio de rol invalida las sesiones según el mecanismo existente. La autorización vincula solicitud, operación, pedido, versión y motivo; no conserva credenciales ni cambia la sesión del solicitante.

## Ubicación de cada parte

| Archivo o símbolo | Finalidad |
|---|---|
| Backend/Aplicacion/Contratos/ICobranzaParcial | Interfaz de nuevas operaciones, heredada por IRepositorioRestaurante |
| Dominio/CuentaSeparada, ParteCuenta, AsignacionProducto | Subcuenta, reparto por importe y asignación de unidades |
| Dominio/ResumenCobranza, AbonoResumen, ResultadoAbono | Lectura de consumo, pagos y saldo sin mezclar controles visuales |
| Dominio/SolicitudDivision, SolicitudAbono, SolicitudConciliacionAbono | Solicitudes inmutables con GUID y versión |
| Dominio/ReglaFrecuente, ClienteFrecuente, SolicitudReglaFrecuente, SolicitudBeneficio | Condiciones, elegibilidad y ajuste porcentual autorizado |
| Dominio/Reglas/ReferenciaPago | Rechazo preventivo de secuencias que parecen PAN válidos por Luhn en referencia/observación; no sustituye la prohibición de ingresarlos |
| Data/Repositorios/RepositorioRestaurante.Abonos | Distribución, transacciones, saldos, conciliación y recibos |
| RepositorioRestaurante.Frecuentes / .Beneficios | Configuración, visitas verificadas y ajuste porcentual no acumulable |
| RepositorioRestaurante.Autorizacion | Nuevos tipos Beneficio y ConciliarAbono con protocolo puntual existente |
| RepositorioRestaurante.Operaciones / SinPagoPendiente | Bloquea cambios incompatibles después de dividir/abonar; no elude un esquema 9 incompleto |
| Frontend/Controles/TarjetaProducto | Nombre, precio, categoría, selección visual, teclado y tooltip |
| Formularios/FormularioDetalleMesa + Designer | Catálogo y comanda; resumen fijo y acciones secundarias |
| FormularioPrincipal / MostrarInicio | Estado operativo y reservas para todos; cifras sensibles por rol |
| FormularioCobranza + Designer | cbCuenta, nudImporte, nudEntregado, cbMetodo, cbResultado, ctReferencia, ctReceptor, ctIdentificacion, btnConfirmar |
| FormularioDivision + Designer | dgvPartes, dgvUnidades, cbModo, nudPersonas y confirmación de reparto |
| FormularioPromocion + Designer | chkActiva, nudVisitas, nudPorcentaje, dtDesde, dtHasta y btnGuardar |
| FormularioAjuste | Porcentaje 5–50, Frecuente o Cortesía 100; autorización antes de guardar |
| FormularioGestionMesa | Consecuencias y botones específicos; dirige cuentas con abonos a cobranza |
| FormularioComprobante | Papel blanco, encabezado, contenido consultable y copia voluntaria |
| Infraestructura/Recuperacion/DiarioOperaciones | Recupera Division, Abono, Beneficio, ConciliarAbono y ReglaFrecuente con sus GUID originales |
| Presentacion/PresentacionPendiente | Explica solicitudes y permite renovar la autorización adecuada |
| BaseDatos/Migracion_009.sql | Añade estructura sin recalcular documentos anteriores |

Backend corresponde a Restaurante.Backend; Frontend a Restaurante.Frontend. Cada clase/registro tiene su archivo. Los pares de formulario y Designer forman una única clase parcial de Windows Forms.

## Invariantes y modelo

Saldo = consumo original − ajuste vigente − suma de importes confirmados aplicados.

CuentasPedido guarda la distribución y sus descuentos; UnidadesCuenta, la asignación opcional de productos; AbonosPedido, cada pago solicitado/confirmado/rechazado/incierto, receptor y conciliación; ReglaFrecuente, la configuración inicialmente inactiva. AjustesPedido incorpora Porcentaje y Promocion; los registros anteriores no se reinterpretan. OperacionesRestaurante conserva resultado/hash para replay; las auditorías guardan solicitante, autorizador y motivo.

Se mantienen los DTO antiguos de cobro y ajuste, para no cambiar los hashes de operaciones pendientes. La interfaz 009 utiliza las nuevas solicitudes para porcentajes y abonos; la API antigua conserva su compatibilidad, pero se bloquea ante subcuentas/pagos nuevos. Los comprobantes anteriores siguen usando su lectura histórica. Los abonos se incorporan al reporte operativo; el reporte de comprobantes sigue reconociendo el pedido al emitir su resumen final, no por cada abono.

Toda mutación usa el protocolo existente Serializable: operación → mesa → pedido. Se comprueba la versión y saldo esperado dentro de la transacción. Un fallo revierte recibo/estado/operación. La unicidad de tarjeta incierta y el bloqueo de mesa evitan carreras entre terminales. El replay se resuelve antes de reevaluar el estado que cambió con la operación.

## Ejemplos para exposición

- Consumo Q 203.00, descuento 10 % = Q 20.30; total Q 182.70. Abono efectivo aplicado Q 100.00, recibido Q 150.00: cambio Q 50.00; saldo Q 82.70. Un segundo abono confirmado en tarjeta por Q 82.70 liquida la cuenta.
- Consumo Q 30.00, descuento 5 % = Q 1.50. Reparto original Q 10.00 / Q 20.00: descuentos Q 0.50 / Q 1.00; subcuentas Q 9.50 / Q 19.00. Pagar la primera conserva mesa ocupada.
- Si un reparto produce fracciones de centavo, se trunca el descuento proporcional y se distribuye el residuo de un centavo por orden de cuenta. No se crean ni pierden centavos.
- Cortesía autorizada del 100 % conserva consumo y ajuste, y cierra por cortesía sin registrar efectivo inventado.

## Límites deliberados

Las promociones son de cuenta completa, no por categorías o productos. No se acumulan y no hay descuentos nuevos después de dividir/cobrar. No se fracciona una unidad de producto en partes físicas; se usa reparto por importe. No se revierten pagos confirmados ni divisiones ni se condona deuda posterior a abonos. No hay agenda de reservas, anticipos, propinas, SAT/FEL, pasarela bancaria ni impresión fiscal. El filtro de PAN es preventivo, no garantiza reconocer toda forma de información sensible escrita en texto libre.
