using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Interfaces.Services;
using FlyzenApi.Application.Mapping;
using FlyzenApi.Domain.Repositories;

namespace FlyzenApi.Application.Implementations.Services
{
    public class CatalogService : ICatalogService
    {
        private readonly IMealOptionRepository _mealOptionRepository;
        private readonly IBaggageOptionRepository _baggageOptionRepository;

        public CatalogService(IMealOptionRepository mealOptionRepository, IBaggageOptionRepository baggageOptionRepository)
        {
            _mealOptionRepository = mealOptionRepository;
            _baggageOptionRepository = baggageOptionRepository;
        }

        public async Task<IEnumerable<MealOptionDto>> GetMealOptionsAsync() =>
            (await _mealOptionRepository.GetAllAsync()).Select(m => m.ToDto());

        public async Task<IEnumerable<BaggageOptionDto>> GetBaggageOptionsAsync() =>
            (await _baggageOptionRepository.GetAllAsync()).Select(b => b.ToDto());
    }
}
