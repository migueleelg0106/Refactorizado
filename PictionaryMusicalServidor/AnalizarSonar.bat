@echo off
title Analisis SonarQube - PictionaryMusical
color 0A

echo ============================================
echo   CONVERTIR ARCHIVOS .CS A UTF-8 SIN BOM
echo ============================================
echo.

REM Ejecutar PowerShell para convertir los .cs
powershell -ExecutionPolicy Bypass -Command ^
"Write-Host 'Iniciando conversion...' -ForegroundColor Cyan; ^
$files = Get-ChildItem -Recurse -Filter *.cs; ^
foreach ($file in $files) { ^
    $content = Get-Content $file.FullName -Raw; ^
    [System.IO.File]::WriteAllText($file.FullName, $content, (New-Object System.Text.UTF8Encoding $false)); ^
    Write-Host ('Convertido: ' + $file.FullName) -ForegroundColor Green; ^
}; ^
Write-Host 'Conversion lista!' -ForegroundColor Yellow"

echo.
echo ============================================
echo        INICIANDO ANALISIS SONARQUBE
echo ============================================
echo.

REM RUTA DEL SCANNER PARA .NET FRAMEWORK
set SCANNER="C:\Users\Usuario\Downloads\SonarNetframework\SonarScanner.MSBuild.exe"

REM TOKEN Y SERVIDOR
set TOKEN=sqp_35c0dc38dbc8d863e3d833570b3a1e34cb6e1612
set HOST=http://localhost:9000

%SCANNER% begin /k:"PictionaryMusical" /d:sonar.host.url="%HOST%" /d:sonar.token="%TOKEN%" /d:sonar.scm.disabled=true

echo.
echo ============================================
echo     COMPILANDO SOLUCION CON MSBUILD
echo ============================================
echo.

msbuild PictionaryMusicalServidor.sln /t:Rebuild

echo.
echo ============================================
echo     FINALIZANDO ANALISIS SONARQUBE
echo ============================================
echo.

%SCANNER% end /d:sonar.token="%TOKEN%"

echo.
echo ================================
echo   ANALISIS COMPLETADO
echo   Abrir SonarQube en:
echo   http://localhost:9000
echo ================================
pause
