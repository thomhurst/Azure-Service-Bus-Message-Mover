using Azure.Messaging.ServiceBus;

namespace TomLonghurst.AzureServiceBus.MessageMover.Services;

public interface IServiceBusReceiverCreator
{
    ServiceBusProcessor CreateReceiverProcessor();
}