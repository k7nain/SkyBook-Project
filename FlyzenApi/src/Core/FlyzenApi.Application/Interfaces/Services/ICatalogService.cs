using FlyzenApi.Application.DTOs;

namespace FlyzenApi.Application.Interfaces.Services
{
    public interface ICatalogService
    {
        Task<IEnumerable<MealOptionDto>> GetMealOptionsAsync();
        Task<IEnumerable<BaggageOptionDto>> GetBaggageOptionsAsync();
    }
}
