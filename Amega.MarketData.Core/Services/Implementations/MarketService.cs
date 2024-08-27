using System.Net.Http;
using System.Text.Json;
using Amega.MarketData.Core.DTOs.Response;
using Amega.MarketData.Core.Helpers;
using Amega.MarketData.Core.Models;
using Amega.MarketData.Core.Models.CustomExceptions;
using Amega.MarketData.Core.Services.Interfaces;

namespace Amega.MarketData.Core.Services.Implementations;

// Unit tests can be added and Moq can be used for more in-depth testing
public class MarketService : IMarketService
{
    public string[] GetInstruments()
    {
        return InstrumentsHelpers.AvailableInstruments;
    }

    public async Task<string> GetPriceAsync(string instrument)
    {
        InstrumentsHelpers.EnsureExists(instrument);
        
        using (var httpContent = new HttpClient())
        {
            // Values like these should better be kept in app configs
            var getPriceUrl = "https://api.binance.com/api/v3/ticker/price?symbol=";
            var response = await httpContent.GetAsync(getPriceUrl + instrument);

            if (!response.IsSuccessStatusCode)
                throw new CustomException(response.ReasonPhrase, (int)response.StatusCode);

            using (var responseStream = await response.Content.ReadAsStreamAsync())
            {
                var jsonSerializerOptions = new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                };

                var priceModel = JsonSerializer.Deserialize<PriceResponseModel>(responseStream, jsonSerializerOptions);
                
                return priceModel.Price;
            }
        }
    }
}
