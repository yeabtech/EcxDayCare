using MediatR;

namespace ECX_Daycare.Common;

/// <summary>
/// Abstraction for sending CQRS requests.
/// </summary>
public interface IMessageBus
{
    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}

/// <summary>
/// MediatR-based implementation of IMessageBus.
/// </summary>
public sealed class MessageBus : IMessageBus
{
    private readonly IMediator _mediator;

    public MessageBus(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        return _mediator.Send(request, cancellationToken);
    }
}
