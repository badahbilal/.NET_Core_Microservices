using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Mango.Services.OrderAPI.RabbitMQSender
{
    public class RabbitMQOrderMessageSender : IRabbitMQOrderMessageSender
    {
        private readonly string _hostname = "localhost";
        private readonly string _username = "guest";
        private readonly string _password = "guest";
        private IConnection? _connection;

            private const string OrderCreatedEmailUpdateQueue = "OrderCreated_Email_Update";
        private const string OrderCreatedRewardsUpdateQueue = "OrderCreated_Rewards_Update";
        private const string EmailRoutingKey = "emailUpdate";
        private const string RewardsRoutingKey = "rewardsUpdate";

        public void SendMessage(object message, string exchangeName)
        {
            if (ConnectionExists())
            {
                using var channel = _connection!.CreateModel();

                // 1. Declare Direct Exchange
                channel.ExchangeDeclare(
                    exchange: exchangeName,
                    type: ExchangeType.Direct,
                    durable: false);

                // 2. Declare Queues
                channel.QueueDeclare(
                    queue: OrderCreatedEmailUpdateQueue,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                channel.QueueDeclare(
                    queue: OrderCreatedRewardsUpdateQueue,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                // 3. Bind Queues to Exchange with specific routing keys
                channel.QueueBind(
                    queue: OrderCreatedEmailUpdateQueue,
                    exchange: exchangeName,
                    routingKey: EmailRoutingKey);

                channel.QueueBind(
                    queue: OrderCreatedRewardsUpdateQueue,
                    exchange: exchangeName,
                    routingKey: RewardsRoutingKey);

                // 4. Serialize payload
                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);

                // 5. Publish to specific routing keys
                channel.BasicPublish(
                    exchange: exchangeName,
                    routingKey: EmailRoutingKey,
                    basicProperties: null,
                    body: body);

                channel.BasicPublish(
                    exchange: exchangeName,
                    routingKey: RewardsRoutingKey,
                    basicProperties: null,
                    body: body);
            }
        }

        private void CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _hostname,
                    UserName = _username,
                    Password = _password
                };
                _connection = factory.CreateConnection();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RabbitMQ connection establishment failed: {ex.Message}");
            }
        }

        private bool ConnectionExists()
        {
            if (_connection != null)
            {
                return true;
            }

            CreateConnection();
            return _connection != null;
        }
    }
}
