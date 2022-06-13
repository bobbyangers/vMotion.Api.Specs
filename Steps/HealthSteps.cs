using System;
using TechTalk.SpecFlow;

namespace vMotion.Api.Specs.Steps
{
    [Binding]
    [Scope(Feature = "Health")]
    public class HealthSteps
    {
        [BeforeScenario(Order = 2)]
        public void BeforeScenario(ScenarioContext context)
        {
            context.TryAdd(Constants.StaffId, Guid.NewGuid().ToString("D"));
        }
    }
}
