namespace Audacia.Mediator;

/// <summary>
/// Marks a command or query that can be dispatched through an <see cref="IMediator"/>.
/// </summary>
/// <typeparam name="TResponse">The type of the response produced by handling the request.</typeparam>
public interface IRequest<TResponse>;
