# Conexión SQL: corrección y verificaciones

La captura reportó DllNotFoundException con error nativo 0x800700CE (nombre o ruta demasiado largos), no una confirmación de base inexistente. En este equipo las bibliotecas SNI estaban presentes: sus rutas anteriores alcanzaban 254 caracteres para x64/x86 y 256 para ARM64. La conexión sí funciona desde una ruta corta. No se ha reproducido exactamente el mismo error 0x800700CE dentro del depurador de Visual Studio.

Directory.Build.props establece la salida y los intermedios en %LOCALAPPDATA%/SaboresBuild/<identificador del checkout>, fuera de OneDrive. La configuración es común a Visual Studio y dotnet, conserva las bibliotecas nativas que copia NuGet y no fuerza x86/x64 ni activa una alternativa de red administrada. --artifacts-path permite elegir otra carpeta. Se excluyen bin/obj anteriores de la compilación. IniciarSabores.cmd usa IniciarSabores.ps1 para compilar y localizar la salida vigente, sin una ruta fija a una compilación anterior.

Después de cambiar estas propiedades, detener la depuración, cerrar y abrir la solución y recompilar antes de ejecutar con F5. No copiar únicamente el EXE: se necesitan las dependencias y la estructura runtimes. No se cambian las versiones de paquetes, la base ni la configuración protegida del usuario.

El formulario diferencia carga de componentes/arquitectura, servidor, espera agotada, permisos, base inaccesible y esquema. No afirma que una base inaccesible sea inexistente. La validación conserva las versiones de esquema admitidas por el repositorio y el mensaje de éxito ya no fija la versión 5. Los errores se capturan en el controlador async; el trabajo SQL continúa fuera del hilo de interfaz y finally restaura los botones y limpia contraseñas.

Los diagnósticos JSONL están en %LOCALAPPDATA%/SaboresRestaurante/Diagnosticos. Registran referencia, UTC, arquitectura, longitud de ruta, tipos de excepción, HResult, métodos y códigos/estado/clase SQL. Se omiten mensajes originales, datos de excepciones, consultas, rutas de archivos fuente, credenciales, tokens y cadenas de conexión para evitar filtraciones. Un fallo de registro se informa sin reemplazar el error original.

## Verificación ejecutada el 28 de octubre de 2026

`dotnet build Restaurante.sln --nologo`: sin errores ni advertencias.

`dotnet run --project Pruebas/Conexion/VerificadorConexion.csproj`: comprobaciones de conexión y mensajes. La primera ejecución produjo 22 comprobaciones correctas sobre LocalDB. Tras adjuntar la base a SQL principal, el ejecutor usa localhost/RestauranteDB e incorpora cinco verificaciones de puertos e instancias. Requiere la base previamente preparada; no instala ni migra bases. Usa una configuración cifrada temporal independiente y elimina solo su archivo temporal.

Casos reales mediante el botón del formulario: RestauranteDB válida; nombre de base inexistente aleatorio (SQL 4060); servidor TCP local en puerto 1; master con esquema incompatible. Se verificaron respuesta del hilo de interfaz, finalización, botones restaurados y conservación exacta de la configuración anterior. No modifica datos de RestauranteDB.

Casos simulados: excepción nativa envuelta, arquitectura incompatible, TimeoutException y UnauthorizedAccessException. Se comprobó además que una excepción con secretos ficticios no los registra. Estas simulaciones no certifican una instalación x86/ARM64 ni un rechazo real de credenciales SQL.

Se realizó también una prueba real de dependencia ausente en una copia aislada de la salida del verificador: se renombraron únicamente allí las bibliotecas SNI y se ejecutó `dotnet Verificador.dll --dependencia-ausente`. El error de carga recibió el mensaje de componente local. La instalación normal no se alteró.

## Pausa del depurador y límites

El controlador ya tenía catch(Exception) alrededor del await. Visual Studio puede interrumpir en el lanzamiento dentro de SqlClient/Task.Run, incluso antes de que await entregue la excepción al catch, según sus opciones de excepciones y Solo mi código. La captura por sí sola no prueba que la aplicación terminara sin controlar la excepción. No se cambiaron esas opciones: se corrigió la salida y se verificó el manejo fuera del depurador. Queda pendiente repetir F5 en la sesión de Visual Studio del usuario y comprobar allí su configuración exacta.

Las pruebas se ejecutaron en este Windows; no certifican otros equipos, arquitecturas ni todas las causas de red/TLS. La prueba del servidor incorrecto produjo el código SQL 258 (espera agotada), que se clasifica junto con -2. La carga ausente real y el error de ruta larga original son causas distintas dentro de la categoría de componentes locales.

Referencias consultadas: https://learn.microsoft.com/en-us/dotnet/core/sdk/artifacts-output y https://github.com/dotnet/SqlClient (distribución de SNI por arquitectura).
