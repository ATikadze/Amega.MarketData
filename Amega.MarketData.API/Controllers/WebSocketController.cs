using Microsoft.AspNetCore.Mvc;
using Amega.MarketData.Core.Services.Interfaces;
using System.Net;
using Amega.MarketData.Core.Models.CustomExceptions;

namespace Amega.MarketData.API.Controllers;

[Route("ws")]
public class WebSocketController : BaseController
{
    private ILogger<WebSocketController> _logger;
    private readonly IWebSocketService _webSocketService;

    public WebSocketController(ILogger<WebSocketController> logger, IWebSocketService webSocketService)
    {
        _logger = logger;
        _webSocketService = webSocketService;
    }

    [HttpGet]
    public async Task<IActionResult> Subscribe()
    {
        var context = ControllerContext.HttpContext;

        if (context.WebSockets.IsWebSocketRequest)
        {
            _logger.LogInformation($"Incoming connection: '{context.Connection.Id}'");

            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();

            _logger.LogInformation($"Accepted connection: '{context.Connection.Id}'");

            await _webSocketService.HandleConnectionAsync(webSocket);

            return Success();
        }
        else
        {
            throw new CustomException("Only socket connections are supprted", 400);
        }
    }
}
