namespace RtFlow.Core.Interfaces;

public interface ISink<T>
{
    Task WriteAsync(IAsyncEnumerable<T> input, CancellationToken cancellationToken);
}
