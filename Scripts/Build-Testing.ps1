$ErrorActionPreference = "Stop"
Set-Location -Path $PSScriptRoot\..

$csprojPath = "AllaganTranslator.csproj"
$pluginmasterPath = "pluginmaster.json"

Write-Host "=== INIZIO BUILD DI TESTING ===" -ForegroundColor Cyan

# 1. Leggi e incrementa la versione nel .csproj
[xml]$csproj = Get-Content $csprojPath
$currentVersion = $csproj.Project.PropertyGroup[0].AssemblyVersion

$versionParts = $currentVersion.Split('.')
if ($versionParts.Length -eq 4) {
    $versionParts[3] = ([int]$versionParts[3] + 1).ToString()
    $newVersion = $versionParts -join "."
} else {
    $newVersion = $currentVersion + ".1"
}

Write-Host "Incremento versione: $currentVersion -> $newVersion" -ForegroundColor Yellow

$csproj.Project.PropertyGroup[0].Version = $newVersion
$csproj.Project.PropertyGroup[0].AssemblyVersion = $newVersion
$csproj.Project.PropertyGroup[0].FileVersion = $newVersion
$csproj.Save((Resolve-Path $csprojPath).Path)

# 2. Compila il progetto
Write-Host "Pulizia vecchie build..."
Remove-Item -Path "bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "obj" -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "Compilazione in corso..."
dotnet build -c Release

# 3. Genera latest_test.zip
$outputZip = "bin\Release\AllaganTranslator\latest.zip"
if (Test-Path $outputZip) {
    Write-Host "Aggiunta icona al pacchetto zip..."
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [System.IO.Compression.ZipFile]::Open($outputZip, 'Update')
    [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, "icon.png", "images/icon.png")
    $zip.Dispose()

    Copy-Item -Path $outputZip -Destination "latest_test.zip" -Force
    Write-Host "Pacchetto latest_test.zip generato con successo." -ForegroundColor Green
} else {
    Write-Error "Compilazione fallita, pacchetto zip non trovato."
    exit
}

# 4. Aggiorna pluginmaster.json
$json = Get-Content $pluginmasterPath -Raw | ConvertFrom-Json
$json[0].TestingAssemblyVersion = $newVersion

$jsonString = ConvertTo-Json -InputObject @($json) -Depth 10
Set-Content -Path $pluginmasterPath -Value $jsonString

Write-Host "pluginmaster.json aggiornato con TestingAssemblyVersion: $newVersion" -ForegroundColor Green
Write-Host "=== BUILD DI TESTING COMPLETATA ===" -ForegroundColor Cyan
Write-Host "Ora puoi controllare le modifiche su VS Code e fare Commit & Push!" -ForegroundColor Magenta

