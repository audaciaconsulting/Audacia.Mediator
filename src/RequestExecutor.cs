namespace Audacia.Mediator;

/// <summary>
/// The non-generic-over-the-request half of a cached dispatch entry, allowing <see cref="Mediator"/> to
/// invoke a closed-generic pipeline without knowing the concrete request type at compile time.
/// </summary>
/// <typeparam name="TResponse">The type of the response produced by handling the request.</typeparam>
internal abstract class RequestExecutor<TResponse>
{
    /// <summary>
    /// Resolves the handler and behaviours for the request and runs the composed pipeline.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="serviceProvider">The scope from which to resolve the handler and behaviours.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The response produced by the pipeline.</returns>
    public abstract Task<TResponse> ExecuteAsync(
        object request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}
