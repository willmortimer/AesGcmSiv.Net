using System.Security.Cryptography;
using Xunit;

namespace AesGcmSiv.Tests
{
    public class AesGcmSivTests
    {
        private readonly byte[] _testKey;
        private readonly byte[] _testNonce;
        private readonly byte[] _testPlaintext;
        private readonly byte[] _testAssociatedData;

        public AesGcmSivTests()
        {
            // Generate test data
            _testKey = new byte[32];
            _testNonce = new byte[12];
            _testPlaintext = new byte[64];
            _testAssociatedData = new byte[16];

            // Fill with deterministic test data
            for (int i = 0; i < _testKey.Length; i++) _testKey[i] = (byte)i;
            for (int i = 0; i < _testNonce.Length; i++) _testNonce[i] = (byte)(i + 100);
            for (int i = 0; i < _testPlaintext.Length; i++) _testPlaintext[i] = (byte)(i + 200);
            for (int i = 0; i < _testAssociatedData.Length; i++) _testAssociatedData[i] = (byte)(i + 300);
        }

        [Fact]
        public void Constructor_WithValidKey_ShouldNotThrow()
        {
            // Act & Assert
            using var aesGcmSiv = new System.Security.Cryptography.AesGcmSiv(_testKey);
            Assert.NotNull(aesGcmSiv);
        }

