using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Threading.Tasks;
using System;
using FlyzenApi.Application.Features.Bookings.Commands.CreateBooking;
using FlyzenApi.Domain.Repositories;

namespace FlyzenApi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IBookingRepository _bookingRepository;

        public BookingsController(IMediator mediator, IBookingRepository bookingRepository)
        {
            _mediator = mediator;
            _bookingRepository = bookingRepository;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookingById(Guid id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
                return NotFound(new { message = "Booking not found" });

            return Ok(new { success = true, data = booking });
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserBookings(Guid userId)
        {
            var bookings = await _bookingRepository.GetBookingsByUserIdAsync(userId);
            return Ok(new { success = true, data = bookings });
        }
    }
}