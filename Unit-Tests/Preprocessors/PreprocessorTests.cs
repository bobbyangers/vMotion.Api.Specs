using System;

using AutoFixture;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Entities;

using vMotion.api.Services;
using vMotion.Dal;
using vMotion.Dal.MongoDb;
using vMotion.Dal.MongoDb.Entities;

using Xunit.Abstractions;

namespace vMotion.Api.Specs
{
    public abstract class PreprocessorTests<T> : AutoFixtureTests<T>
        where T : class
    {
        protected PreprocessorTests(ITestOutputHelper output) : base(output)
        {
            var services = Services;

            services.AddSingleton(_ => new DBContext(TestConfiguration.GetConnectionString()));

            services.AddMemoryCache();
            services.AddTransient<ICache, InMemoryCacheImpl>();

            services.AddTransient<T>();

            CustomiseDomain(Fixture);

            Sut = Container.GetRequiredService<T>();
        }

        protected void CustomiseDomain(IFixture fixture)
        {
            fixture.Customize<StaffEntity>(x => x
                .With(_ => _.ID, Guid.NewGuid().ToObjectId())
                .OmitAutoProperties()
            );

            fixture.Customize<MemberEntity>(x => x
                .With(_ => _.ID, Guid.NewGuid().ToObjectId())
                .With(_ => _.Device)
                .OmitAutoProperties()
            );

            fixture.Customize<DeviceElement>(x => x
                .With(_ => _.DeviceId, Guid.NewGuid().ToString("N"))
                .With(_ => _.DeviceType)
                .With(_ => _.GeoLocation));

            fixture.Customize<GeoLocationElement>(x => x
                .With(_ => _.Latitude)
                .With(_ => _.Longitude));

            fixture.Customize<CallEntity>(x => x
                .With(_ => _.ID, Guid.NewGuid().ToObjectId())
                .With(_ => _.InVideoCallStatus, VideoCallStatus.Active)
                .With(_ => _.ByMember, Guid.NewGuid().ToObjectId())
                .OmitAutoProperties()
            );
        }
    }
}