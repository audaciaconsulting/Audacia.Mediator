using Microsoft.Extensions.DependencyInjection;

namespace Audacia.Mediator;

/// <summary>
/// The closed-generic executor for a single request type. One instance is built per request type by
/// <see cref="Mediator"/> and cached, so dispatch costs a dictionary lookup and a virtual call rather
/// than a reflection scan.
/// </summary>
/// <typeparam name="TRequest">The type of the request being handled.</typeparam>
/// <typeparam name="TResponse">The type of the response produced by handling the request.</typeparam>
internal sealed class ClosedRequestExecutor<TRequest, TResponse> : RequestExecutor<TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <inheritdoc/>
    public override Task<TResponse> ExecuteAsync(
        object request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TRequest, TResponse>>();

        var pipeline = BuildPipeline((TRequest)request, handler, behaviors);

        return pipeline(cancellationToken);
    }

    private static RequestHandlerContinuation<TResponse> BuildPipeline(
        TRequest request,
        IRequestHandler<TRequest, TResponse> handler,
        IEnumerable<IPipelineBehavior<TRequest, TResponse>> behaviors)
    {
        RequestHandlerContinuation<TResponse> continuation = token => handler.HandleAsync(request, token);

        // Wrapping in reverse means the first registered behaviour ends up outermost.
        foreach (var behavior in behaviors.Reverse())
        {
            var rest = continuation;

            continuation = token => behavior.HandleAsync(request, rest, token);
        }

        return continuation;
    }
}
