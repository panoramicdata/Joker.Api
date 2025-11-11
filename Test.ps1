#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Runs tests with code coverage and uploads results to Codacy.

.DESCRIPTION
    This script performs comprehensive testing:
    1. Runs all unit tests
    2. Collects code coverage
    3. Uploads coverage to Codacy (if token provided)
    4. Checks Codacy for code quality issues

.PARAMETER CodacyToken
    Optional Codacy project token. If not provided, will look for codacy-key.txt

.PARAMETER SkipCodacy
    Skip Codacy checks and coverage upload

.PARAMETER Configuration
    Build configuration (Debug or Release). Default is Release.

.EXAMPLE
    .\Test.ps1

.EXAMPLE
    .\Test.ps1 -SkipCodacy

.EXAMPLE
    .\Test.ps1 -CodacyToken "your-token-here"
#>

[CmdletBinding()]
param(
    [Parameter()]
    [string]$CodacyToken,
    
    [Parameter()]
    [switch]$SkipCodacy,
    
    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

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

function Get-CodacyToken {
    if (-not [string]::IsNullOrWhiteSpace($CodacyToken)) {
        return $CodacyToken
    }
    
    $codacyKeyPath = Join-Path $PSScriptRoot 'codacy-key.txt'
    if (Test-Path $codacyKeyPath) {
        $token = Get-Content $codacyKeyPath -Raw | ForEach-Object { $_.Trim() }
        if (-not [string]::IsNullOrWhiteSpace($token)) {
            return $token
        }
    }
    
    return $null
}

function Invoke-Tests {
    Write-Step "Running all unit tests..."
    
    $testOutput = dotnet test --configuration $Configuration --no-build --logger "console;verbosity=minimal" 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Failure "Tests failed"
        $testOutput | Write-Host
        return $false
    }
    
    # Extract test results
    $passedMatch = $testOutput | Select-String -Pattern 'Passed:\s+(\d+)'
    $failedMatch = $testOutput | Select-String -Pattern 'Failed:\s+(\d+)'
    
    if ($passedMatch) {
        $passed = [int]$passedMatch.Matches[0].Groups[1].Value
        Write-Success "All tests passed ($passed tests)"
    }
    
    if ($failedMatch) {
        $failed = [int]$failedMatch.Matches[0].Groups[1].Value
        if ($failed -gt 0) {
            Write-Failure "$failed test(s) failed"
            return $false
        }
    }
    
    return $true
}

function Invoke-CodeCoverage {
    Write-Step "Collecting code coverage..."
    
    $coverageOutput = dotnet test --configuration $Configuration --no-build --collect:"XPlat Code Coverage" --logger "console;verbosity=quiet" 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Failure "Code coverage collection failed"
        $coverageOutput | Write-Host
        return $null
    }
    
    # Find the coverage file
    $coverageFile = Get-ChildItem -Path (Join-Path $PSScriptRoot 'Joker.Api.Test\TestResults') -Filter "coverage.cobertura.xml" -Recurse | 
                    Sort-Object LastWriteTime -Descending | 
                    Select-Object -First 1
    
    if ($coverageFile) {
        Write-Success "Code coverage collected"
        Write-InfoMessage "Coverage file: $($coverageFile.FullName)"
        
        # Parse coverage percentage
        [xml]$coverage = Get-Content $coverageFile.FullName
        $lineRate = [double]$coverage.coverage.'line-rate' * 100
        $branchRate = [double]$coverage.coverage.'branch-rate' * 100
        
        Write-InfoMessage "Line coverage: $($lineRate.ToString('F2'))%"
        Write-InfoMessage "Branch coverage: $($branchRate.ToString('F2'))%"
        
        return $coverageFile.FullName
    }
    else {
        Write-Failure "Code coverage file not found"
        return $null
    }
}

