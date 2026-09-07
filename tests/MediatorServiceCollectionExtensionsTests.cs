using Microsoft.Extensions.DependencyInjection;

namespace Audacia.Mediator.Tests;

/// <summary>
/// Covers what registration puts in the container, as opposed to what dispatch then does with it.
/// </summary>
public class MediatorServiceCollectionExtensionsTests
{
    [Fact]
    public void Adding_the_mediator_registers_every_handler_in_the_assembly()
    {
        var services = new ServiceCollection().AddMediator(typeof(EchoQuery).Assembly);

        Implementations(services, typeof(IRequestHandler<EchoQuery, string>))
            .ShouldBe([typeof(EchoQueryHandler)]);
        Implementations(services, typeof(IRequestHandler<CountQuery, int>))
            .ShouldBe([typeof(CountQueryHandler)]);
    }

    [Fact]
    public void A_handler_of_more_than_one_request_is_registered_against_each_of_them()
    {
        var services = new ServiceCollection().AddMediator(typeof(EchoQuery).Assembly);

        Implementations(services, typeof(IRequestHandler<FirstDualQuery, string>))
            .ShouldBe([typeof(MultiRequestHandler)]);
        Implementations(services, typeof(IRequestHandler<SecondDualQuery, int>))
            .ShouldBe([typeof(MultiRequestHandler)]);
    }

    [Fact]
    public void Adding_the_mediator_registers_the_mediator_and_its_handlers_as_scoped()
    {
        var services = new ServiceCollection().AddMediator(typeof(EchoQuery).Assembly);

        services.ShouldAllBe(descriptor => descriptor.Lifetime == ServiceLifetime.Scoped);
    }

    /// <summary>
    /// Handlers can come from more than one assembly, which means more than one call, which must not
    /// leave a second <see cref="IMediator"/> registration behind.
    /// </summary>
    [Fact]
    public void Adding_the_mediator_twice_registers_one_mediator()
    {
        var assembly = typeof(EchoQuery).Assembly;

        var services = new ServiceCollection()
            .AddMediator(assembly)
            .AddMediator(assembly);

        services.Count(descriptor => descriptor.ServiceType == typeof(IMediator)).ShouldBe(1);
    }

    /// <summary>
    /// Repeat calls with the same assembly must not stack duplicate descriptors up for a handler.
    /// </summary>
    [Fact]
    public void Adding_the_mediator_twice_registers_each_handler_once()
    {
        var assembly = typeof(EchoQuery).Assembly;

        var services = new ServiceCollection()
            .AddMediator(assembly)
            .AddMediator(assembly);

        Implementations(services, typeof(IRequestHandler<EchoQuery, string>))
            .ShouldBe([typeof(EchoQueryHandler)]);
    }

    [Fact]
    public void An_open_generic_behaviour_is_registered_against_the_open_generic_behaviour_interface()
    {
        var services = new ServiceCollection().AddPipelineBehavior(typeof(FirstBehavior<,>));

        Implementations(services, typeof(IPipelineBehavior<,>))
            .ShouldBe([typeof(FirstBehavior<,>)]);
    }

    [Fact]
    public void Adding_the_same_behaviour_twice_registers_it_once()
    {
        var services = new ServiceCollection()
            .AddPipelineBehavior(typeof(FirstBehavior<,>))
            .AddPipelineBehavior(typeof(FirstBehavior<,>));

        Implementations(services, typeof(IPipelineBehavior<,>))
            .ShouldBe([typeof(FirstBehavior<,>)]);
    }

    [Fact]
    public void A_closed_behaviour_is_registered_only_against_the_request_it_names()
    {
        var services = new ServiceCollection().AddPipelineBehavior<ShortCircuitEchoBehavior>();

        Implementations(services, typeof(IPipelineBehavior<EchoQuery, string>))
            .ShouldBe([typeof(ShortCircuitEchoBehavior)]);
        services.Select(descriptor => descriptor.ServiceType)
            .ShouldNotContain(typeof(IPipelineBehavior<,>));
    }

    [Fact]
    public void A_type_that_is_not_a_behaviour_is_rejected_rather_than_registered()
    {
        var services = new ServiceCollection();

        var exception = Should.Throw<ArgumentException>(AddNotABehavior);

        exception.ParamName.ShouldBe("behaviorType");
        services.ShouldBeEmpty();

        void AddNotABehavior()
        {
            services.AddPipelineBehavior<NotABehavior>();
        }
    }

    private static List<Type> Implementations(IServiceCollection services, Type serviceType)
    {
        return
        [
            .. services
                .Where(descriptor => descriptor.ServiceType == serviceType)
                .Select(descriptor => descriptor.ImplementationType!)
        ];
    }
}
