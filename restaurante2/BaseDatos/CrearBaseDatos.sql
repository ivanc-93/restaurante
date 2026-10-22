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
