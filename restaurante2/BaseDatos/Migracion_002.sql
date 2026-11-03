SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
GO
-- Seleccione la base destino antes de ejecutar. No crea ni cambia de base.
-- Respaldar antes de migrar. Ante inconsistencias no borra ni corrige datos automáticamente.
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;
    IF OBJECT_ID(N'dbo.Menu',N'U') IS NULL OR OBJECT_ID(N'dbo.Clientes',N'U') IS NULL
        OR OBJECT_ID(N'dbo.Pedidos',N'U') IS NULL OR OBJECT_ID(N'dbo.Facturas',N'U') IS NULL
        THROW 51000,N'Faltan tablas base. Ejecute primero CrearBaseDatos.sql.',1;

    IF EXISTS(SELECT 1 FROM dbo.Menu WHERE Precio<0 OR LEN(LTRIM(RTRIM(Nombre)))=0 OR LEN(LTRIM(RTRIM(Categoria)))=0 OR (Tipo<>N'Bebida' AND Alcoholica=1))
        THROW 51001,N'Hay productos inválidos. Revise nombre, categoría, precio y tipo antes de migrar.',1;
    IF EXISTS(SELECT 1 FROM dbo.Clientes WHERE LEN(LTRIM(RTRIM(Nombre)))=0)
        THROW 51002,N'Hay clientes sin nombre. Corrija los datos antes de migrar.',1;
    IF EXISTS(SELECT 1 FROM dbo.Mesas WHERE Numero<=0 OR Capacidad NOT BETWEEN 1 AND 100)
        THROW 51003,N'Hay mesas fuera del rango permitido. Corrija los datos antes de migrar.',1;
    IF EXISTS(SELECT MesaId FROM dbo.Pedidos WHERE Estado IN(N'Abierto',N'En preparación') GROUP BY MesaId HAVING COUNT(*)>1)
        THROW 51004,N'Hay varias órdenes abiertas para una mesa. Revise antes de migrar.',1;
    IF EXISTS(SELECT Nombre,Tipo,Categoria FROM dbo.Menu WHERE Activo=1 GROUP BY Nombre,Tipo,Categoria HAVING COUNT(*)>1)
        THROW 51005,N'Hay productos activos duplicados por nombre, tipo y categoría.',1;
    IF EXISTS(SELECT Correo FROM dbo.Clientes WHERE Activo=1 AND Correo IS NOT NULL GROUP BY Correo HAVING COUNT(*)>1)
        THROW 51006,N'Hay correos duplicados entre clientes activos.',1;
    IF EXISTS(SELECT NumeroFactura FROM dbo.Facturas GROUP BY NumeroFactura HAVING COUNT(*)>1)
        THROW 51007,N'Hay números de factura duplicados. Revise antes de migrar.',1;

    IF OBJECT_ID(N'dbo.Restaurante',N'U') IS NULL
        CREATE TABLE dbo.Restaurante(
            Id INT NOT NULL CONSTRAINT PK_Restaurante PRIMARY KEY,
            Nombre NVARCHAR(120) NOT NULL,
            Ubicacion NVARCHAR(250) NOT NULL,
            CONSTRAINT CK_Restaurante_Unico CHECK(Id=1),
            CONSTRAINT CK_Restaurante_Texto CHECK(LEN(LTRIM(RTRIM(Nombre)))>0 AND LEN(LTRIM(RTRIM(Ubicacion)))>0)
        );
    IF NOT EXISTS(SELECT 1 FROM dbo.Restaurante WHERE Id=1)
        INSERT dbo.Restaurante(Id,Nombre,Ubicacion) VALUES(1,N'Sabores',N'Pendiente de configurar');

    IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name=N'CK_Menu_TextoTipo' AND parent_object_id=OBJECT_ID(N'dbo.Menu'))
        ALTER TABLE dbo.Menu WITH CHECK ADD CONSTRAINT CK_Menu_TextoTipo
            CHECK(LEN(LTRIM(RTRIM(Nombre)))>0 AND LEN(LTRIM(RTRIM(Categoria)))>0 AND (Tipo=N'Bebida' OR Alcoholica=0));
    IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name=N'CK_Clientes_Nombre' AND parent_object_id=OBJECT_ID(N'dbo.Clientes'))
        ALTER TABLE dbo.Clientes WITH CHECK ADD CONSTRAINT CK_Clientes_Nombre CHECK(LEN(LTRIM(RTRIM(Nombre)))>0);
    IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name=N'CK_Mesas_Rango' AND parent_object_id=OBJECT_ID(N'dbo.Mesas'))
        ALTER TABLE dbo.Mesas WITH CHECK ADD CONSTRAINT CK_Mesas_Rango CHECK(Numero>0 AND Capacidad BETWEEN 1 AND 100);
    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.Menu') AND name=N'UX_Menu_Activo')
        CREATE UNIQUE INDEX UX_Menu_Activo ON dbo.Menu(Nombre,Tipo,Categoria) WHERE Activo=1;
    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.Clientes') AND name=N'UX_Clientes_CorreoActivo')
        CREATE UNIQUE INDEX UX_Clientes_CorreoActivo ON dbo.Clientes(Correo) WHERE Activo=1 AND Correo IS NOT NULL;
    -- Índice de apoyo. El bloqueo de la mesa serializa apertura y cambio de estado.
    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.Pedidos') AND name=N'IX_Pedidos_MesaEstado')
        CREATE INDEX IX_Pedidos_MesaEstado ON dbo.Pedidos(MesaId,Estado);
    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.Facturas') AND name=N'UX_Facturas_Numero')
        CREATE UNIQUE INDEX UX_Facturas_Numero ON dbo.Facturas(NumeroFactura);
    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.Facturas') AND name=N'IX_Facturas_Fecha')
        CREATE INDEX IX_Facturas_Fecha ON dbo.Facturas(Fecha) INCLUDE(Total,PedidoId);
    COMMIT;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT>0 ROLLBACK;
    THROW;
END CATCH;
