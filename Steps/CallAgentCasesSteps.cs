using AutoFixture;
using FluentAssertions;
using MongoDB.Entities;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Infrastructure;
using vMotion.Dal;
using vMotion.Dal.MongoDb;
using vMotion.Dal.MongoDb.Entities;

namespace vMotion.Api.Specs.Steps
{
    [Binding]
    [Scope(Feature = "CallAgentCases")]
    public class CallAgentCasesSteps : TechTalk.SpecFlow.Steps
    {
        private readonly ScenarioContext _context;
        private readonly WebHostSupport _webHost;
        private readonly IFixture _fixture;
        private readonly ISpecFlowOutputHelper _output;

        public CallAgentCasesSteps(ScenarioContext context, WebHostSupport support, IFixture fixture, ISpecFlowOutputHelper output)
        {
            _context = context;
            _webHost = support;
            _fixture = fixture;
            _output = output;

            CustomizeDomain(fixture);
        }

        private void CustomizeDomain(IFixture f)
        {
            f.Customize<api._Features_.Agents.Cases.Pickup_workflow.Post.Request>(x => x
                .Without(_ => _.Id)
                .Without(_ => _.UserId)
                .With(_ => _.Inspection, new object())
            );


            f.Customize<WorkflowStepDto>(x => x
                .With(_ => _.Data, new object())
            );

            f.Customize<CallEntity>(x => x
                .With(_ => _.ID, (string)null)
                .With(_ => _.Status, CallStatus.NewCase)
                .With(_ => _.InVideoCallStatus, VideoCallStatus.Waiting)
                .With(_ => _.AssignedToUserType, UserTypeEnum.Agent)
                .With(_ => _.InQueueTime, DateTime.Now.AddMinutes(-1))
                .With(_ => _.Device)
                .OmitAutoProperties()
            );

            f.Customize<WorkflowStepElement>(x => x
                .With(_ => _.Color)
                .With(_ => _.Data, "{}")
            );

            f.Customize<CasePickupWorkflowEntity>(x => x
                .With(_ => _.ID, (string)null)
                .With(_ => _.Inspection, "{}")
            );

            f.Customize<DeviceElement>(x => x
                .With(_ => _.DeviceId, () => Guid.NewGuid().ToString("N"))
                .With(_ => _.DeviceType, DeviceType.iOS)
                .With(_ => _.UserAgent, "iOS")
                .With(_ => _.GeoLocation)
            );
        }

        [Given("the member has a current call set")]
        public async Task GivenTheMemberHasACurrentCallSet()
        {
            var member = await _context.GetRecord<MemberEntity>(Constants.MemberId).ConfigureAwait(false);

            var @caseId = DB.Entity<CaseEntity>().GenerateNewID();

            var call = _fixture.Create<CallEntity>().Then(_ =>
            {
                _.ID = null;
                _.ByMember = member.ID;
                _.RelatedCase = @caseId;
            });
            await call.SaveAsync().ConfigureAwait(false);

            _output.WriteLine($"{nameof(CallEntity)}[{call.ID.ToGuid():D}]");

            var caseNbr = await DB.Entity<CaseEntity>().NextSequentialNumberAsync().ConfigureAwait(false);

            var @case = new CaseEntity
            {
                ID = @caseId,
                CaseNumber = caseNbr,
                ByMember = member.ID,
                InfoDateValidated = null
            };

            await @case.SaveAsync().ConfigureAwait(false);

            _output.WriteLine($"{nameof(CaseEntity)}[{@case.ID.ToGuid():D}]");

            await @case.Calls.AddAsync(call.ID).ConfigureAwait(false);

            member.CurrentCall = call.ID;
            await member.SaveOnlyAsync(_ => new { _.CurrentCall }).ConfigureAwait(false);

            _context.Set(@case.ID.ObjectIdToGuidString(), Constants.CaseId);
            _context.Set(call.ID.ObjectIdToGuidString(), Constants.CallId);
        }

