namespace Audacia.Mediator.Tests;

/// <summary>
/// The request used by most of these tests. Carries the value its handler echoes back, so a test can
/// tell the handler's response apart from a short-circuited one.
/// </summary>
/// <param name="Value">The value the handler echoes back.</param>
internal sealed record EchoQuery(string Value) : IRequest<string>;

/// <inheritdoc cref="EchoQuery"/>
/// <param name="log">The log to record to.</param>
internal sealed class EchoQueryHandler(PipelineLog log) : IRequestHandler<EchoQuery, string>
{
    /// <inheritdoc/>
    public Task<string> HandleAsync(EchoQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        log.Record("handler");
        log.HandlerToken = cancellationToken;

        return Task.FromResult(request.Value);
    }
}
