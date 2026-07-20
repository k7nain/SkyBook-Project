using FluentValidation;

namespace FlyzenApi.Application.Features.Flights.Queries.SearchFlights
{
    public class SearchFlightsQueryValidator : AbstractValidator<SearchFlightsQuery>
    {
        public SearchFlightsQueryValidator()
        {
            RuleFor(x => x.FromCityId).NotEmpty().WithMessage("From city is required");
            RuleFor(x => x.ToCityId).NotEmpty().WithMessage("To city is required");
            RuleFor(x => x.DepartureDate).GreaterThan(DateTime.UtcNow).WithMessage("Departure date must be in the future");
            RuleFor(x => x.Passengers).GreaterThan(0).WithMessage("At least one passenger is required");
        }
    }
}
