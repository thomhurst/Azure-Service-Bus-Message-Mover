using Azure.Messaging.ServiceBus;

namespace TomLonghurst.AzureServiceBus.MessageMover.Options;

public record SenderOptions
{
    public required ServiceBusClient Client { get; init; }
    public required string TopicOrQueueName { get; init; }
}