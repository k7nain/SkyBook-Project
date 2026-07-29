namespace FlyzenApi.Application.DTOs
{
    public class ExchangeRatesDto
    {
        // "1 unit of Code = Value AZN", e.g. { "AZN": 1.0, "USD": 1.70, "EUR": 1.9345 }.
        public Dictionary<string, decimal> RatesToAzn { get; set; } = new();
        public DateTime FetchedAtUtc { get; set; }
        public string Source { get; set; } = string.Empty;
    }
}