        [Fact]
        public void Constructor_WithNullKey_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new System.Security.Cryptography.AesGcmSiv(null!));
        }

        [Theory]
        [InlineData(31)] // Too short
        [InlineData(33)] // Too long
        public void Constructor_WithInvalidKeySize_ShouldThrowArgumentException(int keySize)
        {
            // Arrange
            var invalidKey = new byte[keySize];

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new System.Security.Cryptography.AesGcmSiv(invalidKey));
        }

        [Fact]
        public void Encrypt_WithValidParameters_ShouldSucceed()
        {
            // Arrange
            using var aesGcmSiv = new System.Security.Cryptography.AesGcmSiv(_testKey);
            var ciphertext = new byte[_testPlaintext.Length];
            var tag = new byte[16];

            // Act
            aesGcmSiv.Encrypt(_testNonce, _testPlaintext, ciphertext, tag, _testAssociatedData);

            // Assert
            Assert.NotEqual(_testPlaintext, ciphertext); // Ciphertext should be different from plaintext
            Assert.False(tag.All(b => b == 0)); // Tag should not be all zeros
        }

        [Fact]
        public void Encrypt_WithNullNonce_ShouldThrowArgumentNullException()
        {
            // Arrange
            using var aesGcmSiv = new System.Security.Cryptography.AesGcmSiv(_testKey);
            var ciphertext = new byte[_testPlaintext.Length];
            var tag = new byte[16];

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                aesGcmSiv.Encrypt(null!, _testPlaintext, ciphertext, tag));
        }

        [Theory]
        [InlineData(11)] // Too short
        [InlineData(13)] // Too long
        public void Encrypt_WithInvalidNonceSize_ShouldThrowArgumentException(int nonceSize)
        {
            // Arrange
            using var aesGcmSiv = new System.Security.Cryptography.AesGcmSiv(_testKey);
            var invalidNonce = new byte[nonceSize];
            var ciphertext = new byte[_testPlaintext.Length];
            var tag = new byte[16];

            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                aesGcmSiv.Encrypt(invalidNonce, _testPlaintext, ciphertext, tag));
        }

        [Fact]
        public void Encrypt_WithSmallCiphertextBuffer_ShouldThrowArgumentException()
        {
            // Arrange
            using var aesGcmSiv = new System.Security.Cryptography.AesGcmSiv(_testKey);
            var smallCiphertext = new byte[_testPlaintext.Length - 1]; // Too small
            var tag = new byte[16];

            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                aesGcmSiv.Encrypt(_testNonce, _testPlaintext, smallCiphertext, tag));
        }

        [Fact]
        public void Encrypt_WithSmallTagBuffer_ShouldThrowArgumentException()
        {
            // Arrange
            using var aesGcmSiv = new System.Security.Cryptography.AesGcmSiv(_testKey);
            var ciphertext = new byte[_testPlaintext.Length];
            var smallTag = new byte[15]; // Too small

            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                aesGcmSiv.Encrypt(_testNonce, _testPlaintext, ciphertext, smallTag));
        }

        [Fact]
        public void Decrypt_WithValidParameters_ShouldSucceed()
        {
            // Arrange
            using var aesGcmSiv = new System.Security.Cryptography.AesGcmSiv(_testKey);
            var ciphertext = new byte[_testPlaintext.Length];
            var tag = new byte[16];
            var decryptedPlaintext = new byte[_testPlaintext.Length];

            // Encrypt first
            aesGcmSiv.Encrypt(_testNonce, _testPlaintext, ciphertext, tag, _testAssociatedData);

            // Act
            aesGcmSiv.Decrypt(_testNonce, ciphertext, tag, decryptedPlaintext, _testAssociatedData);

            // Assert
            Assert.Equal(_testPlaintext, decryptedPlaintext);
        }

        [Fact]
        public void Decrypt_WithInvalidTag_ShouldThrowCryptographicException()
        {
            // Arrange
            using var aesGcmSiv = new System.Security.Cryptography.AesGcmSiv(_testKey);
            var ciphertext = new byte[_testPlaintext.Length];
            var tag = new byte[16];
            var decryptedPlaintext = new byte[_testPlaintext.Length];

            // Encrypt first
            aesGcmSiv.Encrypt(_testNonce, _testPlaintext, ciphertext, tag, _testAssociatedData);

            // Corrupt the tag
            tag[0] ^= 1;

            // Act & Assert
            Assert.Throws<CryptographicException>(() => 
                aesGcmSiv.Decrypt(_testNonce, ciphertext, tag, decryptedPlaintext, _testAssociatedData));
        }

        [Fact]
        public void Decrypt_WithInvalidNonce_ShouldThrowCryptographicException()
        {
            // Arrange
            using var aesGcmSiv = new System.Security.Cryptography.AesGcmSiv(_testKey);
            var ciphertext = new byte[_testPlaintext.Length];
            var tag = new byte[16];
            var decryptedPlaintext = new byte[_testPlaintext.Length];
            var wrongNonce = new byte[12];
            wrongNonce[0] = 1; // Different nonce

            // Encrypt first
            aesGcmSiv.Encrypt(_testNonce, _testPlaintext, ciphertext, tag, _testAssociatedData);

            // Act & Assert
            Assert.Throws<CryptographicException>(() => 
                aesGcmSiv.Decrypt(wrongNonce, ciphertext, tag, decryptedPlaintext, _testAssociatedData));
        }

        [Fact]
        public void EncryptDecrypt_WithoutAssociatedData_ShouldSucceed()
        {
            // Arrange
            using var aesGcmSiv = new System.Security.Cryptography.AesGcmSiv(_testKey);
            var ciphertext = new byte[_testPlaintext.Length];
            var tag = new byte[16];
            var decryptedPlaintext = new byte[_testPlaintext.Length];

            // Act
            aesGcmSiv.Encrypt(_testNonce, _testPlaintext, ciphertext, tag);
            aesGcmSiv.Decrypt(_testNonce, ciphertext, tag, decryptedPlaintext);

            // Assert
            Assert.Equal(_testPlaintext, decryptedPlaintext);
        }

        [Fact]
        public void EncryptDecrypt_WithEmptyAssociatedData_ShouldSucceed()
        {
            // Arrange
            using var aesGcmSiv = new System.Security.Cryptography.AesGcmSiv(_testKey);
            var ciphertext = new byte[_testPlaintext.Length];
            var ciphertext2 = new byte[_testPlaintext.Length];
            var tag = new byte[16];
            var tag2 = new byte[16];
            var decryptedPlaintext = new byte[_testPlaintext.Length];
            var emptyAad = new byte[0];

            // Act
            aesGcmSiv.Encrypt(_testNonce, _testPlaintext, ciphertext, tag, emptyAad);
            aesGcmSiv.Decrypt(_testNonce, ciphertext, tag, decryptedPlaintext, emptyAad);

            // Assert
            Assert.Equal(_testPlaintext, decryptedPlaintext);
        }

        [Fact]
        public void Dispose_ShouldClearKeyFromMemory()
        {
            // Arrange
            var aesGcmSiv = new System.Security.Cryptography.AesGcmSiv(_testKey);

            // Act
            aesGcmSiv.Dispose();

            // Assert
            Assert.Throws<ObjectDisposedException>(() => 
                aesGcmSiv.Encrypt(_testNonce, _testPlaintext, new byte[_testPlaintext.Length], new byte[16]));
        }

        [Fact]
        public void Encrypt_AfterDispose_ShouldThrowObjectDisposedException()
        {
            // Arrange
            using var aesGcmSiv = new System.Security.Cryptography.AesGcmSiv(_testKey);
            aesGcmSiv.Dispose();

            // Act & Assert
            Assert.Throws<ObjectDisposedException>(() => 
                aesGcmSiv.Encrypt(_testNonce, _testPlaintext, new byte[_testPlaintext.Length], new byte[16]));
        }

        [Fact]
        public void Decrypt_AfterDispose_ShouldThrowObjectDisposedException()
        {
            // Arrange
            using var aesGcmSiv = new System.Security.Cryptography.AesGcmSiv(_testKey);
            aesGcmSiv.Dispose();

            // Act & Assert
            Assert.Throws<ObjectDisposedException>(() => 
                aesGcmSiv.Decrypt(_testNonce, new byte[64], new byte[16], new byte[64]));
        }

        [Fact]
        public void Encrypt_Deterministic_WithSameInputs_ShouldProduceSameOutput()
        {
            // Arrange
            using var aesGcmSiv1 = new System.Security.Cryptography.AesGcmSiv(_testKey);
            using var aesGcmSiv2 = new System.Security.Cryptography.AesGcmSiv(_testKey);
            var ciphertext1 = new byte[_testPlaintext.Length];
            var ciphertext2 = new byte[_testPlaintext.Length];
            var tag1 = new byte[16];
            var tag2 = new byte[16];

            // Act
            aesGcmSiv1.Encrypt(_testNonce, _testPlaintext, ciphertext1, tag1, _testAssociatedData);
            aesGcmSiv2.Encrypt(_testNonce, _testPlaintext, ciphertext2, tag2, _testAssociatedData);

            // Assert - AES-GCM-SIV is deterministic
            Assert.Equal(ciphertext1, ciphertext2);
            Assert.Equal(tag1, tag2);
        }
    }
}
