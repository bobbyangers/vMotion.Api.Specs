using AutoFixture;
using FluentAssertions;
using MongoDB.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;
using vMotion.api._Features_.Clients.Calls.Put.Cancel;
using vMotion.Dal;
using vMotion.Dal.MongoDb.Entities;
using Xunit;
using Xunit.Abstractions;

namespace vMotion.Api.Specs.Unit_Tests._Features_.Clients.Calls.Put.Cancel
{
    [Trait(Constants.Category, Constants.CI)]
    public class CommandPreprocessorTests : PreprocessorTests<CommandPreprocessor>
    {
        public CommandPreprocessorTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task WhenProcess_and_call_is_notForMember()
        {
            var @call = Fixture.Create<CallEntity>();
            await @call.SaveAsync().ConfigureAwait(false);

            var data = Fixture.Build<Request>()
                .With(_ => _.Id, @call.ID.ToGuid())
                .With(_ => _.UserId, Guid.NewGuid())
                .Create();

            var ex = await Assert.ThrowsAsync<CommandForbiddenException>(() => Sut.Process(data, CancellationToken.None)).ConfigureAwait(false);

            ex.Message.Should().ContainAll("Member", "not assigned");

            ShowResult(new { ex.Message });
        }
    }
}