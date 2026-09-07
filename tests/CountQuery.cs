namespace Audacia.Mediator.Tests;

/// <summary>
/// A second request type with a different response type, used to prove that a behaviour registered for
/// one request does not run for another, and that the executor cache does not confuse the two.
/// </summary>
internal sealed record CountQuery : IRequest<int>;

/// <inheritdoc cref="CountQuery"/>
/// <param name="log">The log to record to.</param>
internal sealed class CountQueryHandler(PipelineLog log) : IRequestHandler<CountQuery, int>
{
    /// <summary>
    /// The response this handler always returns.
    /// </summary>
    public const int Count = 42;

    /// <inheritdoc/>
    public Task<int> HandleAsync(CountQuery request, CancellationToken cancellationToken)
    {
        log.Record("count-handler");

        return Task.FromResult(Count);
    }
}
