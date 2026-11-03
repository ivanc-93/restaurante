# Verificación de reservas — 2 de noviembre de 2026

## Resultado

| Verificación | Resultado |
|---|---|
| Suite específica de reservas | 67 comprobaciones aprobadas |
| Regresión general con SQL | 65 correctas, 0 fallidas |
| Auditoría de funciones y seguridad existente | 74 comprobaciones aprobadas |
| Debug y Release de la solución | 0 errores, 0 advertencias |
| Actualización 7 → 8 por instalador | Respaldo, restauración real, comparación, CHECKDB y migración aprobados |
| Repetición de 008 e instalación nueva opt-in | Aprobadas en bases aisladas |

Los 206 resultados son comprobaciones de tres suites, no 206 escenarios independientes. Todas las escrituras de prueba se realizaron sobre bases aisladas y sus copias. No certifican un despliegue de producción ni escalados que no se hayan medido.

## Cobertura de la nueva suite

Permisos y cambios de sesión; cuatro roles incluido Empleado legado; nombre vacío, espacios, longitud/control; disponibilidad física y pedido abierto inconsistente; dos reservas concurrentes; reserva frente a apertura; atención frente a cancelación; autorización de cancelación, datos modificados y autorizador revocado; replay con el mismo GUID; rollback SQL forzado; respuesta perdida y recuperación; consulta en un proceso nuevo; UI con controles reales; filtros; nombre completo; reserva anterior; integridad final.

La prueba UI detectó inicialmente un error de selección en el propio test después de refrescar (el filtro se recrea), que se corrigió. La inspección visual detectó botones fuera de la zona visible; se separó pie y cuerpo y se repitió la suite completa en una fixture nueva. El resultado entregado es el de esa ejecución final.

## Evidencias y límites visuales

Capturas obtenidas con DrawToBitmap de formularios reales y datos ficticios. No son fotografías del escritorio ni maquetas. DPI medido: **168 (175 %)**; no se cambió el escalado de Windows. Se capturó la pantalla principal a 1366 × 768 y 1920 × 1080. A 1366 × 768 y ese escalado se necesita desplazamiento vertical para parte del contenido; la descripción larga del encabezado existente puede recortarse. El titular dispone de elipsis/tooltip y consulta completa. No se certificaron 100, 125, 150 o 200 %, apertura en Designer ni otra computadora.

No se añadió agenda, expiración automática, anticipos, regularización automática de reservas anteriores ni un informe visual de reservas cerradas. El historial permanece en SQL y auditoría. La base habitual sigue sin migrar.

## Reproducir

Proyecto: `Pruebas/Reservas/VerificadorReservas.csproj`. Compile con `dotnet build`, usando artefactos externos si desea evitar bin/obj en el repositorio. Prepare una **base nueva de prueba en esquema 7** con InstalarNuevaBase.ps1 -ConOperaciones y un token ficticio privado. Defina:

- SABORES_RESERVAS_PRUEBAS: conexión Windows a una base cuyo nombre empiece por Restaurante_Pruebas_Reservas_.
- SABORES_TOKEN_PRUEBAS: el mismo token de preparación.
- SABORES_SCRIPTS_PRUEBAS: ruta absoluta de BaseDatos.

Ejecute Verificador.exe con una carpeta de salida. El programa modifica esa fixture, genera datos ficticios y realiza backup/restauración con el servicio existente. No debe ejecutarse sobre una base con datos reales ni reutilizar una fixture ya ejecutada.

Consulte [activación y reversión](../../BaseDatos/Reservas008.md), [manual](../../Documentacion/ManualUsuario/Reservas_20261102.md) y [mapa de código](../../Documentacion/EstructuraCodigo/Reservas_20261102.md).
