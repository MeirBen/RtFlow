using System.Runtime.CompilerServices;
using RtFlow.Core;
using RtFlow.Core.Interfaces;
using RtFlow.Core.Models;
using RtFlow.Sinks;
using RtFlow.Sources;

namespace RtFlow.Tests
{
    public class DataflowPipelineBuilderTests
    {
        // 1) Drops empty payloads:
        private class RawFilter : ITransform<RawEvent, RawEvent>
        {
            public async IAsyncEnumerable<RawEvent> ProcessAsync(
                IAsyncEnumerable<RawEvent> input,
                [EnumeratorCancellation] CancellationToken ct)
            {
                await foreach (var raw in input.WithCancellation(ct))
                    if (!string.IsNullOrWhiteSpace(raw.Payload))
                        yield return raw;
            }
        }

        // 2) Uppercases payload:
        private class RawToEnriched : ITransform<RawEvent, EnrichedEvent>
        {
            public async IAsyncEnumerable<EnrichedEvent> ProcessAsync(
                IAsyncEnumerable<RawEvent> input,
                [EnumeratorCancellation] CancellationToken ct)
            {
                await foreach (var raw in input.WithCancellation(ct))
                    yield return new EnrichedEvent("ENR", raw.Payload.ToUpperInvariant(), raw.Timestamp);
            }
        }

        // 3) No-op printer:
        private class PrintEnriched : ITransform<EnrichedEvent, EnrichedEvent>
        {
            public async IAsyncEnumerable<EnrichedEvent> ProcessAsync(
                IAsyncEnumerable<EnrichedEvent> input,
                [EnumeratorCancellation] CancellationToken ct)
            {
                await foreach (var e in input.WithCancellation(ct))
                {
                    // simulate a bit of work
                    // Console.WriteLine($"Processed: {e.Value}");
                    yield return e;
                }
            }
        }

        [Fact]
        public async Task RunAsync_Handles_Thousands_Of_Events()
        {
            const int total = 5000000;
            var now = DateTime.UtcNow;

            // 1. Generate a total of 5 million events
            var rawEvents = Enumerable
                .Range(1, total)
                .Select(i => new RawEvent(i.ToString(), now))
                .ToList();

            var source = new InMemorySource(rawEvents);

            // 2. Instantiate your transforms and sink
            var filter = new RawFilter();      // ITransform<RawEvent, RawEvent>
            var enricher = new RawToEnriched();  // ITransform<RawEvent, EnrichedEvent>
            var printer = new PrintEnriched();  // ITransform<EnrichedEvent, EnrichedEvent>
            var testSink = new TestSink<EnrichedEvent>();

            // 4. Wire them with the generic builder
            var builder = new DataflowPipelineBuilder<RawEvent>(boundedCapacity: 1000)
                .AddTransform(filter)        // yields RawEvent
                .AddTransform(enricher)      // yields EnrichedEvent
                .AddTransform(printer)       // yields EnrichedEvent
                .AddSink(testSink);        // final sink

            // 5. Run the pipeline over the source
            await builder.RunAsync(
                source.ReadEventsAsync(CancellationToken.None),
                CancellationToken.None);

            // 6. Assert we saw all the events
            Assert.Equal(total, testSink.handled);
        }
    }
}
