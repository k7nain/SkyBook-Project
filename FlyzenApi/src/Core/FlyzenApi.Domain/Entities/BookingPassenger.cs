using System;
using FlyzenApi.Domain.Entities.Common;
using FlyzenApi.Domain.Enums;

namespace FlyzenApi.Domain.Entities
{
    public class BookingPassenger : BaseEntity
    {
        public Guid BookingId { get; set; }
        public Booking Booking { get; set; } = null!;
        
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PassportNumber { get; set; } = string.Empty;
        public PassengerType Type { get; set; }
        
        public Guid? SeatMapId { get; set; }
        public SeatMap? Seat { get; set; }
        
        public Guid? MealOptionId { get; set; }
        public MealOption? MealOption { get; set; }
        
        public Guid? BaggageOptionId { get; set; }
        public BaggageOption? BaggageOption { get; set; }
        
        public decimal PriceCalculated { get; set; }
    }
}