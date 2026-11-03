-- Reservas inmediatas. No altera mesas ni convierte reservas anteriores sin titular.
SET XACT_ABORT ON;
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
IF DB_NAME() IN ('master','model','msdb','tempdb') THROW 51600,N'Seleccione la base de Sabores.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.VersionEsquema WHERE Version=7) THROW 51601,N'Requiere esquema 007 completo.',1;
BEGIN TRANSACTION;
DECLARE @bloqueoReservas int;
EXEC @bloqueoReservas=sys.sp_getapplock @Resource=N'SaboresInstalacion',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=15000;
IF @bloqueoReservas<0 THROW 51602,N'Instalación ocupada.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.VersionEsquema WHERE Version=8)
BEGIN
 IF OBJECT_ID(N'dbo.ReservasMesa',N'U') IS NOT NULL THROW 51603,N'Existe una estructura de reservas sin versión registrada. Solicite revisión.',1;
 CREATE TABLE dbo.ReservasMesa(
  Id int IDENTITY PRIMARY KEY,
  MesaId int NOT NULL REFERENCES dbo.Mesas(Id),
  MesaHistorica int NOT NULL,
  Nombre nvarchar(120) NOT NULL CHECK(LEN(LTRIM(RTRIM(Nombre)))>0),
  Estado nvarchar(15) NOT NULL CHECK(Estado IN(N'Activa',N'Atendida',N'Cancelada')),
  Fecha datetime2 NOT NULL DEFAULT DATEADD(hour,-6,SYSUTCDATETIME()),
  UsuarioId int NOT NULL REFERENCES dbo.Usuarios(Id),
  OperacionId uniqueidentifier NOT NULL UNIQUE REFERENCES dbo.OperacionesRestaurante(Id),
  PedidoId int NULL REFERENCES dbo.Pedidos(Id),
  OperacionCierre uniqueidentifier NULL REFERENCES dbo.OperacionesRestaurante(Id),
  UsuarioCierre int NULL REFERENCES dbo.Usuarios(Id),
  FechaCierre datetime2 NULL,
  MotivoCierre nvarchar(500) NULL,
  Version rowversion NOT NULL,
  CONSTRAINT CK_Reserva_Desenlace CHECK(
    (Estado=N'Activa' AND PedidoId IS NULL AND OperacionCierre IS NULL AND UsuarioCierre IS NULL AND FechaCierre IS NULL AND MotivoCierre IS NULL)
    OR (Estado=N'Atendida' AND PedidoId IS NOT NULL AND OperacionCierre IS NOT NULL AND UsuarioCierre IS NOT NULL AND FechaCierre IS NOT NULL AND MotivoCierre IS NOT NULL AND LEN(LTRIM(RTRIM(MotivoCierre)))>0)
    OR (Estado=N'Cancelada' AND PedidoId IS NULL AND OperacionCierre IS NOT NULL AND UsuarioCierre IS NOT NULL AND FechaCierre IS NOT NULL AND MotivoCierre IS NOT NULL AND LEN(LTRIM(RTRIM(MotivoCierre)))>0)));
 CREATE UNIQUE INDEX UX_Reserva_MesaActiva ON dbo.ReservasMesa(MesaId) WHERE Estado=N'Activa';
 CREATE UNIQUE INDEX UX_Reserva_Pedido ON dbo.ReservasMesa(PedidoId) WHERE PedidoId IS NOT NULL;
 CREATE UNIQUE INDEX UX_Reserva_Cierre ON dbo.ReservasMesa(OperacionCierre) WHERE OperacionCierre IS NOT NULL;
 INSERT dbo.VersionEsquema(Version)VALUES(8);
END;
IF OBJECT_ID(N'dbo.ReservasMesa',N'U') IS NULL
OR EXISTS(SELECT 1 FROM (VALUES(N'Id'),(N'MesaId'),(N'MesaHistorica'),(N'Nombre'),(N'Estado'),(N'Fecha'),(N'UsuarioId'),(N'OperacionId'),(N'PedidoId'),(N'OperacionCierre'),(N'UsuarioCierre'),(N'FechaCierre'),(N'MotivoCierre'),(N'Version')) x(Columna) WHERE COL_LENGTH(N'dbo.ReservasMesa',Columna) IS NULL)
OR (SELECT COUNT(*) FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.ReservasMesa') AND name IN(N'UX_Reserva_MesaActiva',N'UX_Reserva_Pedido',N'UX_Reserva_Cierre') AND is_unique=1 AND is_disabled=0 AND has_filter=1)<>3
 THROW 51604,N'La estructura 008 está incompleta. No continuar.',1;
COMMIT;
-- Diagnóstico, no actualización: estas mesas mantienen su estado y requieren revisión.
SELECT m.Id,m.Numero,N'Reserva anterior sin titular registrado; no se libera automáticamente.' AS Diagnostico
FROM dbo.Mesas m WHERE m.Estado=N'Reservada'
AND NOT EXISTS(SELECT 1 FROM dbo.ReservasMesa r WHERE r.MesaId=m.Id AND r.Estado=N'Activa');
