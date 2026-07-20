using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using FlyzenApi.Domain.Repositories;
using FlyzenApi.Application.Interfaces;

namespace FlyzenApi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly ITicketPdfService _ticketPdfService;
        private readonly IEmailService _emailService;

        public TicketsController(ITicketRepository ticketRepository, IBookingRepository bookingRepository, 
            ITicketPdfService ticketPdfService, IEmailService emailService)
        {
            _ticketRepository = ticketRepository;
            _bookingRepository = bookingRepository;
            _ticketPdfService = ticketPdfService;
            _emailService = emailService;
        }

        [HttpGet("{bookingId}/download")]
        public async Task<IActionResult> DownloadTicket(Guid bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                return NotFound(new { message = "Booking not found" });

            if (booking.Passengers.Count == 0)
                return BadRequest(new { message = "No passengers in booking" });

            var passenger = booking.Passengers.First();
            var pdfBytes = _ticketPdfService.GenerateTicketPdf(
                booking.PNR,
                $"{passenger.FirstName} {passenger.LastName}",
                booking.Flight.FlightNumber,
                booking.Flight.DepartureCity.Name,
                booking.Flight.ArrivalCity.Name,
                booking.Flight.DepartureTime,
                booking.Flight.ArrivalTime,
                passenger.Seat?.SeatNumber ?? "N/A"
            );

            return File(pdfBytes, "application/pdf", $"ticket_{booking.PNR}.pdf");
        }

        [HttpPost("{bookingId}/send-email")]
        public async Task<IActionResult> SendTicketEmail(Guid bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                return NotFound(new { message = "Booking not found" });

            if (booking.Passengers.Count == 0)
                return BadRequest(new { message = "No passengers in booking" });

            var passenger = booking.Passengers.First();
            var pdfBytes = _ticketPdfService.GenerateTicketPdf(
                booking.PNR,
                $"{passenger.FirstName} {passenger.LastName}",
                booking.Flight.FlightNumber,
                booking.Flight.DepartureCity.Name,
                booking.Flight.ArrivalCity.Name,
                booking.Flight.DepartureTime,
                booking.Flight.ArrivalTime,
                passenger.Seat?.SeatNumber ?? "N/A"
            );

            var emailSent = await _emailService.SendEmailAsync(
                booking.User.Email,
                "Your Flight Ticket",
                $"<h1>Flight Ticket for {passenger.FirstName} {passenger.LastName}</h1><p>Flight: {booking.Flight.FlightNumber}</p>",
                pdfBytes,
                $"ticket_{booking.PNR}.pdf"
            );

            if (!emailSent)
                return BadRequest(new { message = "Failed to send email" });

            return Ok(new { success = true, message = "Email sent successfully" });
        }
    }
}
