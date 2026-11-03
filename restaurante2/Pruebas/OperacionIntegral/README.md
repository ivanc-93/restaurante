# Resultados de operación integral 009

## Comprobaciones ejecutadas

| Suite | Resultado |
|---|---|
| Operación integral: abonos, cuentas, promociones, recuperación y UI | 57 aprobadas |
| Reservas | 67 aprobadas |
| Auditoría existente sobre esquema 009 | 74 aprobadas |
| Regresión general con SQL | 65 correctas, 0 fallidas |
| Filtro preventivo de referencia | 3 aprobadas |
| Debug y Release | Sin errores ni advertencias |

Son 266 comprobaciones de varias suites, con cobertura relacionada; no 266 escenarios independientes. Se usaron exclusivamente fixtures `Restaurante_Pruebas_*` y clones generados para ensayos. Las ejecuciones iniciales se conservaron para diagnóstico, pero las evidencias entregadas son de las revisiones finales.

Cobertura: esquema 8 previo, 009 repetida y preservación de pedidos, respaldo/restauración/CHECKDB, cobros antiguos, permisos, sobrepago, efectivo/cambio, tarjeta rechazada/incierta, autorización puntual, replay, dos terminales, subcuentas por importes y productos, centavos, congelación de cambios, historial de beneficios, visitas distintas, cortesía y diario cifrado. Un trigger de prueba verifica rollback real. También se prueba que una estructura incompleta bloquee operaciones en lugar de permitir el flujo antiguo.

## Verificación visual

Formularios reales renderizados mediante DrawToBitmap con datos ficticios; no son maquetas ni capturas del escritorio. DPI observado **168 (175 %)**. Se verificó la carga de Inicio con reservas, comanda, tablas de cobranza, división, cierre y recibo. Se corrigieron tamaños y superposiciones encontrados en las revisiones iniciales y se añadió una comprobación de geometría del botón/tabla.

A ese escalado se utiliza desplazamiento en zonas con mucho contenido. No se certificó apertura en Designer, otro equipo, todos los DPI ni una sesión manual completa con personas usando terminales externas. Las escrituras financieras se verificaron mediante servicios reales y SQL, no mediante una transacción bancaria. El modelo no incluye devoluciones ni modificaciones posteriores a la división/primer pago; sus límites están documentados.

## Reproducción

Compile `Pruebas/OperacionIntegral/VerificadorOperacionIntegral.csproj`. Prepare una base NUEVA en esquema 8 (`InstalarNuevaBase.ps1 -ConReservas`) con token ficticio. Defina SABORES_OPERACION_PRUEBAS con conexión Windows cuyo catálogo empiece por Restaurante_Pruebas_Operacion_, SABORES_TOKEN_PRUEBAS y SABORES_SCRIPTS_PRUEBAS con la ruta absoluta de BaseDatos. Ejecute Verificador.exe con la carpeta de evidencias.

El test crea datos, aplica 009 y restaura un respaldo real en una copia. No reutilice la fixture para una ejecución completa. El modo `--visual carpeta` permite repetir la captura sobre una fixture ya ejecutada y crea datos de ensayo adicionales. El modo `--validar-reglas carpeta` comprueba referencias sintéticas sin modificar SQL.

Enlaces: [manual](../../Documentacion/ManualUsuario/OperacionIntegral009.md), [arquitectura y ejemplos](../../Documentacion/EstructuraCodigo/OperacionIntegral009.md), [migración/recuperación](../../BaseDatos/Operacion009.md).
