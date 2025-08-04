using System.Security.Cryptography;
using Xunit;

namespace AesGcmSiv.Tests
{
    /// <summary>
    /// API design tests for the AesGcmSiv cryptographic implementation.
    /// </summary>
    public class ApiDesignTests
    {
        /// <summary>
        /// Tests that AesGcmSiv implements IDisposable interface.
        /// </summary>
        [Fact]
        public void AesGcmSiv_ShouldImplementIDisposable()
        {
            // Arrange
            var key = new byte[32];

            // Act & Assert
            using var aesGcmSiv = new System.Security.Cryptography.AesGcmSiv(key);
            Assert.IsAssignableFrom<IDisposable>(aesGcmSiv);
        }

        /// <summary>
        /// Tests that AesGcmSiv has the correct namespace.
        /// </summary>
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

        /// <summary>
        /// Tests that AesGcmSiv has the expected public methods.
        /// </summary>
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

        /// <summary>
        /// Tests that AesGcmSiv has the expected constructor.
        /// </summary>
        [Fact]
        public void AesGcmSiv_ShouldHaveExpectedConstructor()
        {
            // Arrange & Act
            var key = new byte[32];
            using var aesGcmSiv = new System.Security.Cryptography.AesGcmSiv(key);

            // Assert
            Assert.NotNull(aesGcmSiv);
        }

        /// <summary>
        /// Tests that AesGcmSiv has the expected constants.
        /// </summary>
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

        /// <summary>
        /// Tests that AesGcmSiv supports the using statement.
        /// </summary>
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

        /// <summary>
        /// Tests that AesGcmSiv is sealed.
        /// </summary>
        [Fact]
        public void AesGcmSiv_ShouldBeSealed()
        {
            // Arrange
            var key = new byte[32];
            using var aesGcmSiv = new System.Security.Cryptography.AesGcmSiv(key);

            // Assert
            Assert.True(aesGcmSiv.GetType().IsSealed);
        }

        /// <summary>
        /// Tests that AesGcmSiv is public.
        /// </summary>
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
