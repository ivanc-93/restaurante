# Resultados y límites de la reorganización

Rama `refactorizacion/estructura-exposicion-20261102`; referencia `cb342db`. No se publicó ni mezcló con main.

| Verificación | Resultado | Log |
|---|---|---|
| Compilación Debug | 0 errores, 0 advertencias | compilacion-debug-final.log |
| Compilación Release | 0 errores, 0 advertencias | compilacion-release-final.log |
| Auditoría inicial y final | 74 en cada ejecución | auditoria-inicial.log / auditoria-final.log |
| Roles y sesiones | 72 correctas | roles-final.log |
| Regresión general | 65 correctas; 0 fallidas | regresion-general-reintento.log |
| Validación de acceso | 23 correctas | validacion-acceso.log |
| Visual y funcional | 22 correctas, DPI observado 96 | visual-final.log |
| Primer inicio servicio | 14 correctas | primera-instalacion-corregida.log |
| Primer inicio interfaz | 8 correctas, proceso finalizó correctamente | primera-interfaz-final.log |
| Recuperación | 61 correctas | recuperacion-verificada.log |
| Autorización y rechazos | 34 correctas | autorizacion-final.log |

Las suites se solapan; no sumarlas como casos únicos. Los logs finales están en Pruebas/EvidenciasEstructura20261102.

| Hallazgo | Estado | Detalle |
|---|---|---|
| A01 | Parcial | SQL en Data; validaciones extraídas. Mutacion y reglas transaccionales críticas permanecen en la misma clase parcial. |
| A02 | Resuelto | Parámetro requiereGerencia expresa su permiso real; no cambió la matriz. |
| A03 | Parcial | Tres contratos pequeños utilizados por Reportes, Usuarios y códigos; IRepositorio conserva otras capacidades por compatibilidad. |
| A04 | Resuelto en alcance | 308 símbolos renombrados por modelo semántico; Name y referencias de pruebas actualizados. Excepciones dinámicas documentadas. |
| A05 | Resuelto | Referencias tipadas para etiquetas de cobro y operaciones/resumen de reportes; retiradas las búsquedas originales Controls[...]. |
| A06 | Mejorado | Formato, declaraciones separadas y carga de reportes en manejador nombrado. Persisten métodos grandes que requieren otra extracción gradual. |
| A07 | Resuelto a DPI observado | Tarjetas compactas, icono explícito, altura automática y acción visible; pruebas a 1366/1920. DPI reales adicionales pendientes. |
| A08 | Resuelto | Fallback de alerta y diagnóstico una vez por clave; comprobación de alternativa y caché. |
| A09 | Resuelto | MonedaGuatemala pertenece a Dominio; ElementoMenu no referencia Servicios. |
| A10 | Resuelto | Configuración valida esquema con ValidadorEsquemaSql, sin construir el repositorio completo. |
| A11 | Resuelto | Diarios en Infraestructura/Recuperacion, manteniendo rutas/DPAPI/miembros persistidos. |
| A12 | Pendiente de diseño | Se mantiene bloqueo ante resultado incierto de administración de usuarios. No se implementó un nuevo protocolo persistente de idempotencia. |
| A13 | Pendiente de entorno | 21 pares Designer conservados; compilan y se renderizan pantallas. No se certificó abrir/serializar todos en Visual Studio. |
| A14 | Resuelto | Estilo antes de aplicar una sola vez el icono de cerrar sesión. |
| A15 | Fuera del alcance local | Sin cambio de permisos SQL. No se certifica despliegue compartido ni mínimo privilegio de producción. |
| Nuevo: LocalDB | Resuelto | Primer inicio no depende de InstanceDefaultBackupPath; usa respaldo y restauración aislada con CHECKDB antes del token. |

## Incidencias de verificación resueltas

La regresión general tuvo primero 64 aprobadas y una espera agotada en catálogo. Se explicitó el contexto de WinForms y se inyectaron diálogos de prueba, manteniendo sus condiciones: la repetición aprobó 65/65. La prueba modeless de primer inicio también necesitó invocar PerformClick con contexto WinForms; se verificó una ejecución final con salida 0. Las referencias antiguas que quedaron en arreglos de nombres de pruebas se actualizaron, sin relajar permisos ni validaciones. Las rutas de migración de recuperación admiten SABORES_SCRIPTS_PRUEBAS para separar evidencias y fuentes. Las referencias a ApplicationConfiguration de los verificadores son explícitas con global::, eliminando la ambigüedad sin desactivar advertencias.

Se detectó un fallo previo del primer inicio LocalDB: SERVERPROPERTY('InstanceDefaultBackupPath') no ofrecía ruta. Ahora la utilidad compartida consulta los archivos de SQL, crea COPY_ONLY/CHECKSUM, ejecuta VERIFYONLY, restaura con nombres y archivos únicos, aplica CHECKDB y coteja cantidad de usuarios antes de continuar. La herramienta técnica conserva su entrada pública como adaptador. Las copias aisladas invalidan códigos de recuperación y su identidad, sin modificar la base original. Se verificó en bases de prueba; no se ejecutó esta preparación sobre RestauranteDB.

## Evidencia visual

Las imágenes son renderizados reales de formularios Windows Forms con datos ficticios mediante el verificador, no mockups ni generación por IA. Se revisaron mesas a 1366 × 768 y 1920 × 1080; hay desplazamiento vertical cuando no caben todas las tarjetas. Se verificó texto de estado completo, imagen y acción dentro de la tarjeta. El DPI observado fue 96. Pedir imágenes de icono a otro tamaño no equivale a probar el sistema a otro DPI.

La captura nativa mediante el conector de Windows no se completó por vencimiento de su aprobación. No se afirma una comprobación de escritorio que no se realizó. No se cambiaron escalado, privacidad ni seguridad de Windows.

## Pendientes explícitos

- Abrir los 21 formularios en el diseñador de Visual Studio, revisar eventos y comprobar que una edición no serialice cambios inesperados. Compilar/renderizar no prueba esa operación.
- Probar otros DPI reales y monitores, navegación de teclado completa, lector de pantalla y contraste medido.
- La extracción completa de mutaciones, seguridad y SQL del repositorio requiere diseño específico que conserve la única transacción y el orden de bloqueos. No se fragmentó para aparentar una separación total.
- No hay nuevo protocolo idempotente de guardado administrativo de usuarios. Se conserva su protección ante resultado incierto y su orientación de comprobación.
- Despliegue académico local; no certificación multiusuario de producción, SAT/FEL ni integración bancaria.

No se añadieron paquetes, no se cambiaron scripts SQL ni el formato de solicitudes persistidas. No se tocaron credenciales, conexión ni datos de la base habitual. Las pruebas crean fixtures Restaurante_Pruebas_* y copias Sabores_Validacion_*; se conservaron para diagnóstico y no se eliminaron automáticamente.

## Comprobación adicional de preservación

Se compararon 21 conjuntos de tokens C# contra el ZIP anterior: Mutacion, SesionEn, validación de cobro extraída, creación/verificación criptográfica, solicitudes y métodos del diario protegido. Todos coinciden en sus cuerpos o contratos comparados; esto no sustituye las pruebas funcionales. Ver InvariantesPreservados.json.

Commits de implementación: f6012ee (formato), 94ed07d (nombres), d199a90 (reorganización, respaldo LocalDB y presentación).

