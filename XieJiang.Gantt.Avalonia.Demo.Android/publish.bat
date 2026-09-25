@echo off
cd /d "%~dp0"

set OUTDIR=bin\Release\net10.0-android\android-arm64\publish

echo === [1/2] Cleaning obj (cache reset) ===
if exist obj rmdir /s /q obj

echo === [2/2] Publishing (Non-AOT) ===
dotnet publish -f net10.0-android -c Release -r android-arm64 -p:AndroidPackageFormat=apk -p:RunAOTCompilation=false -p:AndroidEnableProfiledAot=false

echo.
echo === Done. APK location: ===
echo   %OUTDIR%\com.CompanyName.XieJiang.Gantt.Avalonia.Demo-Signed.apk
pause
