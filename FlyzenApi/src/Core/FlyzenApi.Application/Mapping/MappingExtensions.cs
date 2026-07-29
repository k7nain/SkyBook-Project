using System.Text.Json;
using FlyzenApi.Application.DTOs;
using FlyzenApi.Domain.Entities;

namespace FlyzenApi.Application.Mapping
{
    public static class MappingExtensions
    {
        public static UserDto ToDto(this User user) => new()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            ProfilePictureUrl = user.ProfilePictureUrl,
            CurrencyPreference = user.CurrencyPreference,
            LanguagePreference = user.LanguagePreference,
            PreferredTimezone = user.PreferredTimezone,
            Role = user.Role,
        };

        public static NotificationDto ToDto(this Notification notification) => new()
        {
            Id = notification.Id,
            Title = notification.Title,
            Message = notification.Message,
            TitleKey = notification.TitleKey,
            BodyKey = notification.BodyKey,
            Params = notification.ParamsJson is null
                ? null
                : JsonSerializer.Deserialize<Dictionary<string, string>>(notification.ParamsJson),
            Type = notification.Type,
            IsRead = notification.IsRead,
            BookingId = notification.BookingId,
            CreatedAt = notification.CreatedAt,
        };

        public static PromoCodeDto ToDto(this PromoCode promoCode) => new()
        {
            Id = promoCode.Id,
            Code = promoCode.Code,
            DiscountPercentage = promoCode.DiscountPercentage,
            IsActive = promoCode.IsActive,
            ExpiryDate = promoCode.ExpiryDate,
            MaxUses = promoCode.MaxUses,
            UsedCount = promoCode.UsedCount,
            CreatedAt = promoCode.CreatedAt,
        };

