# Sabores: cuentas identificadas desde la comanda
Actualización 010 · 3 de noviembre de 2026. Complementa el manual general; para cuentas nuevas reemplaza el procedimiento de separación al cobrar del suplemento 009.

## Atención paso a paso
1. Abra Mesas y seleccione Abrir pedido o Atienda la reserva. Abrir la ventana no registra consumo.
2. En Cuenta general puede trabajar una sola cuenta. Use Nombre / retirar para identificarla, o Agregar cuenta para preparar Ana, Carlos, Familia Pérez, etc. Un nombre operativo no crea un cliente ni un usuario.
3. Seleccione la cuenta en la lista superior o en el resumen visible. La selección indica dónde quedará cada producto. Toda la mesa muestra el conjunto; seleccione una cuenta concreta para ordenar.
4. Busque un producto, elija cantidad y pulse Agregar al pedido. El primer producto crea el pedido, asigna las unidades y, si corresponde, atiende la reserva en una sola operación.
5. Puede alternar entre cuentas. El resumen muestra consumo original, descuento, pagado y saldo. Los identificadores distinguen nombres repetidos. Confirme nombres provisionales antes de cobrar.
6. Si una persona ya pagó y pide otra cosa, agregue una cuenta nueva, por ejemplo Ana adicional. No reabra su cuenta liquidada.

## Asignación y correcciones
Mover / corregir distingue dos acciones:
- Mover a otra cuenta: selecciona producto, unidades, cuenta de destino y motivo. Conserva precio, consumo y preparación. Ambas cuentas deben estar sin pagos ni ajustes; restablezca previamente un descuento mediante autorización si necesita moverlo.
- Corregir captura: retira unidades solamente antes de declarar preparación y sin consumo, con motivo y autorización de Gerente/Administrador. Tras preparación se registra incidencia; no se borra consumo.
Solo se retiran cuentas vacías sin pagos ni ajustes. Se conserva su registro. Máximo veinte cuentas activas por mesa.

## Beneficios
Seleccione una cuenta o Toda la mesa y pulse Beneficio. Descuento: 5–50 %. Frecuente: exige promoción activa y elegibilidad del cliente real del pedido. Cortesía: 100 %. Restablecer: 0 %. El porcentaje mostrado coincide con la opción elegida.
Todos exigen motivo y Gerente/Administrador, directamente o mediante autorización puntual al Mesero. Una autorización solo sirve para su operación, importe, versión y alcance exactos. No cambia la sesión del solicitante. Los beneficios reemplazan el ajuste anterior del alcance; no se acumulan. Se aplican antes de cobrar las cuentas afectadas. Toda la mesa requiere que todas estén sin pagos/protección. No modifica precios históricos.
Una cortesía liquida el alcance con historial y autorización; no simula un pago en efectivo. La mesa no se cierra mientras otras cuentas conserven saldo.

## Cobro y comprobantes
1. Seleccione la cuenta y pulse Cobrar esta cuenta. Revise sus productos y saldo.
2. Escriba el importe que se aplicará y el método manual: efectivo o tarjeta. El receptor del recibo se confirma separadamente del nombre operativo.
3. En efectivo, el recibido debe cubrir el importe aplicado. Para recibir Q100 de un saldo Q203, indique importe aplicado Q100 y recibido Q100; quedan Q103. Si ya escribió recibido Q100 e importe Q203, use Aplicar Q100 como abono parcial. Es una acción explícita, no un cambio automático.
4. Confirme los datos y el pago realmente recibido. Revise la confirmación antes de registrar. El cambio es recibido menos aplicado, no reduce saldo adicionalmente.
5. Tarjeta es registro manual. Confirmado reduce saldo; Rechazado no lo reduce; Incierto exige conciliación autorizada con evidencia y no permite repetir el cobro de esa cuenta.
6. Un abono protege únicamente su cuenta. Otras cuentas sin pagos pueden seguir ordenando. Se conservan nombres, unidades, precios y ajustes en el recibo emitido.
7. Consulte Abonos y recibos para cada operación. Al resolverse todo el saldo se genera un resumen final académico y la mesa pasa a limpieza. No se libera una mesa solo por liquidar una persona.
GTQ, símbolo Q. Sin conexión SAT, FEL ni bancaria. No capture PAN, CVV ni datos de tarjeta completos.

## Operaciones pendientes
Revisar pendiente en la comanda/cobro abre recuperación para la mesa/pedido. Inicio conserva la vista de recuperación general del usuario y la base.
Si perdió una respuesta, recupere la misma solicitud: no pulse un nuevo cobro. SQL determina si ya se aplicó y devuelve el resultado sin duplicarlo.
Un rechazo de datos deja de bloquear solo cuando SQL confirma el abandono terminal del identificador. Si SQL no responde, se conserva el pendiente. Los archivos ilegibles se conservan para revisión; no se borran. El historial resuelto permanece cifrado en el perfil del equipo.
Cerrar la ventana no cancela, no borra consumo y no libera la mesa. Un pedido que quedó vacío mediante corrección autorizada puede gestionarse con Cancelar, motivo e historial.

## Datos de versiones anteriores
Las cuentas divididas o con pagos anteriores conservan el flujo 009; no se reparten otra vez ni se convierten silenciosamente. Los pedidos abiertos anteriores sin distribución/pagos aparecen en Cuenta general; solo se materializa al usar explícitamente una cuenta. Los nombres operativos nunca sustituyen datos de facturación.

## Permisos
Mesero puede preparar cuentas, ordenar, trasladar responsabilidad sin eliminar consumo y cobrar. Correcciones, descuentos, cortesías y conciliaciones requieren autorización puntual. Gerente y Administrador operan y autorizan. Usuarios, roles y conexión permanecen exclusivos del Administrador.
