using FlyzenApi.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FlyzenApi.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // Register infrastructure services here like Email, Token, PDF, Currency

            return services;
        }
    }
}