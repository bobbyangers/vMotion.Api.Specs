using AutoFixture;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using vMotion.api._Features_.Clients.Me.Device.Put;
using vMotion.api.Data;
using vMotion.Dal;
using vMotion.Dal.MongoDb;
using vMotion.Dal.MongoDb.Entities;
using Xunit;
using Xunit.Abstractions;

namespace vMotion.Api.Specs.Unit_Tests._Features_.Clients.Me.Put.Device;

public class AutoMapperProfileTests : AutoFixtureTests<IMapper>
{
    public AutoMapperProfileTests(ITestOutputHelper output) : base(output)
    {
        CustomizeDomain(Fixture);

        var services = Services;

        services.AddAutoMapper(c =>
        {
            c.AllowNullCollections = true;
            c.AddGlobalIgnore(nameof(BaseEntity.ID));
            c.AddGlobalIgnore(nameof(BaseEntity.ModifiedOn));
            c.AddGlobalIgnore(nameof(BaseEntity.Version));

            c.AddProfile<AutoMapperProfile>();
            c.AddProfile<_EntityToDtoProfile>();
        });
        services.AddTransient<ImageUrlResolver>();
        services.AddTransient<UserAgentResolver>();
        services.AddTransient<GeoLocationResolver>();


        Sut = Container.GetRequiredService<IMapper>();
    }

    private void CustomizeDomain(IFixture f)
    {
        f.Customize<CallEntity>(_ => _
            .With(x => x.ID)            ////, DB.Entity<CallEntity>().GenerateNewID())
            .With(x => x.RelatedCase)   ////, DB.Entity<CaseEntity>().GenerateNewID())
            .With(x => x.ByMember)      ////, DB.Entity<MemberEntity>().GenerateNewID())
            .With(x => x.OngoingCallBy) ////, DB.Entity<StaffEntity>().GenerateNewID())
            .With(x => x.AssignedStaff) ////, DB.Entity<StaffEntity>().GenerateNewID())
            .With(x => x.Operator)      ////, DB.Entity<OperatorEntity>().GenerateNewID())
            .Without(x => x.Data));

        f.Customize<DeviceElement>(x => x
            .With(_ => _.DeviceId, Guid.NewGuid().ToString("N"))
            .With(_ => _.DeviceType)
            .Without(_ => _.GeoLocation)
        );

        f.Customize<GeoLocationElement>(x => x);

        f.Customize<ExtraHeadersDto>(x => x
            .With(_ => _.GeoLocation, (GeoLocationDto data) => data.ToJson())
        );
    }

    [Fact]
    public void IsConfigValidTest()
    {
        Sut.ConfigurationProvider.AssertConfigurationIsValid();
    }

    [Fact]
    public void WhenMapFrom_Request_DeviceElement()
    {
        var data = Fixture.Create<Request>();

        var result = Sut.Map<DeviceElement>(data);

        result.Should().NotBeNull();

        ShowResult(result);
    }
}