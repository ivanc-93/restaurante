# Cobros manuales y gestión de mesas

## Alcance

Aplicación académica, GTQ, sin SAT/FEL ni servicios bancarios. El comprobante indica: **Comprobante interno académico — no válido como documento tributario**. Los datos y pagos son revisados/declarados por el operador; no se autentican identidades ni pagos mediante un proveedor.

Se conserva la solución de dos proyectos, Windows Forms, SQL Server, interfaces y el dominio abstracto `ElementoMenu` con `Plato`/`Bebida`. La incorporación utiliza los objetos derivados y su cálculo decimal. Cada tipo tiene su archivo; los formularios mantienen sus archivos parciales Designer.

## Guía de operación

1. Entrar como usuario del sistema. La instalación inicial crea `admin` / `Admin123!`.
2. Desde Mesas seleccionar una disponible, cliente opcional, producto y cantidad. El primer producto crea el pedido; abrir una mesa no crea un pedido vacío.
3. Agregar productos. Cada operación guarda las capturas y actualiza la cuenta.
4. Seleccionar **Revisar y cobrar**. Revisar detalle y total. Seleccionar cliente existente para copiar datos o escribir receptor sin crear un cliente. Público general no lleva identificación. Dirección, correo y observación son opcionales.
5. Efectivo: introducir dinero recibido y revisar cambio. No se acepta un importe insuficiente.
6. Tarjeta: indicar confirmado, rechazado o incierto. Confirmado requiere referencia y confirmación explícita de comprobación manual; incierto requiere referencia y no cierra la cuenta. No ingresar números de tarjeta, vencimiento, PIN o códigos de seguridad.
7. Confirmar revisión de datos y recepción/comprobación del pago que corresponda. El servicio verifica nuevamente versión, importe y detalles. Ante cambio concurrente se exige nueva revisión.
8. Un cobro confirmado crea un comprobante interno, cierra el pedido y pasa la mesa a limpieza dentro de una transacción. Consultar el comprobante no genera otra venta.
9. Desde la mesa, **Gestionar cierre de mesa → Confirmar limpieza** la deja disponible después de la confirmación del responsable.

## Excepciones

- **Cancelación:** administrador, motivo y declaración de que no hubo preparación ni consumo. Conserva detalles e importe histórico; excluye la cuenta de ventas y pasa la mesa a limpieza. No cancela un pago o documento emitido.
- **Consumo pendiente:** administrador registra incidencia con motivo e importe pendiente. La mesa puede reutilizarse después de limpieza, pero el saldo permanece en Incidencias. No hay un módulo general de crédito ni liquidación posterior de incidencias en esta versión.
- **Ocupada sin pedido:** administrador corrige con motivo, después de comprobar que no apareció una cuenta activa; pasa a limpieza, no directamente a disponible.
- **Tarjeta incierta:** se bloquean nuevos pagos y productos. Un administrador usa Gestionar cierre → Conciliar tarjeta, comprueba evidencia y declara confirmado o rechazado. Confirmado completa el cierre; rechazado mantiene la cuenta abierta y permite otro intento. La confirmación externa sigue siendo manual.
- **Total cero:** requiere selección de cortesía y autorización explícita de administrador. No se inventa dinero recibido.
- **Error o reinicio:** Inicio → Recuperar operaciones pendientes. Verificar la solicitud y recuperar exactamente la misma operación. Si SQL ya la aplicó, devuelve su resultado; no la repite. Solo puede descartarse localmente si SQL confirma que no fue aplicada.
- **Corrección posterior a comprobante/pago:** reembolsos y correcciones financieras no están habilitados. No editar SQL ni cancelar para ocultar el registro.

## Consistencia y recuperación

Los cambios adquieren bloqueos de operación → mesa → pedido. Se impide renumerar una mesa con cuenta activa. Hay un único pedido activo por mesa y un pago confirmado por pedido.

La versión `rowversion` detecta cambios aunque el importe sea igual. El hash de solicitud impide reutilizar un identificador con otros datos o usuario. La API antigua `FacturarPedido(id)` se conserva solo para rechazar explícitamente el cierre sin pago.

