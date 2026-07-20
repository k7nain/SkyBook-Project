using FlyzenApi.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FlyzenApi.Persistence.DAL;
using FlyzenApi.Persistence.Repositories;

namespace FlyzenApi.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICityRepository, CityRepository>();
            services.AddScoped<IFlightRepository, FlightRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<ITicketRepository, TicketRepository>();

            return services;
        }
    }
}