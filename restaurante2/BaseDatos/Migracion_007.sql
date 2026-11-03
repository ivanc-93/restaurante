-- Correcciones aditivas: nunca elimina consumo original ni reinicializa usuarios.
SET XACT_ABORT ON;
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
IF DB_NAME() IN ('master','model','msdb','tempdb') THROW 51500,N'Seleccione la base de Sabores.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.VersionEsquema WHERE Version=6) THROW 51501,N'Requiere esquema 6 completo.',1;
BEGIN TRANSACTION;
DECLARE @bloqueoAuditoria int;
EXEC @bloqueoAuditoria=sys.sp_getapplock @Resource=N'SaboresInstalacion',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=15000;
IF @bloqueoAuditoria<0 THROW 51502,N'Seguridad ocupada.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.VersionEsquema WHERE Version=7)
BEGIN
 CREATE TABLE dbo.CorreccionesLinea(
  Id bigint IDENTITY PRIMARY KEY,DetalleId int NOT NULL REFERENCES dbo.DetallesPedido(Id),
  OperacionId uniqueidentifier NOT NULL UNIQUE REFERENCES dbo.OperacionesRestaurante(Id),
  CantidadRetirada int NOT NULL CHECK(CantidadRetirada>0),CantidadAnterior int NOT NULL,CantidadPosterior int NOT NULL,
  Motivo nvarchar(500) NOT NULL,UsuarioId int NOT NULL REFERENCES dbo.Usuarios(Id),AutorizadorId int NOT NULL REFERENCES dbo.Usuarios(Id),
  Fecha datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),CONSTRAINT CK_Correccion_Cantidades CHECK(CantidadPosterior>=0 AND CantidadAnterior-CantidadRetirada=CantidadPosterior));
 CREATE INDEX IX_CorreccionesLinea_Detalle ON dbo.CorreccionesLinea(DetalleId);
 CREATE TABLE dbo.ResolucionesIncidencia(
  PedidoId int NOT NULL PRIMARY KEY REFERENCES dbo.IncidenciasConsumo(PedidoId),OperacionId uniqueidentifier NOT NULL UNIQUE REFERENCES dbo.OperacionesRestaurante(Id),
  Desenlace nvarchar(30) NOT NULL CHECK(Desenlace IN(N'Cobro posterior',N'Pérdida',N'Condonación')),
  Importe decimal(12,2) NOT NULL CHECK(Importe>=0),Motivo nvarchar(500) NOT NULL,UsuarioId int NOT NULL REFERENCES dbo.Usuarios(Id),Fecha datetime2 NOT NULL DEFAULT SYSUTCDATETIME());
 INSERT dbo.VersionEsquema(Version)VALUES(7);
END;
EXEC(N'CREATE OR ALTER VIEW dbo.DetallesPedidoVigentes AS
 SELECT d.Id,d.PedidoId,d.MenuId,d.Cantidad-COALESCE(c.Retirada,0) Cantidad,d.PrecioUnitario,d.NombreProducto,d.TipoProducto
 FROM dbo.DetallesPedido d OUTER APPLY(SELECT SUM(CantidadRetirada) Retirada FROM dbo.CorreccionesLinea WHERE DetalleId=d.Id)c
 WHERE d.Cantidad-COALESCE(c.Retirada,0)>0;');
-- Solo fechas NUEVAS: las anteriores conservan su significado original.
DECLARE @df sysname=(SELECT dc.name FROM sys.default_constraints dc JOIN sys.columns c ON c.object_id=dc.parent_object_id AND c.column_id=dc.parent_column_id WHERE dc.parent_object_id=OBJECT_ID(N'dbo.Pedidos') AND c.name=N'Fecha');
IF @df IS NOT NULL BEGIN DECLARE @ddl nvarchar(max)=N'ALTER TABLE dbo.Pedidos DROP CONSTRAINT '+QUOTENAME(@df); EXEC(@ddl); END;
ALTER TABLE dbo.Pedidos ADD CONSTRAINT DF_Pedidos_FechaGuatemala DEFAULT DATEADD(hour,-6,SYSUTCDATETIME()) FOR Fecha;
COMMIT;
