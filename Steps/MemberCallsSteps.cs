using AutoFixture;
using FluentAssertions;
using MongoDB.Entities;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Infrastructure;
using vMotion.Dal;
using vMotion.Dal.MongoDb.Entities;

namespace vMotion.Api.Specs.Steps
{
    [Binding]
    [Scope(Feature = "MemberCalls")]
    public class MemberCallsSteps
    {
        private readonly WebHostSupport _webHost;
        private readonly ScenarioContext _context;
        private readonly IFixture _fixture;
        private readonly ISpecFlowOutputHelper _output;

        public MemberCallsSteps(WebHostSupport support, ScenarioContext context, IFixture fixture, ISpecFlowOutputHelper output)
        {
            _webHost = support;
            _context = context;
            _fixture = fixture;
            _output = output;
        }

        [BeforeScenario(Order = 100)]
        public void BeforeScenario()
        {
            CustomizeDomain(_fixture);
        }

        private void CustomizeDomain(IFixture f)
        {
            f.Customize<CaseEntity>(_ => _
                .With(x => x.ID, (string)null)
                .With(x => x.Status, CaseStatus.New)
                .With(x => x.InfoDateValidated, DateTime.UtcNow.AddMinutes(-1))
                .With(x => x.ReservationId, Guid.NewGuid().ToString("N"))
                .OmitAutoProperties()
            );
            f.Customize<CallEntity>(_ => _
                .With(x => x.ID, (string)null)
                .With(x => x.Status, CallStatus.NewCase)
                .With(x => x.InVideoCallStatus, VideoCallStatus.Waiting)
                .OmitAutoProperties()
            );
        }

        [Given("member has no current call set")]
        public async Task GivenMemberHasNoCurrentCallSet()
        {
            var member = await _context.GetRecord<MemberEntity>(Constants.MemberId).ConfigureAwait(false);
            var result = await DB.Update<MemberEntity>()
                .MatchID(member.ID)
                .Modify(_ => _.CurrentCall, null)
                .ExecuteAsync()
                .ConfigureAwait(false);
            result.ModifiedCount.Should().Be(1);
        }

        [Given("the member has a current call set")]
        public async Task GivenTheMemberHasACurrentCallSet()
        {
            var member = await _context.GetRecord<MemberEntity>(Constants.MemberId).ConfigureAwait(false);
            var staff = await _context.GetRecord<StaffEntity>(Constants.StaffId).ConfigureAwait(false);

            var @caseId = DB.Entity<CaseEntity>().GenerateNewID();

            var call = _fixture.Create<CallEntity>().Then(_ =>
            {
                _.ByMember = member.ID;
                _.RelatedCase = @caseId;
                _.OngoingCallBy = staff.ID;
            });

            await call.SaveAsync().ConfigureAwait(false);

            var @case = _fixture.Create<CaseEntity>();
            await @case.SaveAsync().ConfigureAwait(false);

            await @case.Calls.AddAsync(@call.ID).ConfigureAwait(false);

            _output.WriteLine($"{nameof(CallEntity)}[{call.ID.ToGuid():D}]");
            _output.WriteLine($"{nameof(CaseEntity)}[{@case.ID.ToGuid():D}]");

            var result = await DB.Update<MemberEntity>()
                .MatchID(member.ID)
                .Modify(_ => _.CurrentCall, call.ID)
                .ExecuteAsync()
                .ConfigureAwait(false);

            result.ModifiedCount.Should().Be(1);

            _context.Set(@case.ID.ObjectIdToGuidString(), Constants.CaseId);
            _context.Set(call.ID.ObjectIdToGuidString(), Constants.CallId);
        }

        [Given(@"call status is set to \[(.*)\]")]
        public async Task GivenCallStatusIsSetToActive(VideoCallStatus status)
        {
            var @call = await _context.GetRecord<CallEntity>(Constants.CallId).ConfigureAwait(false);

            var result = await DB.Update<CallEntity>()
                .MatchID(@call.ID)
                .Modify(_ => _.InVideoCallStatus, status)
                .ExecuteAsync()
                .ConfigureAwait(false);

            result.ModifiedCount.Should().Be(1);
        }

        [Then("call info is complete")]
        public async Task ThenCallInfoIsComplete()
        {
            var data = await _webHost.Response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var response = JsonConvert.DeserializeObject<api._Features_.Clients.Cases.Get.InProgress.Response>(data);

            response.Should().NotBeNull();

            response?.VideoConnector.Should().NotBeNull();
            response?.CaseId.Should().NotBeEmpty();
            ////response?.OngoingCallBy.Should().NotBeNull();
        }

        [Then(@"call status must be \[(.*)\]")]
        public async Task ThenCallRecordWasUpdated(VideoCallStatus status)
        {
            var @call = await _context.GetRecord<CallEntity>(Constants.CallId).ConfigureAwait(false);

            @call.InVideoCallStatus.Should().Be(status);
        }
    }
}