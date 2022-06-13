using AutoFixture;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Mail;
using Xunit;
using Xunit.Abstractions;

namespace vMotion.Api.Specs.Unit_Tests
{

#pragma warning disable S2699   //Add at least one assertion to this test case
    [Trait(Constants.Category, Constants.CI)]
    public class ElementBuilderTest
    {
        protected ITestOutputHelper Output { get; }

        protected IFixture Fixture { get; }

        public ElementBuilderTest(ITestOutputHelper output)
        {
            Output = output;

            Fixture = new Fixture();
        }

        [Fact]
        public void TestWithEnum()
        {
            Fixture.Customize<Demo2>(x => x
                .With(_ => _.Data1, (SampleValues y) => ((int)y).ToString())
            );

            var result = Fixture.CreateMany<Demo2>().ToList();

            ShowResult(result);
        }

        [Fact]
        public void TestElementBuilder()
        {
            Fixture.Customizations.Add(new ElementsBuilder<Sample>(
                new Sample("1"),
                new Sample("2"),
                new Sample("3"),
                new Sample("4"),
                new Sample("5"),
                new Sample("6")
            ));

            Fixture.Customize<Demo>(x => x
                .With(_ => _.Data1, (Sample y) => y.Value)
                .With(_ => _.Data2, (Sample y) => y.Value)
                .With(_ => _.Email, (MailAddress y) => y.Address)
                .With(_ => _.Constraint, (Constraints y) => y.ToString())
                .With(_ => _.Sample, (SampleValues y) => ((int)y).ToString())

            );

            var result = Fixture.CreateMany<Demo>().ToList();

            ShowResult(result);
        }

        [Theory]
        [InlineData("1")]
        [InlineData("2")]
        [InlineData("3")]
        [InlineData("4")]
        [InlineData("5")]
        [InlineData("6")]
        public void TestInlineData(string value)
        {
            var result = Fixture.Build<Demo>()
                .With(_ => _.Data1, value)
                .Create();

            ShowResult(new[] { result });
        }

        protected void ShowResult(object result)
        {
            Output.WriteLine(new string('-', 50));
            Output.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        }

        public class Sample
        {
            public Sample()
            {
            }

            public Sample(string x)
            {
                Value = x;
            }

            public string Value { get; set; }
        }

        public class Demo
        {
            public string Data1 { get; set; }
            public string Data2 { get; set; }

            public string Email { get; set; }

            public string Constraint { get; set; }

            public string Sample { get; set; }
        }

        public class Demo2
        {
            public string Data1 { get; set; }
        }

        public enum Constraints
        {
            None,
            Initial,
            Stage2,
            Final
        }

        public enum SampleValues
        {
            Zero = 0,
            One = 1,
            Two = 2,
            Three = 3,
            Four = 4,
            Five = 5,
            Six = 6
        }
    }
#pragma warning restore S2699   //Add at least one assertion to this test case
}