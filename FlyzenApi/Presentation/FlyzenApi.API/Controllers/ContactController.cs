using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FlyzenApi.API.Controllers
{
    [ApiController]
    [Route("api/contact")]
    public class ContactController : ControllerBase
    {
        private readonly IContactService _contactService;

        public ContactController(IContactService contactService)
        {
            _contactService = contactService;
        }

        [HttpPost]
        [EnableRateLimiting("contact")]
        public async Task<ActionResult<MessageResponse>> Submit(ContactRequest request)
        {
            await _contactService.SubmitAsync(request);
            return Ok(new MessageResponse { Message = "Mesajınız göndərildi, tezliklə sizinlə əlaqə saxlayacağıq" });
        }
    }
}
