namespace Amega.MarketData.Core.Services.Interfaces;

public interface IMarketService
{
    string[] GetInstruments();
    Task<string> GetPriceAsync(string instrument);
}
