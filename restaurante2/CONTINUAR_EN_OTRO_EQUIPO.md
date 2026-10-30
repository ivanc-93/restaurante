# Continuar Sabores en otro equipo

Usar la rama `correcciones/auditoria-integral-20261028` del repositorio `https://github.com/ivanc-93/restaurante`.

Si ya existe una copia, guardar primero cualquier cambio local propio. Después ejecutar desde el repositorio:

```powershell
git fetch origin
git switch correcciones/auditoria-integral-20261028
git pull --ff-only origin correcciones/auditoria-integral-20261028
```

Abrir la solución en la carpeta restaurante2 con Visual Studio y desarrollo de escritorio .NET 8, o ejecutar IniciarSabores.cmd. Directory.Build.props coloca la compilación en una ruta corta de cada equipo para evitar problemas de carga de SQL Client desde OneDrive.

La configuración SQL, las cuentas Windows, los tokens y las bases locales no viajan con Git. Seleccionar la instancia y la base que corresponden al otro equipo en el asistente de conexión. No reinicializar una base que ya tenga usuarios. Para trasladar los datos de este equipo hace falta un respaldo SQL y restaurarlo explícitamente en el destino.

Las capturas del rediseño se incluyen en Pruebas/EvidenciasVisuales. El resultado y los pendientes de verificación están en Pruebas/EntregaRedisenoVisual20261030.md; sus rutas de respaldo y ejecutable corresponden al equipo donde se realizó la implementación. Sigue pendiente verificar escalas nativas de Windows al 125 % y 150 %.
