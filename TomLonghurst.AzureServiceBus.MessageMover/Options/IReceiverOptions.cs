using Azure.Messaging.ServiceBus;

namespace TomLonghurst.AzureServiceBus.MessageMover.Options;

public interface IReceiverOptions
{
    public ServiceBusClient Client { get; }
    public SubQueue SubQueue { get; }
    int? Concurrency { get; }
}