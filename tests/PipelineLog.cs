namespace Audacia.Mediator.Tests;

/// <summary>
/// Records what the pipeline did, so a test can assert on the order things ran in rather than only on
/// the response. Registered as scoped, so each scope gets its own.
/// </summary>
internal sealed class PipelineLog
{
    private readonly List<string> _steps = [];

    /// <summary>
    /// Gets every step the pipeline recorded, in the order it ran.
    /// </summary>
    public IReadOnlyList<string> Steps => _steps;

    /// <summary>
    /// Gets or sets the cancellation token the handler was given.
    /// </summary>
    public CancellationToken HandlerToken { get; set; }

    /// <summary>
    /// Records that the named step ran.
    /// </summary>
    /// <param name="step">The name of the step that ran.</param>
    public void Record(string step)
    {
        _steps.Add(step);
    }
}
