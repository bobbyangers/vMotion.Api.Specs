using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture;

using AutoMapper;

using FluentAssertions;

using MongoDB.Entities;

using Newtonsoft.Json;

using NSubstitute;

using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Infrastructure;

using vMotion.Dal;
using vMotion.Dal.MongoDb.Entities;

namespace vMotion.Api.Specs.Steps
{
    [Binding]
    [Scope(Feature = "Guest")]
    public class GuestSteps : TechTalk.SpecFlow.Steps
    {
        private readonly ScenarioContext _context;
        private readonly WebHostSupport _webHost;
        private readonly IFixture _fixture;
        private readonly IMapper _mapper;
        private readonly ISpecFlowOutputHelper _output;

        public GuestSteps(ScenarioContext context, WebHostSupport webHost, IFixture fixture, ISpecFlowOutputHelper output)
        {
            _context = context;
            _webHost = webHost;
            _fixture = fixture;
            _output = output;

            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<NotificationTableData, MemberNotificationEntity>()
                    .ForMember(_ => _.ID, _ => _.MapFrom(s => (string)null))
                    .ForMember(_ => _.NotificationType, _ => _.MapFrom(s => s.NotificationType))
                    .ForMember(_ => _.Description, _ => _.MapFrom(s => s.Description))
                    .ForMember(_ => _.IsRead, _ => _.MapFrom(s => s.IsRead))
                    .ForMember(_ => _.IsArchived, _ => _.MapFrom(s => s.IsArchived))
                    ;
            });

