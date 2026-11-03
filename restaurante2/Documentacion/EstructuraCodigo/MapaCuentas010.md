# Mapa para exposición: cuentas y recuperación
3 de noviembre de 2026. Se conserva Windows Forms y un archivo por tipo nuevo; los archivos partial del repositorio y formularios representan una misma clase, dividida por responsabilidad.

| Ubicación | Qué hace / por qué existe |
|---|---|
| Dominio/CuentaOperativa.cs | Estado de una cuenta identificada y cálculo Total/Saldo/Etiqueta; no controla ventanas. |
| Dominio/DetalleCuenta.cs | Unidades y precio histórico asignados, sin duplicar el detalle original. |
| Dominio/ServicioMesa.cs | Mesa, pedido y cuentas que necesita presentar la comanda. |
| Dominio/Solicitud*Cuenta.cs | Datos explícitos de cada operación, GUID estable y versión esperada. |
| Dominio/AlcanceAbono.cs | Captura inmutable del alcance del recibo. |
| Aplicacion/Contratos/ICuentasOperativas.cs | Interfaz para la capacidad de cuentas; UI depende del contrato. |
| Data/Repositorios/RepositorioRestaurante.CuentasOperativas.cs | Consulta, nombres, asignación y traslado transaccionales. |
| Data/Repositorios/RepositorioRestaurante.AjustesCuenta.cs | Corrección y beneficios autorizados, centavos y liquidación. |
| Data/Repositorios/RepositorioRestaurante.AlcanceAbono.cs | Captura y presentación del recibo histórico. |
| Data/Repositorios/RepositorioRestaurante.Autorizacion.cs | Valida autorización exacta, sesión y versión del autorizador. |
| Data/Repositorios/RepositorioRestaurante.Abandono.cs | Estado técnico terminal para impedir aplicación tardía. |
| BaseDatos/Migracion_010.sql | Cambio aditivo y repetible; separa cuenta operativa de distribución financiera anterior. |
| Formularios/FormularioDetalleMesa.Cuentas.cs | Selección y presentación de las cuentas; delega SQL al contrato. |
| Formularios/FormularioNombreCuenta.cs + Designer | Preparar, confirmar nombre o retirar una cuenta vacía. |
| Formularios/FormularioProductoCuenta.cs + Designer | Diferencia traslado de unidades y corrección de captura. |
| Formularios/FormularioBeneficioCuenta.cs + Designer | Beneficio por alcance, porcentaje visible, autorización y errores recuperables. |
| Infraestructura/Recuperacion/DiarioOperaciones.cs | Diario cifrado antes de enviar, recuperación con el mismo GUID y archivo de historial. |
| Infraestructura/Recuperacion/AlcancePendiente.cs | Evita bloquear otras mesas por un pendiente de alcance conocido. |
| Infraestructura/ContextoInterfaz.cs | Devuelve actualizaciones al hilo propietario después de diálogos. |
| Pruebas/CuentasIdentificadas/Program.cs | Fixtures ficticias, concurrencia, fallos simulados y controles reales. |

## Controles principales y eventos
| Control (Name) | Uso |
|---|---|
| cbCuentasServicio | Selección explícita de cuenta; SelectedIndexChanged → CambiarCuentaServicio. |
| dgvCuentasServicio | Resumen con consumo, descuento, pagado, saldo y estado. CellClick selecciona cuenta. |
| btnCrearCuenta | CrearCuenta_Click abre FormularioNombreCuenta. |
| btnNombreCuenta | NombreCuenta_Click abre identificación/retiro permitido. |
| btnMoverCuenta | MoverCuenta_Click abre movimiento/corrección. |
| btnBeneficioCuenta | BeneficioCuenta_Click define el alcance antes de autorizar. |
| btnPendienteMesa | PendienteMesa_Click abre recuperación filtrada. |
| ctNombre / chkConfirmado | Nombre operativo y confirmación previa al cobro. |
| cbAccion / cbDestino / nudCantidad / ctMotivo | Acción, destino, unidades y motivo de movimiento/corrección. |
| cbTipo / nudPorcentaje / btnAplicar | Tipo de beneficio, porcentaje y envío autorizado. |
| cbCuenta / nudImporte / nudEntregado | Cuenta a cobrar, importe aplicado y efectivo recibido: valores diferentes. |
| btnUsarRecibido | Propone explícitamente un abono del importe recibido. |

## Recorrido que puede exponerse
Formulario → solicitud tipada con GUID/versión → diario cifrado → repositorio → transacción (sesión, operación, mesa, pedido, permisos, unidades) → resultado SQL → archivo del diario → recarga visual. Un error no prueba por sí solo que no hubo commit: por eso se recupera el mismo GUID. SQL conserva precios, unidades, permisos y dinero; el formulario los muestra.

Las clases abstractas, objetos de plato/bebida, contratos y recursos existentes se conservan. No se agregan jerarquías artificiales para esta funcionalidad. Los Designer mantienen la construcción visual separada de los eventos.
