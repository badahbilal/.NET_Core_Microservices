using System.Text;
using Mango.Services.EmailAPI.Services;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Mango.Services.EmailAPI.Messaging
{
    public class RabbitMQAuthConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private readonly string _hostname = "localhost";
        private readonly string _username = "guest";
        private readonly string _password = "guest";
        private readonly string _queueName;
        private IConnection? _connection;
        private IModel? _channel;

        public RabbitMQAuthConsumer(IConfiguration configuration, EmailService emailService)
        {
            _configuration = configuration;
            _emailService = emailService;
            _queueName = _configuration.GetValue<string>("TopicAndQueueNames:RegisterUserQueue")!;

            var factory = new ConnectionFactory
            {
                HostName = _hostname,
                UserName = _username,
                Password = _password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(
                queue: _queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (ch, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    await _emailService.RegisterUserEmailAndLog(message);
                    _channel!.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception)
                {
                    // Optional: Add logging or dead-lettering
                    throw;
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

