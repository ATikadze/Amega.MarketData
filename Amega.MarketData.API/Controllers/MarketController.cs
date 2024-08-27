using Microsoft.AspNetCore.Mvc;
using Amega.MarketData.Core.Services.Interfaces;

namespace Amega.MarketData.API.Controllers;

public class MarketController : BaseController
{
    private readonly IMarketService _marketService;

    public MarketController(IMarketService marketService)
    {
        _marketService = marketService;
    }

    [HttpGet("getInstruments")]
    public IActionResult GetInstruments()
    {
        var result = _marketService.GetInstruments();

        return Success(result);
    }

    [HttpGet("getPrice")]
    public async Task<IActionResult> GetPrice(string? instrument)
    {
        var result = await _marketService.GetPriceAsync(instrument);

        return Success(result);
    }
}
