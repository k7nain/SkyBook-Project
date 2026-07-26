using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlyzenApi.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/promo-codes")]
    public class PromoCodesController : ControllerBase
    {
        private readonly IPromoCodeService _promoCodeService;

        public PromoCodesController(IPromoCodeService promoCodeService)
        {
            _promoCodeService = promoCodeService;
        }

        [HttpPost("validate")]
        public async Task<ActionResult<ValidatePromoCodeResponse>> Validate(ValidatePromoCodeRequest request) =>
            Ok(await _promoCodeService.ValidateAsync(request));
    }
}
