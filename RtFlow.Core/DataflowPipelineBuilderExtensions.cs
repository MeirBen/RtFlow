using System.Threading.Tasks.Dataflow;
using RtFlow.Core.Interfaces;

namespace RtFlow.Core
{
    public static class DataflowPipelineBuilderExtensions
    {
        /// <summary>
        /// Adds an ITransform<TIn,TOut> as a TransformManyBlock,
        /// so filters (0 outputs) or mappers (1+ outputs) both work.
        /// </summary>
        public static DataflowPipelineBuilder<TIn> AddTransform<TIn, TCurr, TOut>(
            this DataflowPipelineBuilder<TIn> builder,
            ITransform<TCurr, TOut> transform,
            int boundedCapacity = 1000)
        {
            var block = new TransformManyBlock<TCurr, TOut>(
                async input =>
                {
                    var list = new List<TOut>();
                    // run your ITransform, which can yield 0..N outputs
                    await foreach (var e in transform
                        .ProcessAsync(new[] { input }.ToAsyncEnumerable(),
                                      CancellationToken.None))
                    {
                        list.Add(e);
                    }
                    return list;       // zero items => filter, one or more => map/expand
                },
                new ExecutionDataflowBlockOptions
                {
                    BoundedCapacity = boundedCapacity,
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                });

            // Link into the pipeline exactly the same way
            builder.AddTransform(block);
            return builder;
        }


        /// <summary>
        /// Adds an ISink<TOut> at the end of the pipeline,
        /// wrapping it in an ActionBlock<TOut>.
        /// </summary>
        public static DataflowPipelineBuilder<TIn> AddSink<TIn, TOut>(
            this DataflowPipelineBuilder<TIn> builder,
            ISink<TOut> sink,
            int boundedCapacity = 1000)
        {
            var block = new ActionBlock<TOut>(
                async item =>
                {
                    await sink.WriteAsync(
                        new[] { item }.ToAsyncEnumerable(),
                        CancellationToken.None);
                },
                new ExecutionDataflowBlockOptions { BoundedCapacity = boundedCapacity, MaxDegreeOfParallelism = 1 }
            );

            builder.AddAction(block);
            return builder;
        }
    }
}
