using Mango.Services.EmailAPI.Messaging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Mango.Services.EmailAPI.Extensions;

public static class ApplicationBuilderExtensions
{
    private static IAzureServiceBusConsumer? ServiceBusConsumer { get; set; }

    public static IApplicationBuilder UseAzureServiceBusConsumer(this IApplicationBuilder app)
    {
        ServiceBusConsumer = app.ApplicationServices.GetService<IAzureServiceBusConsumer>();
        var hostLifetime = app.ApplicationServices.GetService<IHostApplicationLifetime>();

        hostLifetime?.ApplicationStarted.Register(OnStart);
        hostLifetime?.ApplicationStopping.Register(OnStop);

        return app;
    }

    private static void OnStart()
    {
        ServiceBusConsumer?.Start();
    }

    private static void OnStop()
    {
        ServiceBusConsumer?.Stop();
    }
}

