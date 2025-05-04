using System.Runtime.CompilerServices;
using RtFlow.Core.Interfaces;
using RtFlow.Core.Models;

namespace RtFlow.Transforms.Filters;

public class SimpleFilter<TIn, TOut> : ITransform<TIn, TOut>
    where TIn : RawEvent
    where TOut : EnrichedEvent
{
    public async IAsyncEnumerable<TOut> ProcessAsync(
        IAsyncEnumerable<TIn> input,
        [EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var evt in input.WithCancellation(ct))
        {
            if (!string.IsNullOrWhiteSpace(evt.Payload))
            {
                var enrichedEvent = new EnrichedEvent("key", evt.Payload, evt.Timestamp);
                yield return (TOut)(object)enrichedEvent;
            }
        }
    }
}