        [Given("current call is set for staff")]
        public async Task GivenCurrentCallIsSet()
        {
            var callId = _context.Get<string>(Constants.CallId).ToObjectId();
            var staffId = _context.Get<string>(Constants.StaffId).ToObjectId();

            var result = await DB.Update<StaffEntity>()
                .MatchID(staffId)
                .Modify(s => s.CurrentCall, callId)
                .ExecuteAsync()
                .ConfigureAwait(false);
            result.ModifiedCount.Should().Be(1);

            var result2 = await DB.Update<CallEntity>()
                .MatchID(callId)
                .Modify(s => s.OngoingCallBy, staffId)
                .ExecuteAsync()
                .ConfigureAwait(false);
            result2.ModifiedCount.Should().Be(1);

            _output.WriteLine($"Setting {nameof(StaffEntity)}[{staffId.ToGuid():D}].{nameof(StaffEntity.CurrentCall)} => [{callId.ToGuid():D}]");
        }

        [Given(@"calls exists recently ended")]
        public async Task GivenCallsExistsRecentlyEnded()
        {
            var member = await _context.GetRecord<MemberEntity>(Constants.MemberId).ConfigureAwait(false);

            var dt = DateTime.UtcNow;

            var @case1_Id = DB.Entity<CaseEntity>().GenerateNewID();

            await CreateTimeSensitiveCall(@case1_Id, member.ID, dt.AddMinutes(-37));

            await CreateTimeSensitiveCall(@case1_Id, member.ID, dt.AddMinutes(-19));

            await CreateTimeSensitiveCall(@case1_Id, member.ID, dt.AddMinutes(-11));


            var @case2_Id = DB.Entity<CaseEntity>().GenerateNewID();

            await CreateTimeSensitiveCall(@case2_Id, member.ID, dt.AddMinutes(-37));

            await CreateTimeSensitiveCall(@case2_Id, member.ID, dt.AddMinutes(-19));

            await CreateTimeSensitiveCall(@case2_Id, member.ID, dt.AddMinutes(-11));
        }

        [Given(@"calls exists assignedTo")]
        public async Task GivenCallsExistsAssignedTo()
        {
            var member = await _context.GetRecord<MemberEntity>(Constants.MemberId).ConfigureAwait(false);

            var staff = await _context.GetRecord<StaffEntity>(Constants.StaffId).ConfigureAwait(false);

            var dt = DateTime.UtcNow;

            var @case1_Id = DB.Entity<CaseEntity>().GenerateNewID();
            var @case2_Id = DB.Entity<CaseEntity>().GenerateNewID();
            var @case3_Id = DB.Entity<CaseEntity>().GenerateNewID();

            await CreateTimeSensitiveCall(@case1_Id, member.ID, dt.AddDays(-14), staff.ID);

            await CreateTimeSensitiveCall(@case2_Id, member.ID, dt.AddMinutes(-8), staff.ID);

            await CreateTimeSensitiveCall(@case3_Id, member.ID, dt.AddMinutes(-3), staff.ID);
        }


        private async Task CreateTimeSensitiveCall(string caseId, string memberId, DateTime dt, string assignedTo = null)
        {
            var rand = new Random();
            var @call = _fixture.Create<CallEntity>().Then(_ =>
            {
                _.ID = null;
                _.ByMember = memberId;
                _.RelatedCase = caseId;
                _.CallTakenAtDateTime = dt;
                _.EndCallTime = dt.AddMinutes(-1 * rand.Next(2, 10));
            });
            if (!string.IsNullOrWhiteSpace(assignedTo))
            {
                @call.AssignedStaff = assignedTo;
                @call.AssignedToUserType = UserTypeEnum.Agent;
            }

            await @call.SaveAsync().ConfigureAwait(false);
        }

        [Given("a picture needs to be uploaded")]
        public void GivenAPictureNeedsToBeUploaded()
        {
            var image = new MemoryStream(Properties.Resources.star);
            var content = new StreamContent(image);
            var payload = new MultipartFormDataContent
            {
                { content, "file", "star.png" }
            };

            _webHost.Content = payload;
        }

        [Given("some pictures need to be uploaded")]
        public void GivenSomePicturesNeedToBeUploaded()
        {
            var image = new MemoryStream(Properties.Resources.star);
            var content = new StreamContent(image);
            var payload = new MultipartFormDataContent
            {
                { content, "files", "star.png" }
            };

            _webHost.Content = payload;
        }

