# Correcciones de la auditoría de Sabores

Se implementaron las correcciones en una rama independiente, con respaldo del código y respaldo/restauración comprobada de SQL. RestauranteDB quedó en esquema 7 conservando las 17 tablas existentes. No se publicaron cambios ni se mezclaron con main.

## Verificación y límites

Se conservaron datos, usuarios, contraseñas, SQL Server, archivos Designer, objetos del dominio y GTQ. No se añadieron servicios SAT/FEL/bancarios. Las pruebas de negocio se ejecutaron en bases ficticias, separadas de la habitual. Se comprobó restauración y CHECKDB antes de migrar.

La matriz siguiente distingue lo implementado de los riesgos que requieren validación o decisiones posteriores. La aplicación no está certificada para operación comercial de alta demanda. La preparación es por pedido y las incidencias se resuelven completas; esos límites están documentados en el manual.

La regresión general inicialmente detectó lectura UTF-8 incorrecta bajo Windows PowerShell 5; se corrigió y su repetición pasó. La regresión de roles detectó una expectativa sobre el texto Usuario guardado; se conservó ese prefijo, se añadió el usuario objetivo y se repitió la interfaz correctamente. No se ocultaron las fallas ni se debilitó la validación.

## Estado de los hallazgos

| ID | Prioridad | Estado | Evidencia o límite |
| --- | --- | --- | --- |
| F01 | P1 | Corregido | Control central de lectura dentro de la transacción; pruebas por rol, cerrado ajeno y desactivación. |
| F02 | P1 | Corregido y activado | Capacidad 006 completa y esquema 7 instalados con respaldo/restauración/ensayo; usuarios conservados. |
| F03 | P1 | Corregido | Abandono terminal SQL con bloqueo del identificador; exclusión local por archivo; carrera controlada y respuesta perdida verificadas. |
| F04 | P1 | Implementado con alcance aprobado | Corrección de unidades antes de preparación, ledger inmutable y autorización. Preparación por pedido; sin merma parcial por línea. |
| F05 | P1 | Implementado con alcance aprobado | Cobro posterior completo confirmado, pérdida y condonación; resolución única sin cambiar mesa reutilizada. Sin pagos parciales. |
| F06 | P2 | Corregido | Renueva autorización de la misma solicitud y GUID al recuperar; primero comprueba resultado ya registrado. |
| F07 | P2 | Corregido | En limpieza visible y transición de catálogo bloqueada; metadatos preservados. |
| F08 | P2 | Corregido | UI admite el rango positivo de int de servicio/SQL sin truncar; caso 10001 verificado. |
| F09 | P2 | Corregido | Regla compartida antes de solicitar autorización y dentro del servicio. |
| F10 | P2 | Corregido | Bloquea edición y selección durante guardado; resultado identifica cuenta. |
| F11 | P2 | Corregido | Carga/auditoría ocupadas y cierre seguro; prueba de lectura SQL bloqueada y liberación. |
| F12 | P2 | Corregido | Replay valida destino, mecanismo, autorizador y contraseña contra hash adaptativo vigente. Si la contraseña cambió después, devuelve conflicto y permite consultar el recibo original. |
| F13 | P2 | Corregido con cautela operativa | Listado por archivo conserva ilegibles y recupera válidos. Un pedido desconocido por archivo ilegible mantiene bloqueo conservador de nuevas escrituras hasta revisión técnica. |
| F14 | P2 | Corregido con límite explícito | Mensajes SQL diferenciados. Resultado incierto bloquea reenvío en esa ventana y consulta lista; no confirma contraseñas por lectura de campos. |
| F15 | P2 | Corregido | Resúmenes humanos y resultados de cobro sin JSON bruto. |
| F16 | P2 | Mitigado y parcialmente verificado | Tamaño limitado al área útil, scroll y textos ajustados. Capturas reales; matriz completa de resolución/DPI pendiente. |
| F17 | P2 | Parcialmente verificado | Escape comprobado en nuevos formularios y regresión de acceso. Quedan matriz de teclado completa, lector de pantalla y contraste medido. |
| F18 | P2 | Corregido | Ranking rotulado consumo bruto antes de ajustes; no se inventa reparto neto por plato. |
| F19 | P2 | Corregido | Lectura de mesa en Task.Run y await bajo bloqueo de navegación existente. |
| F20 | P2 | Corregido según decisión | Mesero crea clientes para facturar; edición de existentes reservada a Gerente/Administrador en backend y UI. |
| F21 | P2 | Corregido para fechas nuevas | Default explícito Guatemala; registros históricos preservados sin reinterpretación. |
| F22 | P3 | Documentado y pendiente de política | Respaldos verificados y manifiestos conservados. Retención y limpieza automática no impuestas. |
| F23 | P1 | Evaluado para alcance local | Conexión habitual comprobada: sysadmin/db_owner. Alcance académico LocalDB aprobado; despliegue compartido requiere nuevo modelo y pruebas de mínimos privilegios. |
| F24 | P3 | Conservado por ser opcional | No se borraron métodos privados candidatos ni se alteró material didáctico. |
| F25 | P3 | Documentado; Designer visual pendiente | Se conserva Windows Forms, clases de dominio y partial con Designer. No se certificó abrir todos los Designer en Visual Studio. |

