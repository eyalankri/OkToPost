using OkToPost.Utils;
using Xunit;

namespace OkToPost.Tests.Utils
{
    public class ShortCodeValidatorTests
    {
        [Theory]
        [InlineData("abc123", true)]
        [InlineData("ABCDEF", true)]
        [InlineData("123456", true)]
        [InlineData("", false)]
        [InlineData(null, false)]
        [InlineData("abc12", false)]
        [InlineData("abc1234", false)]
        [InlineData("     ", false)]
        public void IsValid_ReturnsExpectedResult(string input, bool expected)
        {
            // Act
            var result = ShortCodeValidator.IsValid(input);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}

   