        [Given(@"a (\d) notes for case")]
        public async Task GivenSetupNotesForCase(int numberOfNotes)
        {
            var member = await _context.GetRecord<MemberEntity>(Constants.MemberId).ConfigureAwait(false);

            var @caseId = DB.Entity<CaseEntity>().GenerateNewID();
            var caseNbr = await DB.Entity<CaseEntity>().NextSequentialNumberAsync().ConfigureAwait(false);

            var @case = _fixture.Build<CaseEntity>()
                .With(_ => _.ID, @caseId)
                .With(_ => _.CaseNumber, caseNbr)
                .With(_ => _.ByMember, member.ID)
                .With(_ => _.Status, CaseStatus.New)
                .With(_ => _.ModifiedOn, DateTime.UtcNow)
                .Without(_ => _.Notes)
                .Without(_ => _.Calls)
                .Without(_ => _.Pictures)
                .Without(_ => _.ClosedTime)
                .Without(_ => _.InfoDateValidated)
                .Create();

            await @case.SaveAsync().ConfigureAwait(false);

            var notes = _fixture.CreateMany<CaseNoteEntity>(numberOfNotes).ToList();

            notes.ForEach(n =>
            {
                n.ID = null;
                n.ByUserId = member.ID.ToGuid();
                n.VisibleUserType = UserTypeEnum.Agent;

                n.SaveAsync().Wait();
            });

            await @case.Notes.AddAsync(notes.Select(_ => _.ID)).ConfigureAwait(false);

            _context.Set(@case.ID.ObjectIdToGuidString(), Constants.CaseId);
            _context.Set(notes.Last().ID.ObjectIdToGuidString(), Constants.NoteId);
        }

        [Given("pickup workflow data exists")]
        public async Task GivenPickupWorkflowDataExists()
        {
            var caseId = _context.Get<string>(Constants.CaseId).ToObjectId();

            var @data = await _context.GetRecord<CasePickupWorkflowEntity>(Constants.CaseId,
                () => _fixture.Create<CasePickupWorkflowEntity>().Then(_ => _.ID = caseId)).ConfigureAwait(false);

            await @data.SaveAsync(null, CancellationToken.None).ConfigureAwait(false);

            _output.WriteLine($"{nameof(CasePickupWorkflowEntity)}[{@data.ID.ToGuid():D}]");
        }

        [Given("pickup workflow is empty")]
        public async Task GivenPickupWorkflowIsEmpty()
        {
            var caseId = _context.Get<string>(Constants.CaseId).ToObjectId();

            await DB.DeleteAsync<CasePickupWorkflowEntity>(caseId).ConfigureAwait(false);
        }

        [Given("pickup workflow payload is set")]
        public void GivenPickupWorkflowPayloadIsSet()
        {
            var data = _fixture.Create<api._Features_.Agents.Cases.Pickup_workflow.Post.Request>();

            _webHost.Content = HttpContentExtensions.CreateJsonContent(data);
        }

        [Then("case record was updated")]
        public async Task ThenCaseRecordWasUpdated()
        {
            var @case = await _context.GetRecord<CaseEntity>(Constants.CaseId).ConfigureAwait(false);

            @case.InfoDateValidated.Should().NotBeNull();
        }

        [Then("case note record was created")]
        public async Task ThenCaseRecordWasCreated()
        {
            var content = await _webHost.Response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var response = JsonConvert.DeserializeObject<vMotion.api._Features_.Agents.Cases.Notes.Post.Response>(content);
            var id = response?.NoteId.ToObjectId();

            var note = await _context.GetRecordById<CaseNoteEntity>(id).ConfigureAwait(false);
            note.Should().NotBeNull();
        }

        [Then("note record was deleted")]
        public async Task ThenNoteRecordWasDeleted()
        {
            var note = await _context.GetRecord<CaseNoteEntity>(Constants.NoteId, () => null).ConfigureAwait(false);
            note.Should().BeNull();
        }
    }
}