function Send-CoverageToCodacy {
    param(
        [Parameter(Mandatory)]
        [string]$CoverageFilePath,
        
        [Parameter(Mandatory)]
        [string]$Token
    )
    
    Write-Step "Uploading coverage to Codacy..."
    
    try {
        # Get git commit hash
        $commit = git rev-parse HEAD 2>$null
        if (-not $commit) {
            Write-Failure "Could not determine git commit hash"
            return $false
        }
        
        Write-InfoMessage "Commit: $commit"
        
        # Upload using Codacy coverage reporter
        # Check if codacy-coverage-reporter is installed
        $reporterPath = "codacy-coverage-reporter"
        
        # Try to download and use the reporter
        $reporterUrl = "https://github.com/codacy/codacy-coverage-reporter/releases/latest/download/codacy-coverage-reporter-assembly.jar"
        $reporterJar = Join-Path $PSScriptRoot "codacy-coverage-reporter.jar"
        
        if (-not (Test-Path $reporterJar)) {
            Write-InfoMessage "Downloading Codacy coverage reporter..."
            Invoke-WebRequest -Uri $reporterUrl -OutFile $reporterJar -ErrorAction Stop
            Write-Success "Coverage reporter downloaded"
        }
        
        # Upload coverage
        Write-InfoMessage "Uploading coverage report..."
        $uploadOutput = java -jar $reporterJar report `
            --project-token $Token `
            --coverage-reports $CoverageFilePath `
            --commit-uuid $commit 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Coverage uploaded to Codacy successfully!"
            return $true
        }
        else {
            Write-Failure "Coverage upload failed"
            $uploadOutput | Write-Host
            return $false
        }
    }
    catch {
        Write-Failure "Coverage upload error: $($_.Exception.Message)"
        Write-InfoMessage "You can manually upload at: https://app.codacy.com/coverage/upload"
        return $false
    }
}

function Test-Codacy {
    param(
        [Parameter(Mandatory)]
        [string]$Token
    )
    
    Write-Step "Checking Codacy for code quality issues..."
    
    # Check if we have an API token (not project token)
    $apiKeyPath = Join-Path $PSScriptRoot 'codacy-api-key.txt'
    if (-not (Test-Path $apiKeyPath)) {
        Write-InfoMessage "Note: Codacy project tokens can only upload coverage, not query issues."
        Write-InfoMessage "To enable automated issue checking, you need an Account API Token."
        Write-InfoMessage "Get it from: https://app.codacy.com/account/apiTokens"
        Write-InfoMessage "Then add it to codacy-api-key.txt (different from project token)"
        Write-InfoMessage "Skipping Codacy issue check - no API token found"
        Write-InfoMessage "Coverage has been uploaded, but we cannot verify issue count"
        return $true  # Don't fail if API key not provided
    }
    
    $apiToken = Get-Content $apiKeyPath -Raw | ForEach-Object { $_.Trim() }
    if ([string]::IsNullOrWhiteSpace($apiToken)) {
        Write-InfoMessage "Note: Codacy project tokens can only upload coverage, not query issues."
        Write-InfoMessage "To enable automated issue checking, you need an Account API Token."
        Write-InfoMessage "Get it from: https://app.codacy.com/account/apiTokens"
        Write-InfoMessage "Skipping Codacy issue check - codacy-api-key.txt is empty"
        return $true
    }
    
    try {
        $headers = @{
            'api-token' = $apiToken
            'Accept' = 'application/json'
        }
        
        # Get repository info
        $gitRemote = git remote get-url origin 2>$null
        if (-not $gitRemote) {
            Write-Failure "Could not determine git remote"
            return $false
        }
        
        # Parse GitHub URL
        if ($gitRemote -match 'github\.com[:/]([^/]+)/(.+?)(\.git)?$') {
            $username = $matches[1].ToLowerInvariant()  # Codacy uses lowercase
            $project = $matches[2]
            $provider = 'gh'
        }
        else {
            Write-Failure "Could not parse repository URL: $gitRemote"
            return $false
        }
        
        Write-InfoMessage "Checking Codacy for: $provider/$username/$project"
        
        # Get current branch
        $branch = git rev-parse --abbrev-ref HEAD 2>$null
        if (-not $branch) {
            $branch = 'main'
        }
        
        Write-InfoMessage "Branch: $branch"
        
        # Query Codacy API v3 for issues (requires POST with branch name)
        $issuesUrl = "https://api.codacy.com/api/v3/analysis/organizations/$provider/$username/repositories/$project/issues/search"
        
        $requestBody = @{
            branchName = $branch
        } | ConvertTo-Json
        
        $issuesResponse = Invoke-RestMethod -Uri $issuesUrl -Headers $headers -Method Post -Body $requestBody -ContentType 'application/json' -ErrorAction Stop
        
        # Check for issues
        if ($issuesResponse.data -and $issuesResponse.data.Count -gt 0) {
            # Group by severity
            $critical = $issuesResponse.data | Where-Object { $_.patternInfo.level -eq 'Error' -or $_.patternInfo.severityLevel -eq 'High' }
            $warnings = $issuesResponse.data | Where-Object { $_.patternInfo.level -eq 'Warning' -or $_.patternInfo.severityLevel -eq 'Warning' }
            $info = $issuesResponse.data | Where-Object { $_.patternInfo.level -eq 'Info' }
            
            Write-InfoMessage "Found $($issuesResponse.data.Count) code quality issues in Codacy"
            
            if ($critical) {
                Write-Failure "  Critical/High issues: $($critical.Count)"
                $critical | Select-Object -First 10 | ForEach-Object {
                    Write-ColorOutput "    - $($_.filePath):$($_.lineNumber) - $($_.message)" -Color $script:Colors.Error
                }
                if ($critical.Count -gt 10) {
                    Write-InfoMessage "    ... and $($critical.Count - 10) more critical issues"
                }
                Write-InfoMessage "View all issues: https://app.codacy.com/$provider/$username/$project/issues"
                return $false  # Fail on critical issues
            }
            
            if ($warnings) {
                Write-InfoMessage "  Warnings: $($warnings.Count) (acceptable)"
                $warnings | Select-Object -First 5 | ForEach-Object {
                    Write-InfoMessage "    - $($_.filePath):$($_.lineNumber) - $($_.message)"
                }
            }
            
            if ($info) {
                Write-InfoMessage "  Info: $($info.Count) (acceptable)"
            }
            
            Write-InfoMessage "View all issues: https://app.codacy.com/$provider/$username/$project/issues"
            
            # Pass if only warnings/info, fail if critical
            if (!$critical) {
                Write-Success "No critical issues - quality check passed"
                return $true
            }
        }
        
        Write-Success "No code quality issues found in Codacy!"
        return $true
    }
    catch {
        $errorMessage = $_.Exception.Message
        
        if ($errorMessage -match '401|Unauthorized|Bad credentials') {
            Write-Failure "Codacy authentication failed - invalid API token"
            Write-InfoMessage "Make sure codacy-api-key.txt contains a valid Account API Token"
            Write-InfoMessage "Get it from: https://app.codacy.com/account/apiTokens"
            return $false
        }
        
        if ($errorMessage -match '404|Not Found|not found') {
            Write-InfoMessage "Codacy project not found or not yet analyzed"
            Write-InfoMessage "Project: $provider/$username/$project"
            Write-InfoMessage "Coverage uploaded successfully - Codacy may need time to analyze"
            return $true  # Don't fail on first push
        }
        
        Write-Failure "Codacy check failed: $errorMessage"
        Write-InfoMessage "Stack: $($_.ScriptStackTrace)"
        return $false
    }
}

# Main execution
Write-ColorOutput "`n╔══════════════════════════════════════════════════════════╗" -Color $script:Colors.Step
Write-ColorOutput "║              Joker.Api Test Runner                     ║" -Color $script:Colors.Step
Write-ColorOutput "╚══════════════════════════════════════════════════════════╝" -Color $script:Colors.Step

$script:TestsPassed = $false
$script:CoveragePath = $null
$script:CodacyPassed = $true

try {
    # Run tests
    if (-not (Invoke-Tests)) {
        Write-ColorOutput "`n❌ Tests failed. Aborting.`n" -Color $script:Colors.Error
        exit 1
    }
    $script:TestsPassed = $true
    
    # Collect coverage
    $script:CoveragePath = Invoke-CodeCoverage
    if (-not $script:CoveragePath) {
        Write-ColorOutput "`n❌ Code coverage collection failed. Aborting.`n" -Color $script:Colors.Error
        exit 1
    }
    
    # Codacy integration (if not skipped)
    if (-not $SkipCodacy) {
        $token = Get-CodacyToken
        
        if ($token) {
            # Upload coverage to Codacy
            $uploadSuccess = Send-CoverageToCodacy -CoverageFilePath $script:CoveragePath -Token $token
            if (-not $uploadSuccess) {
                Write-ColorOutput "`n⚠️  Coverage upload to Codacy failed (continuing anyway)`n" -Color $script:Colors.Warning
            }
            
            # Check Codacy for issues
            $script:CodacyPassed = Test-Codacy -Token $token
            if (-not $script:CodacyPassed) {
                Write-ColorOutput "`n❌ Codacy quality check failed. Fix issues before publishing.`n" -Color $script:Colors.Error
                exit 1
            }
        }
        else {
            Write-InfoMessage "`nNo Codacy token found - skipping Codacy checks"
            Write-InfoMessage "Create codacy-key.txt or set -CodacyToken parameter to enable"
        }
    }
    
    # Success!
    Write-ColorOutput "`n╔══════════════════════════════════════════════════════════╗" -Color $script:Colors.Success
    Write-ColorOutput "║              ✓ ALL TESTS PASSED!                        ║" -Color $script:Colors.Success
    Write-ColorOutput "╚══════════════════════════════════════════════════════════╝`n" -Color $script:Colors.Success
    
    exit 0
}
catch {
    Write-ColorOutput "`n❌ Unexpected error: $_`n" -Color $script:Colors.Error
    Write-ColorOutput $_.ScriptStackTrace -Color $script:Colors.Error
    exit 1
}
