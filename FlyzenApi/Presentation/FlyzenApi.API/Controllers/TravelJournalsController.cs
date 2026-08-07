using FlyzenApi.API.Common;
using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Exceptions;
using FlyzenApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlyzenApi.API.Controllers
{
    [ApiController]
    [Route("api/travel-journals")]
    public class TravelJournalsController : ControllerBase
    {
        private readonly ITravelJournalService _journalService;
        private readonly IFileStorageService _fileStorageService;

        public TravelJournalsController(ITravelJournalService journalService, IFileStorageService fileStorageService)
        {
            _journalService = journalService;
            _fileStorageService = fileStorageService;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<TravelJournalDto>> Create(CreateTravelJournalRequest request) =>
            Ok(await _journalService.CreateAsync(User.GetUserId(), request));

        [HttpGet("city/{cityId:guid}")]
        public async Task<ActionResult<IEnumerable<TravelJournalDto>>> GetByCity(Guid cityId) =>
            Ok(await _journalService.GetByCityIdAsync(cityId));

        // Best-effort link from a Dream Trip catalog city to journals filed
        // against the matching bookable City - see TravelJournalService.GetByTripCityIdAsync.
        [HttpGet("trip-city/{tripCityId:guid}")]
        public async Task<ActionResult<IEnumerable<TravelJournalDto>>> GetByTripCity(Guid tripCityId) =>
            Ok(await _journalService.GetByTripCityIdAsync(tripCityId));

        [Authorize]
        [HttpGet("mine")]
        public async Task<ActionResult<IEnumerable<TravelJournalDto>>> GetMine() =>
            Ok(await _journalService.GetMineAsync(User.GetUserId()));

        [Authorize]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _journalService.DeleteAsync(id, User.GetUserId());
            return NoContent();
        }

        /// <summary>
        /// Uploads a single journal photo (jpg/png/webp, max 10MB) - same
        /// IFileStorageService validation/storage as the admin Dream Trip image
        /// upload, just reachable by any authenticated user instead of admins
        /// only. Returns a URL to include in CreateTravelJournalRequest.ImageUrls.
        /// </summary>
        [Authorize]
        [HttpPost("upload-image")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(11 * 1024 * 1024)]
        public async Task<ActionResult<UploadImageResponse>> UploadImage(IFormFile file, CancellationToken cancellationToken)
        {
            if (file is null)
                throw new BadRequestException("No file was uploaded.");

            await using var stream = file.OpenReadStream();
            var url = await _fileStorageService.SaveImageAsync(stream, file.FileName, file.ContentType, file.Length, cancellationToken);
            return Ok(new UploadImageResponse { Url = url });
        }
    }
}
