using AutoFixture;
using FluentAssertions;
using Newtonsoft.Json;
using System;
using vMotion.Dal;
using Xunit;
using Xunit.Abstractions;

namespace vMotion.Api.Specs.Unit_Tests
{
    [Trait(Constants.Category, Constants.CI)]
    public class JsonConverterTests : AutoFixtureTests
    {
        public JsonConverterTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void TimespanConverter_Convert_write()
        {
            var data = Fixture.Build<SomeTimespanDto>()
                .With(_ => _.Data, TimeSpan.FromMinutes(30))
                .Create();

            var result = JsonConvert.SerializeObject(data, Formatting.None);

            ShowResult(result);

            result.Should().ContainAll("data", $"{data.Data:hh\\:mm}");
        }

        [Fact]
        public void TimespanConverter_Convert_read()
        {
            var data = @"{""data"":""00:30""}";

            var result = JsonConvert.DeserializeObject<SomeTimespanDto>(data);

            ShowResult(result);

            result.Data.Should().Be(TimeSpan.FromMinutes(30));
        }

        [Fact]
        public void CustomDateTimeConverter_Convert_write()
        {
            var data = Fixture.Create<SomeDateTimeDto>();

            var result = JsonConvert.SerializeObject(data, Formatting.None);

            ShowResult(result);

            result.Should().ContainAll("data", $"{data.Data:yyyy-MM-dd}");
        }

        public class SomeTimespanDto
        {
            [JsonConverter(typeof(TimespanConverter))]
            public TimeSpan Data { get; set; }
        }

        public class SomeDateTimeDto
        {
            [JsonConverter(typeof(CustomDateTimeConverter), "yyyy-MM-dd")]
            public DateTime Data { get; set; }
        }
    }
}
