#requires -Version 7.0
param(
 [Parameter(Mandatory=$true)][string]$Servidor,
 [Parameter(Mandatory=$true)][ValidatePattern('^[A-Za-z][A-Za-z0-9_]{0,79}$')][string]$BaseDatos,
 [Parameter(Mandatory=$true)][securestring]$TokenInstalacion,
 [switch]$ConRecuperacion,
 [switch]$ConOperaciones
)
$ErrorActionPreference='Stop'
# Solo bases inexistentes: no borra usuarios ni reinicia bases del grupo.
$existe=& sqlcmd -S $Servidor -E -b -d master -h -1 -W -Q "SET NOCOUNT ON;SELECT COUNT(*) FROM sys.databases WHERE name=N'$BaseDatos';"
if($LASTEXITCODE -ne 0){throw 'No se pudo comprobar el servidor.'}
if(($existe|Out-String).Trim() -ne '0'){throw 'La base ya existe. Use el procedimiento de respaldo y actualización; no se reinicializará.'}
$puntero=[Runtime.InteropServices.Marshal]::SecureStringToBSTR($TokenInstalacion)
try {
 $token=[Runtime.InteropServices.Marshal]::PtrToStringBSTR($puntero)
 if($token.Length -lt 20){throw 'Use un token de instalación de al menos 20 caracteres.'}
 $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($token)))
} finally {[Runtime.InteropServices.Marshal]::ZeroFreeBSTR($puntero);$token=$null}
$archivo=Join-Path ([IO.Path]::GetTempPath()) ('SaboresInstalacion_'+[guid]::NewGuid().ToString('N')+'.sql')
try {
 $base=(Get-Content (Join-Path $PSScriptRoot 'CrearBaseDatos.sql') -Raw -Encoding UTF8).Replace('RestauranteDB',$BaseDatos)
 $base="EXEC sys.sp_set_session_context @key=N'SaboresSinUsuarioDemo',@value=1;`nGO`n"+$base
 $base|Set-Content -LiteralPath $archivo -Encoding utf8
 & sqlcmd -S $Servidor -E -b -I -f 65001 -d master -i $archivo
 if($LASTEXITCODE -ne 0){throw 'Falló la creación. Conserve la base para diagnóstico; no se eliminará automáticamente.'}
 foreach($n in @('002','003','004','005')) {
  & sqlcmd -S $Servidor -E -b -I -f 65001 -d $BaseDatos -i (Join-Path $PSScriptRoot "Migracion_$n.sql")
  if($LASTEXITCODE -ne 0){throw "Falló migración $n. No continuar."}
 }
 if($ConRecuperacion -or $ConOperaciones){
  & sqlcmd -S $Servidor -E -b -I -f 65001 -d $BaseDatos -i (Join-Path $PSScriptRoot 'Migracion_006.sql')
  if($LASTEXITCODE -ne 0){throw 'Falló migración 006. No continuar.'}
 }
 if($ConOperaciones){
  & sqlcmd -S $Servidor -E -b -I -f 65001 -d $BaseDatos -i (Join-Path $PSScriptRoot 'Migracion_007.sql')
  if($LASTEXITCODE -ne 0){throw 'Falló migración 007. No continuar.'}
 }
 & sqlcmd -S $Servidor -E -b -d $BaseDatos -Q "UPDATE dbo.InstalacionSabores SET TokenInstalacionHash='$hash' WHERE Id=1 AND Completada=0 AND NOT EXISTS(SELECT 1 FROM dbo.Usuarios);"
 if($LASTEXITCODE -ne 0){throw 'No se pudo completar el aprovisionamiento.'}
 Write-Output 'Base preparada sin usuarios de demostración. Configure el equipo y cree al primer administrador con el token entregado.'
} finally {if(Test-Path -LiteralPath $archivo){Remove-Item -LiteralPath $archivo};$hash=$null}
