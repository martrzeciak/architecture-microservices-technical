using MassTransit;
using OrderService.Events;

namespace OrderService.Saga;

public class OrderSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = null!;
    public string CustomerId { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OrderProcessingSaga : MassTransitStateMachine<OrderSagaState>
{
    public State AwaitingPayment { get; private set; } = null!;
    public State Completed { get; private set; } = null!;
    public State Cancelled { get; private set; } = null!;

    public Event<OrderCreated> OrderCreated { get; private set; } = null!;
    public Event<PaymentReceived> PaymentReceived { get; private set; } = null!;
    public Event<OrderCompleted> OrderCompleted { get; private set; } = null!;
    public Event<OrderCancelled> OrderCancelled { get; private set; } = null!;

    public OrderProcessingSaga()
    {
        InstanceState(x => x.CurrentState);

        Event(() => OrderCreated,   x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => PaymentReceived, x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => OrderCompleted,  x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => OrderCancelled,  x => x.CorrelateById(ctx => ctx.Message.OrderId));

        Initially(
            When(OrderCreated)
                .Then(ctx =>
                {
                    ctx.Saga.CustomerId = ctx.Message.CustomerId;
                    ctx.Saga.TotalAmount = ctx.Message.TotalPrice;
                    ctx.Saga.CreatedAt = ctx.Message.CreatedAt;
                })
                .TransitionTo(AwaitingPayment)
        );

        During(AwaitingPayment,
            When(PaymentReceived)
                .TransitionTo(Completed),
            When(OrderCancelled)
                .Then(ctx => { /* log reason if needed */ })
                .TransitionTo(Cancelled)
        );

        During(Completed,
            When(OrderCompleted)
                .Finalize()
        );

        SetCompletedWhenFinalized();
    }
}
