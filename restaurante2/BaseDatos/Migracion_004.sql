-- Sabores: cobro manual académico. Ejecutar sobre la base elegida, nunca sobre master.
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
IF DB_NAME() IN (N'master',N'model',N'msdb',N'tempdb') THROW 51200,N'Seleccione la base del restaurante.',1;
BEGIN TRY
 BEGIN TRANSACTION;
 DECLARE @lock int;
 EXEC @lock=sys.sp_getapplock @Resource=N'SaboresMigracion004',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=15000;
 IF @lock<0 THROW 51201,N'No se pudo bloquear la migración.',1;
 IF COL_LENGTH(N'dbo.DetallesPedido',N'NombreProducto') IS NULL THROW 51202,N'Ejecute primero la migración 003.',1;
 IF OBJECT_ID(N'dbo.VersionEsquema',N'U') IS NULL
   CREATE TABLE dbo.VersionEsquema(Version int NOT NULL PRIMARY KEY,Fecha datetime2 NOT NULL DEFAULT SYSUTCDATETIME());
 IF NOT EXISTS(SELECT 1 FROM dbo.VersionEsquema WHERE Version=4)
 BEGIN
   IF EXISTS(SELECT MesaId FROM dbo.Pedidos WHERE Estado IN(N'Abierto',N'En preparación') GROUP BY MesaId HAVING COUNT(*)>1)
     THROW 51203,N'Hay varios pedidos activos por mesa. Diagnostique antes de actualizar.',1;
   ALTER TABLE dbo.Pedidos ADD VersionCuenta rowversion;
   ALTER TABLE dbo.Pedidos ADD ClienteHistorico nvarchar(120) NULL,MesaHistorica int NULL,CapturaLegada bit NOT NULL CONSTRAINT DF_Pedidos_CapturaLegada DEFAULT 1;
   EXEC(N'UPDATE p SET ClienteHistorico=COALESCE(c.Nombre,N''Público general''),MesaHistorica=m.Numero FROM dbo.Pedidos p JOIN dbo.Mesas m ON m.Id=p.MesaId LEFT JOIN dbo.Clientes c ON c.Id=p.ClienteId;');
   ALTER TABLE dbo.Mesas DROP CONSTRAINT CK_Mesas_Estado;
   ALTER TABLE dbo.Mesas ADD CONSTRAINT CK_Mesas_Estado CHECK(Estado IN(N'Disponible',N'Ocupada',N'Reservada',N'En limpieza'));
   ALTER TABLE dbo.Pedidos DROP CONSTRAINT CK_Pedidos_Estado;
   ALTER TABLE dbo.Pedidos ADD CONSTRAINT CK_Pedidos_Estado CHECK(Estado IN(N'Abierto',N'En preparación',N'Facturado',N'Cancelado',N'Incidencia'));
   CREATE UNIQUE INDEX UX_Pedidos_UnActivoMesa ON dbo.Pedidos(MesaId) WHERE Estado IN(N'Abierto',N'En preparación');
   ALTER TABLE dbo.Facturas ADD Legada bit NOT NULL CONSTRAINT DF_Facturas_Legada DEFAULT 1,
     Receptor nvarchar(120) NULL,Identificacion nvarchar(40) NULL,Direccion nvarchar(250) NULL,Correo nvarchar(150) NULL,
     RestauranteHistorico nvarchar(120) NULL,UbicacionHistorica nvarchar(250) NULL,MesaHistorica int NULL,
     Operador nvarchar(100) NULL,UsuarioId int NULL,Moneda char(3) NOT NULL CONSTRAINT DF_Facturas_Moneda DEFAULT 'GTQ';
   EXEC(N'UPDATE f SET Receptor=COALESCE(c.Nombre,N''Público general''),MesaHistorica=m.Numero,RestauranteHistorico=r.Nombre,UbicacionHistorica=r.Ubicacion FROM dbo.Facturas f JOIN dbo.Pedidos p ON p.Id=f.PedidoId JOIN dbo.Mesas m ON m.Id=p.MesaId LEFT JOIN dbo.Clientes c ON c.Id=p.ClienteId CROSS JOIN dbo.Restaurante r;');
   CREATE TABLE dbo.OperacionesRestaurante(
     Id uniqueidentifier NOT NULL PRIMARY KEY,Tipo nvarchar(30) NOT NULL,SolicitudHash char(64) NOT NULL,
     UsuarioId int NOT NULL REFERENCES dbo.Usuarios(Id),Fecha datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),Resultado nvarchar(2000) NOT NULL);
   CREATE TABLE dbo.PagosManuales(
     OperacionId uniqueidentifier NOT NULL PRIMARY KEY REFERENCES dbo.OperacionesRestaurante(Id),
     PedidoId int NOT NULL REFERENCES dbo.Pedidos(Id),Metodo nvarchar(20) NOT NULL,
     Estado nvarchar(20) NOT NULL,Importe decimal(12,2) NOT NULL,Entregado decimal(12,2) NOT NULL,Cambio decimal(12,2) NOT NULL,
     Referencia nvarchar(100) NULL,Observacion nvarchar(250) NULL,SolicitudOriginal nvarchar(max) NOT NULL,Fecha datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
     UsuarioId int NOT NULL REFERENCES dbo.Usuarios(Id),
     CONSTRAINT CK_PagosManuales_Valores CHECK(Importe>=0 AND Entregado>=0 AND Cambio>=0 AND
       Metodo IN(N'Efectivo',N'Tarjeta',N'Cortesía') AND Estado IN(N'Confirmado',N'Rechazado',N'Incierto') AND
       ((Metodo=N'Efectivo' AND Estado=N'Confirmado' AND Entregado=Importe+Cambio) OR
        (Metodo=N'Tarjeta' AND Entregado=Importe AND Cambio=0) OR
        (Metodo=N'Cortesía' AND Estado=N'Confirmado' AND Importe=0 AND Entregado=0 AND Cambio=0))));
   CREATE UNIQUE INDEX UX_PagosManuales_Confirmado ON dbo.PagosManuales(PedidoId) WHERE Estado=N'Confirmado';
   CREATE UNIQUE INDEX UX_PagosManuales_Incierto ON dbo.PagosManuales(PedidoId) WHERE Estado=N'Incierto';
   CREATE TABLE dbo.AuditoriaOperaciones(
     Id bigint IDENTITY PRIMARY KEY,OperacionId uniqueidentifier NOT NULL REFERENCES dbo.OperacionesRestaurante(Id),
     PedidoId int NULL REFERENCES dbo.Pedidos(Id),MesaId int NULL REFERENCES dbo.Mesas(Id),
     Accion nvarchar(40) NOT NULL,Anterior nvarchar(100) NULL,Posterior nvarchar(100) NOT NULL,
     Motivo nvarchar(500) NOT NULL,Importe decimal(12,2) NOT NULL,UsuarioId int NOT NULL REFERENCES dbo.Usuarios(Id),
     Fecha datetime2 NOT NULL DEFAULT SYSUTCDATETIME());
   CREATE TABLE dbo.IncidenciasConsumo(
     PedidoId int NOT NULL PRIMARY KEY REFERENCES dbo.Pedidos(Id),ImportePendiente decimal(12,2) NOT NULL CHECK(ImportePendiente>=0),
     Motivo nvarchar(500) NOT NULL,OperacionId uniqueidentifier NOT NULL REFERENCES dbo.OperacionesRestaurante(Id),
     UsuarioId int NOT NULL REFERENCES dbo.Usuarios(Id),Fecha datetime2 NOT NULL DEFAULT SYSUTCDATETIME());
   INSERT dbo.VersionEsquema(Version)VALUES(4);
 END;
 COMMIT;
END TRY
BEGIN CATCH
 IF @@TRANCOUNT>0 ROLLBACK;
 THROW;
END CATCH;
