# Sabores: recuperar acceso

Esta guía complementa el manual de usuario. La función requiere la actualización 006, que se activa mediante mantenimiento respaldado. No envía correo ni SMS y nunca muestra una contraseña anterior.

## Si olvidaste la contraseña

En **Iniciar sesión**, selecciona **¿Olvidaste tu contraseña?**. Elige un método:

- **Con ayuda de otro Administrador:** escribe tu usuario. Otro Administrador comprueba tu identidad e introduce su cuenta y contraseña vigente. Introduce y confirma tu contraseña nueva. El autorizador no inicia sesión en tu lugar ni se conserva su contraseña. Gerente y Mesero no pueden autorizar este restablecimiento.
- **Con mi código de emergencia:** disponible para Administradores que guardaron previamente su código. Escribe tu usuario, pega el código completo de 64 caracteres y establece una contraseña nueva. El código se consume una sola vez.
- **No tengo administrador ni código:** la pantalla explica cómo solicitar mantenimiento autorizado. No existe una contraseña maestra ni se reinicia la instalación.

La contraseña nueva debe tener entre 12 y 128 caracteres y no puede ser la anterior. Usa una frase larga y personal. La política de contraseña nueva no se aplica a la contraseña vigente del Administrador que verifica tu identidad.

Si se rechaza la verificación, revisa el método, usuario y contraseña o código. El mensaje público no confirma si existe una cuenta. Cinco fallos de código pausan su comprobación durante cinco minutos; no bloquean por sí solos el acceso normal. Los intentos de autenticación del Administrador conservan su bloqueo independiente.

Cancelar antes de enviar no cambia credenciales. Después de completar la recuperación, vuelve al acceso e inicia sesión con tu contraseña nueva. Las sesiones y aprobaciones pendientes anteriores dejan de ser válidas. No cambian tus roles, estado activo ni información comercial.

## Guardar el código de emergencia

En una instalación actualizada, después del primer inicio de sesión del Administrador se solicita crear el código antes de abrir la operación diaria. También se solicita tras recuperar o cambiar credenciales si el código anterior dejó de estar vigente.

1. Confirma tu contraseña vigente.
2. Pulsa **Generar código**. El anterior queda revocado.
3. Marca **Mostrar código de emergencia** si necesitas leerlo. Copia manualmente el valor a un gestor de contraseñas o papel protegido fuera del equipo. No lo adjuntes a capturas, chats ni repositorios.
4. Marca **He guardado el código en un lugar seguro fuera del equipo** y confirma. Esta confirmación debe realizarse durante los cinco minutos siguientes a su generación.

El código solo se muestra en esa ventana. Si la cierras sin guardarlo o se pierde la respuesta de generación, genera un reemplazo; no podrás consultar el anterior. Desde **Usuarios y roles → Mi código de emergencia** puedes rotar el código de tu propia cuenta; no se muestran los de otros Administradores.

Quien tenga el código podría recuperar tu cuenta. Guardarlo en la misma computadora reduce la utilidad y protección del mecanismo.

## Si SQL se desconecta o no sabes si terminó

Usa **Comprobar resultado** antes de repetir. La aplicación guarda localmente un comprobante protegido con Windows DPAPI que contiene identificador y prueba de consulta, sin contraseñas ni código de emergencia. Si cierras y vuelves a abrir la recuperación con el mismo perfil Windows y destino SQL, podrás consultar ese resultado.

Si SQL confirma que terminó, prueba el acceso con la contraseña nueva. Si no hay resultado registrado, puedes reintentar con el identificador original y los datos de recuperación. No elimines el comprobante ni cambies de equipo para forzar otra operación mientras exista incertidumbre. Si el comprobante está dañado, solicita asistencia.

Este comprobante es distinto de la recuperación de pedidos o cobros pendientes. La pantalla anterior de operaciones continúa disponible desde el sistema.

## Único Administrador sin contraseña ni código

Solicita al responsable autorizado del restaurante y de SQL Server una intervención de mantenimiento. La aplicación pública no permite eludir la verificación.

El técnico debe detener la operación diaria, confirmar servidor/base/cuenta, comprobar su autoridad, respaldar y restaurar una copia aislada. La herramienta separada exige permisos SQL de administrador del servidor y una confirmación explícita del destino. Solo cambia la contraseña de un Administrador activo seleccionado; no crea ni reactiva cuentas.

Recibirás una contraseña temporal: deberás cambiarla al ingresar y después guardar tu nuevo código de emergencia. Las credenciales SQL y las cuentas de Sabores son independientes.

## Para el responsable técnico

Compila `Restaurante.sln` y mantén `Restaurante.Frontend` como proyecto de inicio. `MantenimientoRecuperacion` es una herramienta de consola separada, sin acceso desde la pantalla pública. Su ejecución es interactiva: no acepta contraseñas en argumentos ni entrada redirigida.

Para una base existente, desde la carpeta compilada de la herramienta:

```powershell
./MantenimientoRecuperacion.exe --actualizar '(localdb)\MSSQLLocalDB' 'RestauranteDB'
```

El programa muestra el destino y exige escribir `ACTUALIZAR RestauranteDB`. Primero hace un respaldo COPY_ONLY con checksum, verifica el medio, restaura con nombres nuevos sin sobrescribir archivos y ejecuta CHECKDB. Solo si lo anterior funciona aplica `Migracion_006.sql`. No ejecutarlo durante ventas ni sin autorización sobre ese destino. La herramienta usa autenticación Windows y permisos SQL sysadmin; requiere acceso de SQL Server a su carpeta de datos y espacio para respaldo y restauración.

Para el restablecimiento excepcional, después de actualizar la base:

```powershell
./MantenimientoRecuperacion.exe '(localdb)\MSSQLLocalDB' 'RestauranteDB'
```

Introduce la cuenta objetivo, responsable y motivo (sin secretos). Confirma el texto exacto que muestra la herramienta y escribe dos veces una contraseña temporal oculta. Guarda el identificador que se imprime **antes** de ejecutar. Se vuelve a verificar un respaldo restaurado antes del cambio. Si se pierde la respuesta, consulta en SQL la auditoría y `OperacionesRecuperacion` por ese identificador antes de repetir; no se ofrece una consulta pública de intervenciones.

Los respaldos y copias aisladas se conservan. Contienen datos sensibles y hashes: deben permanecer bajo custodia del responsable. Las copias restauradas por la herramienta invalidan códigos de recuperación y cambian la identidad de instalación para no permitir reutilizarlos allí. El mantenimiento no protege contra quien ya tiene control administrativo completo del motor SQL.

Para bases nuevas puede usarse `InstalarNuevaBase.ps1 -ConRecuperacion` junto con los parámetros habituales. No usar el instalador para una base existente.

## Volver al código anterior

Rama de trabajo: `mejoras/recuperacion-contrasenas-20261027`.

Respaldo previo: `respaldo/antes-recuperacion-20261027`, commit `ca2ca77`. Conserva cualquier trabajo posterior antes de cambiar de rama y recompila.

La base habitual no fue migrada durante esta implementación. Si se activa posteriormente el esquema 6, el código anterior que solo acepta esquema 5 requiere un procedimiento de retorno coordinado. No basta cambiar de rama: restaura el respaldo preactualización únicamente después de evaluar los datos nuevos y con operación detenida. No borres la fila de versión para simular compatibilidad.
