using Azure.Messaging.ServiceBus;

namespace TomLonghurst.AzureServiceBus.MessageMover.Services;

public interface IServiceBusSenderCreator
{
    ServiceBusSender CreateSender();
}