using System.Reflection;
using System.Security.Claims;
using System.Text;
using FlyzenApi.API.Middleware;
using FlyzenApi.API.StaticPages;
using FlyzenApi.Application.Implementations.Services;
using FlyzenApi.Application.Interfaces.Services;
using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Repositories;
using FlyzenApi.Infrastructure.Auth;
using FlyzenApi.Infrastructure.Currency;
using FlyzenApi.Infrastructure.Email;
using FlyzenApi.Infrastructure.Localization;
using FlyzenApi.Infrastructure.Notifications;
using FlyzenApi.Infrastructure.Translation;
using FlyzenApi.Persistence.DAL;
using FlyzenApi.Persistence.Implementations;
using FlyzenApi.Persistence.Implementations.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace FlyzenApi.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
            var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
                ?? throw new InvalidOperationException("Jwt configuration section is missing.");

            builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection(SmtpOptions.SectionName));
            builder.Services.Configure<GoogleAuthOptions>(builder.Configuration.GetSection(GoogleAuthOptions.SectionName));
            builder.Services.Configure<AppleAuthOptions>(builder.Configuration.GetSection(AppleAuthOptions.SectionName));
            builder.Services.Configure<NotificationOptions>(builder.Configuration.GetSection(NotificationOptions.SectionName));
            builder.Services.Configure<CurrencyOptions>(builder.Configuration.GetSection(CurrencyOptions.SectionName));
            builder.Services.Configure<ContentTranslationOptions>(builder.Configuration.GetSection(ContentTranslationOptions.SectionName));

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Repositories
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ICityRepository, CityRepository>();
            builder.Services.AddScoped<IAirlineRepository, AirlineRepository>();
            builder.Services.AddScoped<IFlightRepository, FlightRepository>();
            builder.Services.AddScoped<IBookingRepository, BookingRepository>();
            builder.Services.AddScoped<ITicketRepository, TicketRepository>();
            builder.Services.AddScoped<IMealOptionRepository, MealOptionRepository>();
            builder.Services.AddScoped<IBaggageOptionRepository, BaggageOptionRepository>();
            builder.Services.AddScoped<ITripCountryRepository, TripCountryRepository>();
            builder.Services.AddScoped<ITripCityRepository, TripCityRepository>();
            builder.Services.AddScoped<ITripPlaceRepository, TripPlaceRepository>();
            builder.Services.AddScoped<IPromoCodeRepository, PromoCodeRepository>();
            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
            builder.Services.AddScoped<IBookingReminderRepository, BookingReminderRepository>();
            builder.Services.AddScoped<IFlightNotificationLogRepository, FlightNotificationLogRepository>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Application services
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ICityService, CityService>();
            builder.Services.AddScoped<IFlightService, FlightService>();
            builder.Services.AddScoped<IBookingService, BookingService>();
            builder.Services.AddScoped<ITicketService, TicketService>();
            builder.Services.AddScoped<ICatalogService, CatalogService>();
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<IAirlineService, AirlineService>();
            builder.Services.AddScoped<ITripCountryService, TripCountryService>();
            builder.Services.AddScoped<ITripCityService, TripCityService>();
            builder.Services.AddScoped<ITripPlaceService, TripPlaceService>();
            builder.Services.AddScoped<IPromoCodeService, PromoCodeService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();

            // Infrastructure
            builder.Services.AddScoped<ITokenService, JwtTokenService>();
            builder.Services.AddScoped<IEmailService, SmtpEmailService>();
            builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
            builder.Services.AddHttpClient<IAppleAuthService, AppleAuthService>();
            builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
            builder.Services.AddSingleton<ITranslationService, TranslationService>();
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<ICurrencyConversionService, CbarCurrencyConversionService>();
            builder.Services.AddHttpClient<IContentTranslationService, ContentTranslationService>();
            builder.Services.AddHostedService<TripReminderBackgroundService>();
            builder.Services.AddHostedService<FlightNotificationBackgroundService>();

            builder.Services.AddControllers();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("Frontend", policy =>
                {
                    var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                        ?? Array.Empty<string>();
                    policy.WithOrigins(origins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var userIdValue = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                        var tokenVersionValue = context.Principal?.FindFirstValue("tokenVersion");
                        if (userIdValue is null || tokenVersionValue is null || !Guid.TryParse(userIdValue, out var userId))
                        {
                            context.Fail("Invalid token.");
                            return;
                        }

                        var userRepository = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                        var user = await userRepository.GetByIdAsync(userId);
                        if (user is null || user.TokenVersion.ToString() != tokenVersionValue)
                        {
                            context.Fail("Token has been revoked.");
                        }
                    },
                };
            });
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "SkyBook FlyzenApi", Version = "v1" });

                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your JWT token.",
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                };
                options.AddSecurityDefinition("Bearer", securityScheme);
                options.AddSecurityRequirement(new OpenApiSecurityRequirement { { securityScheme, Array.Empty<string>() } });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                    options.IncludeXmlComments(xmlPath);
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await DbInitializer.SeedAsync(context);
            }

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("Frontend");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapGet("/reset-password", () => Results.Content(ResetPasswordPage.Html, "text/html"));

            app.MapControllers();

            app.Run();
        }
    }
}
