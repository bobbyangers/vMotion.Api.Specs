using AutoFixture;
using FluentAssertions;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RichardSzalay.MockHttp;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using vMotion.api.Data;
using vMotion.api.Data.GlobeCarAuth;
using vMotion.api.Services.Implementation;
using vMotion.Dal;
using Xunit;
using Xunit.Abstractions;
using JsonContent = System.Net.Http.Json.JsonContent;

namespace vMotion.Api.Specs.Unit_Tests
{
    [Trait(Constants.Category, Constants.CI)]
    public class GlobeCarAuthServiceTests : AutoFixtureTests
    {
        private const string targetUrl = "https://localhost:5001";

        private MockHttpMessageHandler MockHttp { get; }

        public GlobeCarAuthServiceTests(ITestOutputHelper output) : base(output)
        {
            CustomizeDomain(Fixture);

            var services = Services;

            services.AddLogging(b =>
            {
                b.ClearProviders();
                b.AddXUnit(output, opts => opts.IncludeScopes = true);
            });

            services.AddAutoMapper(c =>
            {
                c.AddProfile<_ToGlobeCarProfile>();

                c.CreateMap<TokenResponse, AuthData>();
            });

            services.AddTransient<FirstNameFromNameResolver>();
            services.AddTransient<LastNameFromNameResolver>();

            services.AddSingleton(_ => Fixture.Create<IdGcServerSettings>());

            MockHttp = new MockHttpMessageHandler();

            services.AddTransient(_ =>
            {
                var factory = Substitute.For<IHttpClientFactory>();
                var client = MockHttp.ToHttpClient(output, targetUrl);

                factory.CreateClient(default).ReturnsForAnyArgs(client);

                return factory;
            });

            services.AddTransient<GlobeCarAuthService>();
        }

        private static void CustomizeDomain(IFixture fixture)
        {
            fixture.Customize<IdGcServerSettings>(x => x
                .With(_ => _.ApiBackendTenantId, "000000000000000000000001")
                .With(_ => _.ApiBackendTokenUrl, "/v1/account/000000000000000000000001/token")
            );

            fixture.Customize<UpdateUserRequest>(x => x
                .With(_ => _.Id, Guid.NewGuid().ToObjectId())
            );
        }

