namespace Audacia.Mediator;

/// <summary>
/// A cross-cutting step that wraps the handling of a <typeparamref name="TRequest"/>.
/// </summary>
/// <remarks>
/// <para>
/// Behaviours are composed in registration order, so the first registered behaviour is the outermost.
/// </para>
/// </remarks>
/// <typeparam name="TRequest">The type of the request being handled.</typeparam>
/// <typeparam name="TResponse">The type of the response produced by handling the request.</typeparam>
public interface IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Runs this behaviour around the rest of the pipeline.
    /// </summary>
    /// <param name="request">The request being handled.</param>
    /// <param name="continuation">The next step in the pipeline.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>
    /// The response produced by the rest of the pipeline, or a short-circuited response if this
    /// behaviour does not call <paramref name="continuation"/>.
    /// </returns>
    Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerContinuation<TResponse> continuation,
        CancellationToken cancellationToken);
}
