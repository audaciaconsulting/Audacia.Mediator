namespace Audacia.Mediator;

/// <summary>
/// Handles a single <typeparamref name="TRequest"/>.
/// </summary>
/// <typeparam name="TRequest">The type of the request being handled.</typeparam>
/// <typeparam name="TResponse">The type of the response produced by handling the request.</typeparam>
public interface IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the given <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The response produced by handling the <paramref name="request"/>.</returns>
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
}
