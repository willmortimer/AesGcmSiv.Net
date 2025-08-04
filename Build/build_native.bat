@echo off
setlocal

echo Building AES-GCM-SIV Native Library...

REM Set up Visual Studio environment
call "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\VC\Auxiliary\Build\vcvars64.bat"

REM Set paths
set NATIVE_DIR=%~dp0..\Native
set OUTPUT_DIR=%~dp0..\bin\native\win-x64
set OPENSSL_DIR=%~dp0..\OpenSSL-Win64

REM Create output directory
if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"

REM Navigate to native directory
cd /d "%NATIVE_DIR%"

REM Build the DLL
echo Building with MSVC...
cl /LD /Fe:aesgcmsiv.dll /I"%OPENSSL_DIR%\include" "%OPENSSL_DIR%\lib\VC\x64\MD\libcrypto.lib" aesgcmsiv.cpp

if %ERRORLEVEL% neq 0 (
    echo Build failed
    exit /b 1
)

REM Copy the DLL to output directory
if exist aesgcmsiv.dll (
    copy aesgcmsiv.dll "%OUTPUT_DIR%\" >nul
    echo Build completed successfully!
    echo DLL location: %OUTPUT_DIR%
    
    REM Also copy to test output directories for immediate testing
    if exist "..\AesGcmSiv.Tests\bin\Release\net9.0\" (
        copy aesgcmsiv.dll "..\AesGcmSiv.Tests\bin\Release\net9.0\" >nul
        echo DLL copied to test directory
    )
    if exist "..\AesGcmSiv.Tests\bin\Debug\net9.0\" (
        copy aesgcmsiv.dll "..\AesGcmSiv.Tests\bin\Debug\net9.0\" >nul
        echo DLL copied to test directory
    )
) else (
    echo DLL not found after build
    exit /b 1
)

exit /b 0 