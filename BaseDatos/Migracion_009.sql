SET XACT_ABORT ON;
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
IF DB_NAME() IN(N'master',N'model',N'msdb',N'tempdb') THROW 51700,N'Seleccione la base de Sabores.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.VersionEsquema WHERE Version=8) THROW 51701,N'Requiere esquema 008.',1;
BEGIN TRANSACTION;
DECLARE @lock int;
EXEC @lock=sys.sp_getapplock @Resource=N'SaboresInstalacion',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=15000;
IF @lock<0 THROW 51702,N'Instalación ocupada.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.VersionEsquema WHERE Version=9)
BEGIN
 IF OBJECT_ID(N'dbo.AbonosPedido') IS NOT NULL OR OBJECT_ID(N'dbo.CuentasPedido') IS NOT NULL THROW 51703,N'Estructura sin versión. Solicite revisión.',1;
 CREATE TABLE dbo.CuentasPedido(Id int IDENTITY PRIMARY KEY,PedidoId int NOT NULL REFERENCES dbo.Pedidos(Id),Nombre nvarchar(80) NOT NULL,
  Original decimal(12,2) NOT NULL CHECK(Original>0),Descuento decimal(12,2) NOT NULL CHECK(Descuento>=0),Total AS (Original-Descuento) PERSISTED,
  OperacionId uniqueidentifier NOT NULL REFERENCES dbo.OperacionesRestaurante(Id),CHECK(Descuento<=Original));
 CREATE INDEX IX_Cuentas_Pedido ON dbo.CuentasPedido(PedidoId);
 CREATE TABLE dbo.UnidadesCuenta(CuentaId int NOT NULL REFERENCES dbo.CuentasPedido(Id),DetalleId int NOT NULL REFERENCES dbo.DetallesPedido(Id),Cantidad int NOT NULL CHECK(Cantidad>0),PRIMARY KEY(CuentaId,DetalleId));
 CREATE TABLE dbo.AbonosPedido(OperacionId uniqueidentifier PRIMARY KEY REFERENCES dbo.OperacionesRestaurante(Id),PedidoId int NOT NULL REFERENCES dbo.Pedidos(Id),
  CuentaId int NULL REFERENCES dbo.CuentasPedido(Id),Estado nvarchar(15) NOT NULL CHECK(Estado IN(N'Confirmado',N'Rechazado',N'Incierto')),
  Metodo nvarchar(15) NOT NULL CHECK(Metodo IN(N'Efectivo',N'Tarjeta')),Importe decimal(12,2) NOT NULL CHECK(Importe>0),
  Entregado decimal(12,2) NOT NULL,Cambio decimal(12,2) NOT NULL CHECK(Cambio>=0),Receptor nvarchar(120) NOT NULL,
  Identificacion nvarchar(40) NULL,Referencia nvarchar(100) NULL,UsuarioId int NOT NULL REFERENCES dbo.Usuarios(Id),Fecha datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  SolicitudOriginal nvarchar(max) NOT NULL,ConciliacionId uniqueidentifier NULL REFERENCES dbo.OperacionesRestaurante(Id),
  CHECK((Metodo=N'Efectivo' AND Estado=N'Confirmado' AND Entregado=Importe+Cambio) OR (Metodo=N'Tarjeta' AND Entregado=Importe AND Cambio=0)));
 CREATE INDEX IX_Abonos_Pedido ON dbo.AbonosPedido(PedidoId,Estado);
 CREATE UNIQUE INDEX UX_Abono_Incierto ON dbo.AbonosPedido(PedidoId) WHERE Estado=N'Incierto';
 CREATE TABLE dbo.ReglaFrecuente(Id int NOT NULL PRIMARY KEY CHECK(Id=1),Activa bit NOT NULL,VisitasMinimas int NOT NULL CHECK(VisitasMinimas BETWEEN 1 AND 10000),
  Porcentaje decimal(5,2) NOT NULL CHECK(Porcentaje BETWEEN 5 AND 50),Desde date NOT NULL,Hasta date NOT NULL,CHECK(Hasta>=Desde));
 INSERT dbo.ReglaFrecuente VALUES(1,0,1,5,'20000101','20000101');
 ALTER TABLE dbo.AjustesPedido ADD Porcentaje decimal(5,2) NULL,Promocion bit NOT NULL DEFAULT 0;
 INSERT dbo.VersionEsquema(Version)VALUES(9);
END;
IF COL_LENGTH(N'dbo.AjustesPedido',N'Porcentaje') IS NULL OR COL_LENGTH(N'dbo.AjustesPedido',N'Promocion') IS NULL OR OBJECT_ID(N'dbo.CuentasPedido') IS NULL OR OBJECT_ID(N'dbo.UnidadesCuenta') IS NULL OR COL_LENGTH(N'dbo.AbonosPedido',N'ConciliacionId') IS NULL OR OBJECT_ID(N'dbo.ReglaFrecuente') IS NULL OR NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.AbonosPedido') AND name=N'UX_Abono_Incierto' AND is_unique=1 AND is_disabled=0) THROW 51704,N'Esquema 009 incompleto.',1;
COMMIT;
