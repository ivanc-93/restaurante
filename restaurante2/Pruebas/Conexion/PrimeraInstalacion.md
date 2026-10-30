# Primer uso sin usuario, contraseña ni token externo

La conexión del equipo y la instalación de la base son cosas distintas. Perder conexion.bin nunca reinicializa una base ni concede acceso a una cuenta existente.

## Equipo con una base vacía preparada

1. Abrir IniciarSabores.cmd. Se compila fuera de OneDrive, en una salida independiente de la depuración de Visual Studio.
2. En Preparar conexión del equipo, usar localhost, RestauranteDB, autenticación Windows y puerto 0 (automático). Probar y guardar la conexión validada.
3. En Crear primer administrador, pulsar Preparar instalación con Windows. Se exige autenticación Windows con permiso sysadmin real de SQL Server; el usuario de Sabores se crea después y no es la cuenta Windows.
4. Si existe una preparación anterior, el asistente avisa que su token quedará invalidado y pide confirmación. Cancelar no modifica ese token.
5. Antes de modificar el aprovisionamiento, el servicio hace BACKUP COPY_ONLY WITH CHECKSUM y RESTORE VERIFYONLY WITH CHECKSUM en la carpeta de respaldos del servidor SQL. Si no puede verificarlos, se detiene. VERIFYONLY verifica el conjunto; no equivale a ensayar una restauración completa.
6. El token se genera criptográficamente con 32 bytes aleatorios, SQL recibe solo SHA256 y el valor permanece únicamente en un objeto privado en memoria durante la ventana. No se muestra ni se entrega por archivos, argumentos, registros o variables de entorno. En .NET las cadenas no admiten borrado garantizado; Dispose elimina la referencia, sin prometer borrado físico inmediato de memoria.
7. Elegir nombre, usuario y contraseña de entre 12 y 128 caracteres. Confirmar personalmente Completar instalación de la base. La cuenta y el consumo del token se confirman en una transacción con bloqueo compartido con el aprovisionamiento.
8. Confirmar el destino cuando el asistente lo solicite e iniciar sesión. Si había una configuración protegida anterior, se conserva una copia cifrada antes de reemplazarla.

Cerrar antes de completar pierde la autorización en memoria; se debe preparar de nuevo, confirmando el reemplazo. Otro equipo que complete primero bloquea la creación duplicada. Una base con usuarios o instalación completada se rechaza, incluso aunque no haya configuración local. No se borra ni se migra automáticamente ninguna base.

## Instalación de una base inexistente por el responsable

Desde PowerShell 7, ejecutar BaseDatos/InstalarNuevaBase.ps1 -Servidor localhost -BaseDatos NombreDeBaseNueva -ConOperaciones. El token externo ya no es obligatorio: prepara explícitamente la base sin usuarios ni token. Después se usa el asistente autorizado anterior. El script rechaza bases existentes. -TokenInstalacion se conserva como opción para instalaciones donde el responsable entrega un token por otro canal.

## Verificaciones del 28 de octubre de 2026

- Compilación Release sin errores ni advertencias en una salida aislada.
- 14 comprobaciones del servicio en Restaurante_Pruebas_Inicial_20261028_120457_66c8b7: falta de aprovisionamiento, generación/hash, respaldo verificado, token perdido, confirmación denegada, reemplazo autorizado, token anterior/incorrecto, Windows sin sysadmin (contexto SQL restringido), concurrencia y rechazo de reinicialización.
- 8 comprobaciones del formulario en Restaurante_Pruebas_InicialUI_20261028_120601_3d7d43: botones reales, confirmaciones No/Sí inyectadas únicamente en pruebas, token oculto y vacío, validación/reintento, cuenta creada sin token manual y consumo en SQL. Captura sin credenciales inspeccionada visualmente.
- Instalador ejecutado sin -TokenInstalacion sobre Restaurante_Pruebas_SinToken_20261028_120843: cero usuarios, instalación pendiente y hash ausente; las 14 comprobaciones del servicio volvieron a pasar.
- La primera ejecución de la prueba de permisos usó una conexión con impersonación en el pool y afectó la sesión siguiente; se aisló con Pooling=false y las ejecuciones posteriores pasaron. La primera prueba UI detectó que el indicador de ocupación se limpiaba antes de habilitar los botones; se corrigió para permitir reintentos fiables.

Reproducir con dotnet run --project Pruebas/Conexion/VerificadorConexion.csproj -c Release -- --primera-instalacion NombreBaseDePruebas o --primera-interfaz NombreBaseDePruebas. Ambas requieren bases aisladas preparadas sin usuarios y rechazan nombres fuera de Restaurante_Pruebas_. La prueba UI requiere un token previo para comprobar su reemplazo; la del servicio modifica únicamente su fixture. Las bases y respaldos se conservan para inspección.

La base real localhost/RestauranteDB se verificó con cero usuarios, esquema 7, instalación pendiente y un token existente. El reemplazo de ese token queda pendiente de la confirmación explícita del usuario. No se crearon cuentas ni se modificó el aprovisionamiento real durante las pruebas.

Se ejecutó además un respaldo real Sabores_AntesPrimerUso_<identificador>.bak, COPY_ONLY/CHECKSUM, y RESTORE VERIFYONLY confirmó su validez. Se abrió la versión nueva desde %LOCALAPPDATA%/SaboresBuild/<identificador del checkout>/aplicacion. Al intentar preparar la ventana, Windows estaba bloqueado; no se enviaron más entradas a las aplicaciones. Quedan pendientes el desbloqueo y la confirmación de reemplazo del token anterior solicitada al usuario. Hasta entonces no se altera el aprovisionamiento ni se crea una cuenta real.

Actualización tras la autorización explícita del usuario y el desbloqueo: el asistente completó el reemplazo del token previo en localhost/RestauranteDB mediante Preparar instalación con Windows. Antes verificó el respaldo Sabores_Inicial_d98675bb31e94b1d9e8b8bd8a762ce0e.bak en la carpeta de respaldos de MSSQLSERVER. Se confirmó el mensaje de preparación correcta y se ocultó la entrada manual del token. La base sigue con cero usuarios e instalación pendiente. El formulario quedó abierto para que el usuario elija sus credenciales y confirme personalmente la creación de la cuenta; después debe confirmar guardar la conexión. El token solo permanece en memoria durante esa ventana y el anterior quedó invalidado.
