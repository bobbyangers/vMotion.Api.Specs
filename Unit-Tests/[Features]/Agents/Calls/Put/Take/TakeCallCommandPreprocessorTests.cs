using AutoFixture;
using FluentAssertions;
using MongoDB.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;
using vMotion.api._Features_.Agents.Calls.Put.Take;
using vMotion.Dal;
using vMotion.Dal.MongoDb.Entities;
using Xunit;
using Xunit.Abstractions;

namespace vMotion.Api.Specs.Unit_Tests._Features_.Agents.Calls.Put.Take
{
    [Trait(Constants.Category, Constants.CI)]
    public class TakeCallCommandPreprocessorTests : PreprocessorTests<CommandPreprocessor>
    {
        public TakeCallCommandPreprocessorTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task WhenProcess_and_staff_is_already_busy()
        {
            var @staff = Fixture.Create<StaffEntity>();
            @staff.CurrentCall = Guid.NewGuid().ToObjectId();
            await @staff.SaveAsync().ConfigureAwait(false);

            var data = Fixture.Build<Request>()
                .With(_ => _.Id, Guid.NewGuid())
                .With(_ => _.UserId, @staff.ID.ToGuid())
                .Create();

            var ex = await Assert.ThrowsAsync<PreConditionFailedException>(() => Sut.Process(data, CancellationToken.None)).ConfigureAwait(false);

            ex.Message.Should().ContainAll("Staff", "already has current call set");

            ShowResult(new { ex.Message });
        }
    }
}