            _mapper = config.CreateMapper();

        }

        [BeforeScenario(Order = 100)]
        public void BeforeScenario()
        {
            CustomizeDomain(_fixture);
        }

        private void CustomizeDomain(IFixture f)
        {
            f.Customize<CaseEntity>(x => x
                .With(_ => _.ID, (string)null)
                .With(_ => _.Status, CaseStatus.New)
                .With(_ => _.InfoDateValidated, DateTime.UtcNow.AddMinutes(-1))
                .With(_ => _.ReservationId, Guid.NewGuid().ToString("N"))
                .OmitAutoProperties()
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

            f.Customize<MemberNotificationEntity>(x => x
                .With(_ => _.ID, (string)null)
                .With(_ => _.Description)
                .With(_ => _.NotificationType, NotificationEnum.Alert)
                .OmitAutoProperties()
            );
        }



        [Given("guest has a current call set")]
        public async Task GivenTheGuestHasACurrentCallSet()
        {
            var @operator = await _context.GetRecord<OperatorEntity>(Constants.OperatorId);

            var member = await _context.GetRecord<MemberEntity>(Constants.MemberId).ConfigureAwait(false);
            var staff = await _context.GetRecord<StaffEntity>(Constants.StaffId).ConfigureAwait(false);

            var @caseId = DB.Entity<CaseEntity>().GenerateNewID();
            var @call = _fixture.Create<CallEntity>().Then(_ =>
            {
                _.ByMember = member.ID;
                _.RelatedCase = @caseId;
                _.Operator = @operator.ID;
                _.AssignedStaff = staff.ID;
                _.OngoingCallBy = staff.ID;
                _.AddToHistory(DateTime.Now, RecordReference.Empty, "Call created", "New case");
            });
            await call.SaveAsync().ConfigureAwait(false);

            var @case = _fixture.Create<CaseEntity>().Then(_ =>
            {
                _.Operator = @operator.ID;
                _.ByMember = member.ID;
            });
            @case.CaseNumber = await DB.Entity<CaseEntity>().NextSequentialNumberAsync();

            await @case.SaveAsync().ConfigureAwait(false);

            await @case.Calls.AddAsync(@call.ID, null, CancellationToken.None).ConfigureAwait(false);

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

        [Given("guest has no current call set")]
        public async Task GivenGuestHasNoCurrentCallSet()
        {
            var member = await _context.GetRecord<MemberEntity>(Constants.MemberId).ConfigureAwait(false);
            var result = await DB.Update<MemberEntity>()
                .MatchID(member.ID)
                .Modify(_ => _.CurrentCall, null)
                .ExecuteAsync()
                .ConfigureAwait(false);
            result.ModifiedCount.Should().Be(1);

        }


        [Given(@"case is assigned")]
        public async Task WhenCaseIsAssigned()
        {
            var @call = await _context.GetRecord<CallEntity>(Constants.CallId).ConfigureAwait(false);
            var staff = await _context.GetRecord<StaffEntity>(Constants.StaffId).ConfigureAwait(false);

            var result = await DB.Update<CallEntity>()
                .MatchID(@call.ID)
                .Modify(_ => _.AssignedStaff, staff.ID)
                .Modify(_ => _.OngoingCallBy, staff.ID)
                .ExecuteAsync()
                .ConfigureAwait(false);

            result.ModifiedCount.Should().Be(1);
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

        [Given("a picture with ext (JPG|PNG) needs to be uploaded")]
        public void GivenAPictureWithExtensionNeedsToBeUploaded(string ext)
        {
            var image = ext == "PNG"
                ? new MemoryStream(Properties.Resources.star)
                : new MemoryStream(Properties.Resources.file_example_JPG_100kB);

            var content = new StreamContent(image);
            var request = new MultipartFormDataContent
            {
                { content, "memberfile", $"star.{ext.ToLower()}" }
            };

            _webHost.Content = request;
        }

        [Given("an update my info request")]
        public void GivenAnUpdateMyInfoRequest()
        {
            _webHost.Data = _fixture.Build<api._Features_.Clients.Me.Put.Request>()
                .Without(_ => _.Id)
                .Create();

            _webHost.Content = JsonContent.Create(_webHost.Data);
        }

        [Given("guest has not completed yet")]
        public async Task GivenGuestHasNotCompletedYet()
        {
            var member = await _context.GetRecord<MemberEntity>(Constants.MemberId).ConfigureAwait(false);

            member.SignUpDate = null;
            await member.SaveAsync().ConfigureAwait(false);
        }

        [Given(@"a create case request")]
        public void GivenACreateCaseRequest()
        {
            var payload = new api._Features_.Clients.Cases.Post.Request
            {
                OperatorId = _context.Get<string>(Constants.OperatorId).ToGuid(),
                ReservationId = "0001",
                Intention = "Need help!"
            };

            _webHost.Content = JsonContent.Create(payload);
        }

        [Given(@"location and device headers are set")]
        public void GivenLocationAndDeviceHeadersAreSet()
        {
            var headers = _webHost.Client.DefaultRequestHeaders;

            headers.Add("x-geolocation", JsonConvert.SerializeObject(new GeoLocationDto { Lat = 45, Lon = -105 }, Formatting.None));
            headers.Add("x-deviceid", Guid.NewGuid().ToString("N"));
            headers.Add("x-devicetype", "iOS");
            headers.Add("x-ipaddress", "127.0.0.1");

            headers.UserAgent.Clear();
            headers.UserAgent.Add(new ProductInfoHeaderValue("iOS", "14.0"));
            headers.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
            headers.UserAgent.Add(new ProductInfoHeaderValue("ONEPLUS", "A5000"));
            headers.UserAgent.Add(new ProductInfoHeaderValue("AppKey", "537.36"));
            headers.UserAgent.Add(new ProductInfoHeaderValue("Chrome", "78.0.3904"));
            headers.UserAgent.Add(new ProductInfoHeaderValue("Safari", "53"));
        }

        [Given(@"a case exists with no active calls")]
        public async Task GivenACaseExistsWithNoActiveCalls()
        {
            var op = await _context.GetRecord<OperatorEntity>(Constants.OperatorId).ConfigureAwait(false);
            var member = await _context.GetRecord<MemberEntity>(Constants.MemberId).ConfigureAwait(false);

            var @case = new CaseEntity
            {
                ID = (string)null,
                CaseNumber = await DB.Entity<CaseEntity>().NextSequentialNumberAsync().ConfigureAwait(false),
                ByMember = member.ID,
                Operator = op.ID,
            };
            await @case.SaveAsync().ConfigureAwait(false);

            _context.Add(Constants.CaseId, @case.ID.ObjectIdToGuidString());
        }

        [Given(@"a create call request")]
        public void GivenACreateCallRequest()
        {
            var payload = new api._Features_.Clients.Calls.Post.Request()
            {
                CaseId = _context.Get<string>(Constants.CaseId).ToGuid(),
            };

            _webHost.Content = JsonContent.Create(payload);
        }


        [Given("with these notifications in database")]
        public async Task GivenWithTheseCallsInDatabase(IEnumerable<NotificationTableData> table)
        {
            var op = await _context.GetRecord<OperatorEntity>(Constants.OperatorId);

            var member = await _context.GetRecord<MemberEntity>(Constants.MemberId)
                .ConfigureAwait(false);

            var records = new List<string>();
            foreach (var row in table)
            {
                var record = _mapper.Map<MemberNotificationEntity>(row);

                record.Operator = op.ID;
                record.Member = member.ID;

                await record.SaveAsync().ConfigureAwait(false);

                records.Add(record.ID);
            }

            await member.Notifications.AddAsync(records);
        }

        [Given("pickup workflow is empty")]
        public async Task GivenPickupWorkflowIsEmpty()
        {
            var @case = await _context.GetRecord<CaseEntity>(Constants.CaseId).ConfigureAwait(false);

            await DB.DeleteAsync<CasePickupWorkflowEntity>(@case.ID).ConfigureAwait(false);
        }

        [Given("a notification record exists")]
        public async Task GivenANotificationRecordExists()
        {
            var opId = _context.Get<string>(Constants.OperatorId).ToObjectId();

            var member = await _context.GetRecord<MemberEntity>(Constants.MemberId)
                .ConfigureAwait(false);

            if (!member.Notifications.Any())
            {
                var data = _fixture.Create<MemberNotificationEntity>();
                data.Operator = opId;
                data.Member = member.ID;

                await data.SaveAsync();

                await member.Notifications.AddAsync(data.ID);
            }

            _context.Set(member.Notifications.First().ID.ObjectIdToGuidString(), Constants.NotificationId);
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

        [Then(@"call status must be \[(.*)\]")]
        public async Task ThenCallRecordWasUpdated(VideoCallStatus status)
        {
            var @call = await _context.GetRecord<CallEntity>(Constants.CallId).ConfigureAwait(false);

            @call.InVideoCallStatus.Should().Be(status);
        }

        [Then("a list of operators")]
        public async Task ThenAListOfOperators()
        {
            var responseData = await _webHost.Response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var body = JsonConvert.DeserializeObject<OperatorSummaryDto[]>(responseData);
            body.Should().HaveCountGreaterThan(0);
        }

        [Then("guest record was updated")]
        public async Task ThenGuestRecordWasUpdated()
        {
            var member = await _context.GetRecord<MemberEntity>(Constants.MemberId).ConfigureAwait(false);

            member.Name.Should().Contain("Joe");
        }

        [Then("guest record has device info")]
        public async Task ThenGuestRecordHasDeviceInfo()
        {
            var member = await _context.GetRecord<MemberEntity>(Constants.MemberId).ConfigureAwait(false);

            member.Device.Should().NotBeNull();

            member.Device.DeviceId.Should().NotBeNullOrWhiteSpace();
        }

        [Then(@"call record has device info")]
        public async Task ThenCallRecordHasDeviceInfo()
        {
            var responseData = await _webHost.Response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var body = JsonConvert.DeserializeObject<vMotion.api._Features_.Clients.Cases.Post.Response>(responseData);

            var callId = body!.Call.Id.ToObjectId();
            var @call = await _context.GetRecordById<CallEntity>(callId).ConfigureAwait(false);

            @call.Device.Should().NotBeNull();
            @call.Device.DeviceId.Should().NotBeNullOrWhiteSpace();
            @call.Device.DeviceType.Should().NotBe(DeviceType.Unknown);
            @call.Device.GeoLocation.Should().NotBeNull();
        }

        [Then("member-notification record was updated")]
        public async Task ThenMemberNotificationRecordWasUpdated()
        {
            ////await Support.FakeDb.Notifications.ReceivedWithAnyArgs().UpdateAsync(default);
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Then("member-notification record was marked \\[(.*)\\]")]
        public async Task ThenMemberNotificationRecordWasUpdatedAndMarkedAsUnread(bool expected)
        {
            var member = await _context.GetRecord<MemberEntity>(Constants.MemberId)
                .ConfigureAwait(false);


            var note = member.Notifications.First();
            note.IsRead.Should().Be(expected);
        }

        [Then("member-notification record was marked as archived")]
        public async Task ThenMemberNotificationRecordWasUpdatedAndMarkedAsArchived()
        {
            var member = await _context.GetRecord<MemberEntity>(Constants.MemberId)
                .ConfigureAwait(false);

            var note = member.Notifications.First();
            note.IsArchived.Should().BeTrue();
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

        [Then(@"blob storage upload was NOT invoked")]
        public void ThenBlobStorageUploadWasNotInvoked()
        {
            _webHost.GetActor<IBlobStorage>().DidNotReceiveWithAnyArgs().UploadBlob(default, default);
        }

    }
}