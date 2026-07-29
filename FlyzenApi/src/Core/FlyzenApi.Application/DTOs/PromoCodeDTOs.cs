using System.ComponentModel.DataAnnotations;

namespace FlyzenApi.Application.DTOs
{
    public class PromoCodeDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal DiscountPercentage { get; set; }
        public bool IsActive { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int? MaxUses { get; set; }
        public int UsedCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreatePromoCodeRequest
    {
        [Required, MaxLength(30)]
        public string Code { get; set; } = string.Empty;

        [Range(0.01, 100)]
        public decimal DiscountPercentage { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime ExpiryDate { get; set; }

        [Range(1, int.MaxValue)]
        public int? MaxUses { get; set; }
    }

    public class UpdatePromoCodeRequest
    {
        [Range(0.01, 100)]
        public decimal DiscountPercentage { get; set; }

        public bool IsActive { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        [Range(1, int.MaxValue)]
        public int? MaxUses { get; set; }
    }

    public class ValidatePromoCodeRequest
    {
        [Required]
        public string Code { get; set; } = string.Empty;
    }

    public class ValidatePromoCodeResponse
    {
        public bool IsValid { get; set; }
        public decimal DiscountPercentage { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
