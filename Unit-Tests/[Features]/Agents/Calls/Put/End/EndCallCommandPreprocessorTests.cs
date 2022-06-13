using AutoFixture;
using FluentAssertions;
using MongoDB.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;
using vMotion.api._Features_.Agents.Calls.Put.End;
using vMotion.Dal;
using vMotion.Dal.MongoDb.Entities;
using Xunit;
using Xunit.Abstractions;

namespace vMotion.Api.Specs.Unit_Tests._Features_.Agents.Calls.Put.End.Tests
{
    [Trait(Constants.Category, Constants.CI)]
    public class CommandPreprocessorTests : PreprocessorTests<CommandPreprocessor>
    {
        public CommandPreprocessorTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task WhenProcess_and_staff_is_not_assigned_to_call()
        {
            var @call = Fixture.Create<CallEntity>();
            await @call.SaveAsync().ConfigureAwait(false);

            var @staff = Fixture.Create<StaffEntity>();
            @staff.CurrentCall = Guid.NewGuid().ToObjectId();
            await @staff.SaveAsync().ConfigureAwait(false);

            var data = Fixture.Build<Request>()
                .With(_ => _.Id, @call.ID.ToGuid())
                .With(_ => _.UserId, @staff.ID.ToGuid())
                .Create();

            var ex = await Assert.ThrowsAsync<CommandForbiddenException>(() => Sut.Process(data, CancellationToken.None)).ConfigureAwait(false);

            ex.Message.Should().ContainAll("Staff", "not assigned to call");

            ShowResult(new { ex.Message });
        }
    }
}