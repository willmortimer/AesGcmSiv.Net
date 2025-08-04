using System.Security.Cryptography;
using Xunit;

namespace AesGcmSiv.Tests
{
    public class ApiDesignTests
    {
        [Fact]
        public void AesGcmSiv_ShouldImplementIDisposable()
        {
            // Arrange
            var key = new byte[32];
            
            // Act & Assert
            using var aesGcmSiv = new System.Security.Cryptography.AesGcmSiv(key);
            Assert.IsAssignableFrom<IDisposable>(aesGcmSiv);
        }

        [Fact]
        public void AesGcmSiv_ShouldHaveCorrectNamespace()
        {
            // Arrange
            var key = new byte[32];
            
            // Act
            using var aesGcmSiv = new System.Security.Cryptography.AesGcmSiv(key);
            
            // Assert
            Assert.Equal("System.Security.Cryptography", aesGcmSiv.GetType().Namespace);
        }

        [Fact]
        public void AesGcmSiv_ShouldHaveExpectedMethods()
        {
            // Arrange
            var key = new byte[32];
            using var aesGcmSiv = new System.Security.Cryptography.AesGcmSiv(key);
            
            // Act & Assert
            var methods = aesGcmSiv.GetType().GetMethods();
            var methodNames = methods.Select(m => m.Name).ToArray();
            
            Assert.Contains("Encrypt", methodNames);
            Assert.Contains("Decrypt", methodNames);
            Assert.Contains("Dispose", methodNames);
        }

        [Fact]
        public void AesGcmSiv_ShouldHaveExpectedConstructor()
        {
            // Arrange & Act
            var key = new byte[32];
            using var aesGcmSiv = new System.Security.Cryptography.AesGcmSiv(key);
            
            // Assert
            Assert.NotNull(aesGcmSiv);
        }

        [Fact]
        public void AesGcmSiv_ShouldHaveExpectedConstants()
        {
            // This test validates that our API design follows the expected patterns
            // from System.Security.Cryptography.AesGcm
            
            // Arrange
            var key = new byte[32];
            using var aesGcmSiv = new System.Security.Cryptography.AesGcmSiv(key);
            
            // Assert - These would be public constants in a real implementation
            // For now, we're just validating the API structure
            Assert.NotNull(aesGcmSiv);
        }

        [Fact]
        public void AesGcmSiv_ShouldSupportUsingStatement()
        {
            // Arrange
            var key = new byte[32];
            
            // Act & Assert - This should not throw
            using (var aesGcmSiv = new System.Security.Cryptography.AesGcmSiv(key))
            {
                Assert.NotNull(aesGcmSiv);
            }
        }

        [Fact]
        public void AesGcmSiv_ShouldBeSealed()
        {
            // Arrange
            var key = new byte[32];
            using var aesGcmSiv = new System.Security.Cryptography.AesGcmSiv(key);
            
            // Assert
            Assert.True(aesGcmSiv.GetType().IsSealed);
        }

        [Fact]
        public void AesGcmSiv_ShouldBePublic()
        {
            // Arrange
            var key = new byte[32];
            using var aesGcmSiv = new System.Security.Cryptography.AesGcmSiv(key);
            
            // Assert
            Assert.True(aesGcmSiv.GetType().IsPublic);
        }
    }
} 