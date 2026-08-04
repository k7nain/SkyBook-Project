using FlyzenApi.Application.DTOs;

namespace FlyzenApi.Application.Interfaces.Services
{
    public interface IContactService
    {
        Task SubmitAsync(ContactRequest request);
    }
}
