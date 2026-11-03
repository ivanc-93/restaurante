# Verificación de cuentas identificadas 010
Solo admite bases cuyo nombre comienza Restaurante_Pruebas_Cuentas_, autenticación integrada y sin AttachDBFilename. Nunca ejecutar contra RestauranteDB.

1. Crear una fixture NUEVA con InstalarNuevaBase.ps1 -ConCobranzaParcial y token ficticio de instalación.
2. Definir SABORES_CUENTAS_PRUEBAS, SABORES_TOKEN_PRUEBAS y SABORES_SCRIPTS_PRUEBAS (directorio BaseDatos).
3. Compilar VerificadorCuentas.csproj y ejecutar el Verificador.exe construido, pasando un directorio externo de evidencias.
4. La prueba crea administrador/usuarios ficticios, preserva datos 009, respalda/restaura realmente, ensaya 010 y luego migra solamente su fixture.
5. El argumento --visual <directorio> solo repite la parte visual en una fixture ya preparada, agregando mesas ficticias. --recuperar <GUID> es auxiliar del proceso hijo.

No reutilizar una fixture inicializada para repetir la suite completa. No borrar bases de diagnóstico ni diarios a mano para hacer pasar pruebas. Los errores de ensayos previos quedan conservados fuera del repositorio. El único archivo ilegible creado por esta prueba se mueve a EvidenciasPrueba del directorio temporal tras comprobar su conservación.

Escenarios: compatibilidad 009, no conversión de pagos/divisiones, nombres antes del consumo, primer producto/reserva atómicos, conservación de unidades/precios, pago por cuenta, traslado/corrección separados, autorización vinculada al alcance, preparación, beneficios/restablecimiento/cortesía, tarjetas inciertas independientes, cierre final único, nombre provisional, retiro lógico, concurrencia obsoleta y aplicar/abandonar, rollback por trigger, desactivación de sesión, lecturas concurrentes, pérdida de respuesta en otro proceso, efectivo insuficiente y diario ilegible. Controles reales crean cuentas, asignan productos, muestran resumen y validan cobro.
