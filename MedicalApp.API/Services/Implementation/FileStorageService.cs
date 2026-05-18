using MedicalApp.API.Services.Interfaces;

namespace MedicalApp.API.Services.Implementation
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;

        // Allowed MIME types by category
        private static readonly HashSet<string> AllowedImageTypes =
            new() { "image/jpeg", "image/png", "image/gif", "image/webp" };

        private static readonly HashSet<string> AllowedAudioTypes =
            new() { "audio/mpeg", "audio/wav", "audio/ogg", "audio/webm", "audio/mp4" };

        private static readonly HashSet<string> AllowedFileTypes =
            new() { "application/pdf", "text/plain",
                    "application/msword",
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document" };

        private const long MaxFileSizeBytes = 20 * 1024 * 1024; // 20 MB

        public FileStorageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        // ─────────────────────────────────────────────
        // Upload profile/avatar image (legacy)
        // ─────────────────────────────────────────────
        public async Task<string> UploadFileAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is required.");

            return await SaveFileAsync(file, "uploads");
        }

        // ─────────────────────────────────────────────
        // Save attachment to a specific folder
        // ─────────────────────────────────────────────
        public async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty.");

            if (file.Length > MaxFileSizeBytes)
                throw new ArgumentException($"File size exceeds the 20 MB limit.");

            ValidateMimeType(file);

            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var folderPath = Path.Combine(webRoot, folder);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(folderPath, fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
            await file.CopyToAsync(stream);

            return $"/{folder}/{fileName}";
        }

        // ─────────────────────────────────────────────
        // Delete file by URL
        // ─────────────────────────────────────────────
        public async Task DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl)) return;

            try
            {
                var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var relativePath = fileUrl.TrimStart('/');
                var fullPath = Path.Combine(webRoot, relativePath);

                if (File.Exists(fullPath))
                    await Task.Run(() => File.Delete(fullPath));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FileStorageService] Error deleting file: {ex.Message}");
            }
        }

        // ─────────────────────────────────────────────
        // MIME type validation (security)
        // ─────────────────────────────────────────────
        private static void ValidateMimeType(IFormFile file)
        {
            var contentType = file.ContentType?.ToLowerInvariant() ?? "";

            var allowed = AllowedImageTypes
                .Union(AllowedAudioTypes)
                .Union(AllowedFileTypes);

            if (!allowed.Contains(contentType))
                throw new ArgumentException($"File type '{contentType}' is not allowed.");
        }
    }
}
