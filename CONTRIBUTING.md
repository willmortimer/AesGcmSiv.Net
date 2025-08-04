# Contributing to AES-GCM-SIV for .NET

Thank you for your interest in contributing to AES-GCM-SIV for .NET! This document provides guidelines for contributing to this project.

## Development Setup

### Prerequisites
- .NET 9.0 SDK
- Visual Studio Build Tools 2022 (for native compilation)
- OpenSSL 3.x (provided in the repository)

### Building Locally
```bash
# Build native library
.\Build\build_native.bat

# Build and test .NET projects
dotnet build --configuration Release
dotnet test --configuration Release
```

## Contributing Guidelines

### Code Style
- Follow the existing code style and patterns
- Use the provided `.editorconfig` for consistent formatting
- Ensure all tests pass before submitting

### Testing
- Add tests for new functionality
- Ensure all existing tests continue to pass
- Test both Debug and Release configurations

### Security
- Follow security best practices
- Report security vulnerabilities privately (see SECURITY.md)
- Never commit sensitive information

### Pull Request Process
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Ensure all tests pass
6. Submit a pull request

### Commit Messages
- Use clear, descriptive commit messages
- Reference issues when applicable
- Follow conventional commit format when possible

## Architecture

### Native Layer
- C++ shim in `Native/aesgcmsiv.cpp`
- OpenSSL 3.x integration
- Clean C ABI for P/Invoke

### .NET Layer
- `AesGcmSiv` class in `Crypto/AesGcmSiv.cs`
- Follows `System.Security.Cryptography` patterns
- Comprehensive parameter validation

### Testing
- Unit tests in `AesGcmSiv.Tests/`
- Test both positive and negative cases
- Verify security properties

## Questions?

If you have questions about contributing, please:
1. Check existing issues and discussions
2. Review the documentation
3. Contact the maintainers via GitHub issues

Thank you for contributing to making AES-GCM-SIV for .NET better! 