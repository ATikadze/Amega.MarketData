using System.Net.WebSockets;

namespace Amega.MarketData.Core.Services.Interfaces;

public interface IWebSocketService
{
    Task HandleConnectionAsync(WebSocket webSocket);
}
