using System;
using TechTalk.SpecFlow;
using vMotion.Dal;

namespace vMotion.Api.Specs.Support
{
    [Binding]
    public class EnumCustomConversions
    {
        [StepArgumentTransformation("(.*)")]
        public UserTypeEnum GetUserType(string data)
        {
            return Enum.TryParse(data, true, out UserTypeEnum result) ? result : UserTypeEnum.Unknown;
        }
    }
}