namespace Audacia.Mediator.Tests;

/// <summary>
/// The dependency this library does not have is what makes it liftable into another repository.
/// Anything needing a validation library, a result type or a logger is a pipeline behaviour, and
/// behaviours belong to the application that writes them.
/// </summary>
public class DependencySurfaceTests
{
    [Fact]
    public void The_library_references_nothing_but_the_framework_and_the_container_abstractions()
    {
        var referenced = typeof(IMediator).Assembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name ?? string.Empty)
            .Where(name => !IsFrameworkAssembly(name));

        referenced.ShouldBe(["Microsoft.Extensions.DependencyInjection.Abstractions"]);
    }

    private static bool IsFrameworkAssembly(string name)
    {
        return name is "netstandard" or "mscorlib" || name.StartsWith("System", StringComparison.Ordinal);
    }
}
