using AutoFixture;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using vMotion.api.Data;
using vMotion.Dal.MongoDb.Entities;
using Xunit;
using Xunit.Abstractions;

namespace vMotion.Api.Specs.Unit_Tests._Features_.Agents.Calls.Post.Charge;

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

            c.AddProfile<vMotion.api._Features_.Agents.Calls.Post.Charge.AutoMapperProfile>();
            c.AddProfile<_EntityToDtoProfile>();
        });
        services.AddTransient<ImageUrlResolver>();

        Sut = Container.GetRequiredService<IMapper>();
    }

    private void CustomizeDomain(IFixture f)
    {
        f.Customize<vMotion.api._Features_.Agents.Calls.Post.Charge.Request>(_ => _
           .With(x => x.CallId)
           .With(x => x.CardId)
           .With(x => x.Amount)
           .With(x => x.Currency, "cad") ////, DB.Entity<StaffEntity>().GenerateNewID())
           .WithAutoProperties()
           );
        ;
    }

    [Fact]
    public void IsConfigValidTest()
    {
        Sut.ConfigurationProvider.AssertConfigurationIsValid();
    }

    [Fact]
    public void WhenMapFrom_Request_StripeTransactionEntity()
    {
        var data = Fixture.Create<vMotion.api._Features_.Agents.Calls.Post.Charge.Request>();

        var result = Sut.Map<StripeTransactionEntity>(data);

        result.Should().NotBeNull();

        ShowResult(result);
    }
}