using Azure.Messaging.ServiceBus;

namespace TomLonghurst.AzureServiceBus.MessageMover.Services;

public interface IMessageMapper
{
    ServiceBusMessage MapMessage(ServiceBusReceivedMessage receivedMessage);
}