using System.Text;
using Mango.Services.RewardAPI.Message;
using Mango.Services.RewardAPI.Services;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Mango.Services.RewardAPI.Messaging
{
    public class RabbitMQOrderConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly RewardService _rewardService;
        private readonly string _hostname = "localhost";
        private readonly string _username = "guest";
        private readonly string _password = "guest";
        private readonly string _exchangeName;
        private readonly string _queueName;
        private IConnection? _connection;
        private IModel? _channel;

        public RabbitMQOrderConsumer(IConfiguration configuration, RewardService rewardService)
        {
            _configuration = configuration;
            _rewardService = rewardService;
            _exchangeName = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic")!;

            var factory = new ConnectionFactory
            {
                HostName = _hostname,
                UserName = _username,
                Password = _password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare fanout exchange
            _channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Fanout, durable: false);

            // Declare dynamic queue and bind to exchange
            _queueName = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(queue: _queueName, exchange: _exchangeName, routingKey: string.Empty);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    RewardsMessage rewardsMessage = JsonConvert.DeserializeObject<RewardsMessage>(message)!;
                    _rewardService.UpdateRewards(rewardsMessage).GetAwaiter().GetResult();
                    _channel!.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing RabbitMQ order message in RewardAPI: {ex.Message}");
                }
            };

            _channel!.BasicConsume(
                queue: _queueName,
                autoAck: false,
                consumer: consumer);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}

