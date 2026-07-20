using FlyzenApi.Application.Interfaces;
using FlyzenApi.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace FlyzenApi.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ITicketPdfService, TicketPdfService>();
            services.AddScoped<ICurrencyService, CurrencyService>();
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
    }
}