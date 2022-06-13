using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture;

using FluentAssertions;

using MongoDB.Entities;

using Newtonsoft.Json;

using NSubstitute;

using TechTalk.SpecFlow;

using vMotion.Dal;
using vMotion.Dal.MongoDb.Entities;

namespace vMotion.Api.Specs.Steps
{
    [Binding]
    [Scope(Feature = "Member")]
    public class MemberSteps : TechTalk.SpecFlow.Steps
    {
        private readonly ScenarioContext _context;
        private readonly WebHostSupport _webHost;
        private readonly IFixture _fixture;

        public MemberSteps(ScenarioContext context, WebHostSupport webHost, IFixture fixture)
        {
            _context = context;
            _webHost = webHost;
            _fixture = fixture;
        }

        [BeforeScenario(Order = 100)]
        public void BeforeScenario()
        {
            var f = _fixture;

            f.Customize<MemberCreditCardEntity>(x => x
                .With(_ => _.ID, (string)null)
                .With(_ => _.Last4, "9960")
                .With(_ => _.Brand, "VI")
                .With(_ => _.Country, "CA")
                .With(_ => _.IsDefault, true)
                .With(_ => _.IsDeleted, false)
                .Without(_ => _.Member)
            );
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

        [Given("member has not completed yet")]
        public async Task GivenMemberHasNotCompletedYet()
        {
            var member = await _context.GetRecord<MemberEntity>(Constants.MemberId).ConfigureAwait(false);

            member.SignUpDate = null;
            await member.SaveAsync().ConfigureAwait(false);
        }

        [Given("member has completed signup")]
        public async Task GivenMemberHasCompletedSignup()
        {
            var member = await _context.GetRecord<MemberEntity>(Constants.MemberId).ConfigureAwait(false);

            member.SignUpDate = DateTime.UtcNow.AddDays(-1).Date;
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
                ID = null,
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


        [Given("configure authentication service")]
        public void WhenConfigureAuthenticationService()
        {
            var memberId = _context.Get<string>(Constants.MemberId);

            var service = _webHost.GetActor<IAuthenticationService>();

            service.DoLogout(Arg.Any<string>(), Arg.Any<string>());

            var response = _fixture.Build<GetUserResponse>().With(x => x.Id, memberId.ToObjectId()).Create();

            service.GetUser(Arg.Any<GetUserRequest>(), Arg.Any<CancellationToken>())
                .Returns(response);
        }

        [Then("a list of operators")]
        public async Task ThenAListOfOperators()
        {
            var responseData = await _webHost.Response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var body = JsonConvert.DeserializeObject<OperatorSummaryDto[]>(responseData);
            body.Should().HaveCountGreaterThan(0);
        }

        [Then("member claim was added")]
        public void ThenMemberClaimWasAdded()
        {
            var idService = _webHost.GetActor<IAuthenticationService>();

            idService.ReceivedWithAnyArgs(1).AddClaims(Guid.Empty, default, default);
        }

        [Then("member record was updated")]
        public async Task ThenMemberRecordWasUpdated()
        {
            var member = await _context.GetRecord<MemberEntity>(Constants.MemberId).ConfigureAwait(false);

            member.Name.Should().Contain("Joe");
        }

        [Then("member record has device info")]
        public async Task ThenMemberRecordHasDeviceInfo()
        {
            var member = await _context.GetRecord<MemberEntity>(Constants.MemberId).ConfigureAwait(false);

            member.Device.Should().NotBeNull();

            member.Device.DeviceId.Should().NotBeNullOrWhiteSpace();
        }

        [Then(@"call record has device info")]
        public async Task ThenCallRecordHasDeviceInfo()
        {
            var responseData = await _webHost.Response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var body = JsonConvert.DeserializeObject<api._Features_.Clients.Cases.Post.Response>(responseData);

            var callId = body!.Call.Id.ToObjectId();
            var @call = await _context.GetRecordById<CallEntity>(callId).ConfigureAwait(false);

            @call.Device.Should().NotBeNull();
            @call.Device.DeviceId.Should().NotBeNullOrWhiteSpace();
            @call.Device.DeviceType.Should().NotBe(DeviceType.Unknown);
            @call.Device.GeoLocation.Should().NotBeNull();
        }

    }
}