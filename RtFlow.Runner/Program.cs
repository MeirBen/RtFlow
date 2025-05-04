using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks.Dataflow;
using RtFlow.Core;
using RtFlow.Sources;
using RtFlow.Core.Interfaces;
using RtFlow.Core.Models;
using RtFlow.Runner;
using RtFlow.Transforms.Filters;
using RtFlow.Sinks;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        // Source, Transform, Sink registrations
        services.AddSingleton<ISource<RawEvent>>(sp => new TcpSource("localhost", 9000));
        services.AddSingleton<ITransform<RawEvent, EnrichedEvent>, SimpleFilter<RawEvent, EnrichedEvent>>();
        services.AddSingleton<ISink<EnrichedEvent>, ConsoleSink<EnrichedEvent>>();

        // Dataflow builder setup
        services.AddSingleton(sp =>
        {
            var builder = new DataflowPipelineBuilder<RawEvent>(boundedCapacity: 500);
            builder
              .AddTransform(new TransformBlock<RawEvent, EnrichedEvent>(
                  raw => Task.FromResult(new EnrichedEvent("key", raw.Payload, raw.Timestamp)),
                  new ExecutionDataflowBlockOptions { BoundedCapacity = 500 }))
              .AddAction(new ActionBlock<EnrichedEvent>(
                  evt => Task.Run(() => Console.WriteLine(evt.Value)),
                  new ExecutionDataflowBlockOptions { BoundedCapacity = 500 }));
            return builder;
        });

        // Hosted service to run builder
        services.AddHostedService<PipelineHostedService>();
    })
    .Build();

await host.RunAsync();