        public static AdminUserDto ToAdminDto(this User user) => new()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            IsEmailConfirmed = user.IsEmailConfirmed,
            CreatedAt = user.CreatedAt,
        };

        public static CityDto ToDto(this City city) => new()
        {
            Id = city.Id,
            Name = city.Name,
            Country = city.Country,
            AirportCode = city.AirportCode,
            Description = city.Description,
            Climate = city.Climate,
            Attractions = city.Attractions,
            TimeZoneId = city.TimeZoneId,
        };

        public static AirlineDto ToDto(this Airline airline) => new()
        {
            Id = airline.Id,
            Name = airline.Name,
            Code = airline.Code,
        };

        public static CityGalleryImageDto ToDto(this CityGalleryImage image) => new()
        {
            Id = image.Id,
            ImageUrl = image.ImageUrl,
            Description = image.Description,
        };

        public static TripCountryDto ToDto(this TripCountry country) => new()
        {
            Id = country.Id,
            Name = country.Name,
            FlagCode = country.FlagCode,
            CoverImage = country.CoverImage,
            CreatedAt = country.CreatedAt,
        };

        public static TripCityDto ToDto(this TripCity city) => new()
        {
            Id = city.Id,
            CountryId = city.CountryId,
            Name = city.Name,
            Image = city.Image,
            ShortDescription = city.ShortDescription,
        };

        public static TripPlaceDto ToDto(this TripPlace place) => new()
        {
            Id = place.Id,
            CityId = place.CityId,
            Name = place.Name,
            Description = place.Description,
            Category = place.Category,
            Latitude = place.Latitude,
            Longitude = place.Longitude,
        };

        public static TripPlaceImageDto ToDto(this TripPlaceImage image) => new()
        {
            Id = image.Id,
            PlaceId = image.PlaceId,
            ImageUrl = image.ImageUrl,
        };

        public static SeatDto ToDto(this SeatMap seat, decimal basePrice) => new()
        {
            Id = seat.Id,
            SeatNumber = seat.SeatNumber,
            Class = seat.Class,
            IsAvailable = seat.IsAvailable,
            PriceMultiplier = seat.PriceMultiplier,
            Price = Math.Round(basePrice * seat.PriceMultiplier, 2),
        };

        public static FlightSummaryDto ToSummaryDto(this Flight flight) => new()
        {
            Id = flight.Id,
            FlightNumber = flight.FlightNumber,
            Airline = flight.Airline?.ToDto(),
            DepartureCity = flight.DepartureCity.ToDto(),
            DepartureTime = flight.DepartureTime,
            ArrivalCity = flight.ArrivalCity.ToDto(),
            ArrivalTime = flight.ArrivalTime,
            BasePrice = flight.BasePrice,
            Currency = flight.Currency,
            AvailableSeats = flight.Seats.Count(s => s.IsAvailable),
        };

        public static FlightDetailDto ToDetailDto(this Flight flight) => new()
        {
            Id = flight.Id,
            FlightNumber = flight.FlightNumber,
            Airline = flight.Airline?.ToDto(),
            DepartureCity = flight.DepartureCity.ToDto(),
            DepartureTime = flight.DepartureTime,
            ArrivalCity = flight.ArrivalCity.ToDto(),
            ArrivalTime = flight.ArrivalTime,
            BasePrice = flight.BasePrice,
            Currency = flight.Currency,
            AvailableSeats = flight.Seats.Count(s => s.IsAvailable),
            Seats = flight.Seats.OrderBy(s => s.SeatNumber).Select(s => s.ToDto(flight.BasePrice)).ToList(),
        };

        public static MealOptionDto ToDto(this MealOption meal) => new()
        {
            Id = meal.Id,
            Type = meal.Type,
            Name = meal.Name,
            Description = meal.Description,
            Price = meal.Price,
            Currency = meal.Currency,
        };

        public static BaggageOptionDto ToDto(this BaggageOption baggage) => new()
        {
            Id = baggage.Id,
            Name = baggage.Name,
            WeightKg = baggage.WeightKg,
            Price = baggage.Price,
            Currency = baggage.Currency,
            DisplayOrder = baggage.DisplayOrder,
        };

        public static BookingPassengerDto ToDto(this BookingPassenger passenger) => new()
        {
            FirstName = passenger.FirstName,
            LastName = passenger.LastName,
            PassportNumber = passenger.PassportNumber,
            Type = passenger.Type,
            SeatNumber = passenger.Seat?.SeatNumber ?? string.Empty,
            MealName = passenger.MealOption?.Name,
            BaggageLabel = passenger.BaggageOption is null ? null : $"{passenger.BaggageOption.WeightKg} kg",
            PriceCalculated = passenger.PriceCalculated,
        };

        public static TicketDto ToDto(this Ticket ticket) => new()
        {
            TicketNumber = ticket.TicketNumber,
            QrCodeData = ticket.QrCodeData,
            IssuedAt = ticket.IssuedAt,
        };

        public static BookingDto ToDto(this Booking booking) => new()
        {
            Id = booking.Id,
            PNR = booking.PNR,
            Status = booking.Status,
            TotalPrice = booking.TotalPrice,
            Currency = booking.Currency,
            Flight = booking.Flight.ToSummaryDto(),
            Passengers = booking.Passengers.Select(p => p.ToDto()).ToList(),
            Ticket = booking.Ticket?.ToDto(),
            CreatedAt = booking.CreatedAt,
        };

        public static AdminBookingDto ToAdminDto(this Booking booking)
        {
            var dto = new AdminBookingDto
            {
                UserName = $"{booking.User.FirstName} {booking.User.LastName}",
                UserEmail = booking.User.Email,
            };
            var baseDto = booking.ToDto();
            dto.Id = baseDto.Id;
            dto.PNR = baseDto.PNR;
            dto.Status = baseDto.Status;
            dto.TotalPrice = baseDto.TotalPrice;
            dto.Currency = baseDto.Currency;
            dto.Flight = baseDto.Flight;
            dto.Passengers = baseDto.Passengers;
            dto.Ticket = baseDto.Ticket;
            dto.CreatedAt = baseDto.CreatedAt;
            return dto;
        }
    }
}
