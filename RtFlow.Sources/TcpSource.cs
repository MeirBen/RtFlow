using System.Net.Sockets;
using System.Text;
using System.Runtime.CompilerServices;
using RtFlow.Core.Interfaces;
using RtFlow.Core.Models;

namespace RtFlow.Sources;

public class TcpSource : ISource<RawEvent>
{
    private readonly string _host;
    private readonly int _port;
    private readonly int _bufferSize;

    public TcpSource(string host, int port, int bufferSize = 4096)
    {
        _host = host;
        _port = port;
        _bufferSize = bufferSize;
    }

    public async IAsyncEnumerable<RawEvent> ReadEventsAsync(
        [EnumeratorCancellation] CancellationToken ct)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(_host, _port);
        using var stream = client.GetStream();
        var buffer = new byte[_bufferSize];

        while (!ct.IsCancellationRequested)
        {
            var count = await stream.ReadAsync(buffer.AsMemory(0, _bufferSize), ct);
            if (count == 0) break;
            var payload = Encoding.UTF8.GetString(buffer, 0, count);
            yield return new RawEvent(payload, DateTime.UtcNow);
        }
    }
}