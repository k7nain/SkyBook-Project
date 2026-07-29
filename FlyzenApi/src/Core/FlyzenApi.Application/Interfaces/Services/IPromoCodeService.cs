using FlyzenApi.Application.DTOs;

namespace FlyzenApi.Application.Interfaces.Services
{
    public interface IPromoCodeService
    {
        Task<IEnumerable<PromoCodeDto>> GetAllAsync();
        Task<PromoCodeDto> CreateAsync(CreatePromoCodeRequest request);
        Task<PromoCodeDto> UpdateAsync(Guid id, UpdatePromoCodeRequest request);
        Task DeleteAsync(Guid id);
        Task<ValidatePromoCodeResponse> ValidateAsync(ValidatePromoCodeRequest request);
    }
}
