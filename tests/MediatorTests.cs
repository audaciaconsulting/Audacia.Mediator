using Microsoft.Extensions.DependencyInjection;

namespace Audacia.Mediator.Tests;

/// <summary>
/// Exercises dispatch as a consumer uses it: services registered by <c>AddMediator</c>, resolved from a
/// scope, sent through <see cref="IMediator"/>.
/// </summary>
public class MediatorTests
{
    private const string EchoedValue = "echoed";

    [Fact]
    public async Task A_request_is_dispatched_to_its_handler_and_the_response_returned()
    {
        await using var provider = CreateProvider();
        using var scope = provider.CreateScope();

        var response = await SendAsync(scope, new EchoQuery(EchoedValue));

        response.ShouldBe(EchoedValue);
    }

    [Fact]
    public async Task Behaviours_are_composed_in_registration_order_around_the_handler()
    {
        await using var provider = CreateProvider(services => services
            .AddPipelineBehavior(typeof(FirstBehavior<,>))
            .AddPipelineBehavior(typeof(SecondBehavior<,>)));
        using var scope = provider.CreateScope();

        await SendAsync(scope, new EchoQuery(EchoedValue));

        LogFor(scope).Steps.ShouldBe(
            ["first:before", "second:before", "handler", "second:after", "first:after"]);
    }

    [Fact]
    public async Task A_behaviour_that_never_calls_its_continuation_stops_the_handler_running()
    {
        await using var provider = CreateProvider(services => services
            .AddPipelineBehavior<ShortCircuitEchoBehavior>());
        using var scope = provider.CreateScope();

        var response = await SendAsync(scope, new EchoQuery(EchoedValue));

        response.ShouldBe(ShortCircuitEchoBehavior.Response);
        LogFor(scope).Steps.ShouldBe(["short-circuit"]);
    }

    [Fact]
    public async Task A_behaviour_registered_for_one_request_does_not_run_for_another()
    {
        await using var provider = CreateProvider(services => services
            .AddPipelineBehavior<ShortCircuitEchoBehavior>());
        using var scope = provider.CreateScope();

        var response = await SendAsync(scope, new CountQuery());

        response.ShouldBe(CountQueryHandler.Count);
        LogFor(scope).Steps.ShouldBe(["count-handler"]);
    }

    [Fact]
    public async Task An_open_generic_behaviour_only_runs_for_requests_that_satisfy_its_constraints()
    {
        await using var provider = CreateProvider(services => services
            .AddPipelineBehavior(typeof(TrackedOnlyBehavior<,>)));
        using var scope = provider.CreateScope();

        await SendAsync(scope, new EchoQuery(EchoedValue));
        await SendAsync(scope, new TrackedQuery());

        LogFor(scope).Steps.ShouldBe(["handler", "tracked-only", "tracked-handler"]);
    }

    [Fact]
    public async Task The_cancellation_token_is_threaded_through_the_pipeline_to_the_handler()
    {
        await using var provider = CreateProvider(services => services
            .AddPipelineBehavior(typeof(FirstBehavior<,>)));
        using var scope = provider.CreateScope();
        using var cancellationTokenSource = new CancellationTokenSource();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.SendAsync(new EchoQuery(EchoedValue), cancellationTokenSource.Token);

        LogFor(scope).HandlerToken.ShouldBe(cancellationTokenSource.Token);
    }

    /// <summary>
    /// The executor for a request type is cached statically and therefore shared between scopes, so
    /// this is what proves the cache holds no scoped state of its own.
    /// </summary>
    [Fact]
    public async Task A_request_sent_from_two_scopes_is_handled_by_each_scope_own_services()
    {
        await using var provider = CreateProvider();
        using var firstScope = provider.CreateScope();
        using var secondScope = provider.CreateScope();

        await SendAsync(firstScope, new EchoQuery(EchoedValue));
        await SendAsync(secondScope, new EchoQuery(EchoedValue));

        LogFor(firstScope).Steps.ShouldBe(["handler"]);
        LogFor(secondScope).Steps.ShouldBe(["handler"]);
    }

    [Fact]
    public async Task Sending_a_null_request_throws_before_anything_is_resolved()
    {
        await using var provider = CreateProvider();
        using var scope = provider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await Should.ThrowAsync<ArgumentNullException>(
            () => mediator.SendAsync<string>(null!, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task A_request_with_no_registered_handler_throws()
    {
        await using var provider = CreateProvider();
        using var scope = provider.CreateScope();

        await Should.ThrowAsync<InvalidOperationException>(() => SendAsync(scope, new UnhandledQuery()));
    }

    private static ServiceProvider CreateProvider(Action<IServiceCollection>? configureBehaviors = null)
    {
        var services = new ServiceCollection()
            .AddScoped<PipelineLog>()
            .AddMediator(typeof(EchoQuery).Assembly);

        configureBehaviors?.Invoke(services);

        return services.BuildServiceProvider();
    }

    private static Task<TResponse> SendAsync<TResponse>(IServiceScope scope, IRequest<TResponse> request)
    {
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        return mediator.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private static PipelineLog LogFor(IServiceScope scope)
    {
        return scope.ServiceProvider.GetRequiredService<PipelineLog>();
    }
}
