using System.ComponentModel.DataAnnotations;

namespace FlyzenApi.Application.DTOs
{
    public class AirlineDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class CreateAirlineRequest
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(10)]
        public string Code { get; set; } = string.Empty;
    }
}
