# Cambios de frontend y reglas de pedidos — 2 de noviembre de 2026

Entrega en `frontend-Bryan` para revisión del equipo. No se ha mezclado con `main`.

## Cómo explicar el trabajo

“Mejoramos la interfaz para que el operador vea lo que va a cobrar y pueda corregir errores antes de confirmar. Las mismas reglas se comprueban en el backend y dentro de transacciones SQL, por lo que no dependemos solamente de deshabilitar botones.”

## Cambios que puedes mostrar

| Antes | Ahora | Dónde demostrarlo |
|---|---|---|
| Agregar productos eliminaba el descuento sin una decisión explícita. | Un pedido con ajuste no admite cambios de productos hasta que un gerente retire el ajuste con motivo. Luego puede aplicarse un nuevo descuento. | Cuenta → Cierre / ajustes. |
| Todo el pedido quedaba bloqueado para correcciones al prepararlo. | Cada envío registra hasta qué línea se preparó. Los productos agregados después siguen pendientes y pueden corregirse con autorización. | Preparar nuevos → agregar otra bebida → Corregir. |
| Se podía preparar un pedido sin unidades vigentes. | Se rechaza la preparación vacía; se conserva la opción de cancelarlo sin consumo. | Retirar todas las unidades antes de preparar. |
| El total podía superar la capacidad de SQL. | Se valida el acumulado antes de insertar; la transacción no deja líneas parciales. | Prueba automatizada de importe máximo. |
| Cambiar de alias SQL cambiaba la identidad de recuperación. | La identidad se obtiene de SQL y permanece igual entre alias del mismo destino. Los registros nuevos se recuperan con el mismo identificador de operación. | Prueba automatizada de respuesta perdida. |
| No había vuelta sencilla al desactivar registros. | Se puede buscar y reactivar productos/clientes manteniendo el ID y el historial, con protección frente a duplicados. | Catálogos → Ver inactivos / Reactivar. |
| Cuenta con poco contexto y navegación cortada. | Cuenta con consumo, ajuste, total, estado de cada línea y cobro visible; navegación más compacta y ventana limitada al monitor. | Inicio y detalle de mesa. |
| El botón de cobro podía parecer listo sin efectivo suficiente. | Se habilita cuando hay importe suficiente y confirmaciones; tarjeta exige referencia cuando corresponde. | Revisar y cobrar. |
| Lista de pedidos extensa sin filtros. | Búsqueda, filtro de estado, fechas opcionales y páginas de 25 filas. | Pedidos. |
| Reportes con conceptos fáciles de confundir. | Pestañas “Comprobantes · neto” y “Platos · unidades y bruto”; resumen adaptable. | Reportes. |

## Reglas que debes poder defender

- **Importe de línea:** cantidad × precio unitario; aritmética decimal.
- **Total:** consumo original − descuento autorizado. El descuento es un importe fijo, no un porcentaje.
- **Efectivo:** recibido ≥ total; cambio = recibido − total.
- **Tarjeta:** registro manual por importe exacto. Una tarjeta incierta no emite comprobante y requiere conciliación.
- **Preparación:** las líneas originales y retiradas se conservan; las correcciones descuentan unidades vigentes, nunca borran el historial.
- **Reintentos:** el mismo identificador recupera la misma operación. No se debe crear un cobro nuevo para “intentar otra vez” una respuesta desconocida.
- **Permisos:** un mesero no puede reactivar productos ni autorizar descuentos por su cuenta; el backend vuelve a comprobarlo.
- **Mesa:** tras cobrar queda en limpieza; no vuelve directamente a disponible.

## Decisiones de implementación

La preparación por tandas utiliza la auditoría transaccional existente: la acción Preparacion registra el último ID de detalle incluido en el envío. Los pedidos preparados de versiones anteriores se tratan conservadoramente como ya preparados; antes de agregar nuevas líneas se registra el límite anterior. Los detalles se agregan con la mesa bloqueada en la misma transacción.

No se introduce una migración destructiva ni se modifica la base de trabajo para probar. Las funciones de corrección por tandas requieren esquema 7; la interfaz oculta esas acciones cuando no está disponible. Conviene que todos los integrantes usen la misma versión del programa para mantener las mismas reglas de operación.

La identidad de recuperación nueva combina servidor real, nombre de base y service_broker_guid, con prefijo DB2. Esto evita que cambiar entre conexión normal y memoria compartida oculte las solicitudes nuevas. Cambiar realmente de servidor/base o cambiar el identificador de SQL requiere revisión, no una asociación automática.

**Compatibilidad de registros anteriores:** los que coinciden con la dirección actual se migran con protección de archivo y cifrado. Un registro antiguo de un destino que no se puede verificar queda visible como “Destino por verificar”: no se repite ni se borra automáticamente. Esto puede exigir volver a la conexión anterior o revisar técnicamente ese archivo. No se supone que cualquier archivo de otro destino pertenezca a la base actual.

La paginación de pedidos es local: mejora el uso de la pantalla, pero la consulta todavía trae la lista completa. Para volúmenes grandes, la siguiente mejora sería paginación en SQL.

## Cómo probar sin tocar las ventas reales

El proyecto `Pruebas/Frontend/VerificarFrontend.csproj` crea una base **nueva y única** con prefijo `Restaurante_Pruebas_Frontend_`. Usa datos y cuentas ficticias, instala los scripts en esa base y conserva los resultados para inspección. No consulta ni modifica RestauranteDB. El servidor de prueba se configura en Programa.cs; en este equipo es `lpc:localhost\MSSQLSERVER01`.

Compilar el verificador y abrir Verificador.exe desde la sesión de Windows. Su carpeta Resultados contiene el informe y las capturas. Cada ejecución crea otra base de pruebas; no borra bases anteriores. No ejecutar este verificador en el servidor de producción de un restaurante real.

## Guion de demostración

1. Entrar y mostrar que Inicio y Cerrar sesión caben en pantalla.
2. Abrir una mesa y agregar dos productos. Explicar consumo y total.
3. Preparar los productos, agregar una bebida nueva y corregir esa bebida; mostrar que lo preparado no se puede retirar.
4. Aplicar un descuento. Mostrar que no se pierde al intentar cambiar productos; retirarlo y reaplicarlo requiere autorización.
5. Cobrar en efectivo: primero importe insuficiente, luego importe correcto y cambio.
6. Mostrar el comprobante, la mesa en limpieza y el reporte neto.
7. Buscar un pedido y reactivar un producto inactivo.

El comprobante es interno académico; no procesa pagos bancarios ni documentos tributarios reales. La estructura de POO continúa en ElementoMenu, Plato y Bebida, y las interfaces mantienen separadas presentación y reglas de negocio.


## Mejora visual final

Tarjetas de mesas con ilustraciones vectoriales locales, estado escrito y color, capacidad e importe. Catálogo del pedido con ilustraciones orientativas, precios y selección visible. Inicio con tarjetas de resumen adaptables. Las filas de texto de mesas ajustan su altura a la fuente para evitar recortes y se amplió el encabezado Cant. No se añaden dependencias de internet.

Validación visual posterior: compilación sin errores ni advertencias; capturas de inicio, mesas y productos en base aislada; altura de etiquetas comprobada con ventanas de 1100 y 1920 píxeles, y capturas a 1366 píxeles, con escalado del equipo. Verificados filtros, dibujos del catálogo y selección sin agregar consumo. No se modificaron ventas reales.

La rama parte de correcciones/auditoria-integral-20261028. Comparar primero contra esa rama permite distinguir esta entrega de los cambios anteriores del equipo que aún no están en main.
