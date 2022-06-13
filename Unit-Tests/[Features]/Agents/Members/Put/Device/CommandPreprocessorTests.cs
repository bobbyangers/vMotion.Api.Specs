using AutoFixture;
using FluentAssertions;
using MongoDB.Entities;
using System.Threading;
using System.Threading.Tasks;
using vMotion.api._Features_.Agents.Members.Device.Put;
using vMotion.Dal;
using vMotion.Dal.MongoDb.Entities;
using Xunit;
using Xunit.Abstractions;

namespace vMotion.Api.Specs.Unit_Tests._Features_.Agents.Calls.Put.Tests
{
    [Trait(Constants.Category, Constants.CI)]
    public class CommandPreprocessorTests : PreprocessorTests<CommandPreprocessor>
    {
        public CommandPreprocessorTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task WhenProcess_device_empty()
        {
            var @member = Fixture.Create<MemberEntity>();
            @member.Device = null;
            await @member.SaveAsync().ConfigureAwait(false);

            var data = Fixture.Build<Request>()
                .With(_ => _.MemberId, @member.ID.ToGuid())
                .Without(_ => _.DeviceId)
                .Create();

            var ex = await Assert.ThrowsAsync<PreConditionFailedException>(() => Sut.Process(data, CancellationToken.None)).ConfigureAwait(false);

            ShowResult(new { ex.Message });
        }

        [Fact]
        public async Task WhenProcess_deviceid_is_set()
        {
            var @member = Fixture.Create<MemberEntity>();
            await @member.SaveAsync().ConfigureAwait(false);

            var data = Fixture.Build<vMotion.api._Features_.Agents.Members.Device.Put.Request>()
                .With(_ => _.MemberId, @member.ID.ToGuid())
                .Without(_ => _.DeviceId)
                .Create();

            await Sut.Process(data, CancellationToken.None).ConfigureAwait(false);

            data.DeviceId.Should().NotBeEmpty();
        }
    }
}