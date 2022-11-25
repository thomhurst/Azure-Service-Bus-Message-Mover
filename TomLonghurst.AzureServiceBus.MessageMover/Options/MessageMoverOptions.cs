using Azure.Messaging.ServiceBus;

namespace TomLonghurst.AzureServiceBus.MessageMover.Options;

public class MessageMoverOptions
{
    public required IReceiverOptions ReceiverOptions { get; init; }
    public required SenderOptions SenderOptions { get; init; }
    public Func<ServiceBusReceivedMessage, ServiceBusMessage>? MessageMapper { get; init; }
    public int? MessagesToProcess { get; init; }
}