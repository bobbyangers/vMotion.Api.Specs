using AutoFixture;
using FluentAssertions;
using MongoDB.Entities;
using System.Threading;
using System.Threading.Tasks;
using vMotion.api._Features_.Agents.Cases.Close.Put;
using vMotion.Dal;
using vMotion.Dal.MongoDb.Entities;
using Xunit;
using Xunit.Abstractions;

namespace vMotion.Api.Specs.Unit_Tests._Features_.Clients.Cases.Put.Close
{
    [Trait(Constants.Category, Constants.CI)]
    public class CommandPreprocessorTests : PreprocessorTests<CommandPreprocessor>
    {
        public CommandPreprocessorTests(ITestOutputHelper output) : base(output)
        {
        }

        private async Task<CaseEntity> GetCaseEntity(CaseStatus status)
        {
            var @case = Fixture.Build<CaseEntity>()
                .With(_ => _.ID, (string)null)
                .With(_ => _.Status, status)
                .Create();
            await @case.SaveAsync().ConfigureAwait(false);
            return @case;
        }

        [Theory]
        [InlineData(CaseStatus.Closed)]
        [InlineData(CaseStatus.Cancelled)]
        public async Task WhenProcess_and_case_is(CaseStatus status)
        {
            var @case = await GetCaseEntity(status).ConfigureAwait(false);

            var data = Fixture.Build<Request>()
                .With(_ => _.Id, @case.ID.ToGuid())
                .Create();

            var ex = await Assert.ThrowsAsync<PreConditionFailedException>(() => Sut.Process(data, CancellationToken.None)).ConfigureAwait(false);

            ex.Message.Should().ContainAll("Case", status.ToString());
        }
    }
}