## Archivos principales

RepositorioRestaurante.Seguridad y Operaciones centralizan la lectura; Abandono registra el estado terminal; Recuperacion valida capacidades y replay. Correcciones implementa la vista de unidades vigentes, preparación y resolución de incidencias. Migracion_007 es aditiva: la captura original permanece en DetallesPedido y los retiros se registran en CorreccionesLinea. ResolucionesIncidencia conserva origen y desenlace.

FormularioCorreccionLinea, FormularioIncidencias y FormularioResolverIncidencia tienen su archivo Designer separado. DiarioOperaciones coordina solicitudes y conserva archivos ilegibles. Los formularios existentes mantienen su arquitectura.

El contrato ImporteOriginal de pedido/comprobante representa el bruto vigente antes de descuentos. La captura anterior a correcciones permanece por línea e historial; no se sobrescriben precios históricos ni documentos emitidos.

## Documentación de uso y recuperación

Consulte Documentacion/ManualUsuario/Actualizacion_20261028.md y Pruebas/RecuperacionAuditoria20261028.md. Los manuales anteriores permanecen como antecedentes; para estos cambios prevalece la actualización. No se compartieron credenciales reales ni el perfil DPAPI.

## Resultados comprobados

- Solución Debug y Release: compilación correcta, cero advertencias y cero errores en ambos registros finales.
- PruebasAuditoria: 74 comprobaciones aprobadas de permisos, correcciones, incidencias, abandono concurrente, respuesta perdida, diarios dañados, recuperación repetida y cierre durante lectura SQL.
- Regresión general: 65 correctas, cero fallidas, incluidas ventas, pagos, concurrencia, historial y Unicode bajo Windows PowerShell 5.
- Regresión de interfaz: 16 comprobaciones aprobadas después de corregir el texto esperado; creación de usuario, cambio obligatorio, ingreso, autorización y menú de Mesero.
- La ejecución amplia de roles aprobó sus comprobaciones previas, incluida instalación concurrente y protección del último administrador, y se detuvo por el prefijo del mensaje de guardado. Después de ajustar ese prefijo se repitió correctamente la parte de interfaz; no se presenta aquel registro interrumpido como una ejecución completa aprobada.
- El verificador auxiliar puede advertir NU1900 al no poder consultar el índice de vulnerabilidades NuGet y CS0436 por ApplicationConfiguration visible entre ensamblados de prueba. No se desactivaron comprobaciones; no se certifica una auditoría actual de dependencias por ese motivo.
- RestauranteDB: esquema 7, CHECKDB sin errores, 17 tablas existentes conservadas por conteos y checksums; dos respaldos independientes restaurados y conservados.
- Conexión realmente guardada en este perfil: RestauranteDB, sysadmin=1, db_owner=1, ALTER=1, SELECT=1. No se imprimió su cadena ni credenciales. El modelo compartido de mínimos privilegios continúa pendiente de diseño y prueba.

Las capturas de las ventanas nuevas son de ejecución real con datos ficticios. Los renders automáticos históricos no sustituyen esa inspección ni prueban todos los DPI. No se modificaron Windows, el escalado del escritorio, SAT, FEL o servicios bancarios.

## Incidencia adicional al generar el código de emergencia

La captura del usuario muestra UnauthorizedAccessException en GenerarCodigoRecuperacion, pero no contiene el mensaje ni permite concluir si hubo contraseña incorrecta, bloqueo o sesión modificada. La ruta anterior lanzaba una excepción para rechazos esperados y el depurador podía detenerse antes del catch del formulario.

FormularioCodigoRecuperacion ahora llama IntentarGenerarCodigoRecuperacion e IntentarConfirmarCodigoRecuperacion. Los rechazos esperados devuelven resultados con mensajes para contraseña no verificada, cuenta bloqueada, cambio obligatorio, rol sin permiso y sesión modificada. Las APIs anteriores se conservan como compatibilidad y mantienen su rechazo de acceso. No se debilitaron permisos ni se desactivaron excepciones del depurador.

El mensaje aclara que se pide la contraseña vigente de ingreso, no el código de emergencia, e indica guardar el código y confirmar su custodia. Un intento con contraseña incorrecta conserva el código vigente; ante un resultado SQL incierto se evita confirmar un código potencialmente reemplazado. No se cambiaron la contraseña ni los códigos reales del usuario para probarlo.

Las comprobaciones de generación, rechazo, bloqueo y confirmación se ejecutaron con cuentas ficticias; el seguimiento FirstChance no registró UnauthorizedAccessException en los rechazos esperados de la nueva ruta. No se identifica como comprobada la causa específica de la captura original.
