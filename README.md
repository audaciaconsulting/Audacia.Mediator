# Audacia.Mediator

A small in-process mediator for .NET. It dispatches each *request* (a command or a query) to the one
*handler* that knows how to handle it, and it can run cross-cutting *behaviours* (logging,
validation, and so on) around that handler.

Its only dependency is `Microsoft.Extensions.DependencyInjection.Abstractions`, so it can be
referenced from an application layer without dragging a web framework, a validation library or a
result type in with it.

## Why use a mediator?

Without a mediator, an entry point such as a controller has to know about every service it uses, and
business rules tend to spread across those entry points as the application grows. With a mediator
the entry point knows about one thing, `IMediator`, and each piece of work lives in its own
handler class, where it is easy to find and easy to test.

There are three building blocks:

| Building block | What it is                                     | Example                                   |
| -------------- | ---------------------------------------------- | ----------------------------------------- |
| **Request**    | A plain object describing a piece of work.     | `GetProductQuery`, `CreateProductCommand` |
| **Handler**    | The one class that carries out that work.      | `GetProductQueryHandler`                  |
| **Behaviour**  | An optional step that runs around the handler. | `LoggingBehavior`, `ValidationBehavior`   |

## 1. Define a request

A request is a marker for its own response type: `IRequest<ProductDto>` reads as "send me and you
will get a `ProductDto` back". That keeps dispatch type safe: the response type never has to be
restated at the call site.

```csharp
public sealed record GetProductQuery(int Id) : IRequest<ProductDto>;
```

## 2. Write the handler

Each request has exactly one handler. Handlers are plain classes with constructor injection, so they
can take whatever services they need:

```csharp
internal sealed class GetProductQueryHandler(IDatabaseContext database)
    : IRequestHandler<GetProductQuery, ProductDto>
{
    public async Task<ProductDto> HandleAsync(
        GetProductQuery request,
        CancellationToken cancellationToken)
    {
        // ...
    }
}
```

## 3. Register everything

```csharp
services
    .AddMediator(typeof(GetProductQueryHandler).Assembly)
    .AddPipelineBehavior(typeof(LoggingBehavior<,>));
```

`AddMediator` registers `IMediator` and every `IRequestHandler<TRequest, TResponse>` implementation
it finds in the assemblies given to it, all scoped. Call it once for each assembly that contains
handlers; registering the same handler twice is ignored.

## 4. Send the request

Inject `IMediator` where the work starts and pass the request in. The response type is inferred from
the request, so it is never restated:

```csharp
public class ProductsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync(int id, CancellationToken cancellationToken)
    {
        var product = await mediator.SendAsync(new GetProductQuery(id), cancellationToken);

        // ...
    }
}
```

A request is dispatched to exactly one handler, never zero and never more than one. If no handler is
registered for a request, `SendAsync` throws from the container.

## Behaviours: wrapping the pipeline

A behaviour is a cross-cutting step that runs around the rest of the pipeline. Call `continuation`
to carry on to the next step, or return a response without calling it to stop early
("short-circuit"), which is how a validation behaviour bails out before the handler runs.

```csharp
public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<TRequest> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerContinuation<TResponse> continuation,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling {Request}", typeof(TRequest).Name);

        return await continuation(cancellationToken);
    }
}
```

### Ordering

Behaviours run in registration order: the first one added is the outermost. Given

```csharp
services
    .AddPipelineBehavior(typeof(LoggingBehavior<,>))
    .AddPipelineBehavior(typeof(ValidationBehavior<,>));
```

a request flows through the pipeline like this, and the response flows back the other way:

```
→ LoggingBehavior → ValidationBehavior → handler
← LoggingBehavior ← ValidationBehavior ←
```

### Choosing which requests a behaviour applies to

A behaviour can apply to every request, to a subset of requests, or to exactly one request,
depending on how it is declared.

**Every request: an open generic behaviour.** The behaviours seen so far are open generics, declared
with two type parameters and registered with the `<,>` syntax:

```csharp
services.AddPipelineBehavior(typeof(LoggingBehavior<,>));
```

The `<,>` means the type parameters are left open; the container fills them in with the request and
response types of each request as it is dispatched. With no further constraints, the behaviour runs
for every request.

**A subset of requests: add constraints.** Constrain the type parameters and the behaviour only runs
for requests whose types satisfy them. When a request is dispatched, a behaviour whose constraints
do not hold for that request is silently skipped. This behaviour, for example, only runs for
requests that return an `ErrorOr` result:

```csharp
public sealed class ErrorAuditingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
{
    // ...
}
```

It is registered the same way, with `typeof(ErrorAuditingBehavior<,>)`, but a request returning a
plain `ProductDto` is never wrapped by it.

**Exactly one request: a closed behaviour.** Declare the behaviour against the concrete request and
response types, and register it with the generic overload:

```csharp
public sealed class ThrottleCreateProductBehavior
    : IPipelineBehavior<CreateProductCommand, ProductDto>
{
    // ...
}

services.AddPipelineBehavior<ThrottleCreateProductBehavior>();
```

It only ever runs for `CreateProductCommand`; every other request is unaffected.

All three kinds can sit in the same pipeline, and the ordering rules above still apply: each
behaviour runs in registration order around the requests it applies to.

## How dispatch works (optional reading)

`SendAsync` is generic over the response but not over the request, so the request's concrete type is
only known at runtime. Rather than reflect over it on every call, the mediator builds a closed
generic executor for each request type once and caches it, so a dispatch costs a dictionary lookup
and a virtual call. The executors are stateless and resolve the handler and behaviours from the
scope they are handed, so the cache is safely shared across scopes.

## What it does not do

There is no notification or publish/subscribe support, no streaming requests, and no
pre/post-processor concept separate from behaviours. Requests are dispatched to exactly one handler,
and a missing handler throws from the container.
