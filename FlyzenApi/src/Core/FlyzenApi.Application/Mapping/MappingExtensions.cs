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
            SkyPointsBalance = user.SkyPointsBalance,
        };

        public static SkyPointsTransactionDto ToDto(this SkyPointsTransaction transaction) => new()
        {
            Id = transaction.Id,
            Amount = transaction.Amount,
            Type = transaction.Type,
            RelatedBookingPnr = transaction.RelatedBooking?.PNR,
            CreatedAt = transaction.CreatedAt,
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

        public static TravelJournalDto ToDto(this TravelJournal journal) => new()
        {
            Id = journal.Id,
            UserId = journal.UserId,
            UserName = $"{journal.User.FirstName} {journal.User.LastName}",
            UserAvatarUrl = journal.User.ProfilePictureUrl,
            BookingId = journal.BookingId,
            DestinationCityId = journal.DestinationCityId,
            DestinationCityName = journal.DestinationCity.Name,
            Title = journal.Title,
            Body = journal.Body,
            Rating = journal.Rating,
            Images = journal.Images
                .OrderBy(i => i.DisplayOrder)
                .Select(i => new TravelJournalImageDto { Id = i.Id, ImageUrl = i.ImageUrl, DisplayOrder = i.DisplayOrder })
                .ToList(),
            CreatedAt = journal.CreatedAt,
        };

        public static PriceProposalDto ToDto(this PriceProposal proposal) => new()
        {
            Id = proposal.Id,
            FlightId = proposal.FlightId,
            FlightNumber = proposal.Flight.FlightNumber,
            Route = $"{proposal.Flight.DepartureCity.Name} -> {proposal.Flight.ArrivalCity.Name}",
            Currency = proposal.Flight.Currency,
            CurrentPrice = proposal.CurrentPrice,
            SuggestedPrice = proposal.SuggestedPrice,
            OccupancyRate = proposal.OccupancyRate,
            VelocityFactor = proposal.VelocityFactor,
            UrgencyFactor = proposal.UrgencyFactor,
            DemandScore = proposal.DemandScore,
            Status = proposal.Status,
            CreatedAt = proposal.CreatedAt,
            DecidedAt = proposal.DecidedAt,
            DecidedByName = proposal.DecidedByUser is null ? null : $"{proposal.DecidedByUser.FirstName} {proposal.DecidedByUser.LastName}",
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
            NameAz = country.NameAz,
            NameEn = country.NameEn,
            NameRu = country.NameRu,
            FlagCode = country.FlagCode,
            CoverImage = country.CoverImage,
            CreatedAt = country.CreatedAt,
        };

        public static TripCityDto ToDto(this TripCity city) => new()
        {
            Id = city.Id,
            CountryId = city.CountryId,
            NameAz = city.NameAz,
            NameEn = city.NameEn,
            NameRu = city.NameRu,
            Image = city.Image,
            ShortDescriptionAz = city.ShortDescriptionAz,
            ShortDescriptionEn = city.ShortDescriptionEn,
            ShortDescriptionRu = city.ShortDescriptionRu,
        };

        public static TripPlaceDto ToDto(this TripPlace place) => new()
        {
            Id = place.Id,
            CityId = place.CityId,
            NameAz = place.NameAz,
            NameEn = place.NameEn,
            NameRu = place.NameRu,
            DescriptionAz = place.DescriptionAz,
            DescriptionEn = place.DescriptionEn,
            DescriptionRu = place.DescriptionRu,
            Category = place.Category,
            Latitude = place.Latitude,
            Longitude = place.Longitude,
        };

        public static TripPlaceImageDto ToDto(this TripPlaceImage image) => new()
        {
            Id = image.Id,
            PlaceId = image.PlaceId,
            ImageUrl = image.ImageUrl,
            DisplayOrder = image.DisplayOrder,
        };

        // language is one of "az" | "en" | "ru", already normalized by the caller
        // (WorldMapService.NormalizeLanguage) - falls back to the always-present
        // Az field the same way DreamTripAiService falls back when a translation
        // hasn't landed yet.
        public static MapMarkerDto ToMarkerDto(this TripPlace place, string language) => new()
        {
            Id = place.Id,
            DestinationName = ResolveLocalizedName(place.NameAz, place.NameEn, place.NameRu, language),
            CountryCode = place.City.Country.FlagCode,
            Latitude = place.Latitude ?? 0,
            Longitude = place.Longitude ?? 0,
            ThumbnailUrl = ResolveThumbnail(place),
            ShortDescription = ResolveLocalizedText(place.DescriptionAz, place.DescriptionEn, place.DescriptionRu, language),
        };

        public static DestinationDetailDto ToDetailDto(this TripPlace place, string language) => new()
        {
            Id = place.Id,
            DestinationName = ResolveLocalizedName(place.NameAz, place.NameEn, place.NameRu, language),
            Description = ResolveLocalizedText(place.DescriptionAz, place.DescriptionEn, place.DescriptionRu, language),
            Category = place.Category,
            Latitude = place.Latitude ?? 0,
            Longitude = place.Longitude ?? 0,
            CountryCode = place.City.Country.FlagCode,
            CountryName = ResolveLocalizedName(place.City.Country.NameAz, place.City.Country.NameEn, place.City.Country.NameRu, language),
            CityName = ResolveLocalizedName(place.City.NameAz, place.City.NameEn, place.City.NameRu, language),
            CityDescription = ResolveLocalizedText(place.City.ShortDescriptionAz, place.City.ShortDescriptionEn, place.City.ShortDescriptionRu, language),
            ThumbnailUrl = ResolveThumbnail(place),
            GalleryImageUrls = place.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).ToList(),
        };

        private static string? ResolveThumbnail(TripPlace place) =>
            place.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl
                ?? place.City.Image
                ?? place.City.Country.CoverImage;

        private static string ResolveLocalizedName(string nameAz, string? nameEn, string? nameRu, string language) => language switch
        {
            "en" => nameEn ?? nameAz,
            "ru" => nameRu ?? nameAz,
            _ => nameAz,
        };

        private static string? ResolveLocalizedText(string? textAz, string? textEn, string? textRu, string language) => language switch
        {
            "en" => textEn ?? textAz,
            "ru" => textRu ?? textAz,
            _ => textAz,
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
            GateNumber = flight.GateNumber,
            OperationalStatus = flight.OperationalStatus,
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
            GateNumber = flight.GateNumber,
            OperationalStatus = flight.OperationalStatus,
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
            IsCheckedIn = booking.IsCheckedIn,
            CheckedInAt = booking.CheckedInAt,
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
            dto.IsCheckedIn = baseDto.IsCheckedIn;
            dto.CheckedInAt = baseDto.CheckedInAt;
            return dto;
        }
    }
}
