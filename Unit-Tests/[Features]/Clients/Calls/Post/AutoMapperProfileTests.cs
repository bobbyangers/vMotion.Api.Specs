using AutoFixture;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Entities;
using System;
using vMotion.api._Features_.Clients.Calls.Post;
using vMotion.api.Data;
using vMotion.Dal;
using vMotion.Dal.MongoDb;
using vMotion.Dal.MongoDb.Entities;
using Xunit;
using Xunit.Abstractions;

namespace vMotion.Api.Specs.Unit_Tests._Features_.Clients.Calls.Post;

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

        services.AddTransient<UserAgentResolver>();
        services.AddTransient<GeoLocationResolver>();

        var container = Services.BuildServiceProvider();

        Sut = container.GetRequiredService<IMapper>();
    }

    private void CustomizeDomain(IFixture f)
    {
        f.Customize<CallEntity>(_ => _
            .With(x => x.ID, DB.Entity<CallEntity>().GenerateNewID())
            .With(x => x.RelatedCase, DB.Entity<CaseEntity>().GenerateNewID())
            .With(x => x.ByMember, DB.Entity<MemberEntity>().GenerateNewID())
            .With(x => x.OngoingCallBy, DB.Entity<StaffEntity>().GenerateNewID())
            .With(x => x.AssignedStaff, DB.Entity<StaffEntity>().GenerateNewID())
            .With(x => x.Operator, DB.Entity<OperatorEntity>().GenerateNewID())
            .Without(x => x.Data));

        f.Customize<DeviceElement>(x => x
            .With(_ => _.DeviceId, Guid.NewGuid().ToString("N"))
            .With(_ => _.DeviceType)
            .Without(_ => _.GeoLocation)
        );

        f.Customize<GeoLocationElement>(x => x);

        f.Customize<ExtraHeadersDto>(x => x
            .With(_ => _.DeviceId)
            .With(_ => _.DeviceType, (DeviceType d) => d.ToString())
            .With(_ => _.GeoLocation, (GeoLocationDto data) => data.ToJson())
            .With(_ => _.IpAddress, () => "127.0.0.1")
            .With(_ => _.UserAgent)
        );
    }


    [Fact]
    public void IsConfigValidTest()
    {
        Sut.ConfigurationProvider.AssertConfigurationIsValid();
    }

    [Fact]
    public void WhenMap_ExtraHeadersDto_DeviceElement()
    {

        var data = Fixture.Create<ExtraHeadersDto>();

        var result = Sut.Map<DeviceElement>(data);

        ShowResult(result);

        result.Should().NotBeNull();
    }


    [Fact]
    public void WhenMap_Request_CallEntity()
    {

        var data = Fixture.Create<Request>();

        var result = Sut.Map<CallEntity>(data);

        ShowResult(result);

        result.Should().NotBeNull();
    }

    ////[Fact]
    ////public void WhenMap_CallEntity_Response()
    ////{

    ////    var data = Fixture.Create<CallEntity>();

    ////    var result = Sut.Map<Response>(data);

    ////    ShowResult(result);

    ////    result.Should().NotBeNull();
    ////}
}