using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using vMotion.api.Services;
using Xunit;
using Xunit.Abstractions;

namespace vMotion.Api.Specs.Unit_Tests
{
    [Trait(Constants.Category, Constants.CI)]
    public class CacheImplTests : AutoFixtureTests<InMemoryCacheImpl>
    {
        public CacheImplTests(ITestOutputHelper output) : base(output)
        {
            var services = Services;

            services.AddMemoryCache();

            services.AddTransient<InMemoryCacheImpl>();

            var container = services.BuildServiceProvider();

            Sut = container.GetRequiredService<InMemoryCacheImpl>();
        }

        [Fact]
        public async Task WhenGet_not_in_cache()
        {
            var key = Fixture.Create<Guid>().ToString("N");

            var result = await Sut.GetAsync<object>(key, CancellationToken.None).ConfigureAwait(false);

            result.Should().BeNull();
        }

        [Fact]
        public async Task WhenGet_in_cache_return_object()
        {
            var key = Fixture.Create<Guid>().ToString("N");

            var data = Fixture.Create<SomeDto>();
            Sut.Cache.Set(key, data);

            var result = await Sut.GetAsync<SomeDto>(key, CancellationToken.None).ConfigureAwait(false);

            result.Should().BeSameAs(data);
        }

        [Fact]
        public async Task WhenSet_in_cache_return_object()
        {
            var key = Fixture.Create<Guid>().ToString("N");

            var data = Fixture.Create<SomeDto>();

            await Sut.SetAsync(key, data, null, null, null, CancellationToken.None).ConfigureAwait(false);

            var result = await Sut.GetAsync<SomeDto>(key, CancellationToken.None).ConfigureAwait(false);

            result.Should().BeSameAs(data);
        }

        [Fact]
        public async Task WhenInvalidate_then_item_is_removed()
        {
            var key = Fixture.Create<Guid>().ToString("N");

            var data = Fixture.Create<SomeDto>();

            await Sut.SetAsync(key, data, null, null, null, CancellationToken.None).ConfigureAwait(false);

            var result1 = await Sut.GetAsync<SomeDto>(key, CancellationToken.None).ConfigureAwait(false);

            result1.Should().BeSameAs(data);

            await Sut.InvalidateCache(key).ConfigureAwait(false);

            var result2 = await Sut.GetAsync<SomeDto>(key, CancellationToken.None).ConfigureAwait(false);
            result2.Should().BeNull();
        }

        private class SomeDto
        {
            public string Data { get; set; }
        }
    }
}