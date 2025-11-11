#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Builds, tests, and publishes the Joker.Api NuGet package with comprehensive quality checks.

.DESCRIPTION
    This script performs the following steps:
    1. Builds the solution with no warnings, errors, or messages
    2. Runs all unit tests
    3. Collects code coverage data
    4. Checks Codacy for code quality issues
    5. Only publishes to NuGet if all checks pass perfectly

    The NuGet API key must be stored in a git-ignored file: nuget-key.txt

.EXAMPLE
    .\Publish.ps1
#>

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

# Colors for output
$script:Colors = @{
    Success = 'Green'
    Error   = 'Red'
    Warning = 'Yellow'
    Info    = 'Cyan'
    Step    = 'Magenta'
}

function Write-ColorOutput {
    param(
        [Parameter(Mandatory)]
        [string]$Message,
        
        [Parameter()]
        [string]$Color = 'White'
    )
    
    Write-Host $Message -ForegroundColor $Color
}

function Write-Step {
    param([Parameter(Mandatory)][string]$Message)
    Write-ColorOutput "`n▶ $Message" -Color $script:Colors.Step
}

function Write-Success {
    param([Parameter(Mandatory)][string]$Message)
    Write-ColorOutput "  ✓ $Message" -Color $script:Colors.Success
}

function Write-Failure {
    param([Parameter(Mandatory)][string]$Message)
    Write-ColorOutput "  ✗ $Message" -Color $script:Colors.Error
}

function Write-InfoMessage {
    param([Parameter(Mandatory)][string]$Message)
    Write-ColorOutput "  ℹ $Message" -Color $script:Colors.Info
}

function Test-Prerequisites {
    Write-Step "Checking prerequisites..."
    
    # Check if nuget-key.txt exists
    $nugetKeyPath = Join-Path $PSScriptRoot 'nuget-key.txt'
    if (-not (Test-Path $nugetKeyPath)) {
        Write-Failure "nuget-key.txt not found in repository root"
        Write-InfoMessage "Create a file named 'nuget-key.txt' containing your NuGet API key"
        Write-InfoMessage "This file is git-ignored for security"
        return $false
    }
    
    $script:NuGetApiKey = Get-Content $nugetKeyPath -Raw | ForEach-Object { $_.Trim() }
    if ([string]::IsNullOrWhiteSpace($script:NuGetApiKey)) {
        Write-Failure "nuget-key.txt is empty"
        return $false
    }
    
    Write-Success "NuGet API key found"
    
    # Check if codacy-key.txt exists
    $codacyKeyPath = Join-Path $PSScriptRoot 'codacy-key.txt'
    if (-not (Test-Path $codacyKeyPath)) {
        Write-Failure "codacy-key.txt not found in repository root"
        Write-InfoMessage "Create a file named 'codacy-key.txt' containing your Codacy project token"
        Write-InfoMessage "Get it from: https://app.codacy.com/project/settings"
        Write-InfoMessage "This file is git-ignored for security"
        return $false
    }
    
    $script:CodacyProjectToken = Get-Content $codacyKeyPath -Raw | ForEach-Object { $_.Trim() }
    if ([string]::IsNullOrWhiteSpace($script:CodacyProjectToken)) {
        Write-Failure "codacy-key.txt is empty"
        return $false
    }
    
    Write-Success "Codacy project token found"
    
    # Check if dotnet is available
    try {
        $dotnetVersion = dotnet --version
        Write-Success "dotnet CLI available (version $dotnetVersion)"
    }
    catch {
        Write-Failure "dotnet CLI not found"
        return $false
    }
    
    return $true
}

function Invoke-Build {
    Write-Step "Building solution with strict quality checks..."
    
    $buildOutput = dotnet build --configuration Release /p:TreatWarningsAsErrors=true 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Failure "Build failed"
        $buildOutput | Write-Host
        return $false
    }
    
    # Check for actual warnings (not just the word "warning" in summary)
    $warningLines = $buildOutput | Where-Object { 
        $_ -match ':\s+warning\s+[A-Z]+\d+:' 
    }
    
    $errorLines = $buildOutput | Where-Object { 
        $_ -match ':\s+error\s+[A-Z]+\d+:' 
    }
    
    # Check for compiler messages
    $messageLines = $buildOutput | Where-Object { 
        $_ -match ':\s+message\s+[A-Z]+\d+:'
    }
    
    if ($errorLines) {
        Write-Failure "Build has errors"
        $errorLines | Write-Host
        return $false
    }
    
    if ($warningLines) {
        Write-Failure "Build has warnings"
        $warningLines | Write-Host
        return $false
    }
    
    if ($messageLines) {
        Write-Failure "Build has compiler messages"
        $messageLines | Write-Host
        return $false
    }
    
    Write-Success "Build completed with zero warnings, errors, and messages"
    return $true
}

