# Running RabbitMQ in Docker
open Docker Desktop and run the following command in the terminal:

docker pull rabbitmq:3-management

docker run -d --hostname rabbitmq-host --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management