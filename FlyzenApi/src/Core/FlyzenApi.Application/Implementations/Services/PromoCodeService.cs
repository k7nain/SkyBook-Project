using System.Globalization;
using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Exceptions;
using FlyzenApi.Application.Interfaces.Services;
using FlyzenApi.Application.Mapping;
using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Repositories;

namespace FlyzenApi.Application.Implementations.Services
{
    public class PromoCodeService : IPromoCodeService
    {
        private readonly IPromoCodeRepository _promoCodeRepository;

        public PromoCodeService(IPromoCodeRepository promoCodeRepository)
        {
            _promoCodeRepository = promoCodeRepository;
        }

        public async Task<IEnumerable<PromoCodeDto>> GetAllAsync() =>
            (await _promoCodeRepository.GetAllAsync()).Select(p => p.ToDto());

        public async Task<PromoCodeDto> CreateAsync(CreatePromoCodeRequest request)
        {
            var code = request.Code.Trim().ToUpper();
            var existing = await _promoCodeRepository.GetByCodeAsync(code);
            if (existing is not null)
                throw new ConflictException("A promo code with this code already exists.");

            var promoCode = new PromoCode
            {
                Code = code,
                DiscountPercentage = request.DiscountPercentage,
                IsActive = request.IsActive,
                ExpiryDate = DateTime.SpecifyKind(request.ExpiryDate, DateTimeKind.Utc),
                MaxUses = request.MaxUses,
            };

            await _promoCodeRepository.AddAsync(promoCode);
            return promoCode.ToDto();
        }

        public async Task<PromoCodeDto> UpdateAsync(Guid id, UpdatePromoCodeRequest request)
        {
            var promoCode = await _promoCodeRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("Promo code not found.");

            promoCode.DiscountPercentage = request.DiscountPercentage;
            promoCode.IsActive = request.IsActive;
            promoCode.ExpiryDate = DateTime.SpecifyKind(request.ExpiryDate, DateTimeKind.Utc);
            promoCode.MaxUses = request.MaxUses;

            await _promoCodeRepository.UpdateAsync(promoCode);
            return promoCode.ToDto();
        }

        public async Task DeleteAsync(Guid id)
        {
            var promoCode = await _promoCodeRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("Promo code not found.");
            await _promoCodeRepository.DeleteAsync(promoCode);
        }

        public async Task<ValidatePromoCodeResponse> ValidateAsync(ValidatePromoCodeRequest request)
        {
            var promoCode = await _promoCodeRepository.GetByCodeAsync(request.Code);
            if (promoCode is null || !promoCode.IsCurrentlyValid())
            {
                return new ValidatePromoCodeResponse
                {
                    IsValid = false,
                    DiscountPercentage = 0,
                    Message = "Invalid or expired promo code.",
                };
            }

            return new ValidatePromoCodeResponse
            {
                IsValid = true,
                DiscountPercentage = promoCode.DiscountPercentage,
                Message = $"{promoCode.DiscountPercentage.ToString(CultureInfo.InvariantCulture)}% discount applied.",
            };
        }
    }
}
