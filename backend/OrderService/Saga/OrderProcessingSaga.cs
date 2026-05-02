using MassTransit;

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

    public Event<Events.OrderCreated> OrderCreated { get; private set; } = null!;

    public OrderProcessingSaga()
    {
        InstanceState(x => x.CurrentState);

        Event(() => OrderCreated, x => x.CorrelateById(context => context.Message.OrderId));

        Initially(
            When(OrderCreated)
                .Then(context =>
                {
                    context.Saga.CustomerId = context.Message.CustomerId;
                    context.Saga.TotalAmount = context.Message.TotalPrice;
                    context.Saga.CreatedAt = context.Message.CreatedAt;
                })
                .TransitionTo(AwaitingPayment)
        );
    }
}