function Publish-Package {
    Write-Step "Publishing to NuGet..."
    
    # Find the .nupkg file
    $packagePath = Get-ChildItem -Path (Join-Path $PSScriptRoot 'Joker.Api\bin\Release') -Filter '*.nupkg' -Recurse | 
                   Sort-Object LastWriteTime -Descending | 
                   Select-Object -First 1
    
    if (-not $packagePath) {
        Write-Failure "No .nupkg file found in Release output"
        return $false
    }
    
    Write-InfoMessage "Package: $($packagePath.Name)"
    
    # Publish to NuGet
    try {
        $publishOutput = dotnet nuget push $packagePath.FullName --api-key $script:NuGetApiKey --source https://api.nuget.org/v3/index.json 2>&1
        
        if ($LASTEXITCODE -ne 0) {
            # Check if it's a duplicate version error
            if ($publishOutput -match 'already exists and cannot be modified') {
                Write-Failure "Package version already exists on NuGet"
                Write-InfoMessage "Increment the version number in Joker.Api.csproj before publishing"
                return $false
            }
            
            Write-Failure "NuGet publish failed"
            $publishOutput | Write-Host
            return $false
        }
        
        Write-Success "Package published to NuGet successfully!"
        Write-InfoMessage "View at: https://www.nuget.org/packages/Joker.Api"
        return $true
    }
    catch {
        Write-Failure "NuGet publish failed with exception: $_"
        return $false
    }
}

# Main execution
Write-ColorOutput "`n╔══════════════════════════════════════════════════════════╗" -Color $script:Colors.Step
Write-ColorOutput "║         Joker.Api NuGet Package Publisher              ║" -Color $script:Colors.Step
Write-ColorOutput "╚══════════════════════════════════════════════════════════╝" -Color $script:Colors.Step

try {
    # Step 1: Prerequisites
    if (-not (Test-Prerequisites)) {
        Write-ColorOutput "`n❌ Prerequisites check failed. Aborting publish.`n" -Color $script:Colors.Error
        exit 1
    }
    
    # Step 2: Build
    if (-not (Invoke-Build)) {
        Write-ColorOutput "`n❌ Build failed. Aborting publish.`n" -Color $script:Colors.Error
        exit 1
    }
    
    # Step 3-5: Run tests, coverage, and Codacy checks via Test.ps1
    Write-Step "Running Test.ps1 for tests, coverage, and quality checks..."
    
    $testScriptPath = Join-Path $PSScriptRoot 'Test.ps1'
    if (-not (Test-Path $testScriptPath)) {
        Write-Failure "Test.ps1 not found"
        exit 1
    }
    
    # Run Test.ps1 with the Codacy token
    & $testScriptPath -Configuration Release -CodacyToken $script:CodacyProjectToken
    
    if ($LASTEXITCODE -ne 0) {
        Write-ColorOutput "`n❌ Tests or quality checks failed. Aborting publish.`n" -Color $script:Colors.Error
        exit 1
    }
    
    Write-Success "All tests and quality checks passed!"
    
    # Step 6: Publish
    if (-not (Publish-Package)) {
        Write-ColorOutput "`n❌ Publishing failed.`n" -Color $script:Colors.Error
        exit 1
    }
    
    # Success!
    Write-ColorOutput "`n╔══════════════════════════════════════════════════════════╗" -Color $script:Colors.Success
    Write-ColorOutput "║              ✓ PUBLISH SUCCESSFUL!                      ║" -Color $script:Colors.Success
    Write-ColorOutput "╚══════════════════════════════════════════════════════════╝`n" -Color $script:Colors.Success
    
    exit 0
}
catch {
    Write-ColorOutput "`n❌ Unexpected error: $_`n" -Color $script:Colors.Error
    Write-ColorOutput $_.ScriptStackTrace -Color $script:Colors.Error
    exit 1
}
