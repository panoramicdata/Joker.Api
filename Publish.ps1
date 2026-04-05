#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Tags and pushes a release version for Joker.Api, triggering CI-based NuGet publishing.

.DESCRIPTION
    This script performs the following steps:
    1. Checks that the working tree is clean
    2. Checks that the current branch is main
    3. Fetches latest from origin
    4. Gets the version from nbgv
    5. Checks that the tag does not already exist
    6. Creates and pushes the version tag to trigger CI publishing

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

# Main execution
Write-ColorOutput "`n╔══════════════════════════════════════════════════════════╗" -Color $script:Colors.Step
Write-ColorOutput "║         Joker.Api NuGet Package Publisher              ║" -Color $script:Colors.Step
Write-ColorOutput "╚══════════════════════════════════════════════════════════╝" -Color $script:Colors.Step

try {
    # Step 1: Check working tree is clean
    Write-Step "Checking working tree is clean..."
    $status = git status --porcelain
    if ($status) {
        Write-Failure "Working tree is not clean. Commit or stash changes before publishing."
        $status | ForEach-Object { Write-InfoMessage $_ }
        exit 1
    }
    Write-Success "Working tree is clean"

    # Step 2: Check current branch is main
    Write-Step "Checking current branch..."
    $branch = git rev-parse --abbrev-ref HEAD
    if ($branch -ne 'main') {
        Write-Failure "Current branch is '$branch'. Switch to 'main' before publishing."
        exit 1
    }
    Write-Success "On branch main"

    # Step 3: Fetch latest from origin
    Write-Step "Fetching latest from origin..."
    git fetch origin main --quiet
    if ($LASTEXITCODE -ne 0) {
        Write-Failure "Failed to fetch from origin"
        exit 1
    }
    Write-Success "Fetched latest from origin"

    # Step 4: Get version from nbgv
    Write-Step "Getting version from nbgv..."
    $versionJson = nbgv get-version -f json | ConvertFrom-Json
    if ($LASTEXITCODE -ne 0) {
        Write-Failure "Failed to get version from nbgv"
        exit 1
    }
    $version = $versionJson.SimpleVersion
    Write-Success "Version: $version"

    # Step 5: Check tag does not already exist
    Write-Step "Checking if tag already exists..."
    $existingTag = git tag -l $version
    if ($existingTag) {
        Write-Failure "Tag '$version' already exists. Version has already been published."
        exit 1
    }
    Write-Success "Tag '$version' does not exist"

    # Step 6: Create and push tag
    Write-Step "Creating tag '$version'..."
    git tag $version
    if ($LASTEXITCODE -ne 0) {
        Write-Failure "Failed to create tag"
        exit 1
    }
    Write-Success "Tag created"

    Write-Step "Pushing tag '$version' to origin..."
    git push origin $version
    if ($LASTEXITCODE -ne 0) {
        Write-Failure "Failed to push tag"
        exit 1
    }
    Write-Success "Tag pushed to origin"

    # Success!
    Write-ColorOutput "`n╔══════════════════════════════════════════════════════════╗" -Color $script:Colors.Success
    Write-ColorOutput "║              ✓ TAG PUSHED SUCCESSFULLY!                 ║" -Color $script:Colors.Success
    Write-ColorOutput "║     CI will now build and publish to NuGet.             ║" -Color $script:Colors.Success
    Write-ColorOutput "╚══════════════════════════════════════════════════════════╝`n" -Color $script:Colors.Success

    exit 0
}
catch {
    Write-ColorOutput "`n❌ Unexpected error: $_`n" -Color $script:Colors.Error
    Write-ColorOutput $_.ScriptStackTrace -Color $script:Colors.Error
    exit 1
}
