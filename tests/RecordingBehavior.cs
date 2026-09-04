namespace Audacia.Mediator.Tests;

/// <summary>
/// An open generic behaviour that records when it ran, on either side of the rest of the pipeline.
/// </summary>
/// <typeparam name="TRequest">The type of the request being handled.</typeparam>
/// <typeparam name="TResponse">The type of the response produced by handling the request.</typeparam>
/// <param name="log">The log to record to.</param>
/// <param name="label">The name to record this behaviour under.</param>
internal abstract class RecordingBehavior<TRequest, TResponse>(PipelineLog log, string label)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <inheritdoc/>
    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerContinuation<TResponse> continuation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(continuation);

        log.Record($"{label}:before");
        var response = await continuation(cancellationToken);
        log.Record($"{label}:after");

        return response;
    }
}

/// <inheritdoc cref="RecordingBehavior{TRequest,TResponse}"/>
/// <typeparam name="TRequest">The type of the request being handled.</typeparam>
/// <typeparam name="TResponse">The type of the response produced by handling the request.</typeparam>
/// <param name="log">The log to record to.</param>
internal sealed class FirstBehavior<TRequest, TResponse>(PipelineLog log)
    : RecordingBehavior<TRequest, TResponse>(log, "first")
    where TRequest : IRequest<TResponse>;

/// <inheritdoc cref="RecordingBehavior{TRequest,TResponse}"/>
/// <typeparam name="TRequest">The type of the request being handled.</typeparam>
/// <typeparam name="TResponse">The type of the response produced by handling the request.</typeparam>
/// <param name="log">The log to record to.</param>
internal sealed class SecondBehavior<TRequest, TResponse>(PipelineLog log)
    : RecordingBehavior<TRequest, TResponse>(log, "second")
    where TRequest : IRequest<TResponse>;
