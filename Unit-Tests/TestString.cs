using FluentAssertions;
using IdentityModel;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace vMotion.Api.Specs.Unit_Tests
{
    [Trait(Constants.Category, Constants.CI)]
    public class TestString
    {
        public ITestOutputHelper Output { get; }

        public TestString(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public void ToSha256()
        {
            Output.WriteLine("511536EF-F270-4058-80CA-1C89C192F69A".ToSha256());

            Assert.True(true);
        }

        [Theory]
        [InlineData("1-718-444-1122")]
        [InlineData("718-444-1122")]
        [InlineData("(718)-444-1122")]
        [InlineData("17184441122")]
        [InlineData("7184441122")]
        [InlineData("718.444.1122")]
        [InlineData("1718.444.1122")]
        [InlineData("1-123-456-7890")]
        [InlineData("1 123-456-7890")]
        [InlineData("1 (123) 456-7890")]
        [InlineData("1 123 456 7890")]
        [InlineData("1.123.456.7890")]
        [InlineData("+91 (123) 456-7890")]
        [InlineData("18005551234")]
        [InlineData("1 800 555 1234")]
        [InlineData("+1 800 555-1234")]
        [InlineData("+86 800 555 1234")]
        [InlineData("1-800-555-1234")]
        [InlineData("1 (800) 555-1234")]
        [InlineData("(800)555-1234")]
        [InlineData("(800) 555-1234")]
        [InlineData("(800)5551234")]
        [InlineData("800-555-1234")]
        [InlineData("800.555.1234")]
        [InlineData("18001234567")]
        [InlineData("1 800 123 4567")]
        [InlineData("1-800-123-4567")]
        [InlineData("+18001234567")]
        [InlineData("+1 800 123 4567")]
        [InlineData("+1 (800) 123 4567")]
        [InlineData("1(800)1234567")]
        [InlineData("+1800 1234567")]
        [InlineData("1.8001234567")]
        [InlineData("1.800.123.4567")]
        [InlineData("+1 (800) 123-4567")]
        [InlineData("+1 800 123-4567")]
        [InlineData("+86 800 123 4567")]
        [InlineData("1 (800) 123-4567")]
        [InlineData("(800)123-4567")]
        [InlineData("(800) 123-4567")]
        [InlineData("(800)1234567")]
        [InlineData("800-123-4567")]
        [InlineData("800.123.4567")]
        [InlineData("1231231231")]
        [InlineData("123-1231231")]
        [InlineData("123123-1231")]
        [InlineData("123-123 1231")]
        [InlineData("123 123-1231")]
        [InlineData("123-123-1231")]
        [InlineData("(123)123-1231")]
        [InlineData("(123)123 1231")]
        [InlineData("(123) 123-1231")]
        [InlineData("(123) 123 1231")]
        [InlineData("+99 1234567890")]
        [InlineData("+991234567890")]
        [InlineData("(555) 444-6789")]
        [InlineData("555-444-6789")]
        [InlineData("555.444.6789")]
        [InlineData("555 444 6789")]
        [InlineData("1.800.555.1234")]
        [InlineData("+1.800.555.1234")]
        [InlineData("(003) 555-1212")]
        [InlineData("(103) 555-1212")]
        [InlineData("(911) 555-1212")]
        [InlineData("+86 800-555-1234")]
        public void PhoneNumberTest(string data)
        {
            var sut = new Regex(vMotion.api.Constants.PhoneNumberRegex, RegexOptions.Singleline);

            sut.IsMatch(data).Should().BeTrue();
        }
    }
}