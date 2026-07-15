using System.Threading.Tasks;

namespace FlyzenApi.Application.Interfaces
{
    public interface ITicketPdfService
    {
        Task<byte[]> GeneratePdfAsync(Domain.Entities.Ticket ticket);
    }
}