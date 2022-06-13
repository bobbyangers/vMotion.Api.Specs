using AutoFixture;
using FluentAssertions;
using IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RichardSzalay.MockHttp;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using vMotion.api.Data.GlobeCarAuth;
using vMotion.api.Services.Implementation;
using vMotion.Dal;
using Xunit;
using Xunit.Abstractions;

namespace vMotion.Api.Specs.Unit_Tests
{
    [Trait(Constants.Category, Constants.CI)]
    public class IdServer4AuthServiceTests : AutoFixtureTests
    {
        private const string TargetUrl = "http://localhost:5001";

        private MockHttpMessageHandler MockHttp { get; }

        public IdServer4AuthServiceTests(ITestOutputHelper output) : base(output)
        {
            Services.AddAutoMapper(c => c.CreateMap<TokenResponse, AuthData>()
                .ForMember(_ => _.AccessToken, _ => _.MapFrom(s => s.AccessToken))
                .ForMember(_ => _.RefreshToken, _ => _.MapFrom(s => s.RefreshToken))
                .ForMember(_ => _.TokenType, _ => _.MapFrom(s => s.TokenType))
                .ForMember(_ => _.ExpiresIn, _ => _.MapFrom(s => s.ExpiresIn))
                );

            MockHttp = new MockHttpMessageHandler();
            Services.AddTransient(_ =>
            {
                var factory = Substitute.For<IHttpClientFactory>();
                var client = FunctionalExtensions.Then(MockHttp.ToHttpClient(output), c => c.BaseAddress = new Uri(TargetUrl));

                factory.CreateClient(default).ReturnsForAnyArgs(client);

                return factory;
            });

            Services.AddTransient<IdServer4AuthService>();
        }

        [Fact]
        public async Task GetTokenTest()
        {
            var user = Fixture.Create<string>();
            var pw = Fixture.Create<string>();

            MockHttp.Expect(HttpMethod.Post, "/connect/token")
                .WithHeaders("Content-Type", "application/x-www-form-urlencoded")
                .Respond(HttpStatusCode.OK, JsonContent.Create(new
                {
                    access_token = Guid.NewGuid().ToString("N"),
                    refresh_token = Guid.NewGuid().ToString("N"),
                    expires_in = 3600,
                    token_type = "token"
                }));

            var sut = Container.GetRequiredService<IdServer4AuthService>();
            var result = await sut.GetToken(user, pw).ConfigureAwait(false);

            result.AccessToken.Should().NotBeEmpty();
            result.RefreshToken.Should().NotBeEmpty();
            result.ExpiresIn.Should().BeGreaterThan(0);
            result.TokenType.Should().Be("token");
        }

#pragma warning disable xUnit1004 // Test methods should not be skipped
        [Fact(Skip = "feature offline")]
#pragma warning restore xUnit1004 // Test methods should not be skipped
        public async Task DoLogoutTest()
        {
            var response = Fixture.Create<TokenRevocationResponse>();

            MockHttp.Expect(HttpMethod.Post, "/connect/revocation")
                .Respond(HttpStatusCode.OK, JsonContent.Create(response));

            var sut = Container.GetRequiredService<IdServer4AuthService>();

            var data = Fixture.Create<string>();
            await sut.DoLogout(data).ConfigureAwait(false);

            MockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task CreateUserTest()
        {
            var data = Fixture.Create<CreateUserRequest>();

            MockHttp.Expect(HttpMethod.Post, new Regex("/api/users"))
                .Respond(System.Net.Http.Json.JsonContent.Create(new GcCreateUserResponse { Data = new UserCreatedResponse() { UserId = Guid.NewGuid().ToObjectId() } }));

            var sut = Container.GetRequiredService<IdServer4AuthService>();
            await sut.CreateUser(data, CancellationToken.None).ConfigureAwait(false);

            MockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task ResetPasswordTest()
        {
            var data = Fixture.Create<string>();

            MockHttp.Expect(HttpMethod.Put, new Regex("/api/users(/([a-z0-9]{8}\\-?([a-z0-9]{4}\\-?){3}[a-z0-9]{12}))/password"))
                .Respond(HttpStatusCode.Accepted);

            var sut = Container.GetRequiredService<IdServer4AuthService>();
            await sut.ResetPassword(Guid.NewGuid(), data, CancellationToken.None).ConfigureAwait(false);

            MockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task DisableUserTest()
        {
            MockHttp.Expect(HttpMethod.Put, new Regex("/api/users(/([a-z0-9]{8}\\-?([a-z0-9]{4}\\-?){3}[a-z0-9]{12}))/lock"))
                .Respond(HttpStatusCode.Accepted);

            var sut = Container.GetRequiredService<IdServer4AuthService>();
            await sut.DisableUser(Guid.NewGuid(), CancellationToken.None).ConfigureAwait(false);

            MockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task EnableUserTest()
        {
            MockHttp.Expect(HttpMethod.Put, new Regex("/api/users(/([a-z0-9]{8}\\-?([a-z0-9]{4}\\-?){3}[a-z0-9]{12}))/unlock"))
                .Respond(HttpStatusCode.Accepted);

            var sut = Container.GetRequiredService<IdServer4AuthService>();
            await sut.EnableUser(Guid.NewGuid(), CancellationToken.None).ConfigureAwait(false);

            MockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task ActivateStaffTest()
        {
            var data = Fixture.Create<string>();

            MockHttp.Expect(HttpMethod.Put, new Regex("/api/users(/([a-z0-9]{8}\\-?([a-z0-9]{4}\\-?){3}[a-z0-9]{12}))/staff/activate"))
                .Respond(HttpStatusCode.Accepted);

            var sut = Container.GetRequiredService<IdServer4AuthService>();
            await sut.ActivateStaff(Guid.NewGuid(), data, CancellationToken.None).ConfigureAwait(false);

            MockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task ActivateMemberTest()
        {
            MockHttp.Expect(HttpMethod.Put, new Regex("/api/users(/([a-z0-9]{8}\\-?([a-z0-9]{4}\\-?){3}[a-z0-9]{12}))/member/activate"))
                .Respond(HttpStatusCode.Accepted);

            var sut = Container.GetRequiredService<IdServer4AuthService>();
            await sut.ActivateMember(Guid.NewGuid(), CancellationToken.None).ConfigureAwait(false);

            MockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task AddClaimsTest()
        {
            MockHttp.Expect(HttpMethod.Post, new Regex("/api/users(/([a-z0-9]{8}\\-?([a-z0-9]{4}\\-?){3}[a-z0-9]{12}))/claims"))
                .Respond(HttpStatusCode.Accepted);

            var data = Fixture.CreateMany<UserClaim>().ToList();

            var sut = Container.GetRequiredService<IdServer4AuthService>();
            await sut.AddClaims(Guid.NewGuid(), data, CancellationToken.None).ConfigureAwait(false);

            MockHttp.VerifyNoOutstandingExpectation();
        }
    }
}
