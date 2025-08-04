# Security Policy

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| 1.0.x   | :white_check_mark: |

## Reporting a Vulnerability

If you discover a security vulnerability in this project, please report it to us privately to give us time to fix it before it becomes public.

**Please do NOT report security vulnerabilities through public GitHub issues.**

Instead, please report them via email to: security@yourdomain.com

### What to include in your report:

1. **Description** - A clear description of the vulnerability
2. **Steps to reproduce** - How to reproduce the issue
3. **Impact** - What could happen if this vulnerability is exploited
4. **Suggested fix** - If you have any suggestions for fixing the issue

### What happens next:

1. We will acknowledge receipt of your report within 48 hours
2. We will investigate and provide updates on our progress
3. Once fixed, we will release a patch and credit you in the release notes
4. We will coordinate public disclosure with you

## Security Best Practices

When using this library:

1. **Always use cryptographically secure random number generators** for keys and nonces
2. **Never reuse nonces** - each encryption operation should use a unique nonce
3. **Keep your keys secure** - store them in secure key management systems
4. **Validate all inputs** - ensure data integrity before processing
5. **Use the latest version** - always update to the most recent release

## Security Features

This library implements AES-GCM-SIV (RFC 8452) which provides:

- **Misuse resistance** - protects against nonce reuse
- **Authenticated encryption** - ensures data integrity and confidentiality
- **Deterministic encryption** - same plaintext + key + nonce = same ciphertext 