using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TechTalk.SpecFlow;

namespace vMotion.Api.Specs.Support
{
#pragma warning disable RCS1102 // Make class static.
    [Binding]
    public class JsonBinding
    {
        [BeforeTestRun]
        public static void BeforeTestRunHook()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Include,
                DateFormatString = "yyyy-MM-ddTHH:mm",
                DateTimeZoneHandling = DateTimeZoneHandling.Local,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateParseHandling = DateParseHandling.None,
                Formatting = Formatting.Indented
            };
        }
    }
#pragma warning restore RCS1102 // Make class static.
}