using FlyzenApi.Application.Exceptions;
using FlyzenApi.Application.Interfaces.Services;
using Microsoft.Extensions.Hosting;

namespace FlyzenApi.Infrastructure.Storage
{
    // Local-disk implementation. Chosen over cloud storage (S3/Blob/Cloudinary)
    // for now: no cloud account/credentials exist in this project yet and it
    // isn't in production. Swapping to cloud later is a new IFileStorageService
    // implementation + one DI line - callers only ever see the returned URL
    // string, same as they already do for external Wikimedia URLs.
    public class LocalFileStorageService : IFileStorageService
    {
        private const string PublicPathPrefix = "/uploads/";
        private const long AdminUploadMaxSizeBytes = 10 * 1024 * 1024;
        private const long MigrationMaxSizeBytes = 20 * 1024 * 1024;

        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp",
        };

        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/webp",
        };

        private readonly string _uploadsRoot;

        public LocalFileStorageService(IHostEnvironment env)
        {
            // ContentRootPath is /app in the Docker image (WORKDIR) and the
            // API project's own directory under `dotnet run` - either way,
            // "uploads" alongside it is what docker-compose.yml's volume and
            // Program.cs's static file mapping both point at.
            _uploadsRoot = Path.Combine(env.ContentRootPath, "uploads");
            Directory.CreateDirectory(_uploadsRoot);
        }

        public Task<string> SaveImageAsync(Stream content, string fileName, string contentType, long length, CancellationToken cancellationToken = default) =>
            SaveAsync(content, fileName, contentType, length, AdminUploadMaxSizeBytes, "10MB", cancellationToken);

        public Task<string> SaveMigratedImageAsync(Stream content, string fileName, string contentType, long length, CancellationToken cancellationToken = default) =>
            SaveAsync(content, fileName, contentType, length, MigrationMaxSizeBytes, "20MB", cancellationToken);

        private async Task<string> SaveAsync(Stream content, string fileName, string contentType, long length, long maxSizeBytes, string maxSizeLabel, CancellationToken cancellationToken)
        {
            if (length <= 0)
                throw new BadRequestException("No file was uploaded.");
            if (length > maxSizeBytes)
                throw new BadRequestException($"File exceeds the {maxSizeLabel} size limit.");

            var extension = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
                throw new BadRequestException("Unsupported file type. Allowed: JPG, PNG, WEBP.");
            if (string.IsNullOrEmpty(contentType) || !AllowedContentTypes.Contains(contentType))
                throw new BadRequestException("Unsupported file type. Allowed: JPG, PNG, WEBP.");

            var storedName = $"{Guid.NewGuid()}{extension.ToLowerInvariant()}";
            var fullPath = Path.Combine(_uploadsRoot, storedName);

            await using (var fileStream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write))
            {
                await content.CopyToAsync(fileStream, cancellationToken);
            }

            return PublicPathPrefix + storedName;
        }

        public void DeleteIfLocal(string? url)
        {
            if (string.IsNullOrEmpty(url) || !url.StartsWith(PublicPathPrefix, StringComparison.OrdinalIgnoreCase))
                return;

            var fileName = url[PublicPathPrefix.Length..];
            // Reject anything that isn't a bare filename (e.g. "../../etc/passwd")
            // before it ever touches Path.Combine.
            if (fileName.Contains('/') || fileName.Contains('\\'))
                return;

            var fullPath = Path.Combine(_uploadsRoot, fileName);
            try
            {
                if (File.Exists(fullPath)) File.Delete(fullPath);
            }
            catch (IOException)
            {
                // Best-effort cleanup - a locked/already-gone file isn't worth failing the request over.
            }
        }
    }
}
