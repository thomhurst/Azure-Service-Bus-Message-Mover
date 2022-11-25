# Azure Service Bus - MessageMover
Replay or Move Messages to different buses

## Install via Nuget
`TomLonghurst.AzureServiceBus.MessageMover`

## How to use

### Register Dependencies via Extension Method
Call  `.AddAzureServiceBusMessageMover` on your ServiceCollection in Startup, passing in some configuration.

```csharp
.AddAzureServiceBusMessageMover(sp => new MessageMoverOptions
  {
      ReceiverOptions = new TopicSubscriptionReceiverOptions // or new QueueReceiverOptions
      {
          Client = client,
          Concurrency = 1000,
          SubQueue = SubQueue.DeadLetter,
          TopicName = topicName,
          SubscriptionName = topicSubscriptionName
      },
      SenderOptions = new SenderOptions
      {
          Client = client,
          TopicOrQueueName = replayQueueName
      },
      MessagesToProcess = 10
  })
```

### Inject in `MessageMoverWorker`

```csharp
public class MyClass
{
  private readonly MessageMoverWorker _messageMoverWorker;
  
  public MyClass(MessageMoverWorker messageMoverWorker)
  {
    _messageMoverWorker = messageMoverWorker;
  }
}
```

### Call `MoveMessages`

```csharp
public async MyMethod()
{
  _messageMoverWorker.MoveMessages();
}
```

The replayer can:
- Honour the count, if one was provided in the configuration, otherwise it'll attempt to process all of the messages in the Queue/Subscription
- Set a concurrency count
- Replay messages as they are, or map the data into a new message with a new format
- Move messages on the same service bus, or between different service buses
- Work out when the queue is empty and stop
- Retry messages 10 times, and then give up on individual messages if they aren't succeeding
