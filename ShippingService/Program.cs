using MassTransit;
using ShippingService.Consumers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        // especificando para o MassTransit a usar o RabbitMQ como o broker de mensagens
        cfg.Host("rabbitmq://localhost");

        // especificando o nome da fila que o serviço ShippingService irá consumir
        cfg.ReceiveEndpoint("order-placed", e =>
        {
            // especificando o consumidor que irá processar as mensagens recebidas
            e.Consumer<OrderPlacedConsumer>();
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