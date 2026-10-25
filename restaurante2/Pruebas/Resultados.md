# Verificación de las correcciones

Fecha: 25 de octubre de 2026. Rama: `correcciones/revision-restaurante`.

## Entorno y alcance

- Windows, SDK .NET 10.0.401, ejecución .NET/WindowsDesktop 8.0.32.
- Compilaciones Debug y Release: 0 errores y 0 advertencias.
- SQL Server LocalDB 2025, instancia `(localdb)\MSSQLLocalDB`.
- Última base aislada: `Restaurante_Pruebas_20261025_150505_0c518335`.
- Base real y configuración de conexión del usuario: no modificadas.
- Instalación nueva y segunda ejecución de la migración: correctas.
- Resultado final: **29 casos correctos; 0 fallidos**.

## Casos ejecutados

1. Cálculo decimal mediante objetos derivados y límites de cantidad.
2. Validaciones de clientes, correo, productos, precios y tipo.
3. Fechas inclusivas y semana de lunes a domingo.
4. Recuperación de configuración dañada en un archivo de prueba.
5–10. Construcción y render de los seis formularios con sus Designer.
11. Datos editables de mesa y botones dentro del área disponible.
12. Login válido, inválido y usuario inactivo.
13. Conservación de caracteres Unicode en la instalación SQL.
14. Persistencia de nombre y ubicación del restaurante mediante otra instancia del repositorio.
15. Alta, edición y duplicados de clientes y productos.
16. Alta, edición, eliminación y validaciones de mesas.
17. Pedido, agregado, total exacto, factura única y rechazo de cambios tras facturar.
18. Facturación de una cortesía de total cero y liberación de mesa.
19. Rollback al solicitar un producto inexistente, sin dejar mesa ocupada ni pedido parcial.
20. Dos aperturas simultáneas: una aceptada.
21. Dos facturaciones simultáneas: una aceptada.
22. Ventas y más vendidos reconciliados, exclusión de abiertos/cancelados y filtros.
23. Agrupación semanal entre años y agrupación mensual separada.
24. Apertura y cambio de estado simultáneos sin liberar la mesa con pedido.
25. Error de conexión controlable ante servidor no disponible.
26. Carga de páginas operativas y cierre de sesión desde la interfaz.
27. Carga de catálogos, datos del restaurante y reportes en formularios reales.
28. Desactivación sin perder historial ni clientes en filtros históricos.
29. Visibilidad de un pedido abierto después de más de 100 pedidos posteriores.

## Revisión visual y límites

Se generaron imágenes mediante DrawToBitmap con controles inicializados y se inspeccionaron las pantallas de mesas, pedidos, productos y reportes. La revisión detectó y permitió corregir la superposición de la tabla de pedidos sobre el editor y el recorte de acciones y etiquetas.

No se abrió el diseñador dentro de Visual Studio. Los formularios tienen constructores sin argumentos, campos de controles, InitializeComponent, clases partial y Dispose; la apertura y edición visual dentro de Visual Studio sigue siendo una comprobación manual. También quedan por revisar distintos monitores/escalas DPI y el recorrido humano completo tras instalar en el servidor final.

La persistencia se verificó con conexiones e instancias nuevas del repositorio, no mediante un reinicio manual completo del ejecutable. No se simularon caída de energía ni pérdida de red exactamente durante el commit SQL.

Las bases aisladas creadas durante las pruebas se conservaron para inspección. No se aplicó la migración a una base real. Los resultados no sustituyen el respaldo y validación necesarios antes de esa operación.

## Reproducir

Desde la raíz del repositorio:

```powershell
./restaurante2/Pruebas/Verificar.ps1 -ConSql
```

Para regenerar además las vistas PNG de los diagramas:

```powershell
./restaurante2/Pruebas/Verificar.ps1 -ActualizarDiagramas
```

El ejecutor temporal y las capturas quedan en `.codex-build/Verificador/Resultados`, excluidos de Git.
