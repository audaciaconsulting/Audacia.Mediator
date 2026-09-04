namespace Audacia.Mediator.Tests;

/// <summary>
/// A behaviour closed over a single request type that never calls its continuation, so the handler
/// behind it never runs.
/// </summary>
/// <param name="log">The log to record to.</param>
internal sealed class ShortCircuitEchoBehavior(PipelineLog log) : IPipelineBehavior<EchoQuery, string>
{
    /// <summary>
    /// The response this behaviour short-circuits the pipeline with.
    /// </summary>
    public const string Response = "short-circuited";

    /// <inheritdoc/>
    public Task<string> HandleAsync(
        EchoQuery request,
        RequestHandlerContinuation<string> continuation,
        CancellationToken cancellationToken)
    {
        log.Record("short-circuit");

        return Task.FromResult(Response);
    }
}
