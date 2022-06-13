using AutoFixture;
using Microsoft.Extensions.DependencyInjection;
using System;
using vMotion.Dal;
using vMotion.Dal.MongoDb.Entities;
using Xunit.Abstractions;

namespace vMotion.Api.Specs
{
    public abstract class PostProcessorTests<T> : AutoFixtureTests<T>
        where T : class
    {
        protected PostProcessorTests(ITestOutputHelper output) : base(output)
        {
            var services = Services;

            services.AddTransient<T>();

            CustomiseDomain(Fixture);

            var container = Services.BuildServiceProvider();

            Sut = container.GetRequiredService<T>();
        }

        protected void CustomiseDomain(IFixture fixture)
        {
            fixture.Customize<StaffEntity>(x => x
                .With(_ => _.ID, Guid.NewGuid().ToObjectId())
                .OmitAutoProperties()
            );

            fixture.Customize<MemberEntity>(x => x
                .With(_ => _.ID, Guid.NewGuid().ToObjectId())
                .OmitAutoProperties()
            );

            fixture.Customize<CallEntity>(x => x
                .With(_ => _.ID, Guid.NewGuid().ToObjectId())
                .With(_ => _.InVideoCallStatus, VideoCallStatus.Active)
                .With(_ => _.ByMember, Guid.NewGuid().ToObjectId())
                .OmitAutoProperties()
            );
        }
    }
}