using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

using BoDi;

using IdentityModel.Client;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using TechTalk.SpecFlow;

namespace vMotion.Api.Specs
{
    [Binding]
    public class WebHostSupport : IDisposable
    {
        private readonly ScenarioContext _context;

        public WebApplicationFactory<Program> Factory { get; private set; }

        public HttpClient Client { get; set; }

        public HttpResponseMessage Response { get; private set; }

        public HttpContent Content { get; set; } = JsonContent.Create(new { });

        public object Data { get; set; }

        public WebHostSupport(ScenarioContext context)
        {
            _context = context;
        }

        ~WebHostSupport()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Client?.Dispose();
            Factory?.Dispose();
        }

        public T GetActor<T>()
        {
            return Factory.Services.GetRequiredService<T>();
        }

        [BeforeFeature("web")]
        public static async Task BeforeFeature(FeatureContext context)
        {
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [BeforeScenario("web", Order = 100)]
        public void BeforeScenario(CustomWebApplicationFactory<Program> factory, IObjectContainer objectContainer) //, ITestOutputHelper output
        {
            Factory = factory;
        }


        public void CreateClient(string token, IDictionary<string, string> extraHeaders = null)
        {
            var handler = _context.ScenarioContainer.Resolve<TraceLogRequestHandler>();
            var client = Factory.CreateDefaultClient(handler);

            if (null != token)
            {
                client.SetBearerToken(token);
            }

            extraHeaders ??= new Dictionary<string, string>();

            foreach (var (key, value) in extraHeaders)
            {
                client.DefaultRequestHeaders.Add(key, value);
            }

            Client = client;
        }

        public async Task RunRequest(HttpRequestMessage request)
        {
            Response = await Client.SendAsync(request, CancellationToken.None).ConfigureAwait(false);
        }
    }
}