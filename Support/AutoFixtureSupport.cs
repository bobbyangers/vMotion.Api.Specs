using AutoFixture;
using AutoFixture.AutoNSubstitute;
using System;
using System.Net.Mail;
using TechTalk.SpecFlow;
using vMotion.Dal;
using vMotion.Dal.MongoDb;
using vMotion.Dal.MongoDb.Entities;

namespace vMotion.Api.Specs
{
    [Binding]
#pragma warning disable RCS1102 // Make class static.
    public class AutoFixtureSupport
#pragma warning restore RCS1102 // Make class static.
    {
        [BeforeFeature(Order = 1)]
        public static void BeforeFeature(FeatureContext context)
        {
            var fixture = GetAutoFixture();

            context.FeatureContainer.RegisterInstanceAs<IFixture>(fixture);
        }

        ////[BeforeScenario(Order = 1)]
        ////public void Initialize(ScenarioContext context)
        ////{
        ////    var fixture = GetAutoFixture();
        ////    context.ScenarioContainer.RegisterInstanceAs(fixture);
        ////}

        private static IFixture GetAutoFixture()
        {
            var fixture = new Fixture();

            fixture.Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });

            fixture.Customize(PostalCodeStringsGenerator.ToCustomization());
            fixture.Customize(CountryCodeStringsGenerator.ToCustomization());

            fixture.Customize(PhoneStringsGenerator.ToCustomization());
            fixture.Customize(EmailAddressStringsGenerator.ToCustomization());
            fixture.Customize(LinkSpecimenBuilder.ToCustomization());
            fixture.Customize(IdInterneSpecimenBuilder.ToCustomization());

            CustomizeDomain(fixture);

            return fixture;
        }

        private static void CustomizeDomain(IFixture fixture)
        {
            fixture.Customize<DeviceDto>(_ => _
                .With(x => x.DeviceId, (Guid id) => id.ToString("D"))
                .With(x => x.DeviceType, DeviceType.iOS)
            );

            fixture.Customize<OperatorEntity>(_ => _
                .With(x => x.ID, (Guid id) => id.ToObjectId())
                .With(x => x.Version, 1)
                .With(x => x.AdultAge, 18)
                .With(x => x.WorkHourStart, TimeSpan.FromHours(9))
                .With(x => x.WorkHourEnd, TimeSpan.FromHours(17))
                .With(x => x.VideoProvider, VideoProvider.None)
                .With(x => x.MapProvider, MapProvider.Google)
                .With(x => x.SyncProvider, SyncProvider.None)
                .With(x => x.Timezone, "EST")
                .Without(x => x.Code)
            );

            fixture.Customize<MemberEntity>(_ => _
                .With(x => x.ID, (Guid id) => id.ToObjectId())
                .With(x => x.Version, 1)
                .With(x => x.Name, "Kris Letang (Customer)")
                .With(x => x.Email, fixture.Create<MailAddress>().Address)
                .With(x => x.SignUpDate, DateTime.Today)
                .With(x => x.Operator, DbNames.Operator1_ID)
                .With(x => x.Device)
                .Without(x => x.CurrentCall)
                .Without(x => x.LinkedOperators)
            );

            fixture.Customizations.Add(new ElementsBuilder<SampleNames>(
                new("Maxime Boutet (Agent)"),
                new("Adèle Tremblay (Agent)"),
                new("Pierre Masson (Agent)"),
                new("Jacques Lamarche (Agent)"),
                new("Marie Blondin (Agent)")
            ));

            fixture.Customize<StaffEntity>(_ => _
                .With(x => x.ID, (Guid id) => id.ToObjectId())
                .With(x => x.Version, 1)
                .With(x => x.Email, fixture.Create<MailAddress>().Address)
                .With(x => x.Name, (SampleNames n) => n.Name)
                .With(x => x.Operator, DbNames.Operator1_ID)
                .With(x => x.UserType, UserTypeEnum.Agent)
                .Without(x => x.CurrentCall)
            );

            fixture.Customize<AddressDto>(_ => _
                .With(x => x.Line1, new Random().Next(1, 99999).ToString())
                .With(x => x.Country, "CA")
            );
        }

        private class SampleNames
        {
            public SampleNames()
            {
            }

            public SampleNames(string n)
            {
                Name = n;
            }
            public string Name { get; set; }
        }
    }
}