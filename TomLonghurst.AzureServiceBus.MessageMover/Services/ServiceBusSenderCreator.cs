using Azure.Messaging.ServiceBus;
using TomLonghurst.AzureServiceBus.MessageMover.Options;

namespace TomLonghurst.AzureServiceBus.MessageMover.Services;

public class ServiceBusSenderCreator : IServiceBusSenderCreator
{
    private readonly MessageMoverOptions _messageMoverOptions;

    public ServiceBusSenderCreator(MessageMoverOptions messageMoverOptions)
    {
        _messageMoverOptions = messageMoverOptions;
    }
    
    public ServiceBusSender CreateSender()
    {
        ArgumentNullException.ThrowIfNull(_messageMoverOptions.SenderOptions);
        ArgumentNullException.ThrowIfNull(_messageMoverOptions.SenderOptions.Client);

        return _messageMoverOptions.SenderOptions.Client.CreateSender(_messageMoverOptions.SenderOptions.TopicOrQueueName);
    }
}