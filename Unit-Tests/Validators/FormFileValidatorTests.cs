using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Threading.Tasks;
using vMotion.api.Data;
using vMotion.api.Validators;
using Xunit;
using Xunit.Abstractions;

namespace vMotion.Api.Specs.Unit_Tests
{
    [Trait(Constants.Category, Constants.CI)]
    public class FormFileValidatorTests : AutoFixtureTests<FormFileValidator>
    {
        private const int Mb = 1024;

        public FormFileValidatorTests(ITestOutputHelper output) : base(output)
        {
            var settings = new AppSettings
            {
                FileSizeLimit = 2 * Mb
            };

            Sut = new FormFileValidator(settings);
        }

        [Theory]
        [InlineData("file.bmp")]
        [InlineData("file.gif")]
        [InlineData("file.jpg")]
        [InlineData("file.jpeg")]
        [InlineData("file.png")]
        [InlineData("file.tif")]
        [InlineData("file.tiff")]
        public async Task WhenFileName_has_valid_extension_is_ok(string filename)
        {
            var data = Substitute.For<IFormFile>().Then(_ =>
            {
                _.FileName.Returns(filename);
            });

            var result = await Sut.TestValidateAsync(data).ConfigureAwait(false);

            result.ShouldNotHaveValidationErrorFor(x => x.FileName);
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("file.bin")]
        [InlineData("file.exe")]
        [InlineData("file.com")]
        [InlineData("file.bat")]
        [InlineData("file.xls")]
        public async Task WhenFileName_has_invalid_extension(string filename)
        {
            var data = Substitute.For<IFormFile>().Then(_ =>
            {
                _.FileName.Returns(filename);
            });

            var result = await Sut.TestValidateAsync(data).ConfigureAwait(false);

            result.ShouldHaveValidationErrorFor(x => x.FileName)
                .WithErrorMessage("[File Name] Supported file types: .JPG,.JPEG,.PNG,.GIF,.BMP,.TIF,.TIFF");
        }

        [Fact]
        public async Task WhenLength_is_too_large()
        {
            var data = Substitute.For<IFormFile>().Then(_ =>
            {
                _.FileName.Returns("file.gif");
                _.Length.Returns(3 * Mb);
            });

            var result = await Sut.TestValidateAsync(data).ConfigureAwait(false);

            result.ShouldHaveValidationErrorFor(x => x.Length)
                .WithErrorMessage("File size should be less than 2 Mb");
        }
    }

}