# Correcciones operativas y cuentas identificadas 010
Fecha de cierre de pruebas: 3 de noviembre de 2026. Rama: correcciones/cuentas-identificadas-20261102. Base de código inicial: 1add5ac.

## Hallazgos y correcciones
| Hallazgo comprobado | Corrección |
|---|---|
| Pedido 1007, Q 203: solicitud de abono Q 203 con solo Q 100 recibidos. No había pago ni operación SQL de ese GUID; el diario local mantenía bloqueo. | Validación de recibido/aplicado antes del diario, protección adicional del servicio, abono parcial explícito y abandono terminal seguro para rechazos. Nunca se simula un pago. |
| Bloqueo de una comanda por cualquier pendiente del usuario. | Alcance por mesa/pedido y recuperación directa; ilegibles/desconocidos siguen conservados y bloqueantes. |
| La separación 009 era una distribución financiera que congelaba todo el pedido. | Cuentas operativas identificadas desde la toma de la mesa; protege únicamente la cuenta abonada y su captura histórica. |
| No había asignación explícita desde el primer producto. | Selector con nombre/identificador, resumen visible, cuenta general y primer producto transaccional; Toda la mesa no permite asignar productos. |
| Un regreso de diálogo podía continuar la recarga fuera del hilo propietario. | ContextoInterfaz conserva/marshala las continuaciones antes de actualizar controles. Reproducido con controles reales. |
| Lectura de pedido podía adquirir bloqueos en orden opuesto a una escritura. | ObtenerPedidoAbierto bloquea mesa antes del pedido, como las operaciones; lecturas y productos concurrentes comprobados. |
| Promoción inactiva o error de SQL desde selección visual podía producir excepción de evento. | Lectura asíncrona protegida, mensaje, botón desactivado y reintento cambiando opción. Cortesía muestra100 %, restablecimiento 0 %. |

## Reglas que se conservaron
Se conservan SQL Server, LocalDB académico, Windows Forms/Designer, estructura de capas, modelos del dominio, contratos y precios históricos. Las nuevas solicitudes y vistas tienen archivo por tipo. No se modificaron credenciales ni se agregó acceso fijo. Se revalidan sesión y autorizador. Los permisos de usuarios/roles/conexión permanecen exclusivos del Administrador.
Los abonos no implican unidades individuales pagadas: cubren un importe dentro del alcance protegido. No se borran consumo ni preparación para trasladar responsabilidades. Los descuentos reemplazan ajustes, no se acumulan. Cortesía explícita100 % requiere autorización. La mesa solo concluye cuando todo su saldo está resuelto.
Los registros009 con división o pagos permanecen en su flujo original; no se inventa reparto histórico. Sin SAT/FEL/pasarela bancaria.

## Evidencia ejecutada
| Suite / entorno aislado | Resultado |
|---|---|
| CuentasIdentificadas, fixture F 009→010 | 62 comprobaciones, incluyendo concurrencia, rollback, reserva, pagos y recuperación en otro proceso; CHECKDB final. |
| Verificar.ps1 -ConSql | 65 correctas; 0 fallidas. |
| Roles --validar-auditoria | 75 comprobaciones. |
| Reservas | 67 comprobaciones. |
| OperacionIntegral 008→009 | 57 comprobaciones; conserva comportamiento anterior. |
| Visual adicional de beneficios | Cortesía100 %, promoción inactiva con mensaje, restablecimiento 0 %; sin grabar cobros reales. |
| Instaladores nuevos/actualización010 | Nueva instalación010; actualización009→010 ensayada en clon restaurado, huellas de27 tablas coincidentes, repetición y CHECKDB. |
| Compilación | Debug de frontend/backend/verificador y Release de solución,0 errores / 0 advertencias. |

Las capturas son de controles reales a DPI168 (175% observado),1500×1000px; no son maquetas. No se afirma validación real de todos los escalados ni de impresoras, hardware de pago o red compartida. LocalDB no representa un despliegue multiusuario en varias computadoras. Los recibos son académicos. Las pruebas se guardan en Pruebas/EvidenciasCuentas010; los ensayos fallidos de diagnóstico y respaldos permanecen fuera del repositorio.

## Uso y exposición
Véase ../ManualUsuario/CuentasIdentificadas010.md y MapaCuentas010.md. El manual Word/PDF general anterior se complementa con este suplemento010; no debe enseñarse el flujo009 como si fuera la nueva toma de cuentas.

## Respaldo y recuperación
Antes de cambios se guardaron archivo del código, bundle Git verificado y etiqueta respaldo/antes-cuentas-identificadas-20261102-1add5ac. No había cambios locales iniciales. No se publicó ni se mezcló con main.
La migración 010 es aditiva y repetible. Para volver al programa 009 no basta con cambiar de rama: exige la base compatible respaldada. Detenga aplicaciones, preserve un respaldo del estado actual y restaure el .bak anterior como base alternativa con archivos separados, compruebe CHECKDB y conecte una copia del código de la etiqueta anterior. No use DROP, REPLACE ni borre filas de VersionEsquema para forzar una vuelta. Los datos posteriores al respaldo no existen en la copia antigua y deben preservarse/reconciliarse antes de sustituir un destino.
No elimine .bin ni altere una solicitud incierta: recupere el mismo identificador. Solo una operación ausente y comprobada como rechazada puede abandonarse de modo terminal. El historial permanece cifrado.

## Activación habitual completada
RestauranteDB de (localdb)\MSSQLLocalDB quedó en versión 10 el 3 de noviembre de 2026. La activación usó el commit local fa15c74, sin publicar.
Antes de modificar se creó RestauranteDB_20261103_000552_0e7469.bak y se restauró realmente como Restaurante_Pruebas_Restauracion_20261103_000552_0e7469. Se compararon 28 tablas dbo con conteos/huellas iguales entre original y copia antes de ensayar; la última migración se repitió en copia y CHECKDB aprobó. Después de migrar el destino se verificó de nuevo CHECKDB y coincidencia de huellas de Usuarios, Mesas, Pedidos, DetallesPedido, Facturas, PagosManuales, Clientes y ReservasMesa.
Conservados: 3 usuarios, 8 mesas, 8 pedidos, 6 facturas, 6 pagos anteriores y 0 abonos nuevos. No se cambiaron contraseñas ni datos de conexión. Respaldos y registros privados están fuera de Git, en CuentasIdentificadas20261102/RespaldoHabitual010 del directorio de evidencias externo.
Se volvió a inspeccionar el diario cifrado del GUID b6942b6b-3216-47d4-b026-b66d162eb389. Coincidía con el respaldo inicial: solicitado 203, recibido 100, efectivo, usuario 1 / pedido 1007. No existía operación/pago SQL y el pedido conservaba versión/estado/importe esperados. Bajo bloqueo exclusivo del diario y transacción Serializable se registró Abandonada y auditoría técnica con actor NULL/Windows autorizado, sin suplantar una sesión del administrador. Solo después se movió el archivo cifrado al historial y se comprobó su hash contra el respaldo. El pedido 1007 sigue Abierto por Q 203, sin pagos nuevos; no se condonó ni se cobró. No se tocaron otros diarios.
