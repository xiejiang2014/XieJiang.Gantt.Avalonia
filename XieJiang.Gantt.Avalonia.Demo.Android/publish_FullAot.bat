@echo off
cd /d "%~dp0"

set OUTDIR=bin\Release\net10.0-android\android-arm64\publish\full_aot

echo === [1/3] Cleaning obj (cache reset) ===
if exist obj rmdir /s /q obj

echo === [2/3] Cleaning previous Full AOT output ===
if exist "%OUTDIR%" rmdir /s /q "%OUTDIR%"

echo === [3/3] Publishing (Full AOT) ===
dotnet publish -f net10.0-android -c Release -r android-arm64 -p:AndroidPackageFormat=apk -p:RunAOTCompilation=true -p:AndroidEnableProfiledAot=false -o "%OUTDIR%"

echo.
echo === Done. APK location: ===
echo   %OUTDIR%\com.CompanyName.XieJiang.Gantt.Avalonia.Demo-Signed.apk
echo.
echo WARNING: Full AOT may break reflection-heavy code (Avalonia XAML).
echo Test thoroughly before shipping to customers.
pause
