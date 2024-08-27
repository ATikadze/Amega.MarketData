using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Amega.MarketData.Core.DTOs.Response;
using Amega.MarketData.Core.Helpers;
using Amega.MarketData.Core.Models;
using Amega.MarketData.Core.Models.CustomExceptions;
using Amega.MarketData.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Amega.MarketData.Core.Services.Implementations;

// Unit tests can be added and Moq can be used for more in-depth testing
// When an error occurs the error message can be sent to the corresponding socket and then displayed on the page
// Binance states that a single socket connection last 24 hours. So it would also be good to refresh the connection every 24 hours, and also reopen the connection in case it suddenly closes
public class WebSocketService : IWebSocketService
{
    private ILogger<WebSocketService> _logger;
    private readonly Dictionary<string, List<WebSocket>> _subscriptions; // Subscribed clients
    private readonly Dictionary<string, ClientWebSocket> _connections; // Connections to the Binance
    private readonly Dictionary<string, SemaphoreSlim> _subscribeLocks;

    public WebSocketService(ILogger<WebSocketService> logger)
    {
        _logger = logger;
        _subscriptions = new Dictionary<string, List<WebSocket>>();
        _connections = new Dictionary<string, ClientWebSocket>();
        _subscribeLocks = new Dictionary<string, SemaphoreSlim>();
    }

    public async Task HandleConnectionAsync(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        var symbol = Encoding.UTF8.GetString(buffer, 0, result.Count).ToLower();

        InstrumentsHelpers.EnsureExists(symbol);
        
        if (!_subscriptions.ContainsKey(symbol))
            _subscriptions[symbol] = new List<WebSocket>();

        _subscriptions[symbol].Add(webSocket);

        if (!_connections.ContainsKey(symbol))
            await SubscribeAsync(symbol);

        await KeepConnectionAliveAsync(webSocket);
    }

    private async Task SubscribeAsync(string symbol)
    {
        if (!_subscribeLocks.ContainsKey(symbol))
            _subscribeLocks[symbol] = new SemaphoreSlim(1, 1);

        await _subscribeLocks[symbol].WaitAsync();

        if (_connections.ContainsKey(symbol))
            return;

        // Configs like these (urls for example) should be kept in app configs
        var streamName = $"{symbol.ToLower()}@ticker";
        var uri = new Uri($"wss://stream.binance.com:443/ws/{streamName}");
        var webSocket = new ClientWebSocket();
        await webSocket.ConnectAsync(uri, CancellationToken.None);

        _logger.LogInformation("Connected to Binance");

        /* Another major improvement in performance would be to send subscriptions as batches.
           So instead of subscribing to the Binance stream individually, as it's being done right now, we can subscribe multiple symbols to the stream.
           For example, right now I have three different subscriptions for three different symbols. Which means that I am going to receive three times as many messages.
           Instead, we should subscribe multiple symbols to the ticker stream. And instead of receiving messages for each symbol, we are going to receive all the information about the symbols in one message.
           This will significantly improve the performance and instead of having a job running per symbol, we're going to have one job in total, which will be receiving information for all symbols. */
        await SendAsync(webSocket, $"{{ \"method\": \"SUBSCRIBE\", \"params\": [ \"{streamName}\" ], \"id\": 1 }}");

        _logger.LogInformation($"Subscribed to {streamName} stream");

        _connections.Add(symbol, webSocket);

        _subscribeLocks[symbol].Release();

        // Fire and forget is a bad approach. Instead, this should be run as a job, using a separate Hosted Background Service or Hangfire
        ReceiveMessagesAsync(webSocket, symbol);
    }

    private async Task ReceiveMessagesAsync(ClientWebSocket webSocket, string symbol)
    {
        var buffer = new byte[1024 * 4];
        
        while (webSocket.State == WebSocketState.Open && _subscriptions[symbol].Count > 0)
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

            _logger.LogInformation($"Received from Binance: {message}");

            // This can be much more optimized, creating new list every time is not efficient
            _subscriptions[symbol] = _subscriptions[symbol].Where(w => w.State == WebSocketState.Connecting || w.State == WebSocketState.Open).ToList();
            
            foreach (var subscription in _subscriptions[symbol])
            {
                if (subscription.State == WebSocketState.Open)
                    await SendAsync(subscription, message);
            }
        }

        /* Binance may close the connection, either suddenly or after 24 hours.
           We need to detect that and reopen the connection.
           In case of a failure, we can implement a retry functionality to occasionally ping the Binance server and once it's up then connect to it.
           Similar to Circuit Breaker pattern. */

        foreach (var subscription in _subscriptions[symbol])
        {
            await CloseAsync(subscription);
        }

        await CloseAsync(webSocket);
        
        _subscriptions.Remove(symbol);
        _connections.Remove(symbol);
    }

    private async Task KeepConnectionAliveAsync(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];

        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await CloseAsync(webSocket);
                break;
            }
        }
    }

    private async Task SendAsync(WebSocket webSocket, string message)
    {
        var bytesToSend = Encoding.UTF8.GetBytes(message);
        await webSocket.SendAsync(new ArraySegment<byte>(bytesToSend), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    public async Task CloseAsync(WebSocket webSocket)
    {
        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
    }
}
