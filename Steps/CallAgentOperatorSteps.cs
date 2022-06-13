using AutoFixture;
using FluentAssertions;
using Newtonsoft.Json;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using vMotion.Dal.MongoDb.Entities;

namespace vMotion.Api.Specs.Steps
{
    [Binding]
    [Scope(Feature = "CallAgentOperator")]
    public class CallAgentOperatorSteps : TechTalk.SpecFlow.Steps
    {
        private readonly ScenarioContext _context;
        private readonly WebHostSupport _webHost;

        private readonly IFixture _fixture;

        public CallAgentOperatorSteps(ScenarioContext context, WebHostSupport webHost, IFixture fixture)
        {
            _context = context;
            _webHost = webHost;
            _fixture = fixture;
        }

        [Then("operator picture was updated")]
        public async Task ThenOperatorPictureWasUpdated()
        {
            var record = await _context.GetRecord<OperatorEntity>(Constants.OperatorId).ConfigureAwait(false);

            record.ImageUrl.Should().NotBeEmpty();
        }

        [Then("reasons list not empty")]
        public async Task ThenReasonsListNotEmpty()
        {
            var response = _webHost.Response;

            var data = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var results = JsonConvert.DeserializeObject<string[]>(data);

            results.Should().NotBeEmpty();
        }
    }
}