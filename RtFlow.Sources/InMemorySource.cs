using System.Runtime.CompilerServices;
using RtFlow.Core.Interfaces;
using RtFlow.Core.Models;

namespace RtFlow.Sources
{
    public class InMemorySource : ISource<RawEvent>
    {
        private readonly IEnumerable<RawEvent> _events;
        public InMemorySource(IEnumerable<RawEvent> events) => _events = events;

        public async IAsyncEnumerable<RawEvent> ReadEventsAsync(
            [EnumeratorCancellation] CancellationToken ct)
        {
            foreach (var e in _events)
            {
                ct.ThrowIfCancellationRequested();
                yield return e;
                await Task.Yield();
            }
        }
    }
}