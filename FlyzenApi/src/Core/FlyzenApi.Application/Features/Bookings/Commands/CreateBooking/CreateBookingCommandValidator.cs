using FluentValidation;

namespace FlyzenApi.Application.Features.Bookings.Commands.CreateBooking
{
    public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
    {
        public CreateBookingCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required");
            RuleFor(x => x.FlightId).NotEmpty().WithMessage("FlightId is required");
            RuleFor(x => x.Passengers).NotEmpty().WithMessage("At least one passenger is required");
            RuleForEach(x => x.Passengers).SetValidator(new PassengerDtoValidator());
        }
    }

    public class PassengerDtoValidator : AbstractValidator<PassengerDto>
    {
        public PassengerDtoValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required");
            RuleFor(x => x.LastName).NotEmpty().WithMessage("Last name is required");
            RuleFor(x => x.PassportNumber).NotEmpty().WithMessage("Passport number is required");
            RuleFor(x => x.Type).IsInEnum().WithMessage("Invalid passenger type");
        }
    }
}
