# Corrección del acceso y validación de contraseñas

Rama: `correcciones/acceso-validacion-20261026`. Punto de respaldo: `7ac587c`, referencia `respaldo/antes-acceso-validacion-20261026`. No se modifica ni migra RestauranteDB. No se publica ni mezcla con main.

## Causas

El acceso usaba coordenadas absolutas, escalado de fuente y Aplicar de FormatoPresentacion. Después de InitializeComponent, el constructor agregado insertaba el botón de conexión con otras coordenadas sin haber pasado por el mismo escalado. El botón invadía el campo de contraseña. Ahora todos los controles se crean en Designer y se distribuyen mediante TableLayoutPanel. Las acciones y mensajes permanecen separados de los campos, con ajuste de líneas y desplazamiento del contenido cuando sea necesario.

Contrasenas.Crear rechazaba correctamente contraseñas nuevas menores de 12 caracteres. Los formularios solo comprobaban confirmación y enviaban el valor al servicio antes de comprobar longitud. Los manejadores existentes capturaban Exception y mostraban un mensaje, por lo que una detención del depurador en throw no demuestra por sí sola un cierre no controlado. No se cambió la configuración de excepciones de Visual Studio.

## Resultado

- Acceso con marca, jerarquía clara, fondo neutro, Segoe UI y botones verde y secundario.
- Mostrar contraseña, Enter para entrar y orden de tabulación declarado.
- Usuario y contraseña vacíos se detectan antes de consultar SQL. No se exige la longitud de contraseña nueva a una contraseña de acceso antigua.
- Procesamiento deshabilita el envío y la configuración, y diferencia un problema SQL del acceso rechazado.
- Contrasenas.ErrorValidacion centraliza la política de 12 a 128 caracteres sin requisitos nuevos. El servicio conserva el rechazo defensivo.
- Creación inicial, creación/restablecimiento de usuarios y cambio voluntario/obligatorio validan antes de enviar. Los campos de confirmación deben coincidir.
- Ayuda permanente y ejemplo orientativo MiRestaurante2026!, sin guardarlo como contraseña predeterminada.
- Los errores enfocan el campo y permiten corregir y reintentar; los datos no sensibles permanecen. Las credenciales nunca aparecen en mensajes ni capturas.
- El mantenimiento de usuarios permite conservar su contraseña cuando ambos campos de nueva contraseña se dejan vacíos al editar.

## Verificación

`Pruebas/EvidenciasAcceso/verificacion.txt`: 23 comprobaciones de reglas y controles. Capturas reales en 1280 por 720 y 1366 por 768, con mensaje largo y comprobación de límites tanto del formulario como del contenedor para botones, mensaje y Mostrar contraseña. El tamaño de ventana no se presenta como cambio de escalado DPI.

`Pruebas/EvidenciasRoles/verificacion.txt`: 72 comprobaciones en bases nuevas aisladas, incluidas contraseña antigua corta, cambio obligatorio, reautenticación y creación de usuario desde controles reales. Las pruebas reutilizan la suite de roles y conservan sus bases para inspección. Debug y Release compilan; la restauración del proyecto de pruebas mantiene la advertencia NU1900 porque NuGet no pudo consultar su fuente de vulnerabilidades.

Para repetir la validación visual sin SQL, después de compilar el proyecto de pruebas:

```powershell
./restaurante2/Pruebas/Roles/bin/Debug/net8.0-windows/VerificadorRoles.exe --validar-acceso ./restaurante2/Pruebas/EvidenciasAcceso
```

Para verificar acceso real con datos ficticios: `./restaurante2/Pruebas/Roles/VerificarRoles.ps1`. Nunca usar RestauranteDB como destino de esas pruebas.

Límites: no se certifica la matriz nativa de Windows 100 125 150 y 200 por ciento, ni edición/guardado de todos los formularios en el diseñador. Tabulación y Enter se verifican mediante configuración/controles; no se afirma un recorrido exploratorio completo con teclado físico. La captura inicial no permite conocer la configuración exacta de detención de excepciones del Visual Studio del usuario.

## Uso

Abra Restaurante.Frontend y utilice su cuenta actual. Si debe cambiar contraseña, introduzca la anterior, una nueva de 12 a 128 caracteres y su confirmación. Ante un error, siga el mensaje y corrija el campo indicado. Use una contraseña personal diferente del ejemplo. Configurar o recuperar conexión sigue exigiendo al administrador del destino y no concede acceso operativo.
