namespace Audacia.Mediator;

/// <summary>
/// Represents the next step in the request pipeline, which is either the following
/// <see cref="IPipelineBehavior{TRequest,TResponse}"/> or the request handler itself.
/// </summary>
/// <typeparam name="TResponse">The type of the response produced by handling the request.</typeparam>
/// <param name="cancellationToken">A token used to cancel the operation.</param>
/// <returns>The response produced by the remainder of the pipeline.</returns>
public delegate Task<TResponse> RequestHandlerContinuation<TResponse>(CancellationToken cancellationToken);
