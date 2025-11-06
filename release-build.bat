@echo off
echo =======================================
echo    NOT ALL NEIGHBOURS - RELEASE BUILD
echo =======================================
echo.

:: Get version from user
set /p VERSION="Enter version number (e.g., 1.0.0): "

:: Build paths
set OUTPUT_DIR=Releases\v%VERSION%
set BUILD_NAME=NotAllNeighbours-v%VERSION%-Windows

:: Clean and create directory
if exist %OUTPUT_DIR% rd /s /q %OUTPUT_DIR%
mkdir %OUTPUT_DIR%

:: Build the game
echo Building version %VERSION%...
call BuildScripts\build-windows.bat

:: Copy to release folder
echo Copying to release folder...
xcopy /E /I Builds\Windows\* "%OUTPUT_DIR%\%BUILD_NAME%"

:: Create README
echo Not All Neighbours > "%OUTPUT_DIR%\README.txt"
echo Version: %VERSION% >> "%OUTPUT_DIR%\README.txt"
echo Build Date: %date% %time% >> "%OUTPUT_DIR%\README.txt"

:: Zip for distribution
echo Creating distribution package...
powershell Compress-Archive -Path "%OUTPUT_DIR%\*" -DestinationPath "%OUTPUT_DIR%.zip"

echo.
echo =======================================
echo    RELEASE BUILD COMPLETE!
echo    Package: %OUTPUT_DIR%.zip
echo =======================================
pause