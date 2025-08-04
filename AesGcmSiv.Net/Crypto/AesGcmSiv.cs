using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace System.Security.Cryptography
{
    /// <summary>
    /// Provides AES-GCM-SIV (Galois/Counter Mode with Synthetic Initialization Vector) authenticated encryption.
    /// This implementation provides misuse resistance through deterministic encryption.
    /// </summary>
    /// <remarks>
    /// AES-GCM-SIV is a deterministic authenticated encryption mode that provides protection against
    /// nonce reuse attacks. Unlike standard AES-GCM, reusing a nonce with the same key will not
    /// lead to security failures, though it will reveal that the same plaintext was encrypted.
    /// 
    /// Key requirements:
    /// - Must be exactly 32 bytes (256 bits)
    /// - Should be generated using a cryptographically secure random number generator
    /// 
    /// Nonce requirements:
    /// - Must be exactly 12 bytes (96 bits)
    /// - Can be random or structured (e.g., user ID + counter)
    /// - Nonce reuse is safe but reveals message duplication
    /// </remarks>
    public sealed class AesGcmSiv : IDisposable
    {
        private bool _disposed;
        private readonly byte[] _key;

        // P/Invoke declarations
        [DllImport("aesgcmsiv", CallingConvention = CallingConvention.Cdecl)]
        private static extern int aesgcmsiv_encrypt(
            [In] byte[] key, nuint key_len,
            [In] byte[] nonce, nuint nonce_len,
            [In] byte[] plaintext, nuint plaintext_len,
            [In] byte[]? aad, nuint aad_len,
            [Out] byte[] ciphertext_out,
            [Out] byte[] tag_out);

        [DllImport("aesgcmsiv", CallingConvention = CallingConvention.Cdecl)]
        private static extern int aesgcmsiv_decrypt(
            [In] byte[] key, nuint key_len,
            [In] byte[] nonce, nuint nonce_len,
            [In] byte[] ciphertext, nuint ciphertext_len,
            [In] byte[]? aad, nuint aad_len,
            [In] byte[] tag,
            [Out] byte[] plaintext_out);

        // Constants
        private const int KeySize = 32;  // 256 bits
        private const int NonceSize = 12; // 96 bits
        private const int TagSize = 16;   // 128 bits

        // Error codes from native layer
        private const int AESGCMSIV_SUCCESS = 0;
        private const int AESGCMSIV_ERROR_INVALID_KEY = -1;
        private const int AESGCMSIV_ERROR_INVALID_NONCE = -2;
        private const int AESGCMSIV_ERROR_INVALID_INPUT = -3;
        private const int AESGCMSIV_ERROR_INVALID_TAG = -4;
        private const int AESGCMSIV_ERROR_DECRYPT_FAILED = -5;
        private const int AESGCMSIV_ERROR_INTERNAL = -6;

        /// <summary>
        /// Initializes a new instance of the <see cref="AesGcmSiv"/> class with the specified key.
        /// </summary>
        /// <param name="key">The encryption key. Must be exactly 32 bytes (256 bits).</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="key"/> is not exactly 32 bytes.</exception>
        public AesGcmSiv(byte[] key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (key.Length != KeySize)
                throw new ArgumentException($"Key must be exactly {KeySize} bytes (256 bits).", nameof(key));

            _key = new byte[KeySize];
            Array.Copy(key, _key, KeySize);
        }

        /// <summary>
        /// Encrypts plaintext using AES-GCM-SIV.
        /// </summary>
        /// <param name="nonce">The nonce. Must be exactly 12 bytes (96 bits).</param>
        /// <param name="plaintext">The plaintext to encrypt.</param>
        /// <param name="ciphertext">The output buffer for the ciphertext. Must be at least as large as the plaintext.</param>
        /// <param name="tag">The output buffer for the authentication tag. Must be at least 16 bytes.</param>
        /// <param name="associatedData">Optional additional authenticated data.</param>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        /// <exception cref="ArgumentNullException">Any required parameter is null.</exception>
        /// <exception cref="ArgumentException">Parameter sizes are invalid.</exception>
        /// <exception cref="CryptographicException">Encryption failed.</exception>
        public void Encrypt(
            byte[] nonce,
            byte[] plaintext,
            byte[] ciphertext,
            byte[] tag,
            byte[]? associatedData = null)
        {
            ThrowIfDisposed();
            ValidateEncryptParameters(nonce, plaintext, ciphertext, tag, associatedData);

            int result = aesgcmsiv_encrypt(
                _key, (nuint)_key.Length,
                nonce, (nuint)nonce.Length,
                plaintext, (nuint)plaintext.Length,
                associatedData, (nuint)(associatedData?.Length ?? 0),
                ciphertext, tag);

            if (result != AESGCMSIV_SUCCESS)
            {
                throw new CryptographicException($"Encryption failed with error code: {result}");
            }
        }

        /// <summary>
        /// Decrypts ciphertext using AES-GCM-SIV.
        /// </summary>
        /// <param name="nonce">The nonce used during encryption. Must be exactly 12 bytes (96 bits).</param>
        /// <param name="ciphertext">The ciphertext to decrypt.</param>
        /// <param name="tag">The authentication tag. Must be exactly 16 bytes.</param>
        /// <param name="plaintext">The output buffer for the plaintext. Must be at least as large as the ciphertext.</param>
        /// <param name="associatedData">Optional additional authenticated data (must match the data used during encryption).</param>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        /// <exception cref="ArgumentNullException">Any required parameter is null.</exception>
        /// <exception cref="ArgumentException">Parameter sizes are invalid.</exception>
        /// <exception cref="CryptographicException">Decryption failed or authentication tag is invalid.</exception>
        public void Decrypt(
            byte[] nonce,
            byte[] ciphertext,
            byte[] tag,
            byte[] plaintext,
            byte[]? associatedData = null)
        {
            ThrowIfDisposed();
            ValidateDecryptParameters(nonce, ciphertext, tag, plaintext, associatedData);

            int result = aesgcmsiv_decrypt(
                _key, (nuint)_key.Length,
                nonce, (nuint)nonce.Length,
                ciphertext, (nuint)ciphertext.Length,
                associatedData, (nuint)(associatedData?.Length ?? 0),
                tag, plaintext);

            if (result != AESGCMSIV_SUCCESS)
            {
                throw new CryptographicException($"Decryption failed with error code: {result}");
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="AesGcmSiv"/> class.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                // Clear the key from memory
                if (_key != null)
                {
                    Array.Clear(_key, 0, _key.Length);
                }
                _disposed = true;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AesGcmSiv));
        }

        private static void ValidateEncryptParameters(
            byte[] nonce, byte[] plaintext, byte[] ciphertext, byte[] tag, byte[]? associatedData)
        {
            if (nonce == null)
                throw new ArgumentNullException(nameof(nonce));
            if (plaintext == null)
                throw new ArgumentNullException(nameof(plaintext));
            if (ciphertext == null)
                throw new ArgumentNullException(nameof(ciphertext));
            if (tag == null)
                throw new ArgumentNullException(nameof(tag));

            if (nonce.Length != NonceSize)
                throw new ArgumentException($"Nonce must be exactly {NonceSize} bytes (96 bits).", nameof(nonce));
            if (ciphertext.Length < plaintext.Length)
                throw new ArgumentException("Ciphertext buffer is too small.", nameof(ciphertext));
            if (tag.Length < TagSize)
                throw new ArgumentException($"Tag buffer must be at least {TagSize} bytes.", nameof(tag));
        }

        private static void ValidateDecryptParameters(
            byte[] nonce, byte[] ciphertext, byte[] tag, byte[] plaintext, byte[]? associatedData)
        {
            if (nonce == null)
                throw new ArgumentNullException(nameof(nonce));
            if (ciphertext == null)
                throw new ArgumentNullException(nameof(ciphertext));
            if (tag == null)
                throw new ArgumentNullException(nameof(tag));
            if (plaintext == null)
                throw new ArgumentNullException(nameof(plaintext));

            if (nonce.Length != NonceSize)
                throw new ArgumentException($"Nonce must be exactly {NonceSize} bytes (96 bits).", nameof(nonce));
            if (tag.Length != TagSize)
                throw new ArgumentException($"Tag must be exactly {TagSize} bytes.", nameof(tag));
            if (plaintext.Length < ciphertext.Length)
                throw new ArgumentException("Plaintext buffer is too small.", nameof(plaintext));
        }
    }
}
