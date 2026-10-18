using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using Tillegal.Api.Controllers;
using Tillegal.Application;
using Tillegal.Domain.Common;
using Tillegal.Infrastructure;
using Tillegal.Persistence.Context;
using Xunit;

namespace Tillegal.ArchitectureTests;

public sealed class CleanArchitectureTests
{
    private static readonly Assembly DomainAssembly = typeof(BaseEntity).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(Tillegal.Application.DependencyInjection).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(Tillegal.Infrastructure.DependencyInjection).Assembly;
    private static readonly Assembly PersistenceAssembly = typeof(AppDbContext).Assembly;
    private static readonly Assembly ApiAssembly = typeof(BaseApiController).Assembly;

    [Fact]
    public void Domain_ShouldNot_HaveDependencyOnOtherProjects()
    {
        var otherProjects = new[]
        {
            ApplicationAssembly.GetName().Name!,
            InfrastructureAssembly.GetName().Name!,
            PersistenceAssembly.GetName().Name!,
            ApiAssembly.GetName().Name!
        };

        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(otherProjects)
            .GetResult();

        result.IsSuccessful.Should().BeTrue("Domain must not depend on any outer architectural layers.");
    }

    [Fact]
    public void Application_ShouldNot_HaveDependencyOn_OuterLayers()
    {
        var outerLayers = new[]
        {
            InfrastructureAssembly.GetName().Name!,
            PersistenceAssembly.GetName().Name!,
            ApiAssembly.GetName().Name!
        };

        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(outerLayers)
            .GetResult();

        result.IsSuccessful.Should().BeTrue("Application must not depend on Infrastructure, Persistence, or Api.");
    }

    [Fact]
    public void Infrastructure_ShouldNot_HaveDependencyOn_PersistenceOrApi()
    {
        var forbiddenProjects = new[]
        {
            PersistenceAssembly.GetName().Name!,
            ApiAssembly.GetName().Name!
        };

        var result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenProjects)
            .GetResult();

        result.IsSuccessful.Should().BeTrue("Infrastructure must not depend on Persistence or Api.");
    }

    [Fact]
    public void Persistence_ShouldNot_HaveDependencyOn_InfrastructureOrApi()
    {
        var forbiddenProjects = new[]
        {
            InfrastructureAssembly.GetName().Name!,
            ApiAssembly.GetName().Name!
        };

        var result = Types.InAssembly(PersistenceAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenProjects)
            .GetResult();

        result.IsSuccessful.Should().BeTrue("Persistence must not depend on Infrastructure or Api.");
    }
}
