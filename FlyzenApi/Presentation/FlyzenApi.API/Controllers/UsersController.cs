using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using FlyzenApi.Domain.Repositories;
using FlyzenApi.Application.Interfaces;

namespace FlyzenApi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            user.FirstName = request.FirstName ?? user.FirstName;
            user.LastName = request.LastName ?? user.LastName;
            user.Email = request.Email ?? user.Email;
            user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
            user.ProfilePictureUrl = request.ProfilePictureUrl ?? user.ProfilePictureUrl;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            return Ok(new { success = true, message = "User updated successfully", data = user });
        }

        [HttpPut("{id}/settings")]
        public async Task<IActionResult> UpdateUserSettings(Guid id, [FromBody] UpdateUserSettingsRequest request)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            user.CurrencyPreference = request.CurrencyPreference ?? user.CurrencyPreference;
            user.LanguagePreference = request.LanguagePreference ?? user.LanguagePreference;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            return Ok(new { success = true, message = "User settings updated successfully", data = user });
        }
    }

    public class UpdateUserRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }

    public class UpdateUserSettingsRequest
    {
        public string? CurrencyPreference { get; set; }
        public string? LanguagePreference { get; set; }
    }
}
