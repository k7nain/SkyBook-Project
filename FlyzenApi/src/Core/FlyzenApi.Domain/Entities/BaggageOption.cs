using FlyzenApi.Domain.Entities.Common;

namespace FlyzenApi.Domain.Entities
{
    public class BaggageOption : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public int WeightKg { get; set; }
        public decimal Price { get; set; }
    }
}