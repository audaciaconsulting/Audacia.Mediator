using System.Collections.Concurrent;

namespace Audacia.Mediator;

/// <summary>
/// A hand-rolled <see cref="IMediator"/> that caches the closed-generic executor for each request type.
/// </summary>
/// <remarks>
/// <para>
/// The executor cache is static and therefore shared across scopes; the executors themselves are
/// stateless and resolve the handler and behaviours from the scope passed into them.
/// </para>
/// </remarks>
/// <param name="serviceProvider">The scope from which handlers and behaviours are resolved.</param>
internal sealed class Mediator(IServiceProvider serviceProvider) : IMediator
{
    private static readonly ConcurrentDictionary<Type, object> Executors = new();

    /// <inheritdoc/>
    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var executor = (RequestExecutor<TResponse>)Executors.GetOrAdd(
            request.GetType(),
            CreateExecutor,
            typeof(TResponse));

        return executor.ExecuteAsync(request, serviceProvider, cancellationToken);
    }

    private static object CreateExecutor(Type requestType, Type responseType)
    {
        var executorType = typeof(ClosedRequestExecutor<,>).MakeGenericType(requestType, responseType);

        return Activator.CreateInstance(executorType)
            ?? throw new InvalidOperationException($"Unable to create a request executor for '{requestType}'.");
    }
}
