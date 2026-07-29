namespace FlyzenApi.Infrastructure.Currency
{
    public class CurrencyOptions
    {
        public const string SectionName = "Currency";

        // How long a fetched rate set is trusted before refetching from CBAR -
        // kept a few hours (not seconds/minutes) since these rates only publish
        // once a day; this just avoids hitting the feed on every conversion.
        public int CacheRefreshHours { get; set; } = 6;
    }
}
