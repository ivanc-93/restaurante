param(
    [Parameter(Mandatory=$true)][string]$Servidor,
    [ValidatePattern('^[A-Za-z][A-Za-z0-9_]{0,79}$')][string]$BaseDatos='RestauranteDB',
    [Parameter(Mandatory=$true)][string]$RutaRespaldos,
    [switch]$ConFuncionesAuditadas
)
$ErrorActionPreference='Stop'
# Windows authentication. For SQL authentication use the SQL scripts from SSMS with that connection.
function Sql([string]$base,[string]$consulta) {
    $salida=& sqlcmd -S $Servidor -E -b -I -f 65001 -l 20 -d $base -W -h -1 -s '|' -Q $consulta
    if($LASTEXITCODE -ne 0){throw "Falló SQL sobre $base. No continúe manualmente sin revisar el error."}
    return $salida
}
function Script([string]$base,[string]$archivo) {
    & sqlcmd -S $Servidor -E -b -I -f 65001 -l 20 -d $base -i $archivo
    if($LASTEXITCODE -ne 0){throw "Falló $archivo. La actualización se detuvo."}
}
$ruta=[IO.Path]::GetFullPath($RutaRespaldos)
if($Servidor -like '(localdb)*' -or $Servidor -in @('.','localhost')) {New-Item -ItemType Directory -Force -Path $ruta|Out-Null}
$existe=(Sql 'master' "SET NOCOUNT ON;SELECT COUNT(*) FROM sys.databases WHERE name=N'$BaseDatos';" | Out-String).Trim()
if($existe -eq '0') {
    $temporal=Join-Path ([IO.Path]::GetTempPath()) ('Sabores_'+[guid]::NewGuid().ToString('N')+'.sql')
    try {
        (Get-Content (Join-Path $PSScriptRoot 'CrearBaseDatos.sql') -Raw -Encoding UTF8).Replace('RestauranteDB',$BaseDatos)|Set-Content $temporal -Encoding utf8
        Script 'master' $temporal
    } finally {if(Test-Path -LiteralPath $temporal){Remove-Item -LiteralPath $temporal}}
}
$sufijo=(Get-Date -Format 'yyyyMMdd_HHmmss')+'_'+[guid]::NewGuid().ToString('N').Substring(0,6)
$copia='Restaurante_Pruebas_Restauracion_'+$sufijo
$bak=Join-Path $ruta ($BaseDatos+'_'+$sufijo+'.bak')
$bakSql=$bak.Replace("'","''")
Sql 'master' "BACKUP DATABASE [$BaseDatos] TO DISK=N'$bakSql' WITH COPY_ONLY,CHECKSUM,INIT;"|Out-Host
$archivos=Sql 'master' "SET NOCOUNT ON;SELECT name,type FROM sys.master_files WHERE database_id=DB_ID(N'$BaseDatos') ORDER BY file_id;"
$moves=@();$i=0
foreach($linea in $archivos) {
    if([string]::IsNullOrWhiteSpace($linea)){continue}
    $partes=$linea.Trim().Split('|');if($partes.Count -ne 2){throw 'No se pudo interpretar el listado de archivos SQL.'}
    $extension=if($partes[1].Trim() -eq '1'){'.ldf'}elseif($i -eq 0){'.mdf'}else{'.ndf'}
    $destino=(Join-Path $ruta ($copia+'_'+$i+$extension)).Replace("'","''")
    $logico=$partes[0].Trim().Replace("'","''")
    $moves+="MOVE N'$logico' TO N'$destino'";$i++
}
if($moves.Count -lt 2){throw 'Listado de archivos incompleto; no se ejecutará la migración.'}
Sql 'master' ("RESTORE DATABASE [$copia] FROM DISK=N'$bakSql' WITH CHECKSUM,"+($moves -join ',')+';')|Out-Host
$tablas=@(Sql $BaseDatos "SET NOCOUNT ON;SELECT name FROM sys.tables WHERE schema_id=SCHEMA_ID(N'dbo') AND is_ms_shipped=0 ORDER BY name;" | Where-Object {![string]::IsNullOrWhiteSpace($_)} | ForEach-Object {$_.Trim()})
$consultas=$tablas|ForEach-Object {$nombre=$_.Replace("'","''");$identificador=$_.Replace(']',']]');"SELECT N'$nombre',COUNT_BIG(*),COALESCE(CHECKSUM_AGG(BINARY_CHECKSUM(*)),0) FROM dbo.[$identificador]"}
$huella="SET NOCOUNT ON;"+($consultas -join ' UNION ALL ')
$original=(Sql $BaseDatos $huella | Out-String).Trim()
$restaurado=(Sql $copia $huella | Out-String).Trim()
if($original -ne $restaurado){throw "La restauración no coincide con el estado actual. No se migró $BaseDatos. Compruebe actividad concurrente y el respaldo."}
$migraciones=@('Migracion_002.sql','Migracion_003.sql','Migracion_004.sql','Migracion_005.sql')
if($ConFuncionesAuditadas){$migraciones+=@('Migracion_006.sql','Migracion_007.sql')}
foreach($archivo in $migraciones) {Script $copia (Join-Path $PSScriptRoot $archivo)}
Script $copia (Join-Path $PSScriptRoot $migraciones[-1])
foreach($archivo in $migraciones) {Script $BaseDatos (Join-Path $PSScriptRoot $archivo)}
$resultado=[ordered]@{Servidor=$Servidor;Base=$BaseDatos;Respaldo=$bak;Restaurada=$copia;Fecha=(Get-Date).ToString('o');TablasVerificadas=$tablas.Count;Verificacion='Restauración real; tablas dbo con conteos/checksum iguales antes de migrar; cadena 002/003/004/005 probada; última migración repetida en copia';Version=[int]((Sql $BaseDatos 'SET NOCOUNT ON;SELECT MAX(Version) FROM dbo.VersionEsquema;' | Out-String).Trim())}
$registro=Join-Path $ruta ($BaseDatos+'_'+$sufijo+'.json')
$resultado|ConvertTo-Json|Set-Content $registro -Encoding utf8
$resultado|ConvertTo-Json
