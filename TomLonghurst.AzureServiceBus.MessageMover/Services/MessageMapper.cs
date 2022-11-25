using Azure.Messaging.ServiceBus;
using TomLonghurst.AzureServiceBus.MessageMover.Options;

namespace TomLonghurst.AzureServiceBus.MessageMover.Services;

public class MessageMapper : IMessageMapper
{
    private readonly MessageMoverOptions _messageMoverOptions;

    public MessageMapper(MessageMoverOptions messageMoverOptions)
    {
        _messageMoverOptions = messageMoverOptions;
    }
    
    public ServiceBusMessage MapMessage(ServiceBusReceivedMessage receivedMessage)
    {
        if (_messageMoverOptions.MessageMapper == null)
        {
            return CloneMessage(receivedMessage);
        }

        return MapMessageFromFunc(receivedMessage);
    }

    private ServiceBusMessage MapMessageFromFunc(ServiceBusReceivedMessage receivedMessage)
    {
        var message = _messageMoverOptions.MessageMapper!.Invoke(receivedMessage);
        
        SetSendCount(receivedMessage, message);
        
        return message;
    }

    private ServiceBusMessage CloneMessage(ServiceBusReceivedMessage receivedMessage)
    {
        var serviceBusMessage = new ServiceBusMessage(receivedMessage);
        
        SetSendCount(receivedMessage, serviceBusMessage);
        
        return serviceBusMessage;
    }

    private static void SetSendCount(ServiceBusReceivedMessage receivedMessage, ServiceBusMessage message)
    {
        var sendCount = 1;
        
        if (receivedMessage.ApplicationProperties.TryGetValue(Constants.Headers.SendCount, out var sendCountObj)
            && int.TryParse(sendCountObj?.ToString(), out var count))
        {
            sendCount = count + 1;
        }

        message.ApplicationProperties[Constants.Headers.SendCount] = sendCount;
    }
}