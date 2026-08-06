using LMS_API.Services.Contract;

namespace LMS_API.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private static readonly HashSet<string> AllowedExtensions =
            new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

        private readonly IWebHostEnvironment _env;

        public LocalFileStorageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string?> SaveAsync(IFormFile file)
        {
            if (file == null || file.Length == 0) return null;
            if (file.Length > MaxFileSizeBytes) return null;

            var extension = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(extension)) return null;

            var uploadsPath = GetUploadsPath();
            Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(uploadsPath, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/assignments/{fileName}";
        }

        public void Delete(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return;

            var fullPath = Path.Combine(GetUploadsPath(),
                Path.GetFileName(relativePath));

            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }

        private string GetUploadsPath()
        {
            var root = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            return Path.Combine(root, "uploads", "assignments");
        }
    }
}
