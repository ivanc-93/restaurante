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
  <ItemGroup><ProjectReference Include="$frontend" /><Compile Include="$fuentes/Verificar.cs" Link="Verificar.cs" /><Compile Include="$fuentes/ConexionPrueba.cs" Link="ConexionPrueba.cs" /><Compile Include="$fuentes/ExportarDiagramas.cs" Link="ExportarDiagramas.cs" /></ItemGroup>
</Project>
"@ | Set-Content (Join-Path $trabajo 'Verificador.csproj') -Encoding utf8
$conexionAnterior=$env:RESTAURANTE_TEST_CONNECTION
try {
    $env:RESTAURANTE_TEST_CONNECTION=$null
    if($ConSql) {
        $basePrueba='Restaurante_Pruebas_'+(Get-Date -Format 'yyyyMMdd_HHmmss')+'_'+[guid]::NewGuid().ToString('N').Substring(0,8)
        $sql=Get-Content (Join-Path $repoRaiz 'restaurante2/BaseDatos/CrearBaseDatos.sql') -Raw
        $sql=$sql.Replace('RestauranteDB',$basePrueba)
        $instalacion=Join-Path $trabajo 'InstalarPruebas.sql'
        Set-Content $instalacion $sql -Encoding utf8
        & sqlcmd -S '(localdb)\MSSQLLocalDB' -E -b -f 65001 -l 15 -i $instalacion
        if($LASTEXITCODE -ne 0) {throw 'Falló la instalación de la base de pruebas.'}
        # Repetir la migración verifica que sea idempotente.
        & sqlcmd -S '(localdb)\MSSQLLocalDB' -E -b -f 65001 -d $basePrueba -i (Join-Path $repoRaiz 'restaurante2/BaseDatos/Migracion_002.sql')
        if($LASTEXITCODE -ne 0) {throw 'Falló la segunda ejecución de la migración.'}
        $env:RESTAURANTE_TEST_CONNECTION="Server=(localdb)\MSSQLLocalDB;Database=$basePrueba;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;Connect Timeout=10"
        Write-Host "Base aislada: $basePrueba (se conserva para inspección)."
    }
    $argumentos=@((Join-Path $trabajo 'Resultados'))
    if($ActualizarDiagramas){$argumentos+=(Join-Path $repoRaiz 'restaurante2/Diagramas')}
    & dotnet run --project (Join-Path $trabajo 'Verificador.csproj') -- @argumentos
    if($LASTEXITCODE -ne 0) {throw 'Hay pruebas fallidas. Consulte la salida.'}
} finally {$env:RESTAURANTE_TEST_CONNECTION=$conexionAnterior}
