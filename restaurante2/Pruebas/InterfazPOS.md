# Interfaz POS de Sabores — 26 de octubre de 2026

## Protección del código

- Rama de trabajo: `mejoras/interfaz-pos-20261026`.
- Rama previa intacta: `respaldo/antes-interfaz-pos-20261026`.
- Commit previo: `5deb777f1ccb3484ff5410f59490be5835cd94ce`.
- No se hizo push ni se modificó `main`.
- No se modificaron fuentes del backend, scripts SQL ni el esquema de la base habitual.
- Una rama Git conserva código; no constituye un respaldo de datos SQL.

## Cambios implementados

- Navegación Inicio, Mesas, Pedidos, Catálogos, Reportes y Configuración.
- Inicio con cuatro indicadores adaptables, saludo neutral, acceso a mesas y hasta cinco pedidos abiertos.
- Mesas con tarjetas, búsqueda y filtros. Dos consultas generales, sin una consulta por tarjeta.
- Seleccionar una mesa permite iniciar o continuar su pedido; su estado se vuelve a consultar antes de abrirla.
- Pantalla compartida de catálogo y pedido con búsqueda, categoría, tipo, cantidad y selección visual.
- El primer producto crea el pedido; abrir una mesa no inserta pedidos vacíos. Las incorporaciones posteriores se guardan inmediatamente.
- Cliente seleccionable antes del primer producto. No se añadieron modificaciones retroactivas de cliente, cantidades ni eliminación de líneas.
- Confirmación de factura, presentación del número y liberación de mesa conservadas.
- Bloqueo de acciones repetidas durante guardado; después de un error se consulta el estado real. Si la recuperación falla, la pantalla permanece bloqueada y solicita volver a abrir la mesa.
- Catálogos conserva selector de Clientes/Productos; la información del restaurante está en Configuración. Aviso antes de descartar ediciones.
- Fechas y agrupación para ventas; fechas para platos; fechas, mesa y cliente para pedidos. Los filtros que no aplican se ocultan según pestaña.
- Segoe UI, verde existente, estados con texto y color, imágenes locales Lucide y formato explícito es-GT (Q), sin convertir importes.
- Se conservan seis formularios parciales y sus Designer, interfaces y clases abstractas. No se añadieron proyectos ni paquetes de UI.

## Verificación

Compilaciones Debug y Release: cero errores y cero advertencias. Suite `Pruebas/Verificar.ps1 -ConSql`: 37 correctas y cero fallidas en la ejecución final registrada.

La suite incluye autenticación, reglas de importes, concurrencia, rollback, facturación única, cortesía, reportes, historial, actualización desde esquema anterior e idempotencia. Esas actualizaciones se ejecutan únicamente sobre bases nuevas de prueba.

Se adaptó el recorrido de sesión a la pantalla POS real: login, primer producto, segundo producto, factura, logout, segundo usuario y recuperación de factura desde otro proceso. También se comprueba que antes del primer producto no existe pedido, que búsqueda filtra, que los iconos están incrustados y que los filtros visibles de reportes corresponden a su pestaña.

Se comprobó acceso a facturación en ventanas de 1280 × 720 y 1366 × 768. Son tamaños de ventana, no cambios de resolución física del monitor. Se inspeccionaron capturas DrawToBitmap de controles reales, no maquetas.

El proceso informó DPI 168 (175 %) y dos monitores. No se cambió el escalado del sistema operativo. La matriz nativa de 100 %, 125 %, 150 % y 200 % queda pendiente; no se equipara redimensionamiento con esa verificación. También queda pendiente un recorrido manual completo con teclado, cambio entre monitores, pérdida de conexión durante el commit y el descarte de ediciones en todos los catálogos.

La edición en el diseñador de Visual Studio sigue pendiente. Se preparó `.codex-build/DesignerPos`, una copia temporal, y se intentó abrir una instancia aparte; no se obtuvo una ventana de la copia accesible por el controlador. La construcción y el render de seis formularios pasaron, pero no demuestran edición/guardado/reapertura en el IDE. No se alteró el proyecto ajeno que estaba abierto en Visual Studio.

Registro completo local: `.codex-build/pos-verificacion-final.txt`. Capturas y JSON entre procesos: `.codex-build/Verificador/Resultados`. Evidencias seleccionadas: [EvidenciasPOS](EvidenciasPOS).

## Recuperar el estado anterior sin descartar la mejora

Desde la raíz del repositorio, consultar primero:

```powershell
git status --short
git show --no-patch --oneline respaldo/antes-interfaz-pos-20261026
```

Para abrir una copia separada del estado previo, elegir una ruta nueva disponible:

```powershell
git worktree add --detach ../restaurante-respaldo-20261026 respaldo/antes-interfaz-pos-20261026
```

No se ejecutó este comando durante la implementación. No sobrescribe el checkout actual. Para volver en el mismo checkout, guardar primero cualquier cambio pendiente y usar `git switch respaldo/antes-interfaz-pos-20261026` únicamente con el árbol limpio. No usar `reset --hard`.

Recuperar el código no restaura SQL. Esta mejora no requiere retroceder migraciones; no borrar ni reemplazar la base habitual.

## Recursos

[Procedencia y licencia de iconos](../Restaurante.Frontend/Recursos/LICENCIAS.md). SVG originales y PNG de 24/32/48 px incluidos. Los iconos blancos se derivan en memoria de los mismos recursos para mantener contraste en botones verdes; no se requiere internet.
