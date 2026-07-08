using MassTransit;
using SharedMessages.Messages;

namespace ShippingService.Consumers;

public class OrderPlacedConsumer : IConsumer<OrderPlaced>
{
    public Task Consume(ConsumeContext<OrderPlaced> context)
    {
        Console.WriteLine($"Order received for shipping: {context.Message.OrderId}");
        return Task.CompletedTask;
    }
}
