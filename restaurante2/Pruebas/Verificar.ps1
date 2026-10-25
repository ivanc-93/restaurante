param([switch]$ConSql, [switch]$ActualizarDiagramas)
$ErrorActionPreference='Stop'
$repoRaiz=Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$trabajo=Join-Path $repoRaiz '.codex-build/Verificador'
New-Item -ItemType Directory -Force -Path $trabajo | Out-Null
$frontend=[System.Security.SecurityElement]::Escape((Join-Path $repoRaiz 'restaurante2/Restaurante.Frontend/Restaurante.Frontend.csproj'))
$fuentes=[System.Security.SecurityElement]::Escape($PSScriptRoot)
@"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0-windows</TargetFramework><UseWindowsForms>true</UseWindowsForms><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup>
  <ItemGroup><ProjectReference Include="$frontend" /><Compile Include="$fuentes/Verificar.cs" Link="Verificar.cs" /><Compile Include="$fuentes/ConexionPrueba.cs" Link="ConexionPrueba.cs" /><Compile Include="$fuentes/ExportarDiagramas.cs" Link="ExportarDiagramas.cs" /><Compile Include="$fuentes/PruebasHistorial.cs" Link="PruebasHistorial.cs" /><Compile Include="$fuentes/PruebasSesion.cs" Link="PruebasSesion.cs" /><Compile Include="$fuentes/DialogosPrueba.cs" Link="DialogosPrueba.cs" /></ItemGroup>
