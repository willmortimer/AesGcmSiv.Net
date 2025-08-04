#!/usr/bin/env pwsh

param(
    [string]$Configuration = "Release",
    [string]$Platform = "x64",
    [switch]$Clean,
    [switch]$SkipNative
)

# Error handling
$ErrorActionPreference = "Stop"

# Script variables
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir
$NativeDir = Join-Path $ProjectRoot "Native"
$BuildDir = Join-Path $ProjectRoot "Build"
$OutputDir = Join-Path $ProjectRoot "bin"

# Create output directories
$NativeOutputDir = Join-Path $OutputDir "native"
$WinX64Dir = Join-Path $NativeOutputDir "win-x64"

if (!(Test-Path $WinX64Dir)) {
    New-Item -ItemType Directory -Path $WinX64Dir -Force | Out-Null
}

Write-Host "Building AES-GCM-SIV Native Library" -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Platform: $Platform" -ForegroundColor Yellow

# Check prerequisites
function Test-Prerequisites {
    Write-Host "Checking prerequisites..." -ForegroundColor Cyan
    
    # Check for CMake
    try {
        $cmakeVersion = cmake --version 2>&1 | Select-Object -First 1
        Write-Host "✓ CMake found: $cmakeVersion" -ForegroundColor Green
    }
    catch {
        Write-Error "CMake not found. Please install CMake and add it to PATH."
        exit 1
    }
    
    # Check for Visual Studio Build Tools or MSVC
    try {
        $clVersion = cl 2>&1 | Select-Object -First 1
        Write-Host "✓ MSVC compiler found" -ForegroundColor Green
    }
    catch {
        Write-Error "MSVC compiler not found. Please install Visual Studio Build Tools."
        exit 1
    }
    
    # Check for OpenSSL (we'll download if not found)
    $opensslPath = Get-Command openssl -ErrorAction SilentlyContinue
    if ($opensslPath) {
        Write-Host "✓ OpenSSL found in PATH" -ForegroundColor Green
    } else {
        Write-Host "! OpenSSL not found in PATH, will download and build" -ForegroundColor Yellow
    }
}

# Use pre-installed OpenSSL
function Get-OpenSSLPath {
    Write-Host "Using pre-installed OpenSSL..." -ForegroundColor Cyan
    
    $opensslPath = Join-Path $ProjectRoot "OpenSSL-Win64"
    
    if (!(Test-Path $opensslPath)) {
        Write-Error "OpenSSL-Win64 directory not found at: $opensslPath"
        exit 1
    }
    
    Write-Host "✓ OpenSSL found at: $opensslPath" -ForegroundColor Green
    return $opensslPath
}

# Build native library
function Build-NativeLibrary {
    param([string]$OpenSSLPath)
    
    Write-Host "Building native library..." -ForegroundColor Cyan
    
    $cmakeBuildDir = Join-Path $BuildDir "cmake-build"
    if (!(Test-Path $cmakeBuildDir)) {
        New-Item -ItemType Directory -Path $cmakeBuildDir -Force | Out-Null
    }
    
    # Configure CMake
    Push-Location $cmakeBuildDir
    try {
        $cmakeArgs = @(
            "-G", "Visual Studio 17 2022",
            "-A", $Platform,
            "-DCMAKE_BUILD_TYPE=$Configuration",
            "-DOPENSSL_ROOT_DIR=$OpenSSLPath",
            "-DOPENSSL_USE_STATIC_LIBS=ON",
            $NativeDir
        )
        
        & cmake @cmakeArgs
        
        if ($LASTEXITCODE -ne 0) {
            Write-Error "CMake configuration failed"
            exit 1
        }
        
        # Build
        & cmake --build . --config $Configuration
        
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Native library build failed"
            exit 1
        }
        
        # Copy output
        $dllSource = Join-Path $cmakeBuildDir "$Configuration\aesgcmsiv.dll"
        if (Test-Path $dllSource) {
            Copy-Item $dllSource $WinX64Dir -Force
            Write-Host "✓ Native library built successfully" -ForegroundColor Green
        } else {
            Write-Error "Native library not found at expected location: $dllSource"
            exit 1
        }
    }
    finally {
        Pop-Location
    }
}

# Build .NET projects
function Build-DotNetProjects {
    Write-Host "Building .NET projects..." -ForegroundColor Cyan
    
    Push-Location $ProjectRoot
    try {
        # Restore packages
        & dotnet restore
        
        # Build main project
        & dotnet build AesGcmSiv.Net.sln --configuration $Configuration
        
        if ($LASTEXITCODE -ne 0) {
            Write-Error ".NET build failed"
            exit 1
        }
        
        Write-Host "✓ .NET projects built successfully" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
}

# Run tests
function Test-Project {
    Write-Host "Running tests..." -ForegroundColor Cyan
    
    Push-Location $ProjectRoot
    try {
        & dotnet test AesGcmSiv.Tests --configuration $Configuration --no-build
        
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Tests failed"
            exit 1
        }
        
        Write-Host "✓ Tests passed" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
}

# Main execution
try {
    # Check prerequisites
    Test-Prerequisites
    
    # Clean if requested
    if ($Clean) {
        Write-Host "Cleaning build artifacts..." -ForegroundColor Yellow
        if (Test-Path $OutputDir) {
            Remove-Item $OutputDir -Recurse -Force
        }
        if (Test-Path (Join-Path $BuildDir "cmake-build")) {
            Remove-Item (Join-Path $BuildDir "cmake-build") -Recurse -Force
        }
    }
    
    # Build native library
    if (!$SkipNative) {
        $opensslPath = Get-OpenSSLPath
        Build-NativeLibrary -OpenSSLPath $opensslPath
    } else {
        Write-Host "Skipping native build as requested" -ForegroundColor Yellow
    }
    
    # Build .NET projects
    Build-DotNetProjects
    
    # Run tests
    Test-Project
    
    Write-Host "Build completed successfully!" -ForegroundColor Green
    Write-Host "Native library location: $WinX64Dir" -ForegroundColor Cyan
}
catch {
    Write-Error "Build failed: $($_.Exception.Message)"
    exit 1
}
