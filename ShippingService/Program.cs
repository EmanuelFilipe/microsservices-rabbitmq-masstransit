using MassTransit;
using RabbitMQ.Client;
using ShippingService.Consumers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderPlacedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        // especificando para o MassTransit a usar o RabbitMQ como o broker de mensagens
        cfg.Host("rabbitmq://localhost");

        // especificando o nome da fila que o serviço ShippingService irá consumir
        cfg.ReceiveEndpoint("shipping-order-queue", e =>
        {
            // especificando o consumidor que irá processar as mensagens recebidas
            // Ex: comentando essa linha ira gerar um skipped queue, ou seja, a fila "shipping-order-queue"
            // será criada mas não terá nenhum consumidor associado a ela e as mensagens publicadas para essa fila não serão consumidas.
            //e.Consumer<OrderPlacedConsumer>(context);
            e.ConfigureConsumer<OrderPlacedConsumer>(context);

            #region [ USING DIRECT ]
            // especificando o binding do exchange "order-placed-exchange" para a fila "shipping-order-queue"
            //e.Bind("order-placed-exchange", x =>
            //{
            //    x.RoutingKey = "order.shipping";
            //    x.ExchangeType = "direct";
            //});
            #endregion

            #region [ USING FANOUT ]

            // especificando o binding do exchange "order-placed-exchange" para a fila "shipping-order-queue"
            //e.Bind("order-placed-exchange", x =>
            //{
            //    x.ExchangeType = "fanout";
            //});

            #endregion

            #region [ USING TOPIC ]

            //// especificando o binding do exchange "order-placed-exchange" para a fila "shipping-order-queue"
            //e.Bind("order-placed-exchange", x =>
            //{
            //    x.RoutingKey = "order.*";
            //    x.ExchangeType = "topic";
            //});

            #endregion

            #region [ USING HEADERS ]

            // especificando o binding do exchange "order-placed-exchange" para a fila "shipping-order-queue"
            //e.Bind("order-placed-exchange", x =>
            //{
            //    x.ExchangeType = ExchangeType.Headers; ;

            //    // * RULES
            //    x.SetBindingArgument("departament", "shipping");
            //    x.SetBindingArgument("priority", "high");
            //    // "all" significa que todas as condições devem ser atendidas para que a mensagem seja roteada para esta fila
            //    x.SetBindingArgument("x-match", "all"); 
            //});

            #endregion

            #region [ USING DIRECT - SKIPPED QUEUES - RETRY ]

            ////especificando o binding do exchange "order-placed-exchange" para a fila "shipping-order-queue"
            //e.Bind("order-placed-exchange", x =>
            //{
            //    x.RoutingKey = "order.created";
            //    x.ExchangeType = ExchangeType.Direct;
            //});

            //// retry policy, em caso de erro irá tentar 3 vezes por um intervalo de 5 segundos
            ////e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5))); // Retry 3 times with 5 seconds interval

            //// Exponential -> em caso de erro irá tentar 3x por intervalo de 5s.
            //// Caso continue irá aguardar por 30s e tentar novamente por 10s de intervalo 
            //e.UseMessageRetry(r => r.Exponential(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(10))); 

            #endregion

            #region [ USING DIRECT - KILL SWITCH/CIRCUIT BREAKER ]

            //especificando o binding do exchange "order-placed-exchange" para a fila "shipping-order-queue"
            e.Bind("order-placed-exchange", x =>
            {
                x.RoutingKey = "order.created";
                x.ExchangeType = ExchangeType.Direct;
            });

            // Kill Switch / Circuit Breaker
            // após 10 falhas consecutivas, e a porcentagem de erros forem 15% o serviço ShippingService irá parar de consumir mensagens por 1 minuto,
            // e após esse tempo irá tentar reiniciar o consumo das mensagens
            e.UseKillSwitch(opt => opt
                .SetActivationThreshold(10)
                .SetTripThreshold(0.15)
                .SetRestartTimeout(m: 1)
            );

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