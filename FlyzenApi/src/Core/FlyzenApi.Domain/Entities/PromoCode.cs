using System;
using FlyzenApi.Domain.Entities.Common;

namespace FlyzenApi.Domain.Entities
{
    public class PromoCode : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public decimal DiscountPercentage { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime ExpiryDate { get; set; }
        public int? MaxUses { get; set; }
        public int UsedCount { get; set; } = 0;

        public bool IsCurrentlyValid() =>
            IsActive
            && ExpiryDate >= DateTime.UtcNow
            && (!MaxUses.HasValue || UsedCount < MaxUses.Value);
    }
}
