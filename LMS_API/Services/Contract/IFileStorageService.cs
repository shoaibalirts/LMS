namespace LMS_API.Services.Contract
{
    public interface IFileStorageService
    {
        Task<string?> SaveAsync(IFormFile file);
        void Delete(string relativePath);
    }
}
