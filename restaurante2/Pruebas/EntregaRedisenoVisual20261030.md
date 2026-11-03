# Entrega del rediseño visual de Sabores

Fecha: 30 de octubre de 2026.

## Implementación

- Inicio: navegación oliva, tipografía ampliada, indicadores y pedidos obtenidos del repositorio; conserva Usuarios, Configuración, Cambio de contraseña, recuperación e incidencias según permisos.
- Mesas: tarjetas adaptables con texto, icono y color. Verde disponible, amarillo limpieza, rojo ocupada y morado reservada. Buscar, filtrar, abrir, continuar y confirmar limpieza usan operaciones existentes. Se vuelve a consultar el estado antes de actuar.
- Pedido y cobro: detalle y total legibles, panel de cobro manual y confirmaciones existentes. Conserva resultado de terminal, referencia, pagos inciertos, recuperación, descuentos y autorizaciones.
- Catálogos: búsqueda real, filas altas, edición agrupada y botones diferenciados. Desactivar conserva historial y los permisos del Mesero siguen vigentes.
- Iconos: recursos existentes y dibujos vectoriales originales complementarios, sin etiquetas incrustadas ni nuevas dependencias. Licencias en Restaurante.Frontend/Recursos/LICENCIAS.md.

Las maquetas se adaptaron a controles reales de Windows Forms: bordes y ventanas nativas, campos opcionales conservados y desplazamiento cuando no caben las tarjetas. No son imágenes colocadas sobre botones. No se modificaron el esquema ni los datos de la base real durante estas pruebas.

## Archivos principales

Restaurante.Frontend/Formularios: Componentes.cs, FormularioPrincipal.cs, FormularioPrincipal.Designer.cs, FormularioPrincipal.Pos.cs, FormularioCatalogos.cs y Designer, FormularioCobro.cs y Designer, FormularioDetalleMesa.Designer.cs, Iconos.cs. Nuevos: EstadoVisualMesa.cs, TarjetaMesa.cs e IconosComplementarios.cs.

Pruebas/Visual contiene un verificador de controles y repositorio con datos ficticios. Pruebas/Roles/PruebasInterfazRoles.cs usa la instancia de su fixture; PruebasRecuperacion.cs identifica el botón por su campo estable y comprueba su texto vigente. No se cambió la lógica de negocio del backend para este rediseño. Había modificaciones locales anteriores de conexión e instalación que se conservaron.

## Verificaciones ejecutadas

- Solución Release: compilación correcta, cero errores y cero advertencias, mediante `dotnet build Restaurante.sln -c Release --no-restore --artifacts-path <salida> --nologo -m:1`.
- Regresión: 72 comprobaciones correctas en bases aisladas, incluidas restricciones por rol y recuperación. El proyecto verificador presenta advertencias preexistentes sobre ApplicationConfiguration; no afectan la compilación de la solución.
- Verificador visual: 17 comprobaciones correctas. Navegación conservada, ocho mesas reales, filtros, apertura y continuación de pedidos, limpieza, búsqueda y edición de clientes, desactivación con historial, tarjeta incierta sin inventar pago confirmado, confirmaciones, cobro en efectivo con cambio y paso a limpieza, y restricciones del Mesero.
- Las operaciones y persistencia SQL fueron reales en bases de prueba. Los cuadros de confirmación se respondieron mediante un adaptador de diálogos del verificador, sin intervención humana; el funcionamiento manual completo no se presenta como probado.
- Capturas de controles reales: Inicio, Mesas, Catálogos y Cobro a 1366×768 y 1920×1080, DPI nativo 96 (100 %). Datos ficticios. No se verificaron escalas nativas de 125 % ni 150 %: quedan pendientes. Las capturas no sustituyen esa comprobación.

Evidencia y ejecutable, fuera de OneDrive:

`C:\Users\VeK\.codex\visualizations\2026\10\28\01a1833b-5f6a-77e3-afa1-e215117601d6\rediseno-20261030`

Archivos: verificacion.txt, regresion-roles.txt, capturas/*.png y bin/Restaurante.Frontend/release/Restaurante.Frontend.exe.

## Abrir y recuperar

Abrir el ejecutable indicado o IniciarSabores.cmd desde la raíz del proyecto para compilar y abrir la fuente vigente. El acceso requiere la cuenta personal existente de Sabores; no se crearon credenciales para la base real.

Respaldo previo a los cambios visuales:

`C:\Users\VeK\OneDrive - Universidad Mariano Gálvez\UNIVERSIDAD\Cursos Atrasados\04. Programación 2\Proyecto Final\restaurante2\restaurante\.codex-build\RespaldoVisual_20261030_152751`

Incluye manifest.json con hashes y estado-git-antes.txt. Para revertir únicamente este rediseño, cerrar Sabores, guardar cualquier trabajo posterior, restaurar desde ese respaldo los archivos de Frontend y pruebas modificados y retirar únicamente los archivos nuevos enumerados arriba y Pruebas/Visual. No usar un restablecimiento global de Git: eliminaría cambios anteriores ajenos a esta entrega. La restauración del código no requiere restaurar ni modificar la base real. No se creó ningún commit.
