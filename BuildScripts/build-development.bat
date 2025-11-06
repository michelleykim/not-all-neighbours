@echo off
echo ========================================
echo Building Not All Neighbours (Development)
echo ========================================

set UNITY_PATH="C:\Program Files\Unity\Hub\Editor\6000.2.10f1\Editor\Unity.exe"
set PROJECT_PATH=%cd%

echo Starting development build...
%UNITY_PATH% -batchmode ^
  -quit ^
  -projectPath "%PROJECT_PATH%" ^
  -executeMethod BuildCommand.BuildWindowsDevelopment ^
  -logFile build-dev.log ^
  -buildTarget StandaloneWindows64

if %ERRORLEVEL% EQU 0 (
  echo BUILD SUCCESSFUL!
  echo Output: Builds\Windows-Dev\
) else (
  echo BUILD FAILED!
  type build-dev.log
)

pause