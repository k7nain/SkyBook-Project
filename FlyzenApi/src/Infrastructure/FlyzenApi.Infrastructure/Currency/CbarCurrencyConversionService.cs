using System.Globalization;
using System.Xml.Linq;
using FlyzenApi.Application.Exceptions;
using FlyzenApi.Application.Interfaces.Services;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlyzenApi.Infrastructure.Currency
{
    // Rates come from the Central Bank of Azerbaijan's public daily bulletin
    // (https://www.cbar.az/currencies/{dd.MM.yyyy}.xml) - the authoritative,
    // no-API-key source for AZN specifically, which is what this app needs
    // ("1 unit of foreign currency = X AZN", already in the direction we want).
    // Registered as a Singleton holding an in-memory cache refreshed at most
    // every CurrencyOptions.CacheRefreshHours - conversions never hit the feed
    // directly, only GetRatesAsync's cache-miss path does.
    public class CbarCurrencyConversionService : ICurrencyConversionService
    {
        // Covers every currency the app's user-facing currency picker offers
        // (src/mock/currencies.js) as well as the admin flight-price selector -
        // both read from this same service so neither can silently drift back
        // to a stale hardcoded rate again.
        private static readonly string[] SupportedForeignCurrencies = { "USD", "EUR", "GBP", "TRY", "RUB" };
        private const int MaxDaysBack = 5; // CBAR doesn't publish on weekends/holidays - walk back to the last business day.

        // Used only if CBAR is unreachable on first-ever fetch (no prior cache
        // to fall back to) - approximate, clearly labeled via ExchangeRates.Source
        // so it's never confused with a live rate in the UI. TRY in particular
        // moves fast (heavy devaluation) - this is a rough placeholder, not a
        // rate anyone should trust; it only exists so the app doesn't crash if
        // CBAR is down on the very first request after a cold start.
        private static readonly Dictionary<string, decimal> StaticFallbackRates = new()
        {
            ["AZN"] = 1.0m,
            ["USD"] = 1.70m,
            ["EUR"] = 1.94m,
            ["GBP"] = 2.25m,
            ["TRY"] = 0.036m,
            ["RUB"] = 0.021m,
        };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CurrencyOptions _options;
        private readonly ILogger<CbarCurrencyConversionService> _logger;
        private readonly SemaphoreSlim _refreshLock = new(1, 1);
        private ExchangeRates? _cached;

        public CbarCurrencyConversionService(
            IHttpClientFactory httpClientFactory,
            IOptions<CurrencyOptions> options,
            ILogger<CbarCurrencyConversionService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<ExchangeRates> GetRatesAsync(CancellationToken cancellationToken = default)
        {
            if (IsFresh(_cached))
                return _cached!;

            await _refreshLock.WaitAsync(cancellationToken);
            try
            {
                // Another request may have refreshed the cache while we were waiting.
                if (IsFresh(_cached))
                    return _cached!;

                var fetched = await FetchFromCbarAsync(cancellationToken);
                _cached = fetched ?? _cached ?? new ExchangeRates(StaticFallbackRates, DateTime.UtcNow, "static fallback (CBAR unreachable)");
                return _cached;
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        public async Task<decimal> ConvertToBaseAsync(decimal amount, string fromCurrency, CancellationToken cancellationToken = default)
        {
            var currency = fromCurrency.Trim().ToUpperInvariant();
            if (currency == ICurrencyConversionService.BaseCurrency)
                return Math.Round(amount, 2, MidpointRounding.AwayFromZero);

            var rates = await GetRatesAsync(cancellationToken);
            if (!rates.RatesToAzn.TryGetValue(currency, out var rate))
                throw new BadRequestException($"Unsupported currency: '{fromCurrency}'.");

            var converted = Math.Round(amount * rate, 2, MidpointRounding.AwayFromZero);
            _logger.LogInformation(
                "Currency conversion: {Amount} {Currency} -> {Converted} AZN (rate 1 {Currency} = {Rate} AZN, {Source}).",
                amount, currency, converted, currency, rate, rates.Source);
            return converted;
        }

        private bool IsFresh(ExchangeRates? rates) =>
            rates is not null && DateTime.UtcNow - rates.FetchedAtUtc < TimeSpan.FromHours(Math.Max(1, _options.CacheRefreshHours));

        private async Task<ExchangeRates?> FetchFromCbarAsync(CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);

            for (var daysBack = 0; daysBack <= MaxDaysBack; daysBack++)
            {
                var date = DateTime.UtcNow.Date.AddDays(-daysBack);
                var url = $"https://www.cbar.az/currencies/{date:dd.MM.yyyy}.xml";

                try
                {
                    var xml = await client.GetStringAsync(url, cancellationToken);
                    var doc = XDocument.Parse(xml);

                    var rates = new Dictionary<string, decimal> { [ICurrencyConversionService.BaseCurrency] = 1.0m };
                    foreach (var code in SupportedForeignCurrencies)
                    {
                        var valute = doc.Descendants("Valute").FirstOrDefault(v => (string?)v.Attribute("Code") == code);
                        if (valute is null)
                            continue;

                        var nominalText = valute.Element("Nominal")?.Value.Trim().Split(' ')[0];
                        var valueText = valute.Element("Value")?.Value.Trim();
                        if (!decimal.TryParse(nominalText, NumberStyles.Number, CultureInfo.InvariantCulture, out var nominal)
                            || !decimal.TryParse(valueText, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
                            || nominal <= 0 || value <= 0)
                            continue;

                        rates[code] = value / nominal;
                        _logger.LogInformation(
                            "CBAR rate parsed: 1 {Code} = {Rate} AZN (raw Nominal={Nominal}, Value={Value}, {Date}).",
                            code, rates[code], nominal, value, date.ToString("dd.MM.yyyy"));
                    }

                    // Only accept this response if we actually found at least one
                    // real foreign rate - an empty/malformed document shouldn't
                    // silently "succeed" with just the trivial AZN=1 entry.
                    if (rates.Count > 1)
                        return new ExchangeRates(rates, DateTime.UtcNow, $"cbar.az ({date:dd.MM.yyyy})");
                }
                catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Xml.XmlException)
                {
                    _logger.LogWarning(ex, "Failed to fetch/parse CBAR exchange rates for {Date}.", date);
                }
            }

            return null;
        }
    }
}
