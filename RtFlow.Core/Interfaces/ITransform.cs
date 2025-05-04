namespace RtFlow.Core.Interfaces;

public interface ITransform<TIn, TOut>
{
    IAsyncEnumerable<TOut> ProcessAsync(
        IAsyncEnumerable<TIn> input,
        CancellationToken cancellationToken);
}
