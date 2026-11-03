# Diseño de operación integral 009

Autorización del usuario: implementar los cambios en una rama conservando respaldo. Rama mejoras/operacion-cuentas-20261102; punto anterior 928ef17.

Se amplía la política de pago completo con abonos manuales y subcuentas. No se cambia el DTO ni el hash de cobros anteriores. Una cuenta sin división admite abonos; para separar se crea una distribución completa y permanente antes de cualquier pago. Los ajustes se aplican antes de dividir o pagar; después se bloquean incorporaciones, correcciones y cierres por incidencia/cancelación para no alterar dinero ya asignado. Si hace falta una excepción financiera, no se simula un borrado: requiere un futuro flujo explícito de reversión.

Cada abono tiene receptor, medio, importe aplicado, entregado, cambio y resultado. Pago incierto bloquea nuevos abonos del pedido hasta conciliación autorizada. Rechazado no reduce saldo. El pago final emite un único resumen académico del pedido; cada pagador tiene su recibo independiente. La factura final no se atribuye al último pagador: receptor Público general, con recibos individualizados. No se reutilizan recibos como documentos fiscales.

Se soporta separación por importes o por unidades completas de productos. En importes se distribuye consumo original; el descuento se reparte proporcionalmente truncando a centavos y asignando el residuo por orden de cuenta. En productos debe asignarse exactamente toda unidad vigente sin duplicarla. No se parte una unidad: un producto compartido puede distribuirse por importe. Las divisiones no mueven ni duplican consumo. No se cierra la mesa hasta saldo global cero. No hay cierre parcial con deuda.

Clientes frecuentes: visitas = pedidos distintos facturados y pagados con importe positivo, sin legados, pendientes ni cancelaciones. No hay umbral inventado: la configuración inicial está inactiva. Gerencia configura porcentaje 5–50, visitas mínimas y vigencia. Alcance inicial: cuenta completa, no acumulable; ajustes manuales o promociones siempre requieren gerencia/autorización puntual. Cortesía explícita del 100 %. No hay datos de costos para garantizar margen.

Las pantallas usan controles existentes y Designer. Inicio muestra reservas y estados para todos los roles; importes financieros de resumen solo para gerencia. Se simplifican tarjetas, cierre y cobro. Primero se ensaya 009 en bases aisladas, con restauración real y CHECKDB; después puede migrarse el destino habitual bajo la autorización actual. Sin publicación ni mezcla con main.
