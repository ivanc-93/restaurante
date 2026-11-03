-- Ejecutar solo con respaldo restaurado y comprobado. No cambia de base.
SET XACT_ABORT ON;
SET NOCOUNT ON;
SET LOCK_TIMEOUT 15000;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
IF DB_NAME() IN ('master','model','msdb','tempdb') THROW 51300,N'Seleccione la base del restaurante.',1;
BEGIN TRY
 BEGIN TRANSACTION;
 DECLARE @lock int;
 EXEC @lock=sys.sp_getapplock @Resource=N'SaboresInstalacion',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=15000;
 IF @lock<0 THROW 51301,N'No se pudo bloquear la instalación.',1;
 IF NOT EXISTS(SELECT 1 FROM dbo.VersionEsquema WHERE Version=4) THROW 51302,N'Ejecute primero la migración 004.',1;
 IF NOT EXISTS(SELECT 1 FROM dbo.VersionEsquema WHERE Version=5)
 BEGIN
  IF EXISTS(SELECT 1 FROM dbo.Usuarios WHERE Rol NOT IN(N'Administrador',N'Gerente',N'Empleado',N'Mesero')) THROW 51303,N'Existen roles desconocidos. Revisar antes de migrar.',1;
  ALTER TABLE dbo.Usuarios ALTER COLUMN ContrasenaHash nvarchar(256) NOT NULL;
  ALTER TABLE dbo.Usuarios ADD Algoritmo nvarchar(30) NOT NULL CONSTRAINT DF_Usuarios_Algoritmo DEFAULT N'SHA256',
   Iteraciones int NOT NULL CONSTRAINT DF_Usuarios_Iteraciones DEFAULT 0,
   CambioObligatorio bit NOT NULL CONSTRAINT DF_Usuarios_Cambio DEFAULT 0,
   IntentosFallidos int NOT NULL CONSTRAINT DF_Usuarios_Intentos DEFAULT 0,
   BloqueadoHasta datetime2 NULL,
   VersionSeguridad int NOT NULL CONSTRAINT DF_Usuarios_Version DEFAULT 1;
  EXEC(N'UPDATE dbo.Usuarios SET CambioObligatorio=1 WHERE Usuario=N''admin'';');
  ALTER TABLE dbo.Usuarios ADD CONSTRAINT CK_Usuarios_Rol CHECK(Rol IN(N'Administrador',N'Gerente',N'Empleado',N'Mesero'));
  CREATE TABLE dbo.InstalacionSabores(Id int NOT NULL PRIMARY KEY CHECK(Id=1),Completada bit NOT NULL,
   Fecha datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),TokenInstalacionHash char(64) NULL);
  INSERT dbo.InstalacionSabores(Id,Completada) VALUES(1,CASE WHEN EXISTS(SELECT 1 FROM dbo.Usuarios) THEN 1 ELSE 0 END);
  CREATE TABLE dbo.AuditoriaSeguridad(Id bigint IDENTITY PRIMARY KEY,Fecha datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
   UsuarioId int NULL REFERENCES dbo.Usuarios(Id),DestinoId int NULL REFERENCES dbo.Usuarios(Id),Accion nvarchar(80) NOT NULL,Detalle nvarchar(500) NOT NULL);
  CREATE TABLE dbo.AutorizacionesOperacion(OperacionId uniqueidentifier NOT NULL PRIMARY KEY,
   Tipo nvarchar(40) NOT NULL,SolicitudHash char(64) NOT NULL,SolicitanteId int NOT NULL REFERENCES dbo.Usuarios(Id),
   AutorizadorId int NOT NULL REFERENCES dbo.Usuarios(Id),VersionAutorizador int NOT NULL,
   Caduca datetime2 NOT NULL,Consumida bit NOT NULL DEFAULT 0,Fecha datetime2 NOT NULL DEFAULT SYSUTCDATETIME());
  ALTER TABLE dbo.Pedidos ADD ImporteOriginal decimal(12,2) NULL,Descuento decimal(12,2) NOT NULL CONSTRAINT DF_Pedidos_Descuento DEFAULT 0;
  EXEC(N'UPDATE dbo.Pedidos SET ImporteOriginal=Total;');
  ALTER TABLE dbo.Facturas ADD ImporteOriginal decimal(12,2) NULL,Descuento decimal(12,2) NOT NULL CONSTRAINT DF_Facturas_Descuento DEFAULT 0;
  EXEC(N'UPDATE dbo.Facturas SET ImporteOriginal=Total;');
  CREATE TABLE dbo.AjustesPedido(Id bigint IDENTITY PRIMARY KEY,PedidoId int NOT NULL REFERENCES dbo.Pedidos(Id),
   OperacionId uniqueidentifier NOT NULL UNIQUE REFERENCES dbo.OperacionesRestaurante(Id),
   ImporteOriginal decimal(12,2) NOT NULL,Descuento decimal(12,2) NOT NULL,TotalFinal decimal(12,2) NOT NULL,
   Tipo nvarchar(20) NOT NULL CHECK(Tipo IN(N'Descuento',N'Cortesía')),Motivo nvarchar(500) NOT NULL,
   UsuarioId int NOT NULL REFERENCES dbo.Usuarios(Id),AutorizadorId int NOT NULL REFERENCES dbo.Usuarios(Id),
   Vigente bit NOT NULL DEFAULT 1,Fecha datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
   CHECK(ImporteOriginal>=0 AND Descuento>=0 AND Descuento<=ImporteOriginal AND TotalFinal=ImporteOriginal-Descuento));
  CREATE UNIQUE INDEX UX_AjustesPedido_Vigente ON dbo.AjustesPedido(PedidoId) WHERE Vigente=1;
  ALTER TABLE dbo.AuditoriaOperaciones ADD AutorizadorId int NULL REFERENCES dbo.Usuarios(Id);
  INSERT dbo.VersionEsquema(Version) VALUES(5);
 END
 COMMIT;
END TRY
BEGIN CATCH
 IF @@TRANCOUNT>0 ROLLBACK;
 THROW;
END CATCH;
