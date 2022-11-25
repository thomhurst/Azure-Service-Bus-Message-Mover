using Microsoft.Extensions.DependencyInjection;
using TomLonghurst.AzureServiceBus.MessageMover.Options;
using TomLonghurst.AzureServiceBus.MessageMover.Services;

namespace TomLonghurst.AzureServiceBus.MessageMover.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddAzureServiceBusMessageMover(this IServiceCollection serviceCollection, Func<IServiceProvider, MessageMoverOptions> messageMoverOptions)
    {
        return serviceCollection.AddLogging()
            .AddTransient<IMessageMapper, MessageMapper>()
            .AddTransient<IServiceBusReceiverCreator, ServiceBusReceiverCreator>()
            .AddTransient<IServiceBusSenderCreator, ServiceBusSenderCreator>()
            .AddTransient<MessageMoverWorker>()
            .AddSingleton(messageMoverOptions);
    }
}