Antes de enviar una incorporación, apertura, cobro o gestión, el cliente conserva una solicitud cifrada DPAPI, vinculada a base y usuario, en el perfil local. No modifica `conexion.bin`. Los diarios de pruebas se guardan en una carpeta temporal separada. La recuperación local requiere el mismo perfil Windows y la misma base; no se afirma recuperación automática de ese archivo desde otro equipo. Los resultados y la auditoría SQL permanecen en el servidor.

Cliente/mesa del pedido, receptor/restaurante del comprobante y producto/precio del detalle se capturan históricamente. Editar los catálogos no modifica esas capturas. Los registros legados se identifican como información conocida al migrar.

## Reportes y tiempo

Ventas por día/semana/mes conserva el significado de **comprobantes emitidos**, no de efectivo recibido. La pestaña financiera separa efectivo confirmado, tarjeta manual confirmada, cancelaciones, incidencias y documentos legados sin pago registrado. Los rechazados e inciertos no se suman como cobros confirmados. No sumar todas las filas de esa pestaña: pagos y comprobantes representan conceptos distintos.

Moneda centralizada `Q 125.00`, decimal sin conversiones. Reportes nuevos usan el día de Guatemala UTC−6: eventos financieros se guardan en UTC y se filtran/presentan con sus límites correctos. Los comprobantes nuevos llevan hora Guatemala; las fechas legadas se conservan como estaban. Al conciliar, la fecha del estado actual del pago se actualiza; los intentos anteriores siguen en auditoría.

## Evidencias y pruebas

Compilación Debug y Release sin errores ni advertencias. Suite SQL: **65 comprobaciones correctas, cero fallidas** en la ejecución final documentada. Las pruebas incluyen:

- Construcción/render de diez formularios, sin SQL en sus constructores de diseño.
- Efectivo exacto/con cambio/insuficiente, receptor y validaciones.
- Tarjeta confirmada/rechazada/incierta y conciliaciones.
- Cortesía cero y permisos de administrador/empleado.
- Cancelación, incidencia, limpieza y estado inconsistente.
- Doble cierre, cancelación concurrente con agregado y versión con importe sin cambios.
- Historial de clientes, mesas, productos y documentos legados sin pagos inventados.
- Fallo SQL de emisión provocado con trigger exclusivamente en base aislada: rollback de pago, operación, comprobante y estado.
- Fallo simulado antes de enviar SQL y pérdida simulada de entrega de respuesta después del commit.
- Recuperación del diario cifrado desde otro proceso; una sola aplicación del cobro.
- Recorrido real de controles: login, dos productos, cobro, comprobante, logout y segundo usuario; otro proceso recupera la operación persistida.
- Acceso al botón de cobro en ventanas de 1280 × 720 y 1366 × 768. Capturas de controles reales, no maquetas.

Registro completo, JSON y capturas seleccionadas: `EvidenciasCobros`. No contienen datos de clientes reales.

Límites: las pérdidas de respuesta se inyectaron en el ejecutor; no se desconectó físicamente el equipo ni el servidor habitual. DPI observado 168 (175 %), dos monitores; no se certifica la matriz nativa 100/125/150/200 %. La edición/guardado/reapertura en el diseñador de Visual Studio continúa pendiente; construir y renderizar no prueba esa capacidad. También queda pendiente una sesión exploratoria completa con teclado y cambio entre monitores.

## Respaldo y colaboración

Rama de trabajo: `mejoras/cobros-cancelaciones-20261026`. Respaldo: `respaldo/antes-cobros-20261026`, commit `e410f93`. Sin push ni cambios a main.

La creación de `RestauranteDB` en LocalDB y su configuración Windows fueron solicitadas expresamente por el usuario durante el trabajo. No se cambió la tecnología de base ni se impone esa instancia al grupo.

Los respaldos SQL y sus restauraciones comprobadas están documentados en [ActualizacionGrupo.md](../BaseDatos/ActualizacionGrupo.md). No se incluyen `.bak`, archivos de datos ni contraseñas en Git. Recuperar código y recuperar datos son procedimientos separados.
