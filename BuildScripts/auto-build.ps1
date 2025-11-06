# Automated local build script
param(
    [string]$BuildType = "development",
    [string]$Version = "0.0.1"
)

$UnityPath = "C:\Program Files\Unity\Hub\Editor\6000.2.10f1\Editor\Unity.exe"
$ProjectPath = Get-Location
$DateTime = Get-Date -Format "yyyyMMdd-HHmm"
$OutputFolder = "Builds\Build-$DateTime-$Version"

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Not All Neighbours - Local Build" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Build Type: $BuildType"
Write-Host "Version: $Version"
Write-Host "Output: $OutputFolder"

# Create output directory
New-Item -ItemType Directory -Force -Path $OutputFolder | Out-Null

# Set build method based on type
$BuildMethod = if ($BuildType -eq "release") {
    "BuildCommand.BuildWindows"
} else {
    "BuildCommand.BuildWindowsDevelopment"
}

# Run build
$buildArgs = @(
    "-batchmode",
    "-quit",
    "-projectPath", $ProjectPath,
    "-executeMethod", $BuildMethod,
    "-logFile", "$OutputFolder\build.log",
    "-buildTarget", "StandaloneWindows64"
)

Write-Host "`nStarting Unity build..." -ForegroundColor Yellow
$buildProcess = Start-Process -FilePath $UnityPath -ArgumentList $buildArgs -PassThru -Wait

if ($buildProcess.ExitCode -eq 0) {
    Write-Host "`nBUILD SUCCESSFUL!" -ForegroundColor Green
    
    # Create version file
    @{
        Version = $Version
        BuildDate = $DateTime
        BuildType = $BuildType
        GitCommit = (git rev-parse HEAD)
        GitBranch = (git rev-parse --abbrev-ref HEAD)
    } | ConvertTo-Json | Out-File "$OutputFolder\version.json"
    
    # Zip the build
    Write-Host "Creating ZIP archive..." -ForegroundColor Yellow
    Compress-Archive -Path "$OutputFolder\*" -DestinationPath "$OutputFolder.zip"
    
    Write-Host "Build package created: $OutputFolder.zip" -ForegroundColor Green
} else {
    Write-Host "`nBUILD FAILED!" -ForegroundColor Red
    Write-Host "Check log file: $OutputFolder\build.log" -ForegroundColor Red
    Get-Content "$OutputFolder\build.log" -Tail 50
}