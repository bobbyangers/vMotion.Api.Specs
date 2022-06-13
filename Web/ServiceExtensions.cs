using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;

namespace vMotion.Api.Specs
{
    public static class ServiceExtensions
    {
        public static IServiceCollection ReplaceWithFake<T>(this IServiceCollection services) where T : class
        {
            ReplaceWithFake<T>(services, _ => { });

            return services;
        }

        public static IServiceCollection ReplaceWithFake<T>(this IServiceCollection services,
            Action<T> configure) where T : class
        {
            var local = Substitute.For<T>();

            services.AddScoped(_ => local);

            configure.Invoke(local);

            return services;
        }
    }
}