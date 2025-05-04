using RtFlow.Core.Interfaces;

namespace RtFlow.Sinks
{
    public class TestSink<T> : ISink<T>
    {
        public int handled;

        public async Task WriteAsync(
            IAsyncEnumerable<T> input,
            CancellationToken ct)
        {
            await foreach (var item in input.WithCancellation(ct))
            {
                handled++;
            }
        }
    }
}