$ErrorActionPreference = "Stop"

$psql = "C:\Program Files\PostgreSQL\16\bin\psql.exe"
$db   = "wicked_mmorpg"
$user = "postgres"

# Hitta repo-root automatiskt
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot  = Resolve-Path "$scriptDir\.."

$sqlFile = "$repoRoot\Migrations\001_InitialSchema.sql"

Write-Host "Running migration:"
Write-Host $sqlFile

& $psql -U $user -d $db -f $sqlFile
