using FluentAssertions;
using vMotion.api.Utils;
using Xunit;
using Xunit.Abstractions;

namespace vMotion.Api.Specs.Unit_Tests;

[Trait(Constants.Category, Constants.CI)]
public class SecurePasswordHasherTests
{
    public ITestOutputHelper Output { get; }

    public SecurePasswordHasherTests(ITestOutputHelper output)
    {
        Output = output;
    }

    [Fact]
    public void GeneratePassword()
    {
        var result = SecurePasswordHasher.GeneratePassword();

        result.Should().NotBeNullOrEmpty();

        Output.WriteLine(result);
    }

}