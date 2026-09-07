namespace Audacia.Mediator.Tests;

/// <summary>
/// A request whose response satisfies the constraint on <see cref="TrackedOnlyBehavior{TRequest,TResponse}"/>.
/// </summary>
internal sealed record TrackedQuery : IRequest<TrackedResponse>;

/// <summary>
/// The response type a behaviour can constrain itself to.
/// </summary>
internal interface ITrackedResponse;

/// <inheritdoc cref="ITrackedResponse"/>
internal sealed record TrackedResponse : ITrackedResponse;

/// <inheritdoc cref="TrackedQuery"/>
/// <param name="log">The log to record to.</param>
internal sealed class TrackedQueryHandler(PipelineLog log) : IRequestHandler<TrackedQuery, TrackedResponse>
{
    /// <inheritdoc/>
    public Task<TrackedResponse> HandleAsync(TrackedQuery request, CancellationToken cancellationToken)
    {
        log.Record("tracked-handler");

        return Task.FromResult(new TrackedResponse());
    }
}
