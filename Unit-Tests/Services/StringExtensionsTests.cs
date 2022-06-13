using FluentAssertions;
using Newtonsoft.Json;
using System;
using Xunit;
using Xunit.Abstractions;

namespace vMotion.Api.Specs.Unit_Tests
{
    [Trait(Constants.Category, Constants.CI)]
    public class StringExtensionsTests
    {
        private readonly ITestOutputHelper _output;

        public StringExtensionsTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [InlineData("1")]
        [InlineData("12")]
        [InlineData("123")]
        [InlineData("1234")]
        [InlineData("12345")]
        [InlineData("123456")]
        [InlineData("1234567")]
        [InlineData("12345678")]
        [InlineData("123456789")]
        [InlineData("1234567890")]
        [InlineData("1234567890a")]
        [InlineData("1234567890ab")]
        [InlineData("1234567890abc")]
        [InlineData("1234567890abcd")]
        [InlineData("1234567890abcde")]
        [InlineData("1234567890abcdef")]
        [InlineData("1234567890abcdef1")]
        [InlineData("1234567890abcdef12")]
        [InlineData("1234567890abcdef123")]
        [InlineData("1234567890abcdef1234")]
        [InlineData("1234567890abcdef12345")]
        [InlineData("1234567890abcdef123456")]
        [InlineData("1234567890abcdef1234567")]
        [InlineData("1234567890abcdef12345678")]
        [InlineData("1234567890abcdef123456789")]
        [InlineData("1234567890abcdef1234567890a")]
        [InlineData("1234567890abcdef1234567890ab")]
        [InlineData("1234567890abcdef1234567890abc")]
        [InlineData("1234567890abcdef1234567890abcd")]
        [InlineData("1234567890abcdef1234567890abcde")]
        [InlineData("1234567890abcdef1234567890abcdef")]
        public void WhenToGuidWithCompleteString(string data)
        {
            var result = data.ToGuid();

            ShowResult(result);
            result.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData("60ecf5f231665bfa78dfa37a")]
        public void WhenRealGuidThenOk(string data)
        {
            var result = data.ToGuid();

            ShowResult(result);
            result.Should().NotBeEmpty();
        }

        [Fact]
        public void WhenRealGuid2ThenOk()
        {
            var result = Guid.NewGuid().ToString("N").ToGuid();

            ShowResult(result);
            result.Should().NotBeEmpty();
        }

        protected void ShowResult(object result)
        {
            ShowResult(JsonConvert.SerializeObject(result, Formatting.Indented));
        }

        protected void ShowResult(string result)
        {
            _output.WriteLine(new string('-', 50));
            _output.WriteLine(result);
        }
    }
}