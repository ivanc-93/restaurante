param([string]$RutaArtefactos,[switch]$SoloAutorizacion)
$ErrorActionPreference='Stop'
$raiz=Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$sufijo=(Get-Date -Format 'yyyyMMdd_HHmmss')+'_'+[guid]::NewGuid().ToString('N').Substring(0,6)
$operaciones='Restaurante_Pruebas_Roles_'+$sufijo
$instalacion='Restaurante_Pruebas_Instalacion_'+$sufijo
$token=[guid]::NewGuid().ToString('N')+[guid]::NewGuid().ToString('N')
$seguro=ConvertTo-SecureString $token -AsPlainText -Force
$anteriores=@{}
foreach($nombre in @('SABORES_ROLES_PRUEBAS','SABORES_INSTALACION_PRUEBAS','SABORES_TOKEN_PRUEBAS')) {$anteriores[$nombre]=[Environment]::GetEnvironmentVariable($nombre)}
try {
 $bases=if($SoloAutorizacion){@($operaciones)}else{@($operaciones,$instalacion)}
 foreach($base in $bases) {
  & (Join-Path $raiz 'BaseDatos/InstalarNuevaBase.ps1') -Servidor '(localdb)\MSSQLLocalDB' -BaseDatos $base -TokenInstalacion $seguro
 }
 $env:SABORES_ROLES_PRUEBAS="Server=(localdb)\MSSQLLocalDB;Database=$operaciones;Integrated Security=True;Encrypt=True;TrustServerCertificate=True"
 $env:SABORES_INSTALACION_PRUEBAS="Server=(localdb)\MSSQLLocalDB;Database=$instalacion;Integrated Security=True;Encrypt=True;TrustServerCertificate=True"
 $env:SABORES_TOKEN_PRUEBAS=$token
 $argumentos=@('build',(Join-Path $PSScriptRoot 'VerificadorRoles.csproj'),'--ignore-failed-sources','-m:1','-nr:false','--verbosity','minimal')
 if($RutaArtefactos){$RutaArtefactos=[IO.Path]::GetFullPath($RutaArtefactos);$argumentos+=@('--artifacts-path',$RutaArtefactos)}
 & dotnet @argumentos
 if($LASTEXITCODE -ne 0){throw 'No se pudo compilar el verificador de roles.'}
 $evidencias=Join-Path $raiz $(if($SoloAutorizacion){'Pruebas/EvidenciasAutorizacion'}else{'Pruebas/EvidenciasRoles'})
 New-Item -ItemType Directory -Force -Path $evidencias|Out-Null
 $ejecutable=if($RutaArtefactos){Join-Path $RutaArtefactos 'bin/VerificadorRoles/debug/VerificadorRoles.exe'}else{Join-Path $PSScriptRoot 'bin/Debug/net8.0-windows/VerificadorRoles.exe'}
 [string[]]$pruebas=@(if($SoloAutorizacion){'--validar-autorizacion';$evidencias}else{$evidencias})
 & $ejecutable @pruebas 2>&1 | Tee-Object (Join-Path $evidencias 'verificacion.txt')
 if($LASTEXITCODE -ne 0){throw 'Pruebas de roles fallidas. Las bases aisladas se conservan para diagnóstico.'}
 if($SoloAutorizacion){Write-Output "Base aislada conservada: $operaciones. No se cambió RestauranteDB."}else{Write-Output "Bases aisladas conservadas: $operaciones y $instalacion. No se cambió RestauranteDB."}
} finally {
 foreach($nombre in $anteriores.Keys){[Environment]::SetEnvironmentVariable($nombre,$anteriores[$nombre])}
 $token=$null
}
