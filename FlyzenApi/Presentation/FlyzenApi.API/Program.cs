using System.Reflection;
using System.Security.Claims;
using System.Text;
using FlyzenApi.API.Hubs;
using FlyzenApi.API.Middleware;
using FlyzenApi.API.StaticPages;
using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Implementations.Services;
using FlyzenApi.Application.Interfaces.Services;
using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Repositories;
using FlyzenApi.Infrastructure.Auth;
using FlyzenApi.Infrastructure.Chat;
using FlyzenApi.Infrastructure.Currency;
using FlyzenApi.Infrastructure.DreamTripAi;
using FlyzenApi.Infrastructure.Email;
using FlyzenApi.Infrastructure.Localization;
using FlyzenApi.Infrastructure.Notifications;
using FlyzenApi.Infrastructure.Storage;
using FlyzenApi.Infrastructure.Translation;
using FlyzenApi.Persistence.DAL;
using FlyzenApi.Persistence.Implementations;
using FlyzenApi.Persistence.Implementations.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
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
            builder.Services.Configure<ContactOptions>(builder.Configuration.GetSection(ContactOptions.SectionName));

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(NormalizePostgresConnectionString(builder.Configuration.GetConnectionString("DefaultConnection"))));

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
            builder.Services.AddScoped<INotificationPusher, SignalRNotificationPusher>();

            // Infrastructure
            builder.Services.AddScoped<ITokenService, JwtTokenService>();
            builder.Services.AddScoped<IEmailService, SmtpEmailService>();
            builder.Services.AddScoped<IContactService, ContactService>();
            builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
            builder.Services.AddHttpClient<IAppleAuthService, AppleAuthService>();
            builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
            builder.Services.AddSingleton<IFileStorageService, LocalFileStorageService>();
            builder.Services.AddSingleton<SeedImageMigrationStatus>();
            builder.Services.AddHttpClient<ISeedImageMigrationService, SeedImageMigrationService>();
            builder.Services.AddSingleton<ITranslationService, TranslationService>();
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<ICurrencyConversionService, CbarCurrencyConversionService>();
            builder.Services.AddHttpClient<IContentTranslationService, ContentTranslationService>();
            builder.Services.AddHttpClient<IChatService, ChatService>();
            builder.Services.AddHttpClient<IDreamTripAiService, DreamTripAiService>();
            builder.Services.AddHostedService<TripReminderBackgroundService>();
            builder.Services.AddHostedService<FlightNotificationBackgroundService>();

            builder.Services.AddControllers();
            builder.Services.AddSignalR();

            // Per-IP limiter for the public contact form - the only unauthenticated
            // POST endpoint that triggers an outbound email, so it's the one worth
            // protecting from spam/abuse. Other endpoints are either read-only or
            // behind [Authorize].
            builder.Services.AddRateLimiter(options =>
            {
                options.AddPolicy("contact", httpContext =>
                    System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 3,
                            Window = TimeSpan.FromMinutes(10),
                            QueueLimit = 0,
                        }));

                // Chat is [Authorize]-gated already, but each message is a metered
                // OpenRouter call - cap per-user usage so one account can't run up
                // an unbounded bill or hammer the endpoint.
                options.AddPolicy("chat", httpContext =>
                    System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                            ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 20,
                            Window = TimeSpan.FromMinutes(10),
                            QueueLimit = 0,
                        }));

                // Dream Trip AI is [Authorize]-gated already, but each request is a
                // metered OpenRouter call (plus a catalog fetch) - cap per-user usage
                // like "chat", just tighter since each call is a heavier prompt.
                options.AddPolicy("dream-trip-ai", httpContext =>
                    System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                            ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromMinutes(30),
                            QueueLimit = 0,
                        }));

                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.HttpContext.Response.WriteAsJsonAsync(
                        new { message = "Çox sayda cəhd edildi. Zəhmət olmasa bir az sonra yenidən cəhd edin." }, token);
                };
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("Frontend", policy =>
                {
                    var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                        ?? Array.Empty<string>();
                    // AllowCredentials is required for the SignalR hub connection (its
                    // WebSocket/negotiate calls are treated as credentialed requests) -
                    // safe to add here since this policy already uses explicit
                    // WithOrigins rather than AllowAnyOrigin, which is the one
                    // combination CORS forbids alongside AllowCredentials.
                    policy.WithOrigins(origins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
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
                    // SignalR's browser/RN clients can't set the Authorization header on
                    // the WebSocket handshake, so the JS client sends the token as
                    // ?access_token=... instead (standard SignalR pattern) - only honored
                    // for the hub path, so normal API requests still require a real header.
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    },
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

            // Must run before anything that inspects scheme/client IP (HTTPS
            // redirection below, IP-partitioned rate limit policies) - behind
            // Render's (or any PaaS) reverse proxy, Kestrel only ever sees a
            // plain-HTTP connection from the proxy's own address; without this,
            // UseHttpsRedirection would 307-loop every request (proxy terminates
            // TLS -> forwards HTTP -> app redirects to HTTPS -> proxy terminates
            // TLS again -> ...) and rate limiting would key off the proxy's IP
            // instead of the real client's. KnownNetworks/KnownProxies are
            // cleared because the proxy's IP isn't fixed/known in advance on a
            // managed platform - safe here since the container is never
            // reachable except through that proxy.
            var forwardedHeadersOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            };
            forwardedHeadersOptions.KnownNetworks.Clear();
            forwardedHeadersOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardedHeadersOptions);

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // Serves uploaded images (LocalFileStorageService) at /uploads/* -
            // a physical provider rather than the wwwroot default so the
            // on-disk folder name matches docker-compose.yml's volume mount
            // (/app/uploads) regardless of convention.
            var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
            Directory.CreateDirectory(uploadsPath);
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(uploadsPath),
                RequestPath = "/uploads",
            });

            app.UseCors("Frontend");

            app.UseRateLimiter();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapGet("/reset-password", () => Results.Content(ResetPasswordPage.Html, "text/html"));

            // Cheap, dependency-free 200 for platform health checks (e.g. Render's
            // healthCheckPath) - deliberately doesn't touch the database, so a slow/
            // recovering DB connection doesn't get the whole service marked unhealthy
            // and cycled.
            app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

            app.MapControllers();
            app.MapHub<NotificationHub>("/hubs/notifications");

            app.Run();
        }

        // Render (like most managed Postgres providers) hands out connection info as
        // a postgres://user:pass@host:port/db URI, not Npgsql's native keyword=value
        // format that appsettings.json's local-dev value uses - normalize here so the
        // same ConnectionStrings:DefaultConnection config key works unchanged in both
        // environments instead of requiring anyone to hand-convert it in the Render
        // dashboard. SSL Mode=Require matches what Render's managed Postgres expects
        // (including over its internal/private network URL).
        private static string NormalizePostgresConnectionString(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return raw ?? string.Empty;

            if (!raw.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
                !raw.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            {
                return raw;
            }

            var uri = new Uri(raw);
            var userInfo = uri.UserInfo.Split(':', 2);
            var username = Uri.UnescapeDataString(userInfo[0]);
            var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
            var database = uri.AbsolutePath.TrimStart('/');
            var port = uri.Port > 0 ? uri.Port : 5432;

            return $"Host={uri.Host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
        }
    }
}
