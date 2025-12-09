# Runs unit tests with code coverage and generates an HTML report
param(
    [string]$TestProjectPath = "./SMARTCAMPUS.Tests/SMARTCAMPUS.Tests.csproj",
    [string]$ReportDir = "coverage-report"
)

# Backup existing coverage history directory (if any)
$histBackup = "$env:TEMP\cov_history_$(Get-Random)"
if (Test-Path "$ReportDir/history") {
    Move-Item "$ReportDir/history" $histBackup
}

# Clean current coverage report directory
if (Test-Path $ReportDir) {
    Remove-Item -Recurse -Force $ReportDir
}
New-Item -Path $ReportDir -ItemType Directory | Out-Null

# Restore previous coverage history
if (Test-Path $histBackup) {
    Move-Item $histBackup "$ReportDir/history"
}

# NOTE: The TestResults folder is not deleted here; coverage XML files are
# preserved for coverage history charts.

Write-Host "Running tests with coverage: $TestProjectPath..." -ForegroundColor Cyan

$resultDir = "$(Split-Path $TestProjectPath)/TestResults"

# Updated exclusions to match requirements (Exclude Migrations, DataSeeder, Program, Context)
dotnet test $TestProjectPath --collect:"XPlat Code Coverage" --results-directory $resultDir -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format="cobertura" DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.ExcludeByFile="**/DataSeeder.cs, **/Mappings/**,**/Migrations/**/*.cs,**/*.Designer.cs,**/*.g*.cs,**/Program.cs,**/*Program.cs,**/Context/*.cs" DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Exclude="[*]*Program,[*.Tests]*"

$coverageFile = Get-ChildItem -Path $resultDir -Recurse -Filter "coverage.cobertura.xml" |
                Sort-Object LastWriteTime -Descending |
                Select-Object -First 1

if (-not $coverageFile) {
    Write-Error "Coverage file not found. Make sure the 'coverlet.collector' package is installed in the test project."
    exit 1
}

Write-Host "`nGenerating HTML coverage report..." -ForegroundColor Cyan
$historyDir = Join-Path $ReportDir "history"

# Ensure ReportGenerator tool is installed
if (-not (Get-Command reportgenerator -ErrorAction SilentlyContinue)) {
    Write-Host "ReportGenerator tool is not installed. Installing..." -ForegroundColor Yellow
    dotnet tool install -g dotnet-reportgenerator-globaltool
}

reportgenerator -reports:$coverageFile.FullName -targetdir:$ReportDir -reporttypes:Html -historydir:$historyDir
Write-Host "`nCoverage report successfully generated: $ReportDir/index.html" -ForegroundColor Green
Write-Host "To open the report: start $ReportDir/index.html" -ForegroundColor Yellow

# Cleanup TestResults directory
if (Test-Path $resultDir) {
    Remove-Item -Recurse -Force $resultDir
    Write-Host "`nTestResults directory has been cleaned up." -ForegroundColor Gray
}
