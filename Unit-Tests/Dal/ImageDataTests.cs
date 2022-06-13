using FluentAssertions;
using System;
using vMotion.Dal;
using Xunit;
using Xunit.Abstractions;

namespace vMotion.Api.Specs.Unit_Tests
{
    [Trait(Constants.Category, Constants.CI)]
    public class ImageDataTests : AutoFixtureTests
    {
        public ImageDataTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void WhenCtor_with_bad_string()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => new ImageData(Guid.NewGuid().ToString("D")));

            ShowResult(new { ex.Message });
        }

        [Fact]
        public void WhenCtor_with_good_string()
        {
            var result = new ImageData("Container1;filename1.jpg");

            result.Container.Should().Be("Container1");
            result.FileName.Should().Be("filename1.jpg");
            ShowResult(result);
        }

        [Fact]
        public void WhenToString__validate()
        {
            var expected = "Container1;filename1.jpg";

            var result = new ImageData("Container1;filename1.jpg").ToString();

            result.Should().Be(expected);
        }
    }
}