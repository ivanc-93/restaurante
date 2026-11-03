# Uso de Sabores con las correcciones del esquema 7

Actualización del 28 de octubre de 2026. Complementa los manuales anteriores; para los flujos modificados prevalece esta guía. Conserva Windows Forms, SQL Server, GTQ y pagos manuales. Los comprobantes son académicos, sin SAT, FEL ni servicios bancarios.

## Ingreso y roles

Abre la solución y establece Restaurante.Frontend como proyecto de inicio. Ingresa con tu usuario individual existente. Esta actualización no cambia contraseñas, no crea usuarios de demostración en la base habitual y no reinicializa una base si falta la configuración del equipo.

- Mesero: comandas, cobros, alta de clientes para facturar y consulta de sus incidencias. No edita clientes existentes ni resuelve incidencias.
- Gerente: vende, administra mesas, productos y clientes, autoriza correcciones y ajustes y resuelve incidencias.
- Administrador: además administra usuarios, roles y conexión SQL. El último administrador activo sigue protegido.

Si tu sesión fue desactivada o cambió de rol, vuelve a iniciar sesión. Para olvidar contraseña, usa el enlace del acceso; si la base no tiene la estructura requerida, el programa informa que necesita una actualización respaldada. Nunca reinstales una base existente para recuperar una cuenta.

## Mesas y comanda

Selecciona Mesas y una mesa disponible. El primer producto abre y guarda el pedido. Revisa las cantidades antes de agregar; cada incorporación se guarda. Una mesa ocupada o en limpieza no se libera editando el catálogo. Después del cierre, confirma la limpieza desde Gestionar cierre.

El editor conserva números válidos mayores de 10000, muestra En limpieza y permite cambiar sus metadatos sin simular un estado diferente. No renumeres mesas con pedidos activos.

## Corregir unidades registradas por error

1. Abre la mesa y pulsa Corregir unidades.
2. Selecciona la línea. El selector muestra unidades vigentes y las originalmente capturadas.
3. Indica cuántas unidades retirar, sin superar las vigentes.
4. Declara que no hubo preparación ni consumo de esas unidades y escribe un motivo.
5. Solicita autorización de Gerente o Administrador. Su contraseña no se guarda ni cambia tu sesión.
6. Revisa el total actualizado. Un descuento previo queda invalidado y debe revisarse de nuevo.

Las líneas originales y precios históricos permanecen; una tabla de correcciones registra unidades retiradas, antes/después, motivo y responsables. Retirar todas las unidades deja la comanda sin productos vigentes: no inventes un cobro; resuelve su cancelación con el responsable.

## Declarar preparación y conservar consumo

Pulsa Declarar preparación cuando el pedido entre en preparación. Es una declaración del operador por pedido, no una integración con cocina ni un estado individual por producto. Una vez declarada, no se permite retirar unidades como error de captura ni cancelar afirmando que no hubo preparación. Usa Gestionar cierre para registrar una incidencia de consumo. Esta versión conserva el consumo completo de esa incidencia; no implementa mermas parciales por línea.

## Resolver incidencias

Desde Inicio abre Incidencias de consumo. La vista muestra pendientes de forma predeterminada; selecciona Todas para revisar el historial. Las tablas permiten desplazamiento horizontal para consultar las columnas completas. Mesero solo ve las incidencias que registró; Gerente y Administrador revisan todas.

Para resolver, un Gerente o Administrador selecciona una pendiente, pulsa Resolver incidencia seleccionada y elige:

- Cobro posterior: pago completo confirmado, en efectivo o tarjeta manual. Revisa receptor, identificación si corresponde, importe, referencia de tarjeta y evidencia. Efectivo admite cambio; tarjeta exige importe exacto. No admite pagos parciales ni confirma un resultado externo incierto. Un saldo cero se resuelve sin registrar efectivo o tarjeta ficticios.
- Pérdida: documenta el consumo sin recuperar dinero. No se registra un pago ficticio.
- Condonación: documenta la decisión de no exigir el saldo. No equivale a un cobro.

