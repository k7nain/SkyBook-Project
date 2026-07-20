using FlyzenApi.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace FlyzenApi.Infrastructure.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly IConfiguration _configuration;
        private Dictionary<string, decimal>? _exchangeRates;

        public CurrencyService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public decimal ConvertCurrency(decimal amount, string fromCurrency, string toCurrency)
        {
            if (fromCurrency == toCurrency)
                return amount;

            LoadExchangeRates();

            var fromRate = _exchangeRates?.GetValueOrDefault(fromCurrency, 1m) ?? 1m;
            var toRate = _exchangeRates?.GetValueOrDefault(toCurrency, 1m) ?? 1m;

            return Math.Round((amount / fromRate) * toRate, 2);
        }

        public Dictionary<string, decimal> GetExchangeRates()
        {
            LoadExchangeRates();
            return _exchangeRates ?? new Dictionary<string, decimal>();
        }

        private void LoadExchangeRates()
        {
            if (_exchangeRates != null)
                return;

            _exchangeRates = new Dictionary<string, decimal>
            {
                { "USD", 1m },
                { "EUR", 0.85m },
                { "GBP", 0.73m },
                { "JPY", 110m },
                { "AED", 3.67m },
                { "TRY", 28m }
            };

            // Try to load from appsettings
            var rates = _configuration.GetSection("CurrencyRates").Get<Dictionary<string, decimal>>();
            if (rates != null)
            {
                _exchangeRates = rates;
            }
        }
    }
}
