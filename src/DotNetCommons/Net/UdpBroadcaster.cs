using System.Net;
using System.Net.Sockets;

namespace DotNetCommons.Net;

public class MessageReceivedArgs(byte[] message, IPEndPoint remoteEndPoint) : EventArgs
{
    public byte[] Message { get; } = message;
    public IPEndPoint RemoteEndPoint { get; } = remoteEndPoint;
}

/// A class that sends and listens for UDP broadcast messages over a given port.
public class UdpBroadcaster : IDisposable
{
    private readonly IPEndPoint _broadcastEndpoint;
    private readonly SemaphoreSlim _lock = new(1);
    private readonly UdpClient _udp;

    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _listeningTask;

    /// <summary>
    /// Event handler for received messages.
    /// </summary>
    /// <remarks>
    /// If your event handler throws an unhandled exception, it may break the listening process and
    /// prevent further messages from being received.
    /// </remarks>
    public event EventHandler<MessageReceivedArgs>? MessageReceived;

    /// Create a new UdpBroadcaster class that listens for and sends messages over a specified port. Uses the global
    /// broadcast address.
    public UdpBroadcaster(int port) : this(port, IPAddress.Broadcast)
    {
    }

    /// Create a new UdpBroadcaster class that listens for and sends messages over a specified port. Uses a specific
    /// broadcast address given. This should be an address like 172.16.255.255 or similar.
    public UdpBroadcaster(int port, IPAddress address)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(port, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(port, 65535);

        _broadcastEndpoint = new IPEndPoint(address, port);

        _udp = new UdpClient(port)
        {
            EnableBroadcast = true
        };
    }

    private async Task Listen(CancellationToken cancellation)
    {
        while (!cancellation.IsCancellationRequested)
        {
            try
            {
                var result  = await _udp.ReceiveAsync(cancellation);
                MessageReceived?.Invoke(this, new MessageReceivedArgs(result.Buffer, result.RemoteEndPoint));
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Send a message to the broadcast address.
    /// </summary>
    /// <remarks>
    /// If you're broadcasting on the subnet, you will most likely also receive your own message back.
    /// </remarks>
    public ValueTask<int> Send(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        return _udp.SendAsync(data, _broadcastEndpoint, ct);
    }

    /// Starts listening for incoming UDP broadcast messages on the specified port.
    /// When a message is received, the MessageReceived event is triggered.
    public async Task StartListening()
    {
        await _lock.WaitAsync();
        try
        {
            if (_listeningTask != null)
                return;

            _cancellationTokenSource = new CancellationTokenSource();
            _listeningTask           = Task.Run(() => Listen(_cancellationTokenSource.Token));
        }
        finally
        {
            _lock.Release();
        }
    }

    /// Stops listening for incoming UDP broadcast messages.
    /// Cancels any ongoing listening task and releases associated resources.
    public async Task StopListening()
    {
        await _lock.WaitAsync();
        try
        {
            if (_listeningTask == null)
                return;

            await _cancellationTokenSource!.CancelAsync();
            await _listeningTask;

            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
            _listeningTask           = null;
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Dispose()
    {
        StopListening().Wait();
        _udp.Dispose();
        _lock.Dispose();
    }
}