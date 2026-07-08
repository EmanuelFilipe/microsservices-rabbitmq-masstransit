using MassTransit;
using SharedMessages.Messages;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddMassTransit((x) =>
{
    // especificando para o MassTransit a usar o RabbitMQ como o broker de mensagens
    x.UsingRabbitMq((MT_context, RMQ_cfg) =>
    {
        // especificando a URL do RabbitMQ, que está rodando localmente
        RMQ_cfg.Host("rabbitmq://localhost");
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapPost("/orders", async (OrderRequest order, IBus bus) =>
{
    var orderPlacedMessages = new OrderPlaced(order.orderId, order.quantity);

    // publicando a mensagem OrderPlaced no RabbitMQ
    await bus.Publish(orderPlacedMessages);
    
    return Results.Created($"/orders/{order.orderId}", orderPlacedMessages);
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();

public record OrderRequest(Guid orderId, int quantity);