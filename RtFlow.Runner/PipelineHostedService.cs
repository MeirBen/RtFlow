using Microsoft.Extensions.Hosting;
using RtFlow.Core;
using RtFlow.Core.Interfaces;
using RtFlow.Core.Models;

namespace RtFlow.Runner;

public class PipelineHostedService : BackgroundService
{
    private readonly DataflowPipelineBuilder<RawEvent> _builder;
    private readonly ISource<RawEvent> _source;

    public PipelineHostedService(
        DataflowPipelineBuilder<RawEvent> builder,
        ISource<RawEvent> source)
    {
        _builder = builder;
        _source = source;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
        => _builder.RunAsync(_source.ReadEventsAsync(stoppingToken), stoppingToken);
}