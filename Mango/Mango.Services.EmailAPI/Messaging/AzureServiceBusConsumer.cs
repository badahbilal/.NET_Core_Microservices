using Azure.Messaging.ServiceBus;

namespace Mango.Services.EmailAPI.Messaging;

public class AzureServiceBusConsumer : IAzureServiceBusConsumer
{
    private readonly string _serviceBusConnectionString;
    private readonly string _emailCartQueue;
    private readonly IConfiguration _configuration;
    private readonly ServiceBusProcessor _emailCartProcessor;

    public AzureServiceBusConsumer(IConfiguration configuration)
    {
        _configuration = configuration;

        _serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString")
            ?? _configuration.GetValue<string>("TopicAndQueueNames:ServiceBusConnectionString")!;
        _emailCartQueue = _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue")!;

        var client = new ServiceBusClient(_serviceBusConnectionString);
        _emailCartProcessor = client.CreateProcessor(_emailCartQueue);
    }

    public async Task Start()
    {
        // Handlers will be attached and processor started here
        await Task.CompletedTask;
    }

    public async Task Stop()
    {
        // Processor stop and disposal will be handled here
        await Task.CompletedTask;
    }
}

