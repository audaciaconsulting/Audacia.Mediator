namespace Audacia.Mediator;

/// <summary>
/// Dispatches commands and queries to their handler, composed with any registered pipeline behaviours.
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Sends the given <paramref name="request"/> through the pipeline to its handler.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response produced by handling the request.</typeparam>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The response produced by handling the <paramref name="request"/>.</returns>
    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken);
}
