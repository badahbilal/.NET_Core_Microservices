using System.Text;
using Azure.Messaging.ServiceBus;
using Mango.Services.EmailAPI.Models.Dto;
using Mango.Services.EmailAPI.Services;
using Mango.Services.EmailAPI.Services.IService;
using Newtonsoft.Json;

namespace Mango.Services.EmailAPI.Messaging;

public class AzureServiceBusConsumer : IAzureServiceBusConsumer
{
    private readonly string _serviceBusConnectionString;
    private readonly string _emailCartQueue;

    // Step 3 : Add a private readonly string _registerUserQueue to hold the queue name for user email registration
    private readonly string _registerUserQueue;
    private readonly IConfiguration _configuration;
    //117. Asynchronous Communication in Action
    private readonly IEmailService _emailService;
    private readonly ServiceBusProcessor _emailCartProcessor;

    // step 5 : Add a private readonly ServiceBusProcessor _registerUserProcessor to process messages from the user email registration queue
    private readonly ServiceBusProcessor _registerUserProcessor;

    public AzureServiceBusConsumer(IConfiguration configuration, IEmailService emailService)
    {
        _configuration = configuration;
        //117. Asynchronous Communication in Action
        _emailService = emailService;

        _serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString")
            ?? _configuration.GetValue<string>("TopicAndQueueNames:ServiceBusConnectionString")!;
        _emailCartQueue = _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue")!;

        // Step 4 : Initialize the _registerUserQueue field with the queue name for user email registration
        _registerUserQueue = _configuration.GetValue<string>("TopicAndQueueNames:RegisterUserQueue")!;

        var client = new ServiceBusClient(_serviceBusConnectionString);
        _emailCartProcessor = client.CreateProcessor(_emailCartQueue);

        // Step 6 : Initialize the _registerUserProcessor field with a new ServiceBusProcessor for the user email registration queue
        _registerUserProcessor = client.CreateProcessor(_registerUserQueue);
    }

    public async Task Start()
    {
        _emailCartProcessor.ProcessMessageAsync += OnEmailCartRequestReceived;
        _emailCartProcessor.ProcessErrorAsync += ErrorHandler;
        await _emailCartProcessor.StartProcessingAsync();

        // Step 7 : Subscribe to the ProcessMessageAsync and ProcessErrorAsync events of the _registerUserProcessor
        _registerUserProcessor.ProcessMessageAsync += OnRegisterUserRequestReceived;
        _registerUserProcessor.ProcessErrorAsync += ErrorHandler;
        await _registerUserProcessor.StartProcessingAsync();
    }

    public async Task Stop()
    {
        await _emailCartProcessor.StopProcessingAsync();
        await _emailCartProcessor.DisposeAsync();


        // Step 8 : Stop processing and dispose the _registerUserProcessor
        await _registerUserProcessor.StopProcessingAsync();
        await _registerUserProcessor.DisposeAsync();
    }

    private async Task OnEmailCartRequestReceived(ProcessMessageEventArgs args)
    {
        var message = args.Message;
        var body = Encoding.UTF8.GetString(message.Body.ToArray());

        CartDto objMessage = JsonConvert.DeserializeObject<CartDto>(body)!;
        try
        {
            await _emailService.EmailCartAndLog(objMessage);
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception)
        {
            throw;
        }
    }

    // Step 9 : Implement the OnRegisterUserRequestReceived method to handle user email registration requests
    private async Task OnRegisterUserRequestReceived(ProcessMessageEventArgs args)
    {
        // Deserialize the message body to get the email address
        var message = args.Message;
        var body = Encoding.UTF8.GetString(message.Body.ToArray());

        // Deserialize the message body to get the email address
        string email = JsonConvert.DeserializeObject<string>(body)!;
        try
        {
            // Call the RegisterUserEmailAndLog method of the _emailService to log the email registration request
            await _emailService.RegisterUserEmailAndLog(email);
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception)
        {
            throw;
        }
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        Console.WriteLine(args.Exception.ToString());
        return Task.CompletedTask;
    }
}
