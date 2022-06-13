using AutoFixture;
using IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using vMotion.api.Data;
using vMotion.api.Services;
using vMotion.Dal;
using Xunit;
using Xunit.Abstractions;
using JsonContent = System.Net.Http.Json.JsonContent;

namespace vMotion.Api.Specs.Unit_Tests
{
    [Trait(Constants.Category, Constants.CI)]
    public class GlobeCarBackendTokenServiceTests : AutoFixtureTests
    {
        private const string targetUrl = "https://localhost:5001";

        private MockHttpMessageHandler MockHttp { get; }

        public GlobeCarBackendTokenServiceTests(ITestOutputHelper output) : base(output)
        {
            CustomizeDomain(Fixture);

            Services.AddAutoMapper(c => c.CreateMap<TokenResponse, AuthData>()
                .ForMember(_ => _.AccessToken, _ => _.MapFrom(s => s.AccessToken))
                .ForMember(_ => _.RefreshToken, _ => _.MapFrom(s => s.RefreshToken))
                .ForMember(_ => _.TokenType, _ => _.MapFrom(s => s.TokenType))
                .ForMember(_ => _.ExpiresIn, _ => _.MapFrom(s => s.ExpiresIn))
                );

            Services.AddScoped(_ => Fixture.Create<IdGcServerSettings>());

            MockHttp = new MockHttpMessageHandler();

            Services.AddTransient(_ =>
            {
                var client = MockHttp.ToHttpClient(output, targetUrl);

                return client;
            });

            Services.AddTransient<GlobeCarBackendTokenService>();
        }

        private static void CustomizeDomain(IFixture fixture)
        {
            var guid1 = new Guid("00000000-0000-0000-0000-000000000001");
            fixture.Customize<IdGcServerSettings>(x => x
                .With(_ => _.ApiBackendTenantId, $"{guid1:N}")
                .With(_ => _.ApiBackendTokenUrl, $"/v1/account/{guid1:N}/token")
                .With(_ => _.ApiBackendUser, "admin@test.com")
                .With(_ => _.ApiBackendPassword, "Test123!")
                .With(_ => _.ApiBackendClientId, Guid.NewGuid().ToString("N"))
                .With(_ => _.ApiBackendSecret, Guid.NewGuid().ToString("N"))
            );
        }

        [Fact]
        public async Task WhenGetTokenAsync_ThenOk()
        {
            MockHttp.Expect(HttpMethod.Post, new Regex("/v1/account/([a-z0-9]{32})/token"))
                .Respond(HttpStatusCode.OK, JsonContent.Create(new
                {
                    access_token = Guid.NewGuid().ToString("N"),
                    refresh_token = Guid.NewGuid().ToString("N"),
                    expires_in = 3600,
                    token_type = "bearer"
                }));

            var sut = Container.GetRequiredService<GlobeCarBackendTokenService>();

            var result = await sut.GetTokenAsync().ConfigureAwait(false);

            ShowResult(result);
        }
    }
}