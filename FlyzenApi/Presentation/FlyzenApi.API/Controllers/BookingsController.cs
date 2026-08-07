using FlyzenApi.API.Common;
using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlyzenApi.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/bookings")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ITicketService _ticketService;

        public BookingsController(IBookingService bookingService, ITicketService ticketService)
        {
            _bookingService = bookingService;
            _ticketService = ticketService;
        }

        [HttpPost]
        public async Task<ActionResult<BookingDto>> Create(CreateBookingRequest request) =>
            Ok(await _bookingService.CreateAsync(User.GetUserId(), request));

        [HttpGet("mine")]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetMine() =>
            Ok(await _bookingService.GetMineAsync(User.GetUserId()));

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<BookingDto>> GetById(Guid id)
        {
            var booking = await _bookingService.GetByIdAsync(id, User.GetUserId());
            return booking is null ? NotFound() : Ok(booking);
        }

        [HttpPost("{id:guid}/cancel")]
        public async Task<ActionResult<BookingDto>> Cancel(Guid id) =>
            Ok(await _bookingService.CancelAsync(id, User.GetUserId()));

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _bookingService.DeleteAsync(id, User.GetUserId());
            return NoContent();
        }

        [HttpGet("{id:guid}/ticket")]
        public async Task<ActionResult<TicketDto>> GetTicket(Guid id)
        {
            var booking = await _bookingService.GetByIdAsync(id, User.GetUserId());
            if (booking is null)
                return NotFound();

            var ticket = await _ticketService.GetByBookingIdAsync(id);
            return ticket is null ? NotFound() : Ok(ticket);
        }

        [HttpPost("{id:guid}/send-ticket")]
        public async Task<IActionResult> SendTicket(Guid id, SendTicketEmailRequest request)
        {
            await _bookingService.SendTicketEmailAsync(id, User.GetUserId(), request.Email);
            return NoContent();
        }

        [HttpGet("{id:guid}/checkin-status")]
        public async Task<ActionResult<CheckInStatusDto>> GetCheckInStatus(Guid id) =>
            Ok(await _bookingService.GetCheckInStatusAsync(id, User.GetUserId()));

        [HttpPost("{id:guid}/checkin")]
        public async Task<ActionResult<CheckInStatusDto>> CheckIn(Guid id) =>
            Ok(await _bookingService.CheckInAsync(id, User.GetUserId()));

        /// <summary>
        /// One entry per passenger on the booking (real airlines issue one
        /// boarding pass per passenger, not one per booking) - all sharing the
        /// same BoardingPassCode/QR payload, since check-in applies to the whole
        /// booking at once. 400 if the booking hasn't been checked in yet.
        /// </summary>
        [HttpGet("{id:guid}/boarding-pass")]
        public async Task<ActionResult<List<BoardingPassDto>>> GetBoardingPass(Guid id) =>
            Ok(await _bookingService.GetBoardingPassAsync(id, User.GetUserId()));
    }
}
