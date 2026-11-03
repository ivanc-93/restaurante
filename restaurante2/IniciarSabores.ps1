$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    $raizArtefactos = dotnet msbuild Restaurante.Frontend/Restaurante.Frontend.csproj -getProperty:ArtifactsPath -nologo
    if ($LASTEXITCODE -ne 0) { throw 'No se pudo localizar la carpeta de compilación.' }
    $salidaArtefactos = Join-Path (($raizArtefactos | Out-String).Trim()) 'aplicacion'
    dotnet build Restaurante.Frontend/Restaurante.Frontend.csproj -c Release --artifacts-path $salidaArtefactos --nologo
    if ($LASTEXITCODE -ne 0) { throw 'No se pudo compilar Sabores.' }
    $salida = dotnet msbuild Restaurante.Frontend/Restaurante.Frontend.csproj -p:Configuration=Release "-p:ArtifactsPath=$salidaArtefactos" -getProperty:TargetPath -nologo
    if ($LASTEXITCODE -ne 0) { throw 'No se pudo localizar el programa compilado.' }
    $ejecutable = [IO.Path]::ChangeExtension(($salida | Out-String).Trim(), '.exe')
    Start-Process -FilePath $ejecutable
} finally { Pop-Location }
