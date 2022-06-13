using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;
using Newtonsoft.Json;
using NSubstitute;
using System;
using vMotion.Dal.MongoDb.Entities;
using Xunit.Abstractions;

namespace vMotion.Api.Specs;

public abstract class AutoFixtureTests<T> : AutoFixtureTests
{
    public T Sut { get; protected set; }

    protected AutoFixtureTests(ITestOutputHelper output) : base(output)
    {
    }
}

public abstract class AutoFixtureTests : IDisposable
{
    private readonly ITestOutputHelper _output;

    private readonly IFixture _fixture;
    protected IFixture Fixture => _fixture;

    private readonly IServiceCollection _services = new ServiceCollection();
    protected IServiceCollection Services => _services;

    private IServiceProvider _container;
    protected IServiceProvider Container => _container ??= Services.BuildServiceProvider();
    protected AutoFixtureTests(ITestOutputHelper output)
    {
        _output = output;

        _fixture = new Fixture();
        _fixture.Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });

        _fixture.Customize(ObjectIdSpecimenBuilder.ToCustomization());

        _fixture.Customize(new RelationshipObject<OperatorEntity>().ToCustomization());
        _fixture.Customize(new RelationshipObject<RoleEntity>().ToCustomization());
        _fixture.Customize(new RelationshipObject<MemberEntity>().ToCustomization());
        _fixture.Customize(new RelationshipObject<StaffEntity>().ToCustomization());
        _fixture.Customize(new RelationshipObject<CaseEntity>().ToCustomization());
        _fixture.Customize(new RelationshipObject<CallEntity>().ToCustomization());

        _fixture.Customize(VersionSpecimenBuilder.ToCustomization());
        _fixture.Customize(PostalCodeStringsGenerator.ToCustomization());
        _fixture.Customize(CountryCodeStringsGenerator.ToCustomization());

        _fixture.Customize(PhoneStringsGenerator.ToCustomization());
        _fixture.Customize(EmailAddressStringsGenerator.ToCustomization());
        _fixture.Customize(LinkSpecimenBuilder.ToCustomization());

        _services.AddSingleton(_ => new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets<AutoFixtureTests>()
            .Build());

        var clock = Substitute.For<ISystemClock>();
        clock.UtcNow.Returns(_ => DateTimeOffset.Now);

        Services.AddSingleton(_ => clock);
    }

    #region IDisposable Implementation

    ~AutoFixtureTests()
    {
        Dispose(false);
    }

    // ReSharper disable once FlagArgument
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion

    protected void ShowResult(object result)
    {
        ShowResult(JsonConvert.SerializeObject(result, Formatting.Indented));
    }

    protected void ShowResult(string result)
    {
        _output.WriteLine(new string('-', 50));
        _output.WriteLine(result);
    }

    public T Create<T>()
    {
        return _fixture.Create<T>();
    }

    public TR Create<T, TR>() where T : TR
    {
        return _fixture.Create<T>().As<TR>();
    }

    protected T CreateActor<T>() where T : class
    {
        var actor = typeof(T).IsInterface ? Substitute.For<T>() : Fixture.Create<T>();
        return SetActor(actor);
    }

    public T GetActor<T>()
    {
        return _fixture.Create<T>();
    }

    public T SetActor<T>(T actor) where T : class
    {
        _fixture.Inject(actor);
        Services.AddSingleton(_ => actor);
        return actor;
    }
}