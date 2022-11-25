namespace TomLonghurst.AzureServiceBus.MessageMover.Extensions;

internal static class CancellationTokenExtensions
{
    public static Task WaitForCancellation(this CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        var taskCompletionSource = new TaskCompletionSource();

        cancellationToken.Register(taskCompletionSource.SetResult);
        
        return taskCompletionSource.Task;
    }
}