using AutoFixture;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Infrastructure;

namespace vMotion.Api.Specs.Steps
{
    [Binding]
    [Scope(Feature = "Call Agent")]
    public class CallAgentSteps : TechTalk.SpecFlow.Steps
    {
        private readonly ScenarioContext _context;
        private readonly WebHostSupport _webHost;
        private readonly ISpecFlowOutputHelper _output;

        private readonly IFixture _fixture;

        public CallAgentSteps(ScenarioContext context, WebHostSupport webHost, ISpecFlowOutputHelper output, IFixture fixture)
        {
            _context = context;
            _webHost = webHost;
            _output = output;
            _fixture = fixture;
        }



    }
}