using AutoFixture;
using FluentAssertions;
using MongoDB.Entities;
using System.Threading;
using System.Threading.Tasks;
using vMotion.api._Features_.Clients.Calls.StaffEvents.Post;
using vMotion.Dal.MongoDb.Entities;
using Xunit;
using Xunit.Abstractions;

namespace vMotion.Api.Specs.Unit_Tests._Features_.Clients.Calls.Post.StaffEvents
{
    [Trait(Constants.Category, Constants.CI)]
    public class CommandPreprocessorTests : PreprocessorTests<CommandPreprocessor>
    {
        public CommandPreprocessorTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task WhenProcess_get_staffId()
        {
            var @member = Fixture.Create<MemberEntity>();
            await @member.SaveAsync().ConfigureAwait(false);

            var @staff = Fixture.Create<StaffEntity>();
            await @staff.SaveAsync().ConfigureAwait(false);

            var @call = Fixture.Create<CallEntity>();
            @call.ByMember = @member.ID;
            @call.OngoingCallBy = @staff.ID;
            await @call.SaveAsync().ConfigureAwait(false);

            @member.CurrentCall = @call.ID;
            await @member.SaveAsync().ConfigureAwait(false);

            @staff.CurrentCall = @call.ID;
            await @staff.SaveAsync().ConfigureAwait(false);

            var data = Fixture.Build<Request>()
                .With(_ => _.UserId, @member.ID.ToGuid())
                .Create();

            await Sut.Process(data, CancellationToken.None).ConfigureAwait(false);

            data.StaffId.Should().NotBeEmpty();
        }
    }
};