        [Fact]
        public async Task GetTokenTest()
        {
            var user = Fixture.Create<Guid>().ToObjectId();
            var pw = Fixture.Create<string>();

            MockHttp.Expect(HttpMethod.Post, new Regex("/v1/account(/[a-f0-9]{24})/token"))
                .WithHeaders("Content-Type", "application/x-www-form-urlencoded")
                .Respond(HttpStatusCode.OK, JsonContent.Create(new
                {
                    access_token = Guid.NewGuid().ToString("N"),
                    refresh_token = Guid.NewGuid().ToString("N"),
                    expires_in = 3600,
                    token_type = "token"
                }));

            var sut = Container.GetRequiredService<GlobeCarAuthService>();
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

            var sut = Container.GetRequiredService<GlobeCarAuthService>();

            var data = Fixture.Create<string>();
            await sut.DoLogout(data).ConfigureAwait(false);

            MockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task ActivateStaffTest()
        {
            MockHttp.Expect(HttpMethod.Patch, new Regex("/v1/users((/[a-f0-9]{24}){2})/activate"))
                .Respond(HttpStatusCode.Accepted);

            var sut = Container.GetRequiredService<GlobeCarAuthService>();

            var data = Fixture.Create<string>();
            await sut.ActivateStaff(Guid.NewGuid(), data, CancellationToken.None).ConfigureAwait(false);

            MockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task ActivateMemberTest()
        {
            MockHttp.Expect(HttpMethod.Patch, new Regex("/v1/users((/[a-f0-9]{24}){2})/activate"))
                .Respond(HttpStatusCode.Accepted);

            var sut = Container.GetRequiredService<GlobeCarAuthService>();
            await sut.ActivateMember(Guid.NewGuid().ToShortGuid(), CancellationToken.None).ConfigureAwait(false);

            MockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task CreateUserNewTest()
        {
            var data = Fixture.Create<CreateUserRequest>();

            data.Claims.Add(new UserClaim(JwtClaimTypes.Role, nameof(UserTypeEnum.Agent)));

            MockHttp.Expect(HttpMethod.Get, new Regex("/v1/users/([a-f0-9]{24})/\\?pageNumber=1&pageSize=10&keyword=(.{8,})$"))
                .Respond(System.Net.Http.Json.JsonContent.Create(
                    new SearchUserResponse()));

            MockHttp.Expect(HttpMethod.Post, new Regex("/v1/users/([a-f0-9]{24})$"))
                .Respond(System.Net.Http.Json.JsonContent.Create(new { Data = new { UserId = Guid.NewGuid().ToObjectId() } }));

            MockHttp.Expect(HttpMethod.Patch, new Regex("/v1/users(/[a-f0-9]{24}){2}/roles$"))
                .Respond(HttpStatusCode.Accepted);

            MockHttp.Expect(HttpMethod.Patch, new Regex("/v1/users(/[a-f0-9]{24}){2}/claims$"))
                .Respond(HttpStatusCode.Accepted);

            MockHttp.Expect(HttpMethod.Patch, new Regex("/v1/users(/[a-f0-9]{24}){2}/activate$"))
                .Respond(HttpStatusCode.Accepted);

            var sut = Container.GetRequiredService<GlobeCarAuthService>();
            await sut.CreateUser(data, CancellationToken.None).ConfigureAwait(false);

            MockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task CreateUserExistingTest()
        {
            var data = Fixture.Create<CreateUserRequest>();
            data.Claims.Add(new UserClaim(JwtClaimTypes.Role, nameof(UserTypeEnum.Agent)));

            var searchData = new SearchUserResponse
            {
                Data =
                {
                    Items = new [] {new SearchUserResponseItem { Id = Guid.NewGuid().ToObjectId() } }
                }
            };

            MockHttp.Expect(HttpMethod.Get, new Regex("/v1/users(/[a-f0-9]{24})/\\?pageNumber=1&pageSize=10&keyword=(.{16,})$"))
                .Respond(System.Net.Http.Json.JsonContent.Create(
                    searchData));

            MockHttp.Expect(HttpMethod.Put, new Regex("/v1/users((/[a-f0-9]{24}){2})$"))
                .Respond(System.Net.Http.Json.JsonContent.Create(new { Data = new { UserId = Guid.NewGuid().ToObjectId() } }));

            MockHttp.Expect(HttpMethod.Patch, new Regex("/v1/users((/[a-f0-9]{24}){2})/roles$"))
                .Respond(HttpStatusCode.Accepted);

            MockHttp.Expect(HttpMethod.Patch, new Regex("/v1/users((/[a-f0-9]{24}){2})/claims$"))
                .Respond(HttpStatusCode.Accepted);

            MockHttp.Expect(HttpMethod.Patch, new Regex("/v1/users((/[a-f0-9]{24}){2})/activate$"))
                .Respond(HttpStatusCode.Accepted);

            var sut = Container.GetRequiredService<GlobeCarAuthService>();
            await sut.CreateUser(data, CancellationToken.None).ConfigureAwait(false);

            MockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task UpdateUserTest()
        {
            var data = Fixture.Create<UpdateUserRequest>();

            MockHttp.Expect(HttpMethod.Put, new Regex("/v1/users((/[a-f0-9]{24}){2})$"))
                .Respond(System.Net.Http.Json.JsonContent.Create(new { Data = new { UserId = Guid.NewGuid().ToObjectId() } }));

            var sut = Container.GetRequiredService<GlobeCarAuthService>();
            await sut.UpdateUser(data, CancellationToken.None).ConfigureAwait(false);

            MockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task AddClaimsTest()
        {
            MockHttp.Expect(HttpMethod.Patch, new Regex("/v1/users((/[a-f0-9]{24}){2})/claims$"))
                .Respond(HttpStatusCode.Accepted);

            var data = Fixture.CreateMany<UserClaim>().ToList();

            var sut = Container.GetRequiredService<GlobeCarAuthService>();
            await sut.AddClaims(Guid.NewGuid(), data, CancellationToken.None).ConfigureAwait(false);

            MockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task DisableUserTest()
        {
            MockHttp.Expect(HttpMethod.Patch, new Regex("/v1/users((/[a-f0-9]{24}){2})/lock$"))
                .Respond(HttpStatusCode.Accepted);

            var sut = Container.GetRequiredService<GlobeCarAuthService>();
            await sut.DisableUser(Guid.NewGuid(), CancellationToken.None).ConfigureAwait(false);

            MockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task EnableUserTest()
        {
            MockHttp.Expect(HttpMethod.Patch, new Regex("/v1/users((/[a-f0-9]{24}){2})/unlock$"))
                .Respond(HttpStatusCode.Accepted);

            var sut = Container.GetRequiredService<GlobeCarAuthService>();
            await sut.EnableUser(Guid.NewGuid(), CancellationToken.None).ConfigureAwait(false);

            MockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task ResetPasswordTest()
        {
            MockHttp.Expect(HttpMethod.Patch, new Regex("/v1/users((/[a-f0-9]{24}){2})/password-reset$"))
                .Respond(HttpStatusCode.Accepted);

            var sut = Container.GetRequiredService<GlobeCarAuthService>();
            await sut.ResetPassword(Guid.NewGuid(), Guid.NewGuid().ToString("N"), CancellationToken.None).ConfigureAwait(false);

            MockHttp.VerifyNoOutstandingExpectation();
        }
    }
}