</Project>
"@ | Set-Content (Join-Path $trabajo 'Verificador.csproj') -Encoding utf8
$conexionAnterior=$env:RESTAURANTE_TEST_CONNECTION
$legadoAnterior=$env:RESTAURANTE_TEST_ANTERIOR
$migracionAnterior=$env:RESTAURANTE_MIGRACION_003
try {
    $env:RESTAURANTE_TEST_CONNECTION=$null
    $env:RESTAURANTE_TEST_ANTERIOR=$null
    $env:RESTAURANTE_MIGRACION_003=Join-Path $repoRaiz 'restaurante2/BaseDatos/Migracion_003.sql'
    if($ConSql) {
        $basePrueba='Restaurante_Pruebas_'+(Get-Date -Format 'yyyyMMdd_HHmmss')+'_'+[guid]::NewGuid().ToString('N').Substring(0,8)
        $sql=Get-Content (Join-Path $repoRaiz 'restaurante2/BaseDatos/CrearBaseDatos.sql') -Raw
        $sql=$sql.Replace('RestauranteDB',$basePrueba)
        $instalacion=Join-Path $trabajo 'InstalarPruebas.sql'
        Set-Content $instalacion $sql -Encoding utf8
        & sqlcmd -S '(localdb)\MSSQLLocalDB' -E -b -I -f 65001 -l 15 -i $instalacion
        if($LASTEXITCODE -ne 0) {throw 'Falló la instalación de la base de pruebas.'}
        # Repetir la migración verifica que sea idempotente.
        & sqlcmd -S '(localdb)\MSSQLLocalDB' -E -b -I -f 65001 -d $basePrueba -i (Join-Path $repoRaiz 'restaurante2/BaseDatos/Migracion_002.sql')
        if($LASTEXITCODE -ne 0) {throw 'Falló la segunda ejecución de la migración.'}
        & sqlcmd -S '(localdb)\MSSQLLocalDB' -E -b -I -f 65001 -d $basePrueba -i (Join-Path $repoRaiz 'restaurante2/BaseDatos/Migracion_003.sql')
        if($LASTEXITCODE -ne 0) {throw 'Falló la segunda ejecución de la migración 003.'}
        $env:RESTAURANTE_TEST_CONNECTION="Server=(localdb)\MSSQLLocalDB;Database=$basePrueba;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;Connect Timeout=10"
        # Fixture reproducible del esquema 2: se deriva del instalador eliminando únicamente las adiciones de historial.
        $legado=($sql -split '-- HISTORIAL VERSION 3')[0]
        $legado=[regex]::Replace($legado,'(?m)^\s*NombreProducto NVARCHAR\(120\) NOT NULL,\r?\n','')
        $legado=[regex]::Replace($legado,'(?m)^\s*TipoProducto NVARCHAR\(20\) NOT NULL,\r?\n','')
        $legado=[regex]::Replace($legado,'(?m)^\s*CONSTRAINT CK_DetallesPedido_Identidad CHECK.*\r?\n','')
        if($legado -match 'NombreProducto|TipoProducto') {throw 'La fixture anterior contiene columnas de la versión 3.'}
        $baseLegada=$basePrueba+'_Anterior'
        $legado=$legado.Replace($basePrueba,$baseLegada)
        $archivoLegado=Join-Path $trabajo 'InstalarVersion2.sql'
        Set-Content $archivoLegado $legado -Encoding utf8
        & sqlcmd -S '(localdb)\MSSQLLocalDB' -E -b -I -f 65001 -i $archivoLegado
        if($LASTEXITCODE -ne 0){throw 'Falló la fixture de versión 2.'}
        $semilla=@"
SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON; SET ANSI_PADDING ON; SET ANSI_WARNINGS ON; SET ARITHABORT ON; SET CONCAT_NULL_YIELDS_NULL ON; SET NUMERIC_ROUNDABORT OFF;
DECLARE @menu int,@pedido int;
INSERT dbo.Menu(Nombre,Tipo,Categoria,Precio) VALUES(N'Producto legado',N'Plato',N'Legado',10);
SET @menu=SCOPE_IDENTITY();
INSERT dbo.Pedidos(MesaId,Estado,Total) SELECT TOP(1) Id,N'Facturado',20 FROM dbo.Mesas;
SET @pedido=SCOPE_IDENTITY();
INSERT dbo.DetallesPedido(PedidoId,MenuId,Cantidad,PrecioUnitario) VALUES(@pedido,@menu,2,10);
INSERT dbo.Facturas(PedidoId,Subtotal,Impuesto,Total) VALUES(@pedido,20,0,20);
"@
        & sqlcmd -S '(localdb)\MSSQLLocalDB' -E -b -I -f 65001 -d $baseLegada -Q $semilla
        if($LASTEXITCODE -ne 0){throw 'Falló la semilla histórica.'}
        & sqlcmd -S '(localdb)\MSSQLLocalDB' -E -b -I -f 65001 -d $baseLegada -i $env:RESTAURANTE_MIGRACION_003
        if($LASTEXITCODE -ne 0){throw 'Falló la actualización desde versión 2.'}
        & sqlcmd -S '(localdb)\MSSQLLocalDB' -E -b -I -f 65001 -d $baseLegada -Q "UPDATE dbo.Menu SET Nombre=N'Nombre actual distinto',Tipo=N'Bebida' WHERE Nombre=N'Producto legado';"
        if($LASTEXITCODE -ne 0){throw 'Falló la edición posterior del catálogo legado.'}
        & sqlcmd -S '(localdb)\MSSQLLocalDB' -E -b -I -f 65001 -d $baseLegada -i $env:RESTAURANTE_MIGRACION_003
        if($LASTEXITCODE -ne 0){throw 'Falló la repetición de la actualización histórica.'}
        $env:RESTAURANTE_TEST_ANTERIOR="Server=(localdb)\MSSQLLocalDB;Database=$baseLegada;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;Connect Timeout=10"
        Write-Host "Base anterior actualizada: $baseLegada."
        Write-Host "Base aislada: $basePrueba (se conserva para inspección)."
    }
    $argumentos=@((Join-Path $trabajo 'Resultados'))
    if($ActualizarDiagramas){$argumentos+=(Join-Path $repoRaiz 'restaurante2/Diagramas')}
    & dotnet run --project (Join-Path $trabajo 'Verificador.csproj') -- @argumentos
    if($LASTEXITCODE -ne 0) {throw 'Hay pruebas fallidas. Consulte la salida.'}
} finally {$env:RESTAURANTE_TEST_CONNECTION=$conexionAnterior; $env:RESTAURANTE_TEST_ANTERIOR=$legadoAnterior; $env:RESTAURANTE_MIGRACION_003=$migracionAnterior}
