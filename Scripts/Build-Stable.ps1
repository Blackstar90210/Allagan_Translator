$ErrorActionPreference = "Stop"
Set-Location -Path $PSScriptRoot\..

$csprojPath = "AllaganTranslator.csproj"
$pluginmasterPath = "pluginmaster.json"

Write-Host "=== INIZIO BUILD STABILE ===" -ForegroundColor Cyan

# 1. Leggi la versione corrente (o puoi chiedere l'input se vuoi un incremento personalizzato)
[xml]$csproj = Get-Content $csprojPath
$currentVersion = $csproj.Project.PropertyGroup[0].AssemblyVersion

Write-Host "Compilazione versione pubblica: $currentVersion" -ForegroundColor Yellow

# 2. Compila il progetto
Write-Host "Pulizia vecchie build..."
Remove-Item -Path "bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "obj" -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "Compilazione in corso..."
dotnet build -c Release

# 3. Genera latest.zip
$outputZip = "bin\Release\AllaganTranslator\latest.zip"
if (Test-Path $outputZip) {
    # Inietta l'icona
    Write-Host "Aggiunta icona al pacchetto zip..."
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [System.IO.Compression.ZipFile]::Open($outputZip, 'Update')
    [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, "icon.png", "images/icon.png")
    $zip.Dispose()

    Copy-Item -Path $outputZip -Destination "latest.zip" -Force
    Write-Host "Pacchetto latest.zip generato con successo." -ForegroundColor Green
} else {
    Write-Error "Compilazione fallita, pacchetto zip non trovato."
    exit
}

# 4. Aggiorna pluginmaster.json
$json = Get-Content $pluginmasterPath -Raw | ConvertFrom-Json
$json[0].AssemblyVersion = $currentVersion

# Per sicurezza allineiamo anche la TestingAssemblyVersion alla stabile per non retrocedere i tester
$json[0].TestingAssemblyVersion = $currentVersion

$jsonString = ConvertTo-Json -InputObject @($json) -Depth 10
Set-Content -Path $pluginmasterPath -Value $jsonString

Write-Host "pluginmaster.json aggiornato in versione stabile!" -ForegroundColor Green
Write-Host "=== BUILD STABILE COMPLETATA ===" -ForegroundColor Cyan
Write-Host "Ora puoi controllare le modifiche su VS Code e fare Commit & Push!" -ForegroundColor Magenta

