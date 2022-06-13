using NSubstitute;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using vMotion.Dal;

namespace vMotion.Api.Specs.Steps
{
    [Binding]
    [Scope(Feature = "Authorisation")]
    public class AuthorisationSteps : TechTalk.SpecFlow.Steps
    {
        public WebHostSupport Support { get; }

        public AuthorisationSteps(WebHostSupport support)
        {
            Support = support;
        }

        [When("a call to echo")]
        public async Task WhenACallToEcho()
        {
            await Support.RunRequest(new HttpRequestMessage(HttpMethod.Post, new Uri("/api/auth/echo", UriKind.Relative))
            {
                Content = JsonContent.Create(new
                {
                    data = "hello"
                })
            }).ConfigureAwait(false);
        }

        [Then("the message is broadcast to the backoffice")]
        public async Task ThenTheMessageIsBroadcastToTheBackoffice()
        {
            await Support.GetActor<IRealtimeNotificationService>().Received(1)
                .BroadcastToBackoffice(MessageName.echo, Arg.Any<object>(), Arg.Any<CancellationToken>())
                .ConfigureAwait(false);
        }
    }
}
