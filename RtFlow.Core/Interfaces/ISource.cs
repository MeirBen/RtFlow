namespace RtFlow.Core.Interfaces;

public interface ISource<T>
{
    IAsyncEnumerable<T> ReadEventsAsync(CancellationToken cancellationToken);
}