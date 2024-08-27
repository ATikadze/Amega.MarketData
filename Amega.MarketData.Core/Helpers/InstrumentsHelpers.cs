using Amega.MarketData.Core.Models.CustomExceptions;

namespace Amega.MarketData.Core.Helpers;

public static class InstrumentsHelpers
{
    // This would obviously be coming from another source like the database or something
    public static readonly string[] AvailableInstruments = { "BTCUSDT", "ETHUSDT", "SOLUSDT" };

    public static void EnsureExists(string instrument)
    {
        if (string.IsNullOrEmpty(instrument))
            throw new CustomException($"The {nameof(instrument)} parameter can't be empty");
        
        if (!AvailableInstruments.Any(s => s.Equals(instrument, StringComparison.OrdinalIgnoreCase)))
            throw new CustomException($"The following instrument does not exist: {instrument}");
    }
}
