namespace Audacia.Mediator.Tests;

/// <summary>
/// A request deliberately left without a handler.
/// </summary>
internal sealed record UnhandledQuery : IRequest<string>;

/// <summary>
/// Not a behaviour at all, used to check the guard on
/// <c>MediatorServiceCollectionExtensions.AddPipelineBehavior</c>.
/// </summary>
internal sealed class NotABehavior;
