using MassTransit;
using RabbitMQ.Client;
using TrackingService.Consumers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderPlacedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        // especificando para o MassTransit a usar o RabbitMQ como o broker de mensagens
        cfg.Host("rabbitmq://localhost");

        // especificando o nome da fila que o serviço ShippingService irá consumir
        cfg.ReceiveEndpoint("tracking-order-placed", e =>
        {
            // especificando o consumidor que irá processar as mensagens recebidas
            e.Consumer<OrderPlacedConsumer>(context);

            #region [ USING DIRECT ]

            //// especificando o binding do exchange "order-placed-exchange" para a fila "tracking-order-placed"
            //e.Bind("order-placed-exchange", x =>
            //{
            //    x.RoutingKey = "order.tracking";
            //    x.ExchangeType = "direct";
            //});

            #endregion

            #region [ USING FANOUT ]

            // especificando o binding do exchange "order-placed-exchange" para a fila "tracking-order-placed"
            //e.Bind("order-placed-exchange", x =>
            //{
            //    x.ExchangeType = "fanout";
            //});

            #endregion

            #region [ USING TOPIC ]

            // especificando o binding do exchange "order-placed-exchange" para a fila "tracking-order-placed"
            //e.Bind("order-placed-exchange", x =>
            //{
            //    x.RoutingKey = "order.#";
            //    x.ExchangeType = "topic";
            //});

            #endregion

            #region [ USING HEADERS ]

            // especificando o binding do exchange "order-placed-exchange" para a fila "tracking-order-placed"
            e.Bind("order-placed-exchange", x =>
            {
                x.ExchangeType = ExchangeType.Headers;

                // * RULES
                x.SetBindingArgument("departament", "tracking");
                x.SetBindingArgument("priority", "low");
                // "all" significa que todas as condições devem ser atendidas para que a mensagem seja roteada para esta fila
                x.SetBindingArgument("x-match", "all");
            });

            #endregion
        });
    });
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();