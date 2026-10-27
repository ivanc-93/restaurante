# Comprobación del diseñador de Visual Studio

Estado: **pendiente de comprobación dentro del IDE**. No se equipara la construcción/render de controles con la edición en el diseñador.

Se inició Visual Studio 2026 y se preparó una copia temporal de la solución. El controlador de interfaz devolvió errores de selección de elementos (`element 134/244 is not available in cached app state for devenv.exe`) y no permitió confirmar de forma fiable el foco del diálogo de apertura. No se llegó a una vista de diseño verificada de la copia. No se guardaron cambios visuales en el proyecto original ni se afirma compatibilidad de edición comprobada.

| Formulario | Construcción y render | Constructor sin SQL | Abrir/seleccionar en IDE | Guardar y reabrir cambio en copia |
|---|---|---|---|---|
| FormularioAcceso | Probado | Inspeccionado; render sin repositorio | Pendiente | Pendiente |
| FormularioPrincipal | Probado | Inspeccionado; render sin repositorio | Pendiente | Pendiente |
| FormularioMesa | Probado | Inspeccionado; render sin repositorio | Pendiente | Pendiente |
| FormularioDetalleMesa | Probado | Inspeccionado; render sin repositorio | Pendiente | Pendiente |
| FormularioCatalogos | Probado | Inspeccionado; render sin repositorio | Pendiente | Pendiente |
| FormularioReportes | Probado | Inspeccionado; render sin repositorio | Pendiente | Pendiente |

## Procedimiento manual pendiente

1. Copiar la solución y fuentes a un directorio temporal. Restaurar y compilar la copia.
2. Abrir cada formulario con **Ver diseñador**.
3. Seleccionar sus controles; comprobar que no se intente conectar a SQL.
4. En la copia, modificar una ubicación o texto, guardar y cerrar el diseñador.
5. Reabrirlo y comprobar controles y eventos; compilar de nuevo.
6. Comparar .cs y .Designer.cs antes/después para detectar pérdidas o conexiones duplicadas.
7. Si aparece un error real, registrar el texto completo y corregir únicamente su causa en las fuentes originales.

No se generaron .resx vacíos: no hay recursos localizados ni binarios asociados a estos controles que los requieran actualmente.

## Revisión POS del 26/10/2026

La construcción y el render se volvieron a comprobar para los seis formularios. Se preparó la copia .codex-build/DesignerPos y se intentó abrir otra instancia de Visual Studio, sin obtener una ventana de esa copia accesible para el controlador. Abrir, editar, guardar y reabrir dentro del IDE continúa pendiente. Véase InterfazPOS.md para separar las comprobaciones realizadas de las pendientes.
