namespace Audacia.Mediator.Tests;

/// <summary>
/// An open generic behaviour whose constraints are only satisfied by a request returning an
/// <see cref="ITrackedResponse"/>.
/// </summary>
/// <typeparam name="TRequest">The type of the request being handled.</typeparam>
/// <typeparam name="TResponse">The type of the response produced by handling the request.</typeparam>
/// <param name="log">The log to record to.</param>
internal sealed class TrackedOnlyBehavior<TRequest, TResponse>(PipelineLog log)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : ITrackedResponse
{
    /// <inheritdoc/>
    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerContinuation<TResponse> continuation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(continuation);

        log.Record("tracked-only");

        return await continuation(cancellationToken);
    }
}
