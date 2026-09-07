using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Audacia.Mediator;

/// <summary>
/// Extensions to <see cref="IServiceCollection"/> that register the mediator, the request handlers it
/// dispatches to, and the pipeline behaviours that run around them.
/// </summary>
public static class MediatorServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="IMediator"/> and every <see cref="IRequestHandler{TRequest,TResponse}"/>
    /// implementation found in the given <paramref name="handlerAssemblies"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Safe to call more than once; <see cref="IMediator"/> is registered only on the first call, and
    /// a handler that is already registered is not registered again, so handlers can be added from
    /// several assemblies, or the same assembly twice, without duplicating registrations.
    /// </para>
    /// </remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to which to add the mediator.</param>
    /// <param name="handlerAssemblies">The assemblies to scan for request handlers.</param>
    /// <returns>The given <paramref name="services"/>.</returns>
    public static IServiceCollection AddMediator(
        this IServiceCollection services,
        params Assembly[] handlerAssemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(handlerAssemblies);

        services.TryAddScoped<IMediator, Mediator>();

        foreach (var (serviceType, implementationType) in FindHandlers(handlerAssemblies))
        {
            services.TryAddEnumerable(ServiceDescriptor.Scoped(serviceType, implementationType));
        }

        return services;
    }

    /// <summary>
    /// Adds a pipeline behaviour that runs around the requests it applies to.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Behaviours are composed in registration order, so the first one added is the outermost.
    /// </para>
    /// </remarks>
    /// <typeparam name="TBehavior">
    /// The behaviour to add. Use the <see cref="AddPipelineBehavior(IServiceCollection,Type)"/> overload
    /// for an open generic behaviour such as <c>typeof(LoggingBehavior&lt;,&gt;)</c>.
    /// </typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which to add the behaviour.</param>
    /// <returns>The given <paramref name="services"/>.</returns>
    public static IServiceCollection AddPipelineBehavior<TBehavior>(this IServiceCollection services)
        where TBehavior : class
    {
        return services.AddPipelineBehavior(typeof(TBehavior));
    }

    /// <summary>
    /// Adds a pipeline behaviour that runs around the requests it applies to.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An open generic <paramref name="behaviorType"/> such as <c>typeof(LoggingBehavior&lt;,&gt;)</c> is
    /// registered against <see cref="IPipelineBehavior{TRequest,TResponse}"/> itself, so it applies to
    /// every request whose type arguments satisfy its constraints. A closed type is registered only
    /// against the request types it names.
    /// </para>
    /// <para>
    /// Behaviours are composed in registration order, so the first one added is the outermost. Adding
    /// the same behaviour twice registers it once.
    /// </para>
    /// </remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to which to add the behaviour.</param>
    /// <param name="behaviorType">The behaviour to add.</param>
    /// <returns>The given <paramref name="services"/>.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="behaviorType"/> does not implement
    /// <see cref="IPipelineBehavior{TRequest,TResponse}"/>.
    /// </exception>
    public static IServiceCollection AddPipelineBehavior(this IServiceCollection services, Type behaviorType)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(behaviorType);

        if (!behaviorType.IsClass || behaviorType.IsAbstract)
        {
            throw new ArgumentException($"'{behaviorType}' must be a non-abstract class.", nameof(behaviorType));
        }

        var behaviorInterfaces = GenericInterfacesOf(behaviorType, typeof(IPipelineBehavior<,>));

        if (behaviorInterfaces.Count == 0)
        {
            throw new ArgumentException(
                $"'{behaviorType}' does not implement '{typeof(IPipelineBehavior<,>)}'.",
                nameof(behaviorType));
        }

        List<Type> serviceTypes = behaviorType.IsGenericTypeDefinition
            ? [typeof(IPipelineBehavior<,>)]
            : behaviorInterfaces;

        foreach (var serviceType in serviceTypes)
        {
            services.TryAddEnumerable(ServiceDescriptor.Scoped(serviceType, behaviorType));
        }

        return services;
    }

    private static List<(Type ServiceType, Type ImplementationType)> FindHandlers(Assembly[] handlerAssemblies)
    {
        return
        [
            .. handlerAssemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(IsConcreteClass)
                .SelectMany(type => GenericInterfacesOf(type, typeof(IRequestHandler<,>))
                    .Select(interfaceType => (ServiceType: interfaceType, ImplementationType: type)))
        ];
    }

    private static bool IsConcreteClass(Type type)
    {
        return type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false };
    }

    private static List<Type> GenericInterfacesOf(Type type, Type openGenericInterface)
    {
        return
        [
            .. type
                .GetInterfaces()
                .Where(interfaceType => interfaceType.IsGenericType
                    && interfaceType.GetGenericTypeDefinition() == openGenericInterface)
        ];
    }
}
