namespace Audacia.Mediator.Tests;

/// <summary>
/// A handler implementing two <see cref="IRequestHandler{TRequest,TResponse}"/> interfaces, to prove
/// assembly scanning registers it against both.
/// </summary>
internal sealed class MultiRequestHandler
    : IRequestHandler<FirstDualQuery, string>, IRequestHandler<SecondDualQuery, int>
{
    /// <inheritdoc/>
    public Task<string> HandleAsync(FirstDualQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(string.Empty);
    }

    /// <inheritdoc/>
    public Task<int> HandleAsync(SecondDualQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(0);
    }
}

/// <summary>
/// The first of the two requests <see cref="MultiRequestHandler"/> handles.
/// </summary>
internal sealed record FirstDualQuery : IRequest<string>;

/// <summary>
/// The second of the two requests <see cref="MultiRequestHandler"/> handles.
/// </summary>
internal sealed record SecondDualQuery : IRequest<int>;
