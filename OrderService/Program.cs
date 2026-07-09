using MassTransit;
using RabbitMQ.Client;
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

        // especificando o nome da fila que o serviço OrderService irá publicar
        RMQ_cfg.Message<OrderPlaced>(x => x.SetEntityName("order-placed-exchange"));

        // especificando o tipo de exchange 'DIRECT' que será usado para publicar a mensagem OrderPlaced
        //RMQ_cfg.Publish<OrderPlaced>(x => x.ExchangeType = "direct");

        //RMQ_cfg.Publish<OrderPlaced>(x => x.ExchangeType = "fanout");
        //RMQ_cfg.Publish<OrderPlaced>(x => x.ExchangeType = ExchangeType.FanOut.ToString());

        //RMQ_cfg.Publish<OrderPlaced>(x => x.ExchangeType = "topic");

        RMQ_cfg.Publish<OrderPlaced>(x => x.ExchangeType = ExchangeType.Headers);
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapPost("/orders", async (OrderRequest order, IBus bus) =>
{
    var orderPlacedMessage = new OrderPlaced(order.orderId, order.quantity);

    #region [ USING ROUTING KEY - DIRECT ]

    // publicando a mensagem OrderPlaced no RabbitMQ
    //await bus.Publish(orderPlacedMessage, context =>
    //{
    //    // especificando o RoutingKey de acordo com a quantidade de itens do pedido
    //    var routingKey = order.quantity > 10 ? "order.shipping" : "order.tracking";
    //    context.SetRoutingKey(routingKey);
    //});

    #endregion

    #region [ USING FANOUT WITHOUT ROUTING KEY ] 

    // publicando a mensagem OrderPlaced no RabbitMQ
    // e irá publicar diretamente no exchange "order-placed-queue" que foi configurado no MassTransit
    //await bus.Publish(orderPlacedMessage);

    #endregion

    #region [ USING ROUTING KEY - TOPIC ]

    //// publicando a mensagem OrderPlaced no RabbitMQ
    //await bus.Publish(orderPlacedMessage, context =>
    //{
    //    // especificando o RoutingKey de acordo com a quantidade de itens do pedido
    //    var routingKey = order.quantity > 10 ? "order.shipping" : "order.regular.tracking";
    //    context.SetRoutingKey(routingKey);
    //});

    #endregion

    #region [ USING HEADERS ]

    var headers = new Dictionary<string, object>();

    if (order.quantity > 10)
    {
        headers["departament"] = "shipping";
        headers["priority"] = "high";
    }
    else
    {
        headers["departament"] = "tracking";
        headers["priority"] = "low";
    }

    // publicando a mensagem OrderPlaced no RabbitMQ com headers personalizados
    await bus.Publish(orderPlacedMessage, context =>
    {
        context.Headers.Set("departament", headers["departament"]);
        context.Headers.Set("priority", headers["priority"]);
    });

    #endregion


    return Results.Created($"/orders/{order.orderId}", orderPlacedMessage);
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