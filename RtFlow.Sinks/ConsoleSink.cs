using RtFlow.Core.Interfaces;

namespace RtFlow.Sinks;

public class ConsoleSink<T> : ISink<T>
{
    public async Task WriteAsync(IAsyncEnumerable<T> input, CancellationToken ct)
    {
        await foreach (var item in input.WithCancellation(ct))
            Console.WriteLine(item);
    }
}