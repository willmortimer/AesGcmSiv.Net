# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

**Build native library only:**
```powershell
.\Build\build_native.bat
```

**Full build with PowerShell (recommended):**
```powershell
.\Build\build.ps1 -Configuration Release
```

**Clean build:**
```powershell
.\Build\build.ps1 -Configuration Release -Clean
```

**Build .NET projects only (skip native):**
```powershell
.\Build\build.ps1 -SkipNative
dotnet build --configuration Release
```

**Run tests:**
```powershell
dotnet test --configuration Release
```

**Create NuGet package:**
```powershell
dotnet pack --configuration Release --output ./nupkgs
```

**Code formatting:**
```powershell
dotnet format
```

## Architecture Overview

This is a .NET cryptographic library that provides AES-GCM-SIV encryption through a hybrid native/managed design:

### Native Layer (`Native/`)
- **aesgcmsiv.cpp**: C++ wrapper around OpenSSL's AES-GCM-SIV implementation
- **aesgcmsiv.h**: Clean C ABI for P/Invoke interop
- Uses OpenSSL 3.x statically linked for cryptographic operations
- Compiles to `aesgcmsiv.dll` for Windows x64

### .NET Layer (`AesGcmSiv.Net/`)
- **AesGcmSiv.cs**: Main .NET API following System.Security.Cryptography patterns
- P/Invoke calls to native DLL functions
- Memory management with IDisposable pattern
- Comprehensive parameter validation and error handling

### Key Design Decisions
- **Static linking**: Native DLL includes required OpenSSL routines to avoid dependencies
- **P/Invoke interop**: Direct calls with minimal marshaling overhead
- **Error mapping**: Native error codes mapped to .NET exceptions
- **Single DLL distribution**: Native library embedded in NuGet package

### Build System
- **MSBuild**: .NET projects use standard SDK-style projects
- **Visual Studio Build Tools**: Required for native compilation
- **CMake**: Available for native builds but batch script is primary method
- **OpenSSL**: Pre-included in `OpenSSL-Win64/` directory

## Project Structure

```
AesGcmSiv.Net/                 # Main library project
├── Crypto/AesGcmSiv.cs        # .NET API implementation
AesGcmSiv.Tests/               # xUnit test project
Native/                        # C++ native layer
├── aesgcmsiv.cpp             # OpenSSL wrapper
├── aesgcmsiv.h               # C ABI header
├── CMakeLists.txt            # CMake configuration
Build/                         # Build scripts
├── build.ps1                 # Main PowerShell build script
├── build_native.bat          # Native-only build script
OpenSSL-Win64/                 # Pre-built OpenSSL for Windows x64
.github/workflows/build.yml    # CI/CD pipeline
```

## Development Notes

### Native Development
- Native DLL must be built before running .NET tests
- DLL automatically copied to test output directories by build scripts
- OpenSSL path configured via `OPENSSL_ROOT_DIR` environment variable

### Testing Framework
- Uses xUnit for unit testing
- Tests require native DLL to be present in output directory
- Comprehensive test coverage including parameter validation and error conditions

### NuGet Packaging
- Native DLL embedded in package under `runtimes/win-x64/native/`
- Symbols package (.snupkg) generated for debugging
- Package includes README.md and follows standard metadata conventions

### Code Quality
- Nullable reference types enabled project-wide
- .NET analyzers and security rules enabled
- Documentation XML files generated
- Format verification in CI pipeline