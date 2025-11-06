@echo off
echo ========================================
echo Building Not All Neighbours for Windows
echo ========================================

set UNITY_PATH="C:\Program Files\Unity\Hub\Editor\6000.2.10f1\Editor\Unity.exe"
set PROJECT_PATH=%cd%
set BUILD_TARGET=StandaloneWindows64
set OUTPUT_PATH=Builds\Windows\NotAllNeighbours.exe

echo Unity: %UNITY_PATH%
echo Project: %PROJECT_PATH%
echo Output: %OUTPUT_PATH%
echo.

echo Cleaning previous build...
if exist Builds\Windows rd /s /q Builds\Windows
mkdir Builds\Windows

echo Starting build...
%UNITY_PATH% -batchmode ^
  -quit ^
  -projectPath "%PROJECT_PATH%" ^
  -executeMethod BuildCommand.BuildWindows ^
  -logFile build.log ^
  -buildTarget %BUILD_TARGET%

if %ERRORLEVEL% EQU 0 (
  echo.
  echo ========================================
  echo BUILD SUCCESSFUL!
  echo Output: %OUTPUT_PATH%
  echo ========================================
) else (
  echo.
  echo ========================================
  echo BUILD FAILED! Check build.log for details
  echo ========================================
  type build.log
)

pause