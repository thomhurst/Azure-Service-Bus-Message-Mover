using Azure.Messaging.ServiceBus;

namespace TomLonghurst.AzureServiceBus.MessageMover.Options;

public record QueueReceiverOptions : IReceiverOptions
{
    public required ServiceBusClient Client { get; init; }
    public required string QueueName { get; init; }
    public required SubQueue SubQueue { get; init; }
}