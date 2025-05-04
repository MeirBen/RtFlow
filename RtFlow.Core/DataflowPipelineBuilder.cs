using System.Threading.Tasks.Dataflow;

namespace RtFlow.Core
{
    public class DataflowPipelineBuilder<TInput>
    {
        private readonly BufferBlock<TInput> _entry;
        private IDataflowBlock _lastBlock;
        private readonly List<Task> _completionTasks = new();

        public DataflowPipelineBuilder(int boundedCapacity = 1000)
        {
            _entry = new BufferBlock<TInput>(
                new DataflowBlockOptions { BoundedCapacity = boundedCapacity });
            _lastBlock = _entry;
        }

        public DataflowPipelineBuilder<TInput> AddTransform<TCurr, TNext>(
            IPropagatorBlock<TCurr, TNext> block)
        {
            ((ISourceBlock<TCurr>)_lastBlock)
                .LinkTo(block, new DataflowLinkOptions { PropagateCompletion = true });
            _lastBlock = block;
            return this;
        }

        public DataflowPipelineBuilder<TInput> AddBatch<TCurr>(
            BatchBlock<TCurr> block)
        {
            ((ISourceBlock<TCurr>)_lastBlock)
                .LinkTo(block, new DataflowLinkOptions { PropagateCompletion = true });
            _lastBlock = block;
            return this;
        }

        public DataflowPipelineBuilder<TInput> AddAction<TCurr>(
            ActionBlock<TCurr> block)
        {
            ((ISourceBlock<TCurr>)_lastBlock)
                .LinkTo(block, new DataflowLinkOptions { PropagateCompletion = true });
            _completionTasks.Add(block.Completion);
            return this;
        }

        public async Task RunAsync(
            IAsyncEnumerable<TInput> source,
            CancellationToken ct)
        {
            await foreach (var item in source.WithCancellation(ct))
                await _entry.SendAsync(item, ct);
            _entry.Complete();
            await Task.WhenAll(_completionTasks.Concat(new[] { _entry.Completion }));
        }
    }
}