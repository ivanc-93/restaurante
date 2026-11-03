# Verificación de la auditoría final

Fecha: 25 de octubre de 2026. Rama: `correcciones/auditoria-final`, creada desde `14bce2d`.

## Resultado

- **37 casos automatizados correctos y 0 fallidos** en la validación completa final.
- Compilaciones de la solución Debug y Release sin errores ni advertencias.
- SDK .NET 10.0.401, destino/ejecución Windows Forms .NET 8; SQL Server LocalDB 2025.
- Base nueva final: `Restaurante_Pruebas_20261025_154515_14db4a1f`.
- Base con esquema anterior actualizado: `Restaurante_Pruebas_20261025_154515_14db4a1f_Anterior`.
- No se alteraron la base real ni el archivo de configuración del usuario. Las bases de prueba se conservaron para inspección.

## Casos

Se mantienen los 29 casos anteriores: importes decimales, validaciones, fechas, configuración dañada, construcción/render de seis formularios, mesas, autenticación, Unicode, mantenimiento, pedidos, factura única, cortesías, rollback, concurrencia, reportes por periodos, historial al desactivar y pedidos abiertos anteriores a los últimos 100.

Se añadieron cinco regresiones de historial:

1. Plato facturado conserva su nombre/tipo e importe después de reclasificarlo, renombrarlo, cambiar su categoría/precio y desactivarlo.
2. Bebida vendida no se incorpora retroactivamente al reporte de platos cuando cambia el catálogo.
3. Distintas versiones de un mismo producto se capturan por agregado, se agrupan por identificador una sola vez y respetan fechas de factura y nombre histórico del intervalo.
4. Repetir dos veces la migración conserva capturas, cantidades e importes mediante comparación del contenido agregado.
5. Actualizar una fixture reproducible de versión 2 conserva el pedido y factura de 20.00 e inicializa los campos con los únicos valores disponibles. Una edición posterior del catálogo y nueva migración no los sobrescriben.

Se añadieron tres casos de aislamiento y sesiones:

6. El modo de pruebas rechaza una base con nombre real y no permite guardar configuración.
7. Un proceso entra mediante los controles del acceso, abre un pedido, agrega otro producto en el detalle y factura 14.00. Hace logout y vuelve a ingresar como un segundo usuario. Se usa el mismo CicloSesion del ejecutable.
8. Otro proceso independiente vuelve a ingresar y recupera la factura de ese pedido por 14.00 después de finalizar el primer proceso.

Las confirmaciones y notificaciones se sustituyen con IDialogosUsuario en el ejecutor automatizado. Los controles, eventos, consultas y ciclo de sesiones son los de la aplicación. En ejecución normal se mantiene MessageBox. No se afirma un ensayo humano completo del ejecutable de producción.

## Comprobaciones adicionales

- Instalación nueva y repetición de migraciones 002/003 correctas.
- Actualización de esquema 2 generada en una base separada, sin depender de Git ni de un servidor real.
- Prueba SQL negativa de la 003 sobre una identidad histórica vacía: error esperado **51103**, contenido conservado e integridad de la restricción restaurada por rollback. Se realizó sobre una base aislada anterior del mismo trabajo (`Restaurante_Pruebas_20261025_154017_8b0aff1e`).
- Verificación de la acción de pedido y de los límites de su editor en tamaño mínimo; captura a **144 DPI (150%)**, un monitor disponible. No se cambiaron las opciones de escala de Windows.
- Inspección de la captura: campos, flechas de selección y acción de pedido accesibles sin invadir la columna del menú. Los listados largos disponen de desplazamiento.
- Diagramas Mermaid y vistas PNG actualizados. La ejecución de regeneración también confirmó los 11 casos que no requieren SQL; son un subconjunto de los 37, no pruebas adicionales independientes.

## Defectos detectados y corregidos durante las comprobaciones

- H1: los reportes utilizaban nombre/tipo actuales del catálogo en vez de capturas por detalle.
- Botones del detalle activos antes de finalizar la carga inicial: ahora comienzan deshabilitados.
- Navegación inicial susceptible de coincidir con una carga pendiente del resumen: permanece bloqueada hasta completarla.
- Editor de pedido conservaba un ancho que invadía la columna del menú al reducir la ventana: se ajustó el auto-tamaño y su columna porcentual.
- La prueba anterior de páginas no esperaba explícitamente el evento Shown: se sincronizó con esa señal y la finalización de carga, conservando sus comprobaciones.

## Evidencia y reproducción

```powershell
./restaurante2/Pruebas/Verificar.ps1 -ConSql
dotnet build restaurante2/Restaurante.sln --configuration Debug --no-restore
dotnet build restaurante2/Restaurante.sln --configuration Release --no-restore
```

El proyecto ejecutor se genera fuera de la solución en `.codex-build/Verificador`. La solución versionada conserva dos proyectos. Las capturas y los JSON de procesos quedan en `.codex-build/Verificador/Resultados`; no contienen contraseñas.

`sesion-proceso-1.json` y `sesion-proceso-2.json` registran PID distintos, pedido, número de factura, total y logins. Las ejecuciones puntuales previas usadas para comprobar tamaños dejaron también `sesion-final-1.json` en una base aislada anterior. La ejecución completa final produjo PID 32120 y 31752, con el pedido 113 y factura F-00000009 por 14.00.

## Límites y pendientes

- Designer en Visual Studio: no se completó la apertura/edición/reapertura por los errores del controlador de interfaz. Véase `DesignerPendientes.md` para los seis formularios y el procedimiento manual.
- No se verificaron todos los monitores, resoluciones o escalas posibles; se comprobó el entorno disponible y dos tamaños de ventana.
- No se simuló pérdida de energía ni desconexión exactamente durante el commit SQL.
- No se aplicaron las migraciones a una base real ni se validó el equipo de presentación final.
- La identidad anterior a la migración solo refleja el catálogo disponible al actualizar; no reconstruye cambios históricos desconocidos.
- Investigación, informe académico, video, integrantes, permisos de enlaces y defensa siguen dependiendo del grupo; véase `Requerimientos.md`.

Por estos pendientes, no se declara que el proyecto esté completamente listo para entregar.


## Commits funcionales y recuperación

- `1d727a5`: identidad histórica, instalación/migración 003 y regresiones de historial.
- `adc6a33`: modo aislado, ciclo de sesiones, diálogos sustituibles, pruebas entre procesos y ajustes de carga/tamaño.

La documentación y diagramas se confirman por separado. No se hizo push ni fusión con main.

Para volver al código previo a esta auditoría, con árbol limpio, cambie a `correcciones/revision-restaurante` (14bce2d). Para deshacer commits conservando historial, use `git revert <hash>` en orden inverso, revisando dependencias. No ejecute el código anterior contra un esquema actualizado sin planificar la recuperación de BD: restaure el respaldo previo en una base separada y concilie los datos registrados después. No hay una reversión automática que borre columnas históricas.
