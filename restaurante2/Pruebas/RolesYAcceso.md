# Instalación usuarios y autorizaciones

Sabores requiere esquema 5. Conserva Windows Forms, Microsoft SQL Server, los archivos Designer y la configuración protegida por perfil de Windows. No integra SAT, FEL ni servicios bancarios.

## Instalación nueva

El asistente no crea ni migra bases. El responsable prepara una base inexistente mediante PowerShell 7:

```powershell
$token = Read-Host 'Token de instalación de al menos 20 caracteres' -AsSecureString
./restaurante2/BaseDatos/InstalarNuevaBase.ps1 `
  -Servidor '(localdb)\MSSQLLocalDB' -BaseDatos RestauranteDB -TokenInstalacion $token
```

El script rechaza bases existentes y crea el esquema sin usuarios de demostración. Solo conserva el hash del token. El primer administrador introduce ese token y su propia contraseña en el asistente; la transacción consume el token y marca la instalación. Dos equipos no pueden crear dos administradores iniciales. Conservar el token por un canal seguro hasta completar la instalación; no incluirlo en Git.

Para actualizar una base existente, usar `BaseDatos/InstalarOActualizar.ps1`: respaldo COPY_ONLY con checksum, restauración en otra base, comparación de todas las tablas dbo y ensayo/repetición de la migración antes de actualizar el destino. Nunca reinicializa los usuarios de la base por ausencia del archivo local.

## Equipo y acceso diario

Sin configuración local se abre exclusivamente Preparar conexión del equipo. Probar conexión y esquema no guarda. Guardar conexión validada exige un administrador de la base elegida, o la creación inicial autorizada cuando la base está preparada y vacía. Una cuenta que debe cambiar contraseña vuelve a introducir la nueva antes de guardar. La última conexión válida se conserva si falla la prueba o el esquema.

En el acceso diario existe Configurar o recuperar conexión. Permite comprobar un destino accesible y autenticar allí a su administrador; no concede acceso operativo ni autentica usuarios contra un SQL inaccesible. La recuperación técnica de credenciales administrativas perdidas requiere al responsable de SQL. Eliminar conexion.bin no crea otro administrador.

LocalDB utiliza `(localdb)\MSSQLLocalDB`, sin puerto TCP. Usuario SQL vacío corresponde a Windows. La configuración DPAPI pertenece al perfil; no se comparte entre integrantes. Las solicitudes pendientes siguen vinculadas a su usuario y base original y no se trasladan al cambiar de destino.

## Roles

| Función | Mesero o Empleado | Gerente | Administrador |
|---|---|---|---|
| Pedidos abiertos compartidos cobros y limpieza | Sí | Sí | Sí |
| Comprobantes cerrados | Los emitidos por el propio usuario | Todos | Todos |
| Clientes de atención | Crear y editar activos | Mantener | Mantener |
| Productos precios mesas restaurante y reportes | No | Sí | Sí |
| Cancelar registrar incidencia corregir estado conciliar tarjeta | Con autorización puntual | Sí | Sí |
| Descuento y cortesía de consumo | Con autorización puntual | Sí | Sí |
| Usuarios roles auditoría de seguridad y conexión SQL | No | No | Sí |

Empleado conserva su valor histórico y se trata como Mesero. Jefe de meseros corresponde al rol Gerente. El gerente también vende sin cambiar de cuenta. El servicio valida usuario activo, versión de seguridad y permiso; ver un botón no concede acceso. Las lecturas sensibles también están protegidas. No se permite degradar ni desactivar al último administrador activo, incluso concurrentemente.

## Contraseñas

Las nuevas contraseñas usan PBKDF2 HMAC SHA256, 600000 iteraciones y sal aleatoria individual. Se requieren de 12 a 128 caracteres. Los hashes SHA256 anteriores se verifican y actualizan al autenticar si la contraseña cumple longitud; las claves antiguas cortas deben cambiarse. La cuenta inicial admin exige cambio obligatorio después de migrar. Las contraseñas temporales también exigen cambio y un nuevo inicio de sesión. Cinco intentos erróneos bloquean la cuenta durante cinco minutos. Los cambios de rol estado o contraseña invalidan sesiones anteriores.

La pantalla diaria ya no publica ni acepta un acceso administrativo fijo sin SQL. La credencial de demostración de instalaciones anteriores solo puede autenticar la cuenta real existente y obliga a cambiarla; no es un mecanismo de instalación.

## Autorización y ajustes

El mesero solicita una operación concreta con sus datos. El gerente introduce su propia cuenta en un diálogo; no se conserva su contraseña ni se eleva la sesión del solicitante. La aprobación tiene dos minutos de vigencia, queda ligada al hash de la solicitud completa y a la versión de seguridad del autorizador, y se consume dentro de la transacción de negocio. Cambios de cuenta importe motivo acción o autorizador requieren nueva revisión.

Los descuentos se introducen en quetzales, sin acumulación: un nuevo ajuste reemplaza el vigente y mantiene el historial. La cortesía representa el descuento del consumo completo, sin inventar dinero ni borrar productos. Original descuento y total final permanecen en el pedido y comprobante. Agregar productos invalida el ajuste y restaura el importe original; debe autorizarse nuevamente. Las tarjetas inciertas bloquean ajustes y nuevos cobros.

La cancelación conserva la declaración de ausencia de preparación y consumo. Si hubo consumo pendiente corresponde incidencia. La mesa pasa a limpieza; su disponibilidad no perdona saldos. No se implementan pagos parciales mixtos ni devoluciones o correcciones posteriores a un comprobante.

El resultado de una operación ya aplicada se recupera por el mismo identificador sin reutilizar una autorización para otra operación. El diario incorpora el tipo Ajuste y conserva los formatos anteriores.

## Pruebas reproducibles

```powershell
./restaurante2/Pruebas/Roles/VerificarRoles.ps1
./restaurante2/Pruebas/Verificar.ps1 -ConSql
```

El primer comando crea dos bases `Restaurante_Pruebas_*` con token aleatorio y sin modificar RestauranteDB. El segundo ejecuta la regresión en otras bases aisladas. Se conservan para diagnóstico, no se eliminan automáticamente.

Evidencia: `Pruebas/EvidenciasRoles/verificacion.txt`, capturas de formularios y recorridos reales, y `regresion.txt`. La ejecución final previa a actualizar la base habitual produjo 70 comprobaciones de roles correctas y 65 de regresión correctas. El render de constructores se distingue de los recorridos que crean usuario, cambian contraseña, autentican y autorizan un descuento mediante controles.

Límites: pruebas observadas en el entorno Windows y DPI del equipo, sin certificar todos los escalados ni despliegue remoto. NuGet no pudo consultar la fuente de vulnerabilidades durante la restauración del proyecto de pruebas y emitió NU1900; no se desactivó la auditoría. La solución principal compila sin errores. Las interrupciones de comunicación se verifican mediante inyección de fallos de la suite de cobros, no mediante caída física del servidor.

## Respaldo y recuperación

Punto previo de código: `27ca235`, rama `respaldo/antes-roles-20261026`. Trabajo: `mejoras/instalacion-roles-20261026`. No se publica ni se mezcla con main.

Antes de actualizar RestauranteDB se generó `RestauranteDB_20261026_225434_5892df.bak`. Se restauró realmente en `Restaurante_Pruebas_Restauracion_20261026_225434_5892df` y se compararon 14 tablas dbo; el ensayo y repetición de 005 pasaron antes de migrar el destino. Los archivos físicos y registros completos permanecen en `.codex-build/RespaldosRolesFinal`, fuera de Git.

Volver al código anterior no revierte el esquema ni recupera datos. Conservar la base actual y restaurar el respaldo compatible con otro nombre y archivos nuevos. No usar WITH REPLACE sobre la base habitual ni eliminar columnas para forzar código anterior. Revisar las operaciones posteriores al respaldo antes de una recuperación operativa.

La conexión directa desde un escritorio a SQL no protege frente al administrador del motor ni frente a un cliente modificado con privilegios directos equivalentes. Para producción con mayor aislamiento se necesita un servicio central y permisos SQL mínimos. Esta implementación conserva el alcance académico aprobado.
