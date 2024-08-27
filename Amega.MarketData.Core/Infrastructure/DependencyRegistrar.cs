using Microsoft.Extensions.DependencyInjection;
using Amega.MarketData.Core.Services.Interfaces;
using Amega.MarketData.Core.Services.Implementations;

namespace Amega.MarketData.Core.Infrastructure
{
    public static class DependencyRegistrar
    {
        public static void RegisterCoreServices(this IServiceCollection services)
        {
            services.AddTransient<IMarketService, MarketService>();
            services.AddSingleton<IWebSocketService, WebSocketService>();
        }
    }
}
