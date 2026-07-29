namespace FlyzenApi.Application.Interfaces.Services
{
    // Rates are always expressed as "1 unit of Code = Value AZN" (e.g. USD -> 1.70),
    // so converting an amount in that currency to AZN is `amount * Value`.
    public record ExchangeRates(IReadOnlyDictionary<string, decimal> RatesToAzn, DateTime FetchedAtUtc, string Source);

    // Centralizes exchange-rate fetching (cached, refreshed periodically - not
    // hit on every conversion) and the AZN conversion math, so flight creation,
    // flight price editing and admin statistics don't each duplicate it.
    // AZN is always the stored/base currency app-wide; this service only
    // supports converting *into* AZN, never the other direction, since nothing
    // in the app stores a non-AZN price.
    public interface ICurrencyConversionService
    {
        const string BaseCurrency = "AZN";

        Task<ExchangeRates> GetRatesAsync(CancellationToken cancellationToken = default);

        Task<decimal> ConvertToBaseAsync(decimal amount, string fromCurrency, CancellationToken cancellationToken = default);
    }
}
