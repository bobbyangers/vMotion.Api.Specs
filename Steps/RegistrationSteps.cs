using AutoFixture;
using FluentAssertions;
using MongoDB.Entities;
using Newtonsoft.Json;
using NSubstitute;
using System;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using vMotion.Dal;
using vMotion.Dal.MongoDb;
using vMotion.Dal.MongoDb.Entities;

namespace vMotion.Api.Specs.Steps
{
    [Binding]
    [Scope(Feature = "Registration")]
    public class RegistrationSteps : TechTalk.SpecFlow.Steps
    {
        private readonly ScenarioContext _context;
        private readonly WebHostSupport _webHost;
        private readonly IFixture _fixture;

        public RegistrationSteps(ScenarioContext context, WebHostSupport webHost, IFixture fixture)
        {
            _context = context;
            _webHost = webHost;
            _fixture = fixture;
        }

        #region Overrides of AutoFixtureTests
        [BeforeScenario(Order = 100)]
        public void BeforeScenario(IFixture fixture)
        {
            fixture.Customize<AuthData>(_ => _
                .With(x => x.ExpiresIn, TimeSpan.FromMinutes(60).Seconds)
                .With(x => x.TokenType, "Bearer")
            );

            fixture.Customize<api._Features_.Clients.Register.Post.Request>(_ => _
                .With(x => x.OperatorId, DbNames.Operator1_ID.ToGuid())
                .Without(x => x.UserId)
                .Without(x => x.Now)
            );

            fixture.Customize<api._Features_.Clients.Me.Put.Request>(_ => _
                .Without(x => x.Id)
            );
        }

        #endregion

        [Given("complete registration data")]
        public void GivenCompleteRegistrationData()
        {
            var data = _fixture.Create<api._Features_.Clients.Register.Post.Request>();

            _webHost.Content = JsonContent.Create(data);
        }

        [Given("incomplete registration data")]
        public void GivenIncompleteRegistrationData()
        {
            var data = _fixture.Create<api._Features_.Clients.Register.Post.Request>();
            data.Password = null;
            data.Email = null;

            _webHost.Content = JsonContent.Create(data);
        }

        [Given("registration data with existing email")]
        public async Task GivenRegistrationDataWithExistingEmail()
        {
            var member = _fixture.Create<MemberEntity>();
            await member.SaveAsync().ConfigureAwait(false);

            var data = _fixture.Create<api._Features_.Clients.Register.Post.Request>();
            data.Email = member.Email;

            _webHost.Content = JsonContent.Create(data);
        }

        [Given("configure authentication service")]
        public void GivenConfigureAuthenticationService()
        {
            if (!_context.TryGetValue(Constants.MemberId, out string memberId))
            {
                memberId = Guid.NewGuid().ToString("D");
                _context.Add(Constants.MemberId, memberId);
            }

            var service = _webHost.GetActor<IAuthenticationService>();

            service.CreateUser(Arg.Any<CreateUserRequest>(), Arg.Any<CancellationToken>())
                .Returns(memberId.ToObjectId());

            var response = _fixture.Build<GetUserResponse>().With(x => x.Id, memberId.ToObjectId()).Create();

            service.GetUser(Arg.Any<GetUserRequest>(), Arg.Any<CancellationToken>())
                .Returns(response);
        }

        [Then("should receive a token")]
        public async Task ThenShouldReceiveAToken()
        {
            var response = _webHost.Response;

            var body = JsonConvert.DeserializeObject<AuthData>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            body.Should().NotBeNull();
            body.AccessToken.Should().NotBeEmpty();
        }

        [Then("user was created")]
        public async Task ThenUserWasCreated()
        {
            await _webHost.GetActor<IAuthenticationService>().ReceivedWithAnyArgs(1)
                .CreateUser(default, default)
                .ConfigureAwait(false);
        }
    }
}