namespace FlyzenApi.Application.Interfaces.Services
{
    // Framework-agnostic on purpose (Stream, not IFormFile) so the Application
    // layer stays free of an ASP.NET Core reference - the controller extracts
    // the stream/metadata from IFormFile and passes primitives in.
    public interface IFileStorageService
    {
        /// <summary>
        /// Validates and saves an uploaded image, returning the public URL
        /// (e.g. "/uploads/{guid}.jpg") to store on the owning record.
        /// Throws BadRequestException for invalid type/size.
        /// </summary>
        Task<string> SaveImageAsync(Stream content, string fileName, string contentType, long length, CancellationToken cancellationToken = default);

        /// <summary>
        /// Same validation/storage as SaveImageAsync but with a higher size
        /// ceiling, for the one-time seed-image migration - curated Wikimedia
        /// photos are trusted content, not an arbitrary admin upload, and can
        /// legitimately be larger than the 5MB UI-upload guard allows.
        /// </summary>
        Task<string> SaveMigratedImageAsync(Stream content, string fileName, string contentType, long length, CancellationToken cancellationToken = default);

        /// <summary>
        /// Best-effort delete of a previously-uploaded local file. No-ops
        /// (does not throw) for external URLs or files that no longer exist -
        /// this is cleanup, not a correctness-critical operation.
        /// </summary>
        void DeleteIfLocal(string? url);
    }
}
