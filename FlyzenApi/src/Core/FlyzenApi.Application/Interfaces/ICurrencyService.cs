using System.Threading.Tasks;

namespace FlyzenApi.Application.Interfaces
{
    public interface ICurrencyService
    {
        Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency);
    }
}