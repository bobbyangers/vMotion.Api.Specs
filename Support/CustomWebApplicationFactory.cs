using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;

using CorePush.Apple;
using CorePush.Interfaces;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

using NSubstitute;

using vMotion.api.Data;
using vMotion.api.Extensions;
using vMotion.api.Services;
using vMotion.api.Telemetry;
using vMotion.Api.Specs.Unit_Tests;
using vMotion.Dal;

using Xunit.Abstractions;

namespace vMotion.Api.Specs;


public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
    where TStartup : class
{
    private readonly ITestOutputHelper _testOutputHelper;

    public CustomWebApplicationFactory(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }


    protected override void ConfigureClient(HttpClient client)
    {
        var headers = client.DefaultRequestHeaders;

        headers.Add(HeaderNames.AcceptLanguage, string.Join(",", "en-US", "en-CA", "fr-CA"));
        headers.Add(HeaderNames.Accept, System.Net.Mime.MediaTypeNames.Application.Json);

        headers.UserAgent.Add(new ProductInfoHeaderValue("DotnetCore", "16.0"));

        headers.Add("X-Skip-Activity-Recording", bool.TrueString);
        headers.Add("X-Timezone-Offset", "300");


        base.ConfigureClient(client);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var prefixToSkipWhenWarning = new[] {
            ////"Microsoft.AspNetCore.Authorization",
            "Microsoft.AspNetCore.DataProtection" ,
            "Microsoft.AspNetCore.Hosting",
            "Microsoft.AspNetCore.HttpsPolicy",
            ////"Microsoft.AspNetCore.Mvc.Infrastructure",
            "Microsoft.AspNetCore.Routing.EndpointMiddleware"
        };

        // Register the xUnit logger
        builder.UseEnvironment(Constants.TestEnv)
            .UseSetting(api.Constants.GetSignalROptionsKey("Disabled"), bool.TrueString)
            .UseSetting(ApplicationInsightExtensions.DisableTelemetryFromConfig, bool.TrueString)
            .UseSetting(AppSettings.AuthSystemKey, AuthSystemEnum.SelfSigned.ToString())
            .ConfigureLogging(b =>
            {
                b.ClearProviders(); // Remove other loggers

                b.SetMinimumLevel(LogLevel.Trace);


                b.AddXUnit(_testOutputHelper, opts =>
                {
                    opts.IncludeScopes = true;
                    opts.Filter += (ns, level) =>
                    {
                        if (prefixToSkipWhenWarning.Any(x =>
                                ns.StartsWith(x) && level < LogLevel.Warning))
                            return false;

                        return true;
                    };
                });
            })
            .ConfigureTestServices(svc =>
            {
                svc.RemoveAll(typeof(IHostedService));

                svc.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, o =>
                    {
                        o.SaveToken = false;
                        o.IncludeErrorDetails = true;

                        o.Audience = AuthorizationConstants.AUDIENCE;

                        var p = o.TokenValidationParameters;

                        p.ValidateIssuerSigningKey = false;
                        p.ValidIssuer = AuthorizationConstants.ISSUER;
                        p.IssuerSigningKey = AuthorizationConstants.JWT_SECRET_KEY.ToSymmetricKey();

                        p.NameClaimType = ClaimTypes.NameIdentifier;
                        p.RoleClaimType = ClaimTypes.Role;
                    });

                svc.ReplaceWithFake<Microsoft.Extensions.Internal.ISystemClock>(f =>
                {
                    f.UtcNow.Returns(DateTimeOffset.UtcNow);
                });


                svc.AddSingleton<IRealtimeNotificationService>(_ =>
                {
                    var logger = _.GetRequiredService<ILogger<StubNotificationService>>();
                    var stub = Substitute.ForPartsOf<StubNotificationService>(logger);

                    return stub;
                });
                ////svc.ReplaceWithFake<IRealtimeNotificationService>(f =>
                ////{
                ////    f.WhenForAnyArgs(x => x.BroadcastToBackoffice(default, default, default))
                ////        .Do(info =>
                ////        {

                ////        });

                ////    f.WhenForAnyArgs(x => x.SendToStaff(default, default, default))
                ////        .Do(info => ShowResult(info.Args()[1]));

                ////    f.WhenForAnyArgs(x => x.SendToMember(default, default, default))
                ////        .Do(info => ShowResult(info.Args()[1]));
                ////});

                svc.ReplaceWithFake<IAuthenticationService>(s =>
                {
                    var token = new AuthData
                    {
                        AccessToken = Guid.NewGuid().ToString("N"),
                        RefreshToken = Guid.NewGuid().ToString("N"),
                        ExpiresIn = 1800,
                        TokenType = "bearer"
                    };
                    s.GetToken(default, default).ReturnsForAnyArgs(token);
                });

                svc.ReplaceWithFake<IHubNotificationsService>();
                svc.ReplaceWithFake<IEmailService>();

                svc.ReplaceWithFake<IBlobStorage>(f =>
                {
                    f.GetBlobUrl(default).ReturnsForAnyArgs(_ =>
                    {
                        var data = new ImageData(_[0].ToString());
                        return new Uri($"http://localhost/{data.Container}/{data.FileName}?sv={Guid.NewGuid():N}");
                    });
                });

                svc.ReplaceWithFake<IApnSender>(f =>
                {
                    f.SendAsync(default, default, default).ReturnsForAnyArgs(i => new ApnsResponse
                    {
                        IsSuccess = true
                    });
                });
            });
    }
}