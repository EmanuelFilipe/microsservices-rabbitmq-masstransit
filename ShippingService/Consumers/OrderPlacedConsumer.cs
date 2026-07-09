using MassTransit;
using SharedMessages.Messages;

namespace ShippingService.Consumers;

public class OrderPlacedConsumer : IConsumer<OrderPlaced>
{
    public Task Consume(ConsumeContext<OrderPlaced> context)
    {
        // este codigo irá criar no RabbitMQ um shipping-order-queue_error, contentdo o trace e metadados da mensagem automaticamente
        if (context.Message.Quantity <= 0)
        {
            Console.WriteLine($"Rejected order with ID: {context.Message.OrderId}");
            throw new Exception("Invalid quantity, rejecting the message.");
        }

        Console.WriteLine($"Order received for shipping: {context.Message.OrderId} and quantity {context.Message.Quantity}");
        return Task.CompletedTask;
    }
}
