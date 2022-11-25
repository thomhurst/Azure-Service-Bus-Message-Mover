using Azure.Messaging.ServiceBus;
using TomLonghurst.AzureServiceBus.MessageMover.Options;

namespace TomLonghurst.AzureServiceBus.MessageMover.Services;

public class ServiceBusReceiverCreator : IServiceBusReceiverCreator
{
    private readonly MessageMoverOptions _messageMoverOptions;

    public ServiceBusReceiverCreator(MessageMoverOptions messageMoverOptions)
    {
        _messageMoverOptions = messageMoverOptions;
    }
    
    public ServiceBusProcessor CreateReceiverProcessor()
    {
        ArgumentNullException.ThrowIfNull(_messageMoverOptions.ReceiverOptions);
        ArgumentNullException.ThrowIfNull(_messageMoverOptions.ReceiverOptions.Client);

        var innerOptions = new ServiceBusProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.PeekLock, 
            SubQueue = _messageMoverOptions.ReceiverOptions.SubQueue,
            AutoCompleteMessages = false,
            MaxConcurrentCalls = _messageMoverOptions.ReceiverOptions.Concurrency ?? 1
        };

        return _messageMoverOptions.ReceiverOptions switch
        {
            TopicSubscriptionReceiverOptions topicSubscriptionOptions => _messageMoverOptions.ReceiverOptions.Client.CreateProcessor(
                topicSubscriptionOptions.TopicName, topicSubscriptionOptions.SubscriptionName, innerOptions
            ),

            QueueReceiverOptions queueOptions => _messageMoverOptions.ReceiverOptions.Client.CreateProcessor(queueOptions.QueueName, innerOptions),

            _ => throw new ArgumentException(
                "Please supply TopicSubscriptionOptions or QueueOptions",
                nameof(_messageMoverOptions.ReceiverOptions)
            )
        };
    }
}