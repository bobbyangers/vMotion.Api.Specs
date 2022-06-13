using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture;

using CorePush.Interfaces;

using FluentAssertions;

using MongoDB.Entities;

using Newtonsoft.Json;

using NSubstitute;

using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Infrastructure;

using vMotion.Dal;
using vMotion.Dal.MongoDb;
using vMotion.Dal.MongoDb.Entities;

namespace vMotion.Api.Specs.Steps
{
    [Binding]
    [Scope(Feature = "CallAgentCalls")]
    public class CallAgentCallsSteps
    {
        private readonly ScenarioContext _context;
        private readonly WebHostSupport _webHost;

        private readonly IFixture _fixture;
        private readonly ISpecFlowOutputHelper _output;

        public CallAgentCallsSteps(ScenarioContext context, WebHostSupport webHost, IFixture fixture, ISpecFlowOutputHelper output)
        {
            _context = context;
            _webHost = webHost;
            _fixture = fixture;
            _output = output;
        }

        #region BeforeScenario
        [BeforeScenario(Order = 100)]
        public void BeforeScenario()
        {
            var f = _fixture;

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

            f.Customize<DeviceElement>(x => x
                .With(_ => _.DeviceId, () => Guid.NewGuid().ToString("N"))
                .With(_ => _.DeviceType, DeviceType.iOS)
                .With(_ => _.UserAgent, "iOS")
                .With(_ => _.GeoLocation)
            );

            var rdn = new Random();
            var nextYr = DateTime.Today.AddYears(1).Year;

            f.Customize<MemberCreditCardAddressElement>(x => x
                .With(_ => _.Line1, "234 somewhere street")
                .With(_ => _.City, "Montreal")
                .With(_ => _.Zip, "H1H1H1")
                .With(_ => _.State, "QC")
                .With(_ => _.Country, "CA")
            );

            f.Customize<MemberCreditCardEntity>(x => x
                .With(_ => _.StripeId, (Guid seed) => seed.ToString("N"))
                .With(_ => _.Last4, (string s) => rdn.Next(1, 9999).ToString("D4"))
                .With(_ => _.ExpiryYear, (int i) => rdn.Next(nextYr, nextYr + 10))
                .With(_ => _.ExpiryMonth, (int i) => rdn.Next(1, 12))
                .With(_ => _.Brand, "visa")
                .With(_ => _.Country, "ca")
                .With(_ => _.Address)
                .With(_ => _.StripeCustomerId, (Guid seed) => seed.ToString("N"))
                .With(_ => _.IsDefault, true)
                .With(_ => _.IsDeleted, false)
                .Without(_ => _.ID)
                .Without(_ => _.CreatedOn)
            );
        }
        #endregion

        [Given("a call exists")]
        public async Task GivenACallExists()
        {
            var @operator = await _context.GetRecord<OperatorEntity>(Constants.OperatorId);
            var member = await _context.GetRecord<MemberEntity>(Constants.MemberId);

            var @case = _fixture.Create<CaseEntity>().Then(_ =>
            {
                _.ByMember = member.ID;
                _.Operator = @operator.ID;
                _.SelectedCard = Guid.NewGuid().ToObjectId();
            });
            @case.CaseNumber = await DB.Entity<CaseEntity>().NextSequentialNumberAsync();

            await @case.SaveAsync();

            _output.WriteLine($"{nameof(CaseEntity)}[{@case.ID.ToGuid():D}]");

            var @call = _fixture.Create<CallEntity>()
                .Then(_ =>
                {
                    _.ByMember = member.ID;
                    _.RelatedCase = @case.ID;
                    _.Operator = @operator.ID;

                    _.AddToHistory(DateTime.Now, RecordReference.Empty, "Call created", "New case");
                });

            await call.SaveAsync().ConfigureAwait(false);

            _output.WriteLine($"{nameof(CallEntity)}[{@call.ID.ToGuid():D}]");

            await @case.Calls.AddAsync(@call.ID, null, CancellationToken.None).ConfigureAwait(false);

            var result = await DB.Update<MemberEntity>()
                .MatchID(member.ID)
                .Modify(_ => _.CurrentCall, @call.ID)
                .ExecuteAsync()
                .ConfigureAwait(false);
            result.ModifiedCount.Should().Be(1);

            _context.Set(@case.ID.ObjectIdToGuidString(), Constants.CaseId);
            _context.Set(call.ID.ObjectIdToGuidString(), Constants.CallId);
        }

        [Given("device info is saved for member")]
        public async Task GivenDeviceInfoSaveForMember()
        {
            var member = await _context.GetRecord<MemberEntity>(Constants.MemberId);

            var device = _fixture.Create<DeviceElement>();

            var result = await DB.Update<MemberEntity>()
                .MatchID(member.ID)
                .Modify(_ => _.Device, device)
                .ExecuteAsync()
                .ConfigureAwait(false);
            result.ModifiedCount.Should().Be(1);

        }

        [Given("no current call is set for staff")]
        public async Task WhenNoCurrentCallIsSet()
        {
            var staffId = _context.Get<string>(Constants.StaffId).ToObjectId();

            var result = await DB.Update<StaffEntity>()
                .MatchID(staffId)
                .Modify(s => s.CurrentCall, null)
                .ExecuteAsync()
                .ConfigureAwait(false);

            result.ModifiedCount.Should().Be(1);
        }

        [Given("current call is set for staff")]
        public async Task GivenCurrentCallIsSet()
        {
            var callId = _context.Get<string>(Constants.CallId).ToObjectId();
            var staffId = _context.Get<string>(Constants.StaffId).ToObjectId();

            var result1 = await DB.Update<StaffEntity>()
                .MatchID(staffId)
                .Modify(s => s.CurrentCall, callId)
                .ExecuteAsync()
                .ConfigureAwait(false);
            result1.ModifiedCount.Should().Be(1);

            var result2 = await DB.Update<CallEntity>()
                .MatchID(callId)
                .Modify(s => s.OngoingCallBy, staffId)
                .ExecuteAsync()
                .ConfigureAwait(false);
            result2.ModifiedCount.Should().Be(1);

            _output.WriteLine($"Setting [{staffId.ToGuid():D}].CurrentCal => [{callId.ToGuid():D}]");
        }

        [Given("member current call is set")]
        public async Task GivenMemberCurrentCallIsSet()
        {
            var member = await _context.GetRecord<MemberEntity>(Constants.MemberId);
            var @call = await _context.GetRecord<CallEntity>(Constants.CallId);

            var result = await DB.Update<MemberEntity>()
                .MatchID(member.ID)
                .Modify(s => s.CurrentCall, call.ID)
                .ExecuteAsync()
                .ConfigureAwait(false);
            result.ModifiedCount.Should().Be(1);
            _output.WriteLine($"Setting [{member.ID.ToGuid():D}].CurrentCall ({@call.ID.ToGuid():D})");
        }

        [Given("an add data to call request")]
        public void GivenAnAddDataToCallRequest()
        {
            var data = new
            {
                Data = new { custom = "value" }
            };

            _webHost.Content = JsonContent.Create(data);
        }


        [Given("a complete request")]
        public void GivenACompleteRequest()
        {
            var data = _fixture.Create<api._Features_.Agents.Calls.Put.Complete.Request>();

            _webHost.Content = JsonContent.Create(data);
        }

        [Given("an reassignment request to user")]
        public async Task GivenAnAssignmentRequestToUser()
        {
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Given("an reassignment request to type (.*)")]
        public void GivenAnAssignmentRequestToType(UserTypeEnum user)
        {
            var data = new
            {
                AssignToType = user
            };

            _webHost.Content = JsonContent.Create(data);
        }

        [Given("a request to reschedule")]
        public void GivenARequestToReschedule()
        {
            var data = new
            {
                AssignTo = Guid.Empty,
                ScheduleCallbackTime = DateTime.UtcNow.AddDays(7)
            };

            _webHost.Content = JsonContent.Create(data);
        }

        [Given(@"a member has a credit card set")]
        public async Task GivenAMemberHasACreditCardSet()
        {
            var @member = await _context.GetRecord<MemberEntity>(Constants.MemberId).ConfigureAwait(false);
            var card = _fixture.Create<MemberCreditCardEntity>();
            card.Member = @member.ID;

            await DB.SaveAsync(card);

            await @member.CreditCards.AddAsync(card.ID);

            _context.Set(card.ID.ObjectIdToGuidString(), Constants.CCardId);

        }


        [Then("staff current call was updated")]
        public async Task ThenStaffCurrentCallWasUpdated()
        {
            var record = await _context.GetRecord<StaffEntity>(Constants.StaffId).ConfigureAwait(false);

            var callId = _context.Get<string>(Constants.CallId)?.ToObjectId();
            record.CurrentCall.ID.Should().Be(callId);
        }

        [Then(@"call status must be \[(.*)\]")]
        public async Task ThenCallRecordWasUpdated(VideoCallStatus status)
        {
            var @call = await _context.GetRecord<CallEntity>(Constants.CallId).ConfigureAwait(false);

            @call.InVideoCallStatus.Should().Be(status);
        }

        [Then(@"call has data set")]
        public async Task ThenCallHasDataSet()
        {
            var @call = await _context.GetRecord<CallEntity>(Constants.CallId).ConfigureAwait(false);

            @call.Data.Should().NotBeNullOrWhiteSpace();
        }


        [Then(@"response data should have (\d+) item\(s\)")]
        public async Task ThenResponseDataShouldHaveOneItem(int count)
        {
            var response = _webHost.Response;

            var payload = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var responseData = JsonConvert.DeserializeObject<List<object>>(payload);

            (responseData?.Should().HaveCount(count))
                .Should().NotBeNull();
        }

        [Then("push notification service was invoked")]
        public async Task ThenNotificationHubWasInvoked()
        {
            await _webHost.GetActor<IApnSender>().Received(1)
                .SendAsync(Arg.Any<object>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<int>(),
                    Arg.Any<int>(),
                    Arg.Any<bool>(),
                    Arg.Any<CancellationToken>())
                .ConfigureAwait(false);
        }

        [Then("staff current call was cleared")]
        public async Task ThenStaffCurrentCallWasCleared()
        {
            var record = await _context.GetRecord<StaffEntity>(Constants.StaffId).ConfigureAwait(false);

            record.CurrentCall.Should().BeNull();
        }

        [Then("member current call was cleared")]
        public async Task ThenMemberCurrentCallWasCleared()
        {
            var record = await _context.GetRecord<MemberEntity>(Constants.MemberId).ConfigureAwait(false);

            record.CurrentCall.Should().BeNull();
        }
    }
}