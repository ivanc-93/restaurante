SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
GO
-- Versión 3: identidad del producto capturada por detalle.
-- Para filas antiguas solo se conoce el catálogo ACTUAL, no cambios históricos anteriores.
-- Seleccionar explícitamente la base de destino y respaldar antes de actualizar.
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;
    IF OBJECT_ID(N'dbo.DetallesPedido',N'U') IS NULL OR OBJECT_ID(N'dbo.Menu',N'U') IS NULL
        THROW 51100,N'Faltan tablas base. Aplique instalación y migración 002 antes de la 003.',1;

    IF COL_LENGTH(N'dbo.DetallesPedido',N'NombreProducto') IS NULL
        ALTER TABLE dbo.DetallesPedido ADD NombreProducto NVARCHAR(120) NULL;
    IF COL_LENGTH(N'dbo.DetallesPedido',N'TipoProducto') IS NULL
        ALTER TABLE dbo.DetallesPedido ADD TipoProducto NVARCHAR(20) NULL;

    IF EXISTS(SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.DetallesPedido')
        AND ((name=N'NombreProducto' AND (TYPE_NAME(user_type_id)<>N'nvarchar' OR max_length<>240))
          OR (name=N'TipoProducto' AND (TYPE_NAME(user_type_id)<>N'nvarchar' OR max_length<>40))))
        THROW 51101,N'Las columnas históricas existentes tienen tipos o longitudes incompatibles.',1;

    -- SQL dinámico: permite añadir columnas y utilizarlas en el mismo lote sin errores de compilación.
    EXEC sys.sp_executesql N'
        IF EXISTS(SELECT 1 FROM dbo.DetallesPedido d LEFT JOIN dbo.Menu m ON m.Id=d.MenuId
            WHERE (d.NombreProducto IS NULL OR d.TipoProducto IS NULL)
                AND (m.Id IS NULL OR LEN(LTRIM(RTRIM(m.Nombre)))=0 OR m.Tipo NOT IN(N''Plato'',N''Bebida'')))
            THROW 51102,N''No se puede inicializar la identidad de algunos detalles. Revise sus productos.'',1;

        UPDATE d SET NombreProducto=COALESCE(d.NombreProducto,m.Nombre),
            TipoProducto=COALESCE(d.TipoProducto,m.Tipo)
        FROM dbo.DetallesPedido d JOIN dbo.Menu m ON m.Id=d.MenuId
        WHERE d.NombreProducto IS NULL OR d.TipoProducto IS NULL;

        IF EXISTS(SELECT 1 FROM dbo.DetallesPedido WHERE NombreProducto IS NULL OR TipoProducto IS NULL
            OR LEN(LTRIM(RTRIM(NombreProducto)))=0 OR TipoProducto NOT IN(N''Plato'',N''Bebida''))
            THROW 51103,N''Hay valores históricos inválidos. No se sobrescribieron identidades ya capturadas.'',1;

        ALTER TABLE dbo.DetallesPedido ALTER COLUMN NombreProducto NVARCHAR(120) NOT NULL;
        ALTER TABLE dbo.DetallesPedido ALTER COLUMN TipoProducto NVARCHAR(20) NOT NULL;

        IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE parent_object_id=OBJECT_ID(N''dbo.DetallesPedido'') AND name=N''CK_DetallesPedido_Identidad'')
            ALTER TABLE dbo.DetallesPedido WITH CHECK ADD CONSTRAINT CK_DetallesPedido_Identidad
                CHECK(LEN(LTRIM(RTRIM(NombreProducto)))>0 AND TipoProducto IN(N''Plato'',N''Bebida''));
    ';
    COMMIT;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT>0 ROLLBACK;
    THROW;
END CATCH;
