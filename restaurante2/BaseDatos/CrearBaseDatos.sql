SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
GO
USE master;
GO
IF DB_ID(N'RestauranteDB') IS NULL
    CREATE DATABASE RestauranteDB;
GO
USE RestauranteDB;
GO

IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Usuarios
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Usuarios PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL,
        Usuario NVARCHAR(50) NOT NULL CONSTRAINT UQ_Usuarios_Usuario UNIQUE,
        ContrasenaHash CHAR(64) NOT NULL,
        Sal NVARCHAR(64) NOT NULL,
        Rol NVARCHAR(30) NOT NULL CONSTRAINT DF_Usuarios_Rol DEFAULT N'Empleado',
        Activo BIT NOT NULL CONSTRAINT DF_Usuarios_Activo DEFAULT 1,
        FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_Usuarios_Fecha DEFAULT SYSUTCDATETIME()
    );
END;
GO

IF OBJECT_ID(N'dbo.Mesas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Mesas
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Mesas PRIMARY KEY,
        Numero INT NOT NULL CONSTRAINT UQ_Mesas_Numero UNIQUE,
        Capacidad INT NOT NULL,
        Estado NVARCHAR(20) NOT NULL CONSTRAINT DF_Mesas_Estado DEFAULT N'Disponible',
        CONSTRAINT CK_Mesas_Capacidad CHECK (Capacidad > 0),
        CONSTRAINT CK_Mesas_Estado CHECK (Estado IN (N'Disponible', N'Ocupada', N'Reservada'))
    );
END;
GO

IF OBJECT_ID(N'dbo.Menu', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Menu
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Menu PRIMARY KEY,
        Nombre NVARCHAR(120) NOT NULL,
        Tipo NVARCHAR(20) NOT NULL,
        Categoria NVARCHAR(60) NOT NULL,
        Precio DECIMAL(10,2) NOT NULL,
        Alcoholica BIT NOT NULL CONSTRAINT DF_Menu_Alcoholica DEFAULT 0,
        Activo BIT NOT NULL CONSTRAINT DF_Menu_Activo DEFAULT 1,
        CONSTRAINT CK_Menu_Tipo CHECK (Tipo IN (N'Plato', N'Bebida')),
        CONSTRAINT CK_Menu_Precio CHECK (Precio >= 0)
    );
END;
GO

IF OBJECT_ID(N'dbo.Clientes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Clientes
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Clientes PRIMARY KEY,
        Nombre NVARCHAR(120) NOT NULL,
        Telefono NVARCHAR(30) NULL,
        Correo NVARCHAR(150) NULL,
        Activo BIT NOT NULL CONSTRAINT DF_Clientes_Activo DEFAULT 1,
        FechaRegistro DATETIME2 NOT NULL CONSTRAINT DF_Clientes_Fecha DEFAULT SYSDATETIME()
    );
END;
GO

IF OBJECT_ID(N'dbo.Pedidos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Pedidos
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Pedidos PRIMARY KEY,
        MesaId INT NOT NULL,
        ClienteId INT NULL,
        Fecha DATETIME2 NOT NULL CONSTRAINT DF_Pedidos_Fecha DEFAULT SYSDATETIME(),
        Estado NVARCHAR(25) NOT NULL CONSTRAINT DF_Pedidos_Estado DEFAULT N'Abierto',
        Total DECIMAL(12,2) NOT NULL CONSTRAINT DF_Pedidos_Total DEFAULT 0,
        CONSTRAINT FK_Pedidos_Mesas FOREIGN KEY (MesaId) REFERENCES dbo.Mesas(Id),
        CONSTRAINT FK_Pedidos_Clientes FOREIGN KEY (ClienteId) REFERENCES dbo.Clientes(Id),
        CONSTRAINT CK_Pedidos_Estado CHECK (Estado IN (N'Abierto', N'En preparación', N'Facturado', N'Cancelado')),
        CONSTRAINT CK_Pedidos_Total CHECK (Total >= 0)
    );
END;
GO

IF OBJECT_ID(N'dbo.DetallesPedido', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DetallesPedido
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DetallesPedido PRIMARY KEY,
        PedidoId INT NOT NULL,
        MenuId INT NOT NULL,
        Cantidad INT NOT NULL,
        PrecioUnitario DECIMAL(10,2) NOT NULL,
        CONSTRAINT FK_DetallesPedido_Pedidos FOREIGN KEY (PedidoId) REFERENCES dbo.Pedidos(Id),
        CONSTRAINT FK_DetallesPedido_Menu FOREIGN KEY (MenuId) REFERENCES dbo.Menu(Id),
        CONSTRAINT CK_DetallesPedido_Cantidad CHECK (Cantidad > 0),
        CONSTRAINT CK_DetallesPedido_Precio CHECK (PrecioUnitario >= 0)
    );
END;
GO

IF OBJECT_ID(N'dbo.Facturas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Facturas
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Facturas PRIMARY KEY,
        NumeroFactura AS (N'F-' + RIGHT(REPLICATE(N'0', 8) + CONVERT(NVARCHAR(12), Id), 8)) PERSISTED,
        PedidoId INT NOT NULL CONSTRAINT UQ_Facturas_Pedido UNIQUE,
        Fecha DATETIME2 NOT NULL CONSTRAINT DF_Facturas_Fecha DEFAULT SYSDATETIME(),
        Subtotal DECIMAL(12,2) NOT NULL,
        Impuesto DECIMAL(12,2) NOT NULL CONSTRAINT DF_Facturas_Impuesto DEFAULT 0,
        Total DECIMAL(12,2) NOT NULL,
        CONSTRAINT FK_Facturas_Pedidos FOREIGN KEY (PedidoId) REFERENCES dbo.Pedidos(Id),
        CONSTRAINT CK_Facturas_Importes CHECK (Subtotal >= 0 AND Impuesto >= 0 AND Total >= 0)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE Usuario = N'admin')
BEGIN
    DECLARE @SalAdmin NVARCHAR(64) = N'SaboresAdmin2026';
    INSERT dbo.Usuarios (Nombre, Usuario, ContrasenaHash, Sal, Rol)
    VALUES (N'Administrador', N'admin',
        CONVERT(CHAR(64), HASHBYTES('SHA2_256', CONVERT(VARCHAR(200), @SalAdmin + N':Admin123!')), 2),
        @SalAdmin, N'Administrador');
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Mesas)
BEGIN
    INSERT dbo.Mesas (Numero, Capacidad) VALUES
    (1, 2), (2, 2), (3, 4), (4, 4), (5, 4), (6, 6), (7, 6), (8, 8);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Menu)
BEGIN
    INSERT dbo.Menu (Nombre, Tipo, Categoria, Precio, Alcoholica) VALUES
    (N'Tacos de la casa', N'Plato', N'Platos principales', 145.00, 0),
    (N'Enchiladas verdes', N'Plato', N'Platos principales', 165.00, 0),
    (N'Ensalada de temporada', N'Plato', N'Entradas', 110.00, 0),
    (N'Pastel de chocolate', N'Plato', N'Postres', 85.00, 0),
    (N'Agua de jamaica', N'Bebida', N'Bebidas', 38.00, 0),
    (N'Limonada', N'Bebida', N'Bebidas', 42.00, 0),
    (N'Café de olla', N'Bebida', N'Bebidas', 48.00, 0);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Clientes)
BEGIN
    INSERT dbo.Clientes (Nombre, Telefono, Correo) VALUES
    (N'Cliente frecuente', N'555-0100', N'cliente@ejemplo.com');
END;
GO

-- Estructuras y restricciones de la versión 2 (también disponibles como migración independiente).
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
