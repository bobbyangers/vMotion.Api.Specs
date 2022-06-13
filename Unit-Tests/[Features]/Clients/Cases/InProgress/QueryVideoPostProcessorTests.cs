using System;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Entities;

using vMotion.api._Features_.Clients.Cases.Get.InProgress;
using vMotion.api.Services;
using vMotion.Dal;
using vMotion.Dal.MongoDb.Entities;

using Xunit;
using Xunit.Abstractions;


namespace vMotion.Api.Specs.Unit_Tests._Features_.Clients.Cases.Get.InProgress
{
    [Trait(Constants.Category, Constants.CI)]
    public class QueryVideoPostProcessorTests : AutoFixtureTests<QueryVideoPostProcessor>
    {
        public QueryVideoPostProcessorTests(ITestOutputHelper output) : base(output)
        {
            var services = Services;

            services.AddSingleton(_ => new DBContext(TestConfiguration.GetConnectionString()));

            services.AddMemoryCache();
            services.AddTransient<ICache, InMemoryCacheImpl>();


            services.AddTransient<Func<VideoProvider, IVideoProvider>>(_ => p => new EmptyVideoProvider());

            services.AddTransient<QueryVideoPostProcessor>();

            Sut = Container.GetRequiredService<QueryVideoPostProcessor>();
        }

        [Fact]
        public async Task WhenProcess_get_video()
        {
            var callId = Guid.NewGuid();

            Fixture.Customize<CallSummaryDto>(x => x
                .With(_ => _.Id, callId)
            );

            var op = Fixture.Build<OperatorEntity>()
                .With(_ => _.ID, DB.Entity<OperatorEntity>().GenerateNewID)
                .With(_ => _.VideoProvider, VideoProvider.None)
                .Without(_ => _.Members)
                .Without(_ => _.Roles)
                .Create();

            await op.SaveAsync().ConfigureAwait(false);


            var data = Fixture.Build<Response>()
                .With(_ => _.OperatorId, op.ID.ToGuid())
                .With(_ => _.Call)
                .Without(_ => _.VideoConnector)
                .Create();

            await Sut.Process(null, data, CancellationToken.None).ConfigureAwait(false);

            ShowResult(data);

            data.VideoConnector.Should().NotBeNull();
            data.VideoConnector.RoomId.Should().Be(callId.ToString("D"));
        }
    }
}