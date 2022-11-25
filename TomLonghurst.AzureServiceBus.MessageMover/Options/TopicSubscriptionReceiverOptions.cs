using Azure.Messaging.ServiceBus;

namespace TomLonghurst.AzureServiceBus.MessageMover.Options;

public record TopicSubscriptionReceiverOptions : IReceiverOptions
{
    public required ServiceBusClient Client { get; init; }
    public required string TopicName { get; init; }
    public required string SubscriptionName { get; init; }
    public required SubQueue SubQueue { get; init; }
    public int? Concurrency { get; init; }
}