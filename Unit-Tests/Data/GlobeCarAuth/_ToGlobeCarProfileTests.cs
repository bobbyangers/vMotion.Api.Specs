using AutoFixture;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Entities;
using vMotion.api.Data;
using vMotion.api.Data.GlobeCarAuth;
using vMotion.Dal;
using vMotion.Dal.MongoDb;
using vMotion.Dal.MongoDb.Entities;
using Xunit;
using Xunit.Abstractions;

namespace vMotion.Api.Specs.Unit_Tests
{
    [Trait(Constants.Category, Constants.CI)]
    public class _ToGlobeCarProfileTests : AutoFixtureTests<IMapper>
    {
        public _ToGlobeCarProfileTests(ITestOutputHelper output) : base(output)
        {
            CustomizeDomain(Fixture);

            Services.AddAutoMapper(c =>
            {
                c.AllowNullCollections = true;
                c.AddGlobalIgnore(nameof(BaseEntity.ID));
                c.AddGlobalIgnore(nameof(BaseEntity.ModifiedOn));
                c.AddGlobalIgnore(nameof(BaseEntity.Version));

                //NOTE: These are found in _EntityToDtoProfile
                // ========================
                c.CreateMap<DeviceDto, DeviceElement>();

                c.CreateMap<GeoLocationDto, GeoLocationElement>()
                    .ForMember(_ => _.Longitude, _ => _.MapFrom(s => s.Lon))
                    .ForMember(_ => _.Latitude, _ => _.MapFrom(s => s.Lat))
                    ;

                c.AddProfile<_ToGlobeCarProfile>();
            });

            Services.AddTransient<FirstNameFromNameResolver>();
            Services.AddTransient<LastNameFromNameResolver>();
            var container = Services.BuildServiceProvider();

            Sut = container.GetRequiredService<IMapper>();
        }

        private void CustomizeDomain(IFixture fixture)
        {
            fixture.Customize<CreateUserRequest>(x => x
                .With(_ => _.Name, "Joe Smith")
            );

            fixture.Customize<StaffOnlineEntity>(x => x
                .With(_ => _.ID, DB.Entity<StaffOnlineEntity>().GenerateNewID())
            );

            fixture.Customize<GcUserDevice>(x => x
                .With(_ => _.DeviceType, DeviceType.iOS.ToString())
            );
        }

        [Fact]
        public void IsConfigValidTest()
        {
            Sut.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Fact]
        public void WhenMap_CreateUserRequest_GcUserCreateDto()
        {
            var data = Fixture.Create<CreateUserRequest>();

            var result = Sut.Map<GcUserCreateDto>(data);

            result.Should().NotBeNull();
            ShowResult(result);
        }

        [Fact]
        public void WhenMap_CreateUserRequest_GcUserUpdateDto()
        {
            var data = Fixture.Create<CreateUserRequest>();

            var result = Sut.Map<GcUserUpdateDto>(data);

            result.Should().NotBeNull();
            ShowResult(result);
        }

        [Fact]
        public void WhenMap_UpdateUserRequest_GcUserUpdateDto()
        {
            var data = Fixture.Create<UpdateUserRequest>();

            var result = Sut.Map<GcUserUpdateDto>(data);

            result.Should().NotBeNull();
            ShowResult(result);
        }

        [Fact]
        public void WhenMap_GcGetUser_GetUserResponse()
        {
            var data = Fixture.Create<GcGetUser>();

            var result = Sut.Map<GetUserResponse>(data);

            result.Should().NotBeNull();
            ShowResult(result);
        }

        [Fact]
        public void WhenMap_GcAddressDto_AddressDto()
        {
            var data = Fixture.Create<GcAddressDto>();

            var result = Sut.Map<AddressDto>(data);

            result.Should().NotBeNull();
            ShowResult(result);
        }

        [Fact]
        public void WhenMap_GcUserDevice_DeviceElement()
        {
            var data = Fixture.Create<GcUserDevice>();

            var result = Sut.Map<DeviceElement>(data);

            result.Should().NotBeNull();
            ShowResult(result);
        }
    }
}