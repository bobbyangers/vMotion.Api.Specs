using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture;

using FluentAssertions;

using MongoDB.Entities;

using Newtonsoft.Json;

using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Infrastructure;

using vMotion.Dal;
using vMotion.Dal.MongoDb;
using vMotion.Dal.MongoDb.Entities;

namespace vMotion.Api.Specs.Steps
{
    [Binding]
    [Scope(Feature = "MemberCases")]
    public class MemberCasesSteps : TechTalk.SpecFlow.Steps
    {
        private readonly ScenarioContext _context;
        private readonly WebHostSupport _webHost;

        private IFixture _fixture;
        private readonly ISpecFlowOutputHelper _output;

        public MemberCasesSteps(WebHostSupport support, ScenarioContext context, ISpecFlowOutputHelper output)
        {
            _context = context;
            _webHost = support;
            _output = output;

        }

        [BeforeScenario(Order = 100)]
        public void BeforeScenario(IFixture fixture)
        {
            _fixture = fixture;

            fixture.Customize<CallEntity>(_ => _
                .With(x => x.ID, (string)null)
                .With(x => x.Status, CallStatus.NewCase)
                .With(x => x.InVideoCallStatus, VideoCallStatus.Waiting)
                .With(x => x.AssignedToUserType, UserTypeEnum.Agent)
                .With(x => x.InQueueTime, DateTime.Now.AddMinutes(-1))
                .OmitAutoProperties()
            );

            fixture.Customize<CaseEntity>(_ => _
                .With(x => x.ID, (string)null)
                .With(x => x.Status, CaseStatus.New)
                .With(x => x.InfoDateValidated, DateTime.UtcNow.AddMinutes(-1))
                .With(x => x.ReservationId, Guid.NewGuid().ToString("N"))
                .OmitAutoProperties()
            );

            fixture.Customize<CaseNoteEntity>(_ => _
                .With(x => x.ID, (string)null)
            );

            fixture.Customize<CasePickupWorkflowEntity>(_ => _
                .With(x => x.ID, (string)null)
                .With(x => x.Inspection, "{}")
            );

            fixture.Customize<MemberCreditCardEntity>(x => x
                .With(_ => _.ID, (string)null)
                .With(_ => _.Last4, "9960")
                .With(_ => _.Brand, "VI")
                .With(_ => _.Country, "CA")
                .With(_ => _.IsDefault, true)
                .With(_ => _.IsDeleted, false)
                .Without(_ => _.Member)
            );

            fixture.Customize<WorkflowStepDto>(x => x
                .With(_ => _.Data, new object())
            );

            fixture.Customize<WorkflowStepElement>(_ => _
                .With(x => x.Color)
                .With(x => x.Data, "{}")
            );

            fixture.Customize<api._Features_.Clients.Cases.Pickup_workflow.Post.Request>(x => x
                .Without(_ => _.Id)
                .Without(_ => _.UserId)
                .With(_ => _.Inspection, new object())
            );

        }

        [Given("the member has a current call set")]
        public async Task GivenTheMemberHasACurrentCallSet()
        {
            var @operator = await _context.GetRecord<OperatorEntity>(Constants.OperatorId).ConfigureAwait(false);
            var member = await _context.GetRecord<MemberEntity>(Constants.MemberId).ConfigureAwait(false);

            var @case = _fixture.Create<CaseEntity>().Then(_ =>
            {
                _.ByMember = member.ID;
                _.Operator = @operator.ID;
                _.SelectedCard = Guid.NewGuid().ToObjectId();
            });
            @case.CaseNumber = await DB.Entity<CaseEntity>().NextSequentialNumberAsync();
            await @case.SaveAsync();

            var call = _fixture.Create<CallEntity>();
            call.ByMember = member.ID;
            call.RelatedCase = @case.ID;
            await call.SaveAsync();

            await @case.Calls.AddAsync(@call.ID, null, CancellationToken.None);

            var result = await DB.Update<MemberEntity>()
                .MatchID(member.ID)
                .Modify(_ => _.CurrentCall, @call.ID)
                .ExecuteAsync()
                .ConfigureAwait(false);

            result.ModifiedCount.Should().Be(1);

            _context.Set(@case.ID.ObjectIdToGuidString(), Constants.CaseId);
            _context.Set(call.ID.ObjectIdToGuidString(), Constants.CallId);

            _output.WriteLine($"{nameof(CaseEntity)}[{@case.ID.ToGuid():D}]");
            _output.WriteLine($"{nameof(CallEntity)}[{call.ID.ToGuid():D}]");
        }

        [Given(@"case is assigned")]
        public async Task WhenCaseIsAssigned()
        {
            var @call = await _context.GetRecord<CallEntity>(Constants.CallId).ConfigureAwait(false);
            var staff = await _context.GetRecord<StaffEntity>(Constants.StaffId).ConfigureAwait(false);

            var result = await DB.Update<CallEntity>()
                .MatchID(@call.ID)
                .Modify(_ => _.AssignedStaff, staff.ID)
                .ExecuteAsync()
                .ConfigureAwait(false);
            result.ModifiedCount.Should().Be(1);
        }

        [Given(@"a credit card exists")]
        public async Task GivenACreditCardExists()
        {
            var member = await _context.GetRecord<MemberEntity>(Constants.MemberId).ConfigureAwait(false);
            var cc = _fixture.Create<MemberCreditCardEntity>();

            cc.Member = member.ID;

            await DB.SaveAsync(cc);

            await member.CreditCards.AddAsync(cc.ID);

            _context.Set(cc.ID.ObjectIdToGuidString(), Constants.CCardId);
        }

        [Given(@"setup (\d) notes for case")]
        public async Task GivenSetupNotesForCase(int numberOfNotes)
        {
            var @case = await _context.GetRecord<CaseEntity>(Constants.CaseId).ConfigureAwait(false);
            var @member = await _context.GetRecord<MemberEntity>(Constants.MemberId).ConfigureAwait(false);

            var notes = _fixture.CreateMany<CaseNoteEntity>(numberOfNotes).ToList();

            notes.ForEach(n =>
            {
                n.ByUserId = member.ID.ToGuid();
                n.VisibleUserType = UserTypeEnum.Customer;
            });

            await DB.SaveAsync(notes, null, CancellationToken.None).ConfigureAwait(false);

            await @case.Notes.AddAsync(notes.Select(_ => _.ID)).ConfigureAwait(false);
        }

        [Given("pickup workflow is empty")]
        public async Task GivenPickupWorkflowIsEmpty()
        {
            var @case = await _context.GetRecord<CaseEntity>(Constants.CaseId).ConfigureAwait(false);

            await DB.DeleteAsync<CasePickupWorkflowEntity>(@case.ID).ConfigureAwait(false);
        }

        [Given("pickup workflow data exists")]
        public async Task GivenPickupWorkflowDataExists()
        {
            var @case = await _context.GetRecord<CaseEntity>(Constants.CaseId).ConfigureAwait(false);

            var @data = await DB.Find<CasePickupWorkflowEntity>().OneAsync(@case.ID).ConfigureAwait(false)
                ?? _fixture.Create<CasePickupWorkflowEntity>().Then(_ =>
                {
                    _.ID = @case.ID;
                });

            await @data.SaveAsync(null, CancellationToken.None).ConfigureAwait(false);

            _output.WriteLine($"{nameof(CasePickupWorkflowEntity)}[{@data.ID.ToGuid():D}]");
        }

        [Given("pickup workflow payload is set")]
        public void GivenPickupWorkflowPayloadIsSet()
        {
            var data = _fixture.Create<api._Features_.Clients.Cases.Pickup_workflow.Post.Request>();

            _webHost.Content = HttpContentExtensions.CreateJsonContent(data);
        }

        [Then(@"case notes has (\d)")]
        public async Task ThenCaseNotesHas(int expected)
        {
            var body = JsonConvert.DeserializeObject<List<CaseNoteDto>>(
                await _webHost.Response.Content.ReadAsStringAsync().ConfigureAwait(false));

            body.Should().NotBeNull();
            body!.Count.Should().Be(expected);
        }

        [Then(@"the call info is not null")]
        public async Task ThenTheCallInfoIsNotNull()
        {
            var body = JsonConvert.DeserializeObject<CaseSummaryDto>(
                await _webHost.Response.Content.ReadAsStringAsync().ConfigureAwait(false));

            body.Should().NotBeNull();
            body!.Call.Should().NotBeNull();
        }
    }
}