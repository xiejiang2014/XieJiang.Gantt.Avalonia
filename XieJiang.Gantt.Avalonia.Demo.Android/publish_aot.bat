@echo off
cd /d "%~dp0"

set OUTDIR=bin\Release\net10.0-android\android-arm64\publish\aot

echo === [1/3] Cleaning obj (cache reset) ===
if exist obj rmdir /s /q obj

echo === [2/3] Cleaning previous AOT output ===
if exist "%OUTDIR%" rmdir /s /q "%OUTDIR%"

echo === [3/3] Publishing (Profiled AOT) ===
dotnet publish -f net10.0-android -c Release -r android-arm64 -p:AndroidPackageFormat=apk -p:RunAOTCompilation=true -p:AndroidEnableProfiledAot=true -o "%OUTDIR%"

echo.
echo === Done. APK location: ===
echo   %OUTDIR%\com.CompanyName.XieJiang.Gantt.Avalonia.Demo-Signed.apk
pause