Confirma el desenlace real antes de registrar. La resolución es única, conserva el saldo de origen y su historial. Un cobro posterior emite el comprobante académico y no cambia el estado actual de la mesa, que puede estar atendiendo otro pedido. Una incidencia resuelta no admite un segundo desenlace.

## Recuperar una operación pendiente

Si SQL no respondió, abre Inicio y Recuperar operaciones pendientes. No vuelvas a crear la misma intención con otro identificador.

- Recuperar conserva los datos y el identificador originales. Si SQL ya la aplicó, recupera el resultado sin duplicarla.
- Si necesita autorización vigente, el responsable autoriza de nuevo exactamente la solicitud original. Si la cuenta cambió, la solicitud no se fuerza con importes nuevos.
- Abandonar solo sirve para una solicitud que no se aplicó. SQL deja una marca terminal que impide que otra instancia la ejecute después. No cancela pedidos, consumo, pagos o documentos.
- Un registro ilegible se conserva para asistencia técnica. Los registros válidos siguen disponibles para recuperar. Por cautela, un registro cuyo pedido no puede identificarse puede bloquear nuevas escrituras hasta revisarlo.

Las solicitudes permanecen cifradas por perfil de Windows. La exclusión por archivo coordina procesos del mismo perfil; la marca SQL protege el identificador incluso si una instancia ya tenía preparada la petición.

## Usuarios y mensajes de error

Durante una carga o guardado de usuarios, espera a que termine antes de cerrar o iniciar otra edición. El mensaje identifica el usuario guardado. Usuario duplicado, permisos SQL y problemas de conexión tienen mensajes distintos.

Si un guardado queda con resultado incierto, esa ventana no repite el envío. Revisa la cuenta y la auditoría antes de otra intervención. Consultar los campos de una cuenta no demuestra que un cambio de contraseña haya concluido. No compartas contraseñas en capturas ni registros.

## Reportes y fechas

El ranking de platos muestra consumo bruto antes de descuentos y cortesías, utilizando unidades vigentes después de corregir capturas. Comprobantes emitidos, pagos confirmados, incidencias y sus resoluciones son conceptos diferentes; no sumes todas sus filas como efectivo recibido.

Las fechas nuevas de pedidos usan Guatemala explícitamente. Las fechas históricas no fueron reinterpretadas. No se asignan descuentos a cada producto para inventar un neto por plato.

## Límites y soporte

Se probaron los flujos indicados en el informe de correcciones con bases ficticias. La validación visual se realizó en el escritorio disponible y no certifica todos los tamaños, escalados DPI o lectores de pantalla. No es una certificación para operación comercial de alta demanda.

El despliegue aprobado es académico local con LocalDB. El propietario de la instancia puede administrar SQL fuera de los roles de Sabores; para un despliegue compartido se necesita aprobar y probar una cuenta operativa de privilegios mínimos y separar mantenimiento.

## Generar y confirmar el código de emergencia

1. Inicia sesión con tu cuenta de Administrador y abre Mi código de emergencia. Si aparece durante el ingreso, completa el mismo procedimiento allí.
2. En Contraseña vigente escribe la contraseña con la que ingresas a Sabores; no escribas el código de emergencia en ese campo.
3. Pulsa Generar código. Si la contraseña no se verifica, el acceso cambió o hay bloqueo temporal, lee el mensaje y corrige la causa indicada. No necesitas inventar una contraseña nueva ni cumplir una regla de longitud distinta para reautenticarte.
4. Si se genera correctamente, guarda el reemplazo en un lugar seguro fuera del equipo. El código anterior queda revocado.
5. Marca He guardado el código en un lugar seguro fuera del equipo y pulsa He guardado el código. Esa confirmación activa su uso para recuperación; generar por sí solo no completa este paso.

No envíes tu contraseña ni tu código en capturas, chats o registros. Una contraseña incorrecta no revoca tu código existente. Si el resultado SQL es incierto, comprueba la conexión y genera un reemplazo para conservar y confirmar el último obtenido.
