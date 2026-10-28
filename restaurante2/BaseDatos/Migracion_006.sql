-- Recuperación: aplicar manualmente después de respaldo y restauración comprobada.
SET XACT_ABORT ON;
SET NOCOUNT ON;
IF DB_NAME() IN ('master','model','msdb','tempdb') THROW 51400,N'Seleccione la base de Sabores.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.VersionEsquema WHERE Version=5) THROW 51401,N'Requiere esquema 5.',1;
BEGIN TRANSACTION;
DECLARE @r int;
EXEC @r=sys.sp_getapplock @Resource=N'SaboresInstalacion',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=15000;
IF @r<0 THROW 51402,N'Seguridad ocupada.',1;
IF OBJECT_ID(N'dbo.RecuperacionInstalacion',N'U') IS NULL
BEGIN
 CREATE TABLE dbo.RecuperacionInstalacion(Id int NOT NULL PRIMARY KEY CHECK(Id=1),InstalacionId uniqueidentifier NOT NULL);
 INSERT dbo.RecuperacionInstalacion VALUES(1,NEWID());
 CREATE TABLE dbo.CodigosRecuperacion(UsuarioId int NOT NULL PRIMARY KEY REFERENCES dbo.Usuarios(Id),
  Verificador binary(32) NOT NULL,VersionSeguridad int NOT NULL,Consumido bit NOT NULL DEFAULT 0,Confirmado bit NOT NULL DEFAULT 0,
  Intentos int NOT NULL DEFAULT 0,PausaHasta datetime2 NULL,Fecha datetime2 NOT NULL DEFAULT SYSUTCDATETIME());
 CREATE TABLE dbo.OperacionesRecuperacion(Id uniqueidentifier NOT NULL PRIMARY KEY,UsuarioId int NOT NULL REFERENCES dbo.Usuarios(Id),
  Prueba binary(32) NOT NULL,Fecha datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),Mecanismo nvarchar(30) NOT NULL);
 INSERT dbo.VersionEsquema(Version) VALUES(6);
END;
COMMIT;
