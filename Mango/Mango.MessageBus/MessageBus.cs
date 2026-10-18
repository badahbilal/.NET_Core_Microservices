using System.Text;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Mango.MessageBus;

public class MessageBus : IMessageBus
{
    private readonly string _connectionString;

    public MessageBus(IConfiguration configuration)
    {
        _connectionString = configuration.GetValue<string>("TopicAndQueueNames:ServiceBusConnectionString")
            ?? configuration.GetValue<string>("ServiceBusConnectionString")
            ?? configuration.GetConnectionString("ServiceBusConnectionString")
            ?? string.Empty;
    }

    public MessageBus(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task PublishMessage(object message, string topic_queue_Name)
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new InvalidOperationException("Azure Service Bus connection string is not configured. Please set 'ServiceBusConnectionString' or 'TopicAndQueueNames:ServiceBusConnectionString' in your settings.");
        }

        await using var client = new ServiceBusClient(_connectionString);

        ServiceBusSender sender = client.CreateSender(topic_queue_Name);

        var jsonMessage = JsonConvert.SerializeObject(message);

        ServiceBusMessage finalMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(jsonMessage))
        {
            CorrelationId = Guid.NewGuid().ToString()
        };

        await sender.SendMessageAsync(finalMessage);
        await client.DisposeAsync();
    }
}
