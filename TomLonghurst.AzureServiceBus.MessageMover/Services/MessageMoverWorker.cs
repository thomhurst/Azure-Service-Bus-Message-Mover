﻿using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using TomLonghurst.AzureServiceBus.MessageMover.Extensions;
using TomLonghurst.AzureServiceBus.MessageMover.Options;

namespace TomLonghurst.AzureServiceBus.MessageMover.Services;

public class MessageMoverWorker
{
    private readonly MessageMoverOptions _managerOptions;
    private readonly IServiceBusReceiverCreator _serviceBusReceiverCreator;
    private readonly IServiceBusSenderCreator _serviceBusSenderCreator;
    private readonly IMessageMapper _messageMapper;
    private readonly ILogger<MessageMoverWorker> _logger;
    private readonly CancellationTokenSource _backgroundWorkersCancellationTokenSource = new();
    private DateTime _lastTick;

    public MessageMoverWorker(MessageMoverOptions managerOptions,
        IServiceBusReceiverCreator serviceBusReceiverCreator,
        IServiceBusSenderCreator serviceBusSenderCreator,
        IMessageMapper messageMapper,
        ILogger<MessageMoverWorker> logger)
    {
        _managerOptions = managerOptions;
        _serviceBusReceiverCreator = serviceBusReceiverCreator;
        _serviceBusSenderCreator = serviceBusSenderCreator;
        _messageMapper = messageMapper;
        _logger = logger;
    }

    public async Task DoWork(CancellationToken cancellationToken = default)
    {
        var receiver = _serviceBusReceiverCreator.CreateReceiverProcessor();

        var messageProcessorCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var sender = _serviceBusSenderCreator.CreateSender();
        
        _lastTick = DateTime.UtcNow;

        receiver.ProcessMessageAsync += Process(sender);

        PollForMessageCompletion(messageProcessorCancellationTokenSource);

        await receiver.StartProcessingAsync(cancellationToken);

        await messageProcessorCancellationTokenSource.Token.WaitForCancellation();

        await receiver.DisposeAsync();

        _backgroundWorkersCancellationTokenSource.Cancel();
        _backgroundWorkersCancellationTokenSource.Dispose();
        
        await sender.DisposeAsync();
    }

    private Func<ProcessMessageEventArgs, Task> Process(ServiceBusSender sender)
    {
        var messagesProcessedCount = 0;
        
        return async args =>
        {
            var messageCount = Interlocked.Increment(ref messagesProcessedCount);
            
            if (_managerOptions.MessagesToProcess is > 0
                && messageCount > _managerOptions.MessagesToProcess.Value)
            {
                _logger.LogInformation("Skipping message as user defined limit has been reached. Limit: {Limit} | Current Message Count: {CurrentMessageCount}", _managerOptions.MessagesToProcess, messageCount);
                await args.AbandonMessageAsync(args.Message, cancellationToken: args.CancellationToken);
                return;
            }

            if (args.Message.ApplicationProperties.TryGetValue(Constants.Headers.SendCount, out var sendCountObj)
                && int.TryParse(sendCountObj?.ToString(), out var sendCount)
                && sendCount >= 10)
            {
                _logger.LogInformation("Skipping as message didn't succeed after 10 attempts");
                await args.AbandonMessageAsync(args.Message, cancellationToken: args.CancellationToken);
                return;
            }

            _lastTick = DateTime.UtcNow;
            var newMessage = _messageMapper.MapMessage(args.Message);
            
            _logger.LogDebug("Processing Message {MessageId}", args.Message.MessageId);

            try
            {
                await sender.SendMessageAsync(newMessage, args.CancellationToken);
                await args.CompleteMessageAsync(args.Message, cancellationToken: args.CancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception during sending of message {MessageId}", args.Message.MessageId);
                
                await args.AbandonMessageAsync(args.Message, new Dictionary<string, object>
                {
                    [Constants.Headers.SendCount] = newMessage.ApplicationProperties[Constants.Headers.SendCount],
                    [Constants.Headers.Exception] = $"{e.Message} {e.StackTrace}"
                }, cancellationToken: args.CancellationToken);
            }
        };
    }

    private void PollForMessageCompletion(CancellationTokenSource messageProcessorCancellationTokenSource)
    {
        var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        
        Task.Run(async () =>
        {
            while (await periodicTimer.WaitForNextTickAsync(_backgroundWorkersCancellationTokenSource.Token))
            {
                _logger.LogDebug("Checking if messages have stopped processing");
                if (DateTime.UtcNow - _lastTick > TimeSpan.FromSeconds(30))
                {
                    _logger.LogInformation("No more messages found... Stopping processor");
                    messageProcessorCancellationTokenSource.Cancel();
                    periodicTimer.Dispose();
                    return;
                }
            }
        }, _backgroundWorkersCancellationTokenSource.